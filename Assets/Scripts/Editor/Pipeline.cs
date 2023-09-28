
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace BrainAtlas.Editor
{
    public static class Pipeline
    {
        public static string DataFolder = "C:\\proj\\VBL\\BrainAtlas\\Pipelines\\data";

        /// <summary>
        /// (string name, string fullPath)
        /// </summary>
        private static List<(string atlasName, string fullPath)> _atlasInfoList;
        private static AtlasMetaData _atlasMetaData;
        private static AddressableAssetSettings _addressableSettings;

        [MenuItem("BrainAtlas/Run Pipeline")]
        public static void RunPipeline()
        {
            _addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            _atlasInfoList = new();
            _atlasMetaData = ScriptableObject.CreateInstance<AtlasMetaData>();

            Debug.Log("(BrainAtlas) Running pipeline...");
            GetAtlasList();

            // Save the atlas metadata object
            string atlasMetaPath = Path.Join("Assets/AddressableAssets", "metadataSO.asset");
            CreateAddressablesHelper(_atlasMetaData, atlasMetaPath, _addressableSettings.DefaultGroup);


            foreach (var atlasInfo in _atlasInfoList)
            {
                var updatedAtlasInfo = SetupAddressables(atlasInfo);
                

                // Build the Atlas ScriptableObjects
                AtlasMeta2SO(updatedAtlasInfo);

                // Convert mesh files 2 prefabs
                MeshFiles2Prefabs(updatedAtlasInfo);

                // 
            }

            EditorUtility.SetDirty(_addressableSettings);
            AssetDatabase.SaveAssets();
        }

        public static void GetAtlasList()
        {
            string[] subdirectories = Directory.GetDirectories(DataFolder);

            _atlasMetaData.AtlasNames = new();

            foreach (string atlasPath in subdirectories)
            {
                string atlasName = Path.GetFileName(atlasPath);
                _atlasInfoList.Add((atlasName, atlasPath));

                _atlasMetaData.AtlasNames.Add(atlasName);
            }
        }

        public static (string atlasName, string atlasPath, AddressableAssetGroup assetGroup) SetupAddressables((string atlasName, string atlasPath) atlasInfo)
        {
            string path = Path.Join("Assets/AddressableAssets", atlasInfo.atlasName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Create a new group
            AddressableAssetGroup atlasGroup = (_addressableSettings.groups.Any(x => x.Name == atlasInfo.atlasName)) ?
                _addressableSettings.groups.Find(x => x.Name == atlasInfo.atlasName) :
                _addressableSettings.CreateGroup(atlasInfo.atlasName, false, false, true, null);

            // Set the bundle mode to pack separately
            BundledAssetGroupSchema schema = atlasGroup.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
            {
                schema = atlasGroup.AddSchema<BundledAssetGroupSchema>();
            }
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            schema.BuildPath.SetVariableByName(_addressableSettings,
                        _addressableSettings.profileSettings.GetValueById("Default", "RemoteBuildPath"));
            schema.LoadPath.SetVariableByName(_addressableSettings,
                        _addressableSettings.profileSettings.GetValueById("Default", "RemoteLoadPath"));

            return (atlasInfo.atlasName, atlasInfo.atlasPath, atlasGroup);
        }


        /// <summary>
        /// Convert the metadata, structure tree, and mesh center coordinates to an Atlas scriptable object
        /// </summary>
        public static void AtlasMeta2SO((string atlasName, string atlasPath, AddressableAssetGroup assetGroup) atlasInfo)
        {
            // Create a new ReferenceAtlasData SO
            ReferenceAtlasData atlasData = ScriptableObject.CreateInstance<ReferenceAtlasData>();

            // Load the metadata json file into a string
            string metadataFile = Path.Join(atlasInfo.atlasPath, "meta.json");
            string metaStr = File.ReadAllText(metadataFile);

            // Deserialize the JSON string into a C# object
            MetaJSON data = JsonUtility.FromJson<MetaJSON>(metaStr);

            atlasData.name = data.name;
            // note that we invet dimensions to get ap/ml/dv instead of a/s/r
            Vector3 dimensionsIdx = new Vector3(data.shape[0], data.shape[2], data.shape[1]);
            Vector3 resolution = new Vector3(data.resolution[0], data.resolution[2], data.resolution[1]);
            // to get full dimensions we need the full resolution
            atlasData.Dimensions = Vector3.Scale(dimensionsIdx, resolution) / 1000f;
            Debug.Log(atlasData.Dimensions);
            atlasData.Resolution = resolution;

            // STRUCTURE ONTOLOGY
            // Load the ontology file
            string structuresFile = Path.Join(atlasInfo.atlasPath, "structures.json");
            string structuresListStr = File.ReadAllText(structuresFile);
            // wrap the structure list so that it can be parsed properly
            string wrappedStructuresListStr = $"{{\"structures\":{structuresListStr}}}";

            // Two step process to obtain the JSON out of this
            StructureListJSON structuresList = JsonUtility.FromJson<StructureListJSON>(wrappedStructuresListStr);
            // Now parse the structures
            List<OntologyTuple> ontologyData = new();
                
                //int, (string, string, Color, int[])> ontologyData = new();

            foreach (var structure in structuresList.structures)
            {
                ontologyData.Add(new OntologyTuple(structure.id, 
                    structure.acronym, 
                    structure.name, 
                    new Color(structure.rgb_triplet[0] / 255f, structure.rgb_triplet[1] / 255f, structure.rgb_triplet[2] / 255f),
                    structure.structure_id_path));
            }

            // Initialize ontology
            atlasData._privateOntologyData = ontologyData;

            // Create a temporary Ontology for local use
            Ontology tempOntology = new Ontology(ontologyData, null);

            // MESH CENTERS

            // Load the mesh centers file
            string meshCentersFile = Path.Join(atlasInfo.atlasPath, "mesh_centers.csv");
            string meshCentersStr = File.ReadAllText(meshCentersFile);

            var meshCentersCSV = CSVReader.ParseText(meshCentersStr);
            List<IV3Tuple> meshCentersum = new();

            for (int i = 0; i < meshCentersCSV.Count; i++)
            {
                var row = meshCentersCSV[i];
                string acronym = (string)row["structure_name"];
                Vector3 center = new Vector3(
                    (float)row["ap_um"],
                    (float)row["ml_um"],
                    (float)row["dv_um"]);
                meshCentersum.Add(new IV3Tuple(tempOntology.Acronym2ID(acronym), center));
            }

            atlasData._privateMeshCenters = meshCentersum;

            // Save the atlas data into the Addressables folder
            //atlasData
            string atlasSOPath = Path.Join("Assets/AddressableAssets", atlasInfo.atlasName, "metadataSO.asset");
            CreateAddressablesHelper(atlasData, atlasSOPath, atlasInfo.assetGroup);
        }

        public static void MeshFiles2Prefabs((string atlasName, string atlasPath, AddressableAssetGroup atlasGroup) atlasInfo)
        {
            // MESH FILES 2 ADDRESSABLES
            string targetFolderPath = Path.Join("Assets/ImportedObjFiles", atlasInfo.atlasName);
            if (!Directory.Exists(targetFolderPath))
                Directory.CreateDirectory(targetFolderPath);
            string meshFolderPath = Path.Join("Assets/AddressableAssets/", atlasInfo.atlasName, "meshes");
            if (!Directory.Exists(meshFolderPath))
                Directory.CreateDirectory(meshFolderPath);
            string atlasFolderPath = Path.Join("Assets/AddressableAssets/", atlasInfo.atlasName);

            // get the metadata for this atlas
            var atlasData = AssetDatabase.LoadAssetAtPath<ReferenceAtlasData>(Path.Join("Assets/AddressableAssets", atlasInfo.atlasName, "metaDataSO.asset"));

            // get the contents of the mesh directory, copy them all over into addressables
            Debug.Log(atlasInfo.atlasPath);
            string meshPath = File.ReadAllText(Path.Join(atlasInfo.atlasPath, "mesh_path.txt"));
            string[] meshFiles = Directory.GetFiles(meshPath, "*.obj");

            // Create the parent prefab
            GameObject parentGO = new GameObject();
            parentGO.name = $"{atlasInfo.atlasName}Parent";
            parentGO.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 180));
            parentGO.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            parentGO.transform.localPosition = new Vector3(-atlasData.Dimensions.y / 2f, atlasData.Dimensions.z / 2f, atlasData.Dimensions.x / 2f);
            string parentPrefabPath = Path.Join(atlasFolderPath, $"{parentGO.name}.prefab");
            PrefabUtility.SaveAsPrefabAsset(parentGO, parentPrefabPath);

            GameObject.DestroyImmediate(parentGO);

            for (int i = 0; i < 8; i++)
            //foreach (string meshFile in meshFiles)
            {
                string meshFile = meshFiles[i];

                string targetFilePath = Path.Join(targetFolderPath, Path.GetFileName(meshFile));

                File.Copy(meshFile, targetFilePath, true);
                AssetDatabase.ImportAsset(targetFilePath, ImportAssetOptions.ForceUpdate);

                GameObject objModel = AssetDatabase.LoadAssetAtPath<GameObject>(targetFilePath);

                string prefabPath = Path.Join(meshFolderPath, $"{Path.GetFileNameWithoutExtension(targetFilePath)}.prefab");
                PrefabUtility.SaveAsPrefabAsset(objModel, prefabPath);

                MarkAssetAddressable(prefabPath, atlasInfo.atlasGroup);
            }
        }

        /// <summary>
        /// Build the annotation color texture, and reference texture
        /// </summary>
        public static void AnnotationReference2Textures()
        {
            // Load the bytes file

            // Create the texture

            // Save to an asset file

            // Make the asset addressable
        }

        /// <summary>
        /// Build the annotation blob asset
        /// </summary>
        public static void Annotation2BlobAsset()
        {

        }

        private static void CreateAddressablesHelper(UnityEngine.Object assetData, string assetPath, AddressableAssetGroup assetGroup)
        {
            if (File.Exists(assetPath))
                AssetDatabase.DeleteAsset(assetPath);

            AssetDatabase.CreateAsset(assetData, assetPath);

            MarkAssetAddressable(assetPath, assetGroup);
        }

        private static void MarkAssetAddressable(string prefabPath, AddressableAssetGroup assetGroup)
        {
            _addressableSettings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(prefabPath), assetGroup);
        }
    }

    [Serializable]
    public class MetaJSON
    {
        public string name;
        public string citation;
        public string atlas_link;
        public string species;
        public bool symmetric;
        public float[] resolution;
        public string orientation;
        public string version;
        public float[] shape;
        //public List<List<float>> transform_to_bg; // we have to ignore this because we can't parse a list<vec4>
        public string[] additional_references;
    }

    [Serializable]
    public class StructureListJSON
    {
        public StructureJSON[] structures;
        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public class StructureJSON
    {
        public string acronym;
        public int id;
        public string name;
        public int[] structure_id_path;
        public int[] rgb_triplet;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
#endif