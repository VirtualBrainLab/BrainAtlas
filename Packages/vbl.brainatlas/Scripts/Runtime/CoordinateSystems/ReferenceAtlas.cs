using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrainAtlas
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
        Dictionary<int, (string name, Vector3Int color, int parent, int[] children)> ontology;
        public Vector3Int AreaColor(int areaID)
        {
            return ontology[areaID].color;
        }

        // Mesh centers
        public Dictionary<int, Vector3> MeshCenters;

        public Vector3 MeshCentersIdx(int meshID)
        {
            return Vector3.Scale(MeshCenters[meshID], ResolutionInverse);
        }

        // Annotation data
        public Texture3D AnnotationData;
        public int[] AnnotationIDs;

        public int Annotation(Vector3 coordIdx)
        {
            return AnnotationIDs[Mathf.RoundToInt(coordIdx.z * (Dimensions.z * Dimensions.y) + coordIdx.y * Dimensions.y + coordIdx.x)];
        }
    }

    /// <summary>
    /// A Reference Atlas is a collection of metadata about the dimensions and axis directions
    /// of an atlas. To use, instantiate a copy and set the Dimensions, AxisDirections, 
    /// </summary>
    public class ReferenceAtlas
    {
        #region Variables
        private ReferenceAtlasData _data;
        #endregion

        #region Properties
        public string Name { get => _data.name; }

        /// <summary>
        /// Dimensions of the Atlas
        /// </summary>
        public Vector3 Dimensions { get => _data.Dimensions; }

        /// <summary>
        /// The resolution of the atlas is the um each voxel takes up on (x,y,z)
        /// </summary>
        public Vector3 Resolution { get => _data.Resolution; }

        private Vector3 ResolutionInverse { get { return new Vector3(
            1000f / _data.Resolution.x, 
            1000f / _data.Resolution.y, 
            1000f / _data.Resolution.z); } }

        /// <summary>
        /// The zero offset indicates the position of the (0,0,0) coordinate in the ReferenceAtlas
        /// relative to the world (0,0,0) coordinate.
        /// 
        /// It should be used by the W2A and A2W functions to transform coordinates
        /// </summary>
        public Vector3 ZeroOffset { get => _data.ZeroOffset; }

        /// <summary>
        /// The reference coordinate is the point from which all transformed coordinates will be measured
        /// 
        /// For example, in the CCF this defaults to Bregma.
        /// </summary>
        public Vector3 ReferenceCoordinate
        { 
            get => _data.ReferenceCoordinate; 
            set => _data.ReferenceCoordinate = value; 
        }
        #endregion

        public ReferenceAtlas(ReferenceAtlasData referenceAtlasData)
        {
            _data = referenceAtlasData;
        }

        /// <summary>
        /// Move a coordinate from world space into the ReferenceAtlas space in um
        /// </summary>
        /// <param name="worldCoord"></param>
        /// <returns></returns>
        public Vector3 World2Atlas(Vector3 worldCoord)
        {
            return World2Atlas_Vector(worldCoord + ReferenceCoordinate) - ZeroOffset;
        }

        /// <summary>
        /// Move a coordinate from world space into the ReferenceAtlas voxel space
        /// </summary>
        /// <param name="worldCoord"></param>
        /// <returns></returns>
        public Vector3 World2AtlasIdx(Vector3 worldCoord)
        {
            return World2Atlas(Vector3.Scale(ResolutionInverse, worldCoord));
        }

        /// <summary>
        /// Move a coordinate from the ReferenceAtlas space into world space
        /// </summary>
        /// <param name="atlasCoord"></param>
        /// <returns></returns>
        public Vector3 Atlas2World(Vector3 atlasCoord)
        {
            return Atlas2World_Vector(atlasCoord + ZeroOffset) - ReferenceCoordinate;
        }

        public Vector3 AtlasIdx2World(Vector3 atlasIdxCoord)
        {
            return Vector3.Scale(Atlas2World(atlasIdxCoord), Resolution);
        }

        /// <summary>
        /// Rotate a normalized vector in the ReferenceAtlas space into world space
        /// </summary>
        /// <param name="normalizedAtlasVector"></param>
        /// <returns></returns>
        public Vector3 Atlas2World_Vector(Vector3 normalizedAtlasVector)
        {
            return new Vector3(normalizedAtlasVector.y, -normalizedAtlasVector.z, -normalizedAtlasVector.x);
        }

        /// <summary>
        /// Flip the Unity (x,y,z) axis directions of a vector in the world space
        /// into the ReferenceAtlas space
        /// 
        /// We define all reference atlases as being in (ap, ml, dv)
        /// </summary>
        /// <param name="normalizedWorldVector"></param>
        /// <returns></returns>
        public Vector3 World2Atlas_Vector(Vector3 normalizedWorldVector)
        {
            return new Vector3(-normalizedWorldVector.z, normalizedWorldVector.x, -normalizedWorldVector.y);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is ReferenceAtlas space &&
                   Name == space.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
