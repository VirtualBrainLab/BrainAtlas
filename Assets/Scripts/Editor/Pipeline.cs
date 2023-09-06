using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace BrainAtlas.Editor
{
    public static class Pipeline
    {
        public static string DataFolder = "C:\\proj\\VBL\\BrainAtlas\\Pipelines\\data";

        /// <summary>
        /// (string name, string fullPath)
        /// </summary>
        private static List<(string atlasName, string fullPath)> _atlasInfoList;
        private static Dictionary<string, ReferenceAtlasData> _atlasData;

        [MenuItem("BrainAtlas/Run Pipeline")]
        public static void RunPipeline()
        {
            _atlasInfoList = new();
            _atlasData = new();

            Debug.Log("(BrainAtlas) Running pipeline...");
            GetAtlasList();

            foreach (var atlasInfo in _atlasInfoList)
            {
                AtlasMetaMesh2SO(atlasInfo);
            }
        }

        public static void GetAtlasList()
        {
            string[] subdirectories = Directory.GetDirectories(DataFolder);

            foreach (string atlasPath in subdirectories)
            {
                _atlasInfoList.Add((Path.GetFileName(atlasPath), atlasPath));
            }
        }

        /// <summary>
        /// Convert the metadata, structure tree, and mesh center coordinates to an Atlas scriptable object
        /// </summary>
        public static void AtlasMetaMesh2SO((string atlasName, string atlasPath) atlasInfo)
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
            atlasData.Dimensions = Vector3.Scale(dimensionsIdx, resolution);
            atlasData.Resolution = resolution;

            // Now you can access the properties of the deserialized object
            Debug.Log(JsonUtility.ToJson(atlasData));

            // STRUCTURE ONTOLOGY
            // Load the ontology file
            string structuresFile = Path.Join(atlasInfo.atlasPath, "structures.json");
            string structuresListStr = File.ReadAllText(structuresFile);
            // wrap the structure list so that it can be parsed properly
            string wrappedStructuresListStr = "{\"structures\":" + structuresListStr + "}";

            // Two step process to obtain the JSON out of this
            StructureListJSON structuresList = JsonUtility.FromJson<StructureListJSON>(wrappedStructuresListStr);
            // Now parse the structures
            foreach (var structure in structuresList.structures)
            {
                Debug.Log(structure);
            }


            // MESH CENTERS

            // Load the mesh centers file
        }

        /// <summary>
        /// Build the annotation color texture, and reference texture
        /// </summary>
        public static void AnnotationReference2Textures()
        {

        }

        /// <summary>
        /// Build the annotation blob asset
        /// </summary>
        public static void Annotation2BlobAsset()
        {

        }
    }

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

    public class StructureListJSON
    {
        public string[] structures;
    }

    public class StructureJSON
    {
        public string acronym;
        public int id;
        public string name;
        public int[] structure_id_path;
        public int[] rgb_triplet;
    }
}