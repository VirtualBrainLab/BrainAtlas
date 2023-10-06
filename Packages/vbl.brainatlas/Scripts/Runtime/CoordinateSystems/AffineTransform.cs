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
        /// <param name="centerCoord">(0,0,0) coordinate of transform x/y/z</param>
        /// <param name="scaling">scaling on x/y/z</param>
        /// <param name="rotation">rotation around z, y, x in that order (or on xy plane, then xz plane, then yz plane)</param>
        public AffineTransform(Vector3 scaling, Vector3 rotation)
        {
            _scaling = scaling;
            _inverseScaling = new Vector3(1f / _scaling.x, 1f / _scaling.y, 1f / _scaling.z);
            _rotation = Quaternion.Euler(rotation);
            _inverseRotation = Quaternion.Inverse(_rotation);
        }

        public override Vector3 U2T(Vector3 ccfCoord)
        {
            return Vector3.Scale(_rotation*ccfCoord, _scaling);
        }

        public override Vector3 T2U(Vector3 coordTransformed)
        {
            return _inverseRotation*Vector3.Scale(coordTransformed, _inverseScaling);
        }

        public override Vector3 U2T_Vector(Vector3 coordSpace)
        {
            return new Vector3(
                Mathf.Sign(_scaling.x) * coordSpace.x,
                Mathf.Sign(_scaling.y) * coordSpace.y,
                Mathf.Sign(_scaling.z) * coordSpace.z);
        }

        public override Vector3 T2U_Vector(Vector3 coordTransformed)
        {
            return new Vector3(
                Mathf.Sign(_inverseScaling.x) * coordTransformed.x,
                Mathf.Sign(_inverseScaling.y) * coordTransformed.y,
                Mathf.Sign(_inverseScaling.z) * coordTransformed.z);
        }
    }
}