using System;
using UnityEngine;


namespace BrainAtlas.CoordinateSystems
{
    public abstract class AtlasTransform : CoordinateTransform
    {
    }

    public class NullTransform : AtlasTransform
    {
        public override string Name => "null";

        public override string Prefix => "";

        public override Vector3 U2T(Vector3 atlasCoordU)
        {
            return atlasCoordU;
        }

        public override Vector3 U2T_Vector(Vector3 normalizedAtlasVectorU)
        {
            return normalizedAtlasVectorU;
        }

        public override Vector3 T2U(Vector3 atlasCoordT)
        {
            return atlasCoordT;
        }

        public override Vector3 T2U_Vector(Vector3 normalizedAtlasVectorT)
        {
            return normalizedAtlasVectorT;
        }
    }
}