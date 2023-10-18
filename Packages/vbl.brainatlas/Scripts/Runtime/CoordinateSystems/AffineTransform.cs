using UnityEngine;

namespace BrainAtlas.CoordinateSystems
{
    public abstract class AffineTransform : AtlasTransform
    {
        private Vector3 _scaling;
        private Vector3 _inverseScaling;
        private Quaternion _rotation;
        private Quaternion _inverseRotation;

        /// <summary>
        /// Define an AffineTransform by passing the translation, scaling, and rotation that go from origin space to this space
        /// </summary>
        /// <param name="scaling">scaling on x/y/z</param>
        /// <param name="rotation">rotation around z, y, x in that order (or on xy plane, then xz plane, then yz plane)</param>
        public AffineTransform(Vector3 scaling, Vector3 rotation)
        {
            _scaling = scaling;
            _inverseScaling = new Vector3(1f / _scaling.x, 1f / _scaling.y, 1f / _scaling.z);
            _rotation = Quaternion.Euler(rotation);
            _inverseRotation = Quaternion.Inverse(_rotation);
        }

        public override Vector3 U2T(Vector3 coordU)
        {
            return Vector3.Scale(_rotation*coordU, _scaling);
        }

        public override Vector3 T2U(Vector3 coordT)
        {
            return _inverseRotation*Vector3.Scale(coordT, _inverseScaling);
        }

        public override Vector3 U2T_Vector(Vector3 vectorU)
        {
            return new Vector3(
                Mathf.Sign(_scaling.x) * vectorU.x,
                Mathf.Sign(_scaling.y) * vectorU.y,
                Mathf.Sign(_scaling.z) * vectorU.z);
        }

        public override Vector3 T2U_Vector(Vector3 vectorT)
        {
            return new Vector3(
                Mathf.Sign(_inverseScaling.x) * vectorT.x,
                Mathf.Sign(_inverseScaling.y) * vectorT.y,
                Mathf.Sign(_inverseScaling.z) * vectorT.z);
        }
    }
}
