using UnityEngine;

namespace BrainAtlas.CoordinateSystems
{
    
    /// <summary>
    /// CoordinateSpace representing a BrainGlobe reference atlas in (ap, lr, dv)
    /// 
    /// The (0,0,0) coordinate is the anterior, left, dorsal coordinate
    /// 
    /// The axes point toward posterior, right, ventral
    /// </summary>
    public class BGAtlasSpace : CoordinateSpace
    {
        public override string Name { get; }

        public override Vector3 Dimensions { get; }

        public BGAtlasSpace(string name, Vector3 dimensions)
        {
            Name = name;
            Dimensions = dimensions;

            ZeroOffset = new Vector3(Dimensions.y / 2f, Dimensions.z / 2f, - Dimensions.x / 2f);
        }
        
        public override Vector3 World2Space(Vector3 coordWorld)
        {
            return World2Space_Vector(coordWorld + ZeroOffset) - ReferenceCoord;
        }

        public override Vector3 Space2World(Vector3 spaceCoord)
        {
            return Space2World_Vector(spaceCoord + ReferenceCoord) - ZeroOffset;
        }

        public override Vector3 Space2World_Vector(Vector3 vectorSpace)
        {
            return new Vector3(vectorSpace.y, -vectorSpace.z, -vectorSpace.x);
        }

        public override Vector3 World2Space_Vector(Vector3 vectorWorld)
        {
            return new Vector3(-vectorWorld.z, vectorWorld.x, -vectorWorld.y);
        }
    }
}