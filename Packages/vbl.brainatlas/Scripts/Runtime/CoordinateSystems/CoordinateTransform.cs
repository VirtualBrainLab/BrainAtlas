using System;
using UnityEngine;


namespace BrainAtlas.CoordinateSystems
{
    public abstract class CoordinateTransform
    {
        public abstract string Name { get; }
        public abstract string Prefix { get; }

        /// <summary>
        /// For a point in a CoordinateSpace, go from the Transformed space
        /// back to the Untransformed space
        /// </summary>
        /// <param name="coordT"></param>
        /// <returns></returns>
        public abstract Vector3 T2U(Vector3 coordT);

        /// <summary>
        /// For a point in a CoordinateSpace, go from the Untransformed space
        /// to the Transformed space
        /// </summary>
        /// <param name="coordU"></param>
        /// <returns></returns>
        public abstract Vector3 U2T(Vector3 coordU);

        /// <summary>
        /// For a normalized vector in a Coordinate Space, go from the Transformed space
        /// back to the Untransformed space
        /// 
        /// This applies rotations and axis changes, but not scaling
        /// </summary>
        /// <param name="vectorT"></param>
        /// <returns></returns>
        public abstract Vector3 T2U_Vector(Vector3 vectorT);

        /// <summary>
        /// For a normalized vector in a CoordinateSpace, go from the Untransformed space
        /// to the Transformed space
        /// 
        /// This applies rotations and axis changes, but not scaling
        /// </summary>
        /// <param name="vectorU"></param>
        /// <returns></returns>
        public abstract Vector3 U2T_Vector(Vector3 vectorU);


        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateTransform transform &&
                   Name == transform.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}