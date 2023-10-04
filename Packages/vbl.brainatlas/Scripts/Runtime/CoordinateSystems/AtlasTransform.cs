using System;
using UnityEngine;


namespace BrainAtlas
{
    public abstract class AtlasTransform
    {
        public abstract string Name { get; }
        public abstract string Prefix { get; }

        /// <summary>
        /// Convert a transformed atlas coordinate back to the un-transformed space
        /// </summary>
        /// <param name="atlasCoordT">Transformed coordinate</param>
        /// <returns></returns>
        public abstract Vector3 T2Atlas(Vector3 atlasCoordT);

        /// <summary>
        /// Convert an atlas coordinate into a transformed space
        /// </summary>
        /// <param name="atlasCoordU">CCF coordinate in ap/dv/lr</param>
        /// <returns></returns>
        public abstract Vector3 Atlas2T(Vector3 atlasCoordU);

        public abstract Vector3 T2Atlas_Vector(Vector3 normalizedAtlasVectorT);

        public abstract Vector3 Atlas2T_Vector(Vector3 normalizedAtlasVectorU);


        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is AtlasTransform transform &&
                   Name == transform.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }

    public class NullTransform : AtlasTransform
    {
        public override string Name => "null";

        public override string Prefix => "0";

        public override Vector3 Atlas2T(Vector3 atlasCoordU)
        {
            return atlasCoordU;
        }

        public override Vector3 Atlas2T_Vector(Vector3 normalizedAtlasVectorU)
        {
            return normalizedAtlasVectorU;
        }

        public override Vector3 T2Atlas(Vector3 atlasCoordT)
        {
            return atlasCoordT;
        }

        public override Vector3 T2Atlas_Vector(Vector3 normalizedAtlasVectorT)
        {
            return normalizedAtlasVectorT;
        }
    }
}