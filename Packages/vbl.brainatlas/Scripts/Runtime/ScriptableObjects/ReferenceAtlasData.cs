using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrainAtlas.ScriptableObjects
{
    public class ReferenceAtlasData : ScriptableObject
    {
        // Basic metadata
        public Vector3 Dimensions;
        public Vector3 Resolution;
        public Vector3 ResolutionInverse
        {
            get
            {
                return new Vector3(
                1000f / Resolution.x,
                1000f / Resolution.y,
                1000f / Resolution.z);
            }
        }
        public Vector3 ZeroOffset
        {
            get
            {
                return Vector3.Scale(Dimensions, new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
        public Vector3 ReferenceCoordinate;

        // Structure ontology
        public List<OntologyTuple> _privateOntologyData;

        /// <summary>
        /// Mesh centers in um
        /// </summary>
        public List<IV3Tuple> _privateMeshCenters;

        public int[] DefaultAreas;
    }

    [Serializable]
    public class IV3Tuple
    {
        public int i;
        public Vector3 v3;
        public Vector3 v3lh;

        public IV3Tuple(int i, Vector3 v3, Vector3 v3lh)
        {
            this.i = i;
            this.v3 = v3;
            this.v3lh = v3lh;
        }
    }

    [Serializable]
    public class OntologyTuple
    {
        public int id;
        public string acronym;
        public string name;
        public Color color;
        public int[] structure_id_path;
        public int remap_no_layers;
        public int remap_defaults;

        public OntologyTuple(int id, string acronym, string name, Color color, int[] structure_id_path, int remap_no_layers, int remap_defaults)
        {
            this.id = id;
            this.acronym = acronym;
            this.name = name;
            this.color = color;
            this.structure_id_path = structure_id_path;
            this.remap_no_layers = remap_no_layers;
            this.remap_defaults = remap_defaults;
        }
    }
}
