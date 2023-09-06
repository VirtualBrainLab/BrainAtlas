using UnityEngine;


namespace BrainAtlas
{
    public class CCFTransform : AtlasTransform
    {
        public override string Name
        {
            get
            {
                return "Allen CCF";
            }
        }

        public override string Prefix
        {
            get
            {
                return "ccf";
            }
        }

        public override Vector3 T2Atlas(Vector3 coord)
        {
            return coord;
        }

        public override Vector3 Atlas2T(Vector3 coord)
        {
            return coord;
        }

        public override Vector3 T2Atlas_Vector(Vector3 coordTransformed)
        {
            return coordTransformed;
        }

        public override Vector3 Atlas2T_Vector(Vector3 coordSpace)
        {
            return coordSpace;
        }
    }
}