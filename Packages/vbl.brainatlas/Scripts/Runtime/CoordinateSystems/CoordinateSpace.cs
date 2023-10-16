using System;
using UnityEngine;

namespace BrainAtlas.CoordinateSystems
{
    public abstract class CoordinateSpace
    {
        public abstract string Name { get; }

        /// <summary>
        /// Dimensions are in mm
        /// </summary>
        public abstract Vector3 Dimensions { get; }

        /// <summary>
        /// If the (0,0,0) coordinate is not the World (0,0,0) coordinate, 
        /// ZeroOffset should return the relative position as a vecWorld
        /// </summary>
        public Vector3 ZeroOffset { get; set; }

        /// <summary>
        /// The ReferenceCoordinate is a vecSpace that defines where the 
        /// reference point of the coordinate space is located, this is a
        /// (0,0,0) coordinate that users might actually care about,
        /// say Bregma or Lambda on the rodent skull
        /// </summary>
        public Vector3 ReferenceCoord { get; set; } = Vector3.zero;

        public abstract Vector3 Space2World(Vector3 coordSpace, bool useReference = true);
        public abstract Vector3 World2Space(Vector3 coordWorld, bool useReference = true);

        public abstract Vector3 Space2World_Vector(Vector3 vecSpace);

        public abstract Vector3 World2Space_Vector(Vector3 vecWorld);

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateSpace space &&
                   Name == space.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}