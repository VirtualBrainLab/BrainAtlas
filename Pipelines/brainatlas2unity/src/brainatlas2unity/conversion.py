from brainglobe_atlasapi.bg_atlas import BrainGlobeAtlas
import numpy as np
import os
import json
import pandas as pd
import shutil
import bpy
import os
import bg_space as bg

def intermediate_meta(atlas, atlas_name, data_path):
    """Build the metadata file

    Parameters
    ----------
    atlas_name : string
        bg-atlas name
    """

    meta_file = os.path.join(data_path,atlas_name,"meta.json")

    with open(meta_file, 'w', encoding='utf-8') as f:
        json.dump(atlas.metadata, f, ensure_ascii=False, indent=4)

def intermediate_ref_image(atlas, atlas_name, data_path):
    """Build the intermediate reference image. Input is the bg-atlas reference image (float32) output is a flattened .bytes file.

    Parameters
    ----------
    atlas_name : string
        bg-atlas name
    """

    reference_file = os.path.join(data_path,atlas_name,"reference.bytes")

    reference = atlas.reference

    reference = reference.astype(np.float32)
    reference = reference / np.max(reference)

    if not isinstance(reference[0,0,0], np.float32):
        print("Warning: atlas has incorrect reference image format")

    reference.flatten().tofile(reference_file)

def intermediate_annot_image(atlas, atlas_name, data_path):
    """Build the intermediate annotation image. Input is the bg-atlas annotation image (uint32) output is a flattened .bytes file

    Parameters
    ----------
    atlas_name : string
        bg-atlas name
    """

    annotation_file = os.path.join(data_path,atlas_name,"annotation.bytes")

    if os.path.exists(annotation_file):
        return

    annotation = atlas.annotation

    if not isinstance(annotation[0,0,0], np.uintc):
        print("Warning: atlas has incorrect reference image format")

    annotation.flatten().tofile(annotation_file)

def intermediate_mesh_centers(atlas, atlas_name, data_path):
    """Build the intermediate mesh center CSV files"""
    
    def f3f(val):
        return f'{val:0.3f}'

    mesh_center_file = os.path.join(data_path, atlas_name, "mesh_centers.csv")

    if os.path.exists(mesh_center_file):
        return

    res = atlas.resolution
    root_name = "root"

    all_structures = atlas.get_structure_descendants(root_name)
    all_structures.insert(0,root_name)

    df = pd.DataFrame(columns=["structure_name","ap","ml","dv","ap-lh","ml-lh","dv-lh"])


    for i, structure in enumerate(all_structures):
        mask = atlas.get_structure_mask(atlas.structures[structure]["id"])
        mask_left = mask[:,:,0:int(mask.shape[2]/2)]
        # the mask is in both hemispheres, but we actually only want to calculate one side. We'll do the left side, so to calculate right
        # you have to do dimension_size - value on each dimension

        if not np.any(mask.flatten()) or not np.any(mask_left.flatten()):
            df.loc[i] = [structure, -1, -1, -1, -1, -1, -1]
        else:
            coords_full = np.mean(np.argwhere(mask), axis=0)
            coords_left = np.mean(np.argwhere(mask_left), axis=0)
            # coords are ap/dv/ml, so flip 1/2
            df.loc[i] = [structure, f3f(np.float32(coords_full[0]*res[0])), f3f(np.float32(coords_full[2]*res[2])),
                         f3f(np.float32(coords_full[1]*res[1])), f3f(np.float32(coords_left[0]*res[0])),
                         f3f(np.float32(coords_left[2]*res[2])), f3f(np.float32(coords_left[1]*res[1]))]

    df.to_csv(mesh_center_file, float_format='%0.3f', index=False)



def intermediate_structures(atlas, atlas_name, data_path):
    """Save the structure hierarchy data (this is a symlink to the brainglobe file)

    Parameters
    ----------
    atlas_name : string
        bg-atlas name
    """

    shutil.copyfile(os.path.join(atlas.root_dir, 'structures.json'),os.path.join(data_path,atlas_name,"structures.json"))

# Function to recalculate normals and save the object
def modifiers_normals_smooth(obj_file, normals, smoothing):
    # Import the .obj file
    bpy.ops.wm.obj_import(filepath=obj_file)

    # Select the imported object
    obj = bpy.context.selected_objects[0]

    # Switch to Edit Mode and recalculate normals
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    

    # Apply the Smooth modifier with factor=1 and repeat=10
    if smoothing:
        bpy.ops.object.modifier_add(type='SMOOTH')
        smooth_modifier = obj.modifiers[-1]  # Get the last added modifier (assuming no other modifiers are added in between)
        smooth_modifier.factor = 1
        smooth_modifier.iterations = 5

    if normals:
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.normals_make_consistent(inside=False)

    bpy.ops.object.mode_set(mode='OBJECT')

    # Save the object with recalculated normals
    bpy.ops.wm.obj_export(filepath=obj_file, export_selected_objects =True, export_normals =True, export_materials =False)

    # Unlink the imported object (remove it from the scene)
    bpy.data.objects.remove(obj)

def cleanup_folder(atlas_name, data_path):
    """Remove extra #L.obj #LL.obj and *.mat files

    Parameters
    ----------
    atlas_name : _type_
        _description_
    """

    def is_desired_filename(filename):
        return filename.endswith(".obj") and filename[0].isdigit() and not filename.endswith("L.obj") and not filename.endswith("LL.obj")

    folder = os.path.join(data_path, atlas_name,'meshes')

    # Get a list of all files in the folder
    all_files = os.listdir(folder)

    # Iterate through the files and delete those that don't match the pattern
    for filename in all_files:
        if not is_desired_filename(filename):
            file_to_delete = os.path.join(folder, filename)
            os.remove(file_to_delete)

def apply_blender_repairs(atlas_name, data_path, normals = True, smoothing = False):
    """Load all OBJ files and recalculate the normals

    Parameters
    ----------
    atlas_name : _type_
        _description_
    """

    if not normals and not smoothing:
        return

    folder = os.path.join(data_path, atlas_name, 'meshes')

    # List all .obj files in the folder
    obj_files = [f for f in os.listdir(folder) if f.endswith(".obj") and not f.endswith("L.obj") and not f.endswith("LL.obj")]

    # Process each .obj file
    for obj_file in obj_files:
        modifiers_normals_smooth(os.path.join(folder, obj_file), normals, smoothing)


def intermediate_mesh_files(atlas, atlas_name, data_path):
    """Run the Blender slicer to make the single-hemisphere files
    """
    slice_depth = - atlas.metadata['shape'][2] * atlas.metadata['resolution'][2] / 2

    folder = os.path.join(data_path, atlas_name,'meshes')

    files = [file for file in os.listdir(folder) if file.endswith('.obj')]

    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()

    for file in files:
        fpath = os.path.join(folder, file)
        fpath_out = os.path.join(folder, os.path.splitext(file)[0] + 'L' + os.path.splitext(file)[1])

        if os.path.exists(fpath_out):
            continue

        fpath_mtl = os.path.join(folder, os.path.splitext(file)[0] + 'L.mtl')

        bpy.ops.object.select_all(action='DESELECT')
        bpy.ops.wm.obj_import(filepath=fpath)
        bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]

        #obj = bpy.context.object
    #    bpy.ops.transform.resize(value=(0.001, 0.001, 0.001))

        bpy.ops.object.editmode_toggle()
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.bisect(plane_co=(0.0,slice_depth,0.0),plane_no=(0.0,-1.0,0.0),use_fill=True,clear_outer=True)
        bpy.ops.object.modifier_add(type='TRIANGULATE')
        bpy.ops.object.editmode_toggle()

        bpy.ops.wm.obj_export(filepath=fpath_out, export_selected_objects =True, export_normals =True, export_materials =False)

        bpy.ops.object.delete()

        # trash the .mtl file
        if os.path.exists(fpath_mtl):
            os.remove(fpath_mtl)

def copy_mesh_files(atlas, atlas_name, data_path):

    folder = os.path.join(atlas.root_dir,'meshes')
    local_folder = os.path.join(data_path,atlas_name,"meshes")

    if not os.path.isdir(local_folder):
        os.mkdir(local_folder)

    # copy all the files
    # List all .obj files in the folder
    obj_files = [f for f in os.listdir(folder) if f.endswith(".obj") and not f.endswith("L.obj") and not f.endswith("LL.obj")]

    # Process each .obj file
    for obj_file in obj_files:
        source = os.path.join(folder, obj_file)
        dest = os.path.join(local_folder, obj_file)
        shutil.copy(source, dest)

    # write a text file with the location of the files
    mesh_path_file = os.path.join(data_path,atlas_name,"mesh_path.txt")
    with open(mesh_path_file, 'w') as f:
        f.write(local_folder)
        f.close()