using UnityEngine;
using BrainAtlas;
using UnityEngine.Events;

public class CoordinateSpaceManager : MonoBehaviour
{
    /// <summary>
    /// Stores the original transform, when the active transform is a custom transform
    /// </summary>
    public static AtlasTransform OriginalTransform;

    public static ReferenceAtlas ActiveCoordinateSpace;
    public static AtlasTransform ActiveCoordinateTransform;
    public static CoordinateSpaceManager Instance;

    public UnityEvent RelativeCoordinateChangedEvent;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Convert a world coordinate into the corresponding world coordinate after transformation
    /// </summary>
    /// <param name="coordWorld"></param>
    /// <returns></returns>
    public static Vector3 WorldU2WorldT(Vector3 coordWorld)
    {
        return ActiveCoordinateSpace.Atlas2World(ActiveCoordinateTransform.T2Atlas_Vector(ActiveCoordinateTransform.Atlas2T(ActiveCoordinateSpace.World2Atlas(coordWorld))));
    }

    public static Vector3 WorldT2WorldU(Vector3 coordWorldT)
    {
        return ActiveCoordinateSpace.Atlas2World(ActiveCoordinateTransform.T2Atlas(ActiveCoordinateTransform.Atlas2T_Vector(ActiveCoordinateSpace.World2Atlas(coordWorldT))));
    }

    /// <summary>
    /// Helper function
    /// Convert a world coordinate into a transformed coordinate using the reference coordinate and the axis change
    /// </summary>
    /// <param name="coordWorld"></param>
    /// <returns></returns>
    public static Vector3 World2TransformedAxisChange(Vector3 coordWorld)
    {
        return ActiveCoordinateTransform.Atlas2T_Vector(ActiveCoordinateSpace.World2Atlas(coordWorld));
    }

    public static Vector3 Transformed2WorldAxisChange(Vector3 coordTransformed)
    {
        return ActiveCoordinateSpace.Atlas2World(ActiveCoordinateTransform.T2Atlas_Vector(coordTransformed));
    }
}