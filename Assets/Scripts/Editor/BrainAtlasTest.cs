using BrainAtlas;
using BrainAtlas.CoordinateSystems;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BrainAtlasTest : MonoBehaviour
{
    public Texture2D APSlice;

    [SerializeField] private BrainAtlasManager _BAM;

    [SerializeField] private Material _sideMaterialTest;
    [SerializeField] private Color _sideColor;

    public Texture3D AnnotationTexture;
    public Texture3D ReferenceTexture;

    private OntologyNode _root;

    public async void RunTest()
    {
        var loadTask = BrainAtlasManager.LoadAtlas(BrainAtlasManager.AtlasNames[0]);
        await loadTask;

        Debug.Log("Mouse atlas loaded");

        // Load some nodes
        ReferenceAtlas _referenceAtlas = loadTask.Result;

        // Test node loading, transforms, materials, colors
        if (false)
        {
            List<Task> tasks = new();
            foreach (int areaID in _referenceAtlas.DefaultAreas)
                tasks.Add(_referenceAtlas.Ontology.ID2Node(areaID).LoadMesh(OntologyNode.OntologyNodeSide.Full));
            await Task.WhenAll(tasks);

            await Task.Delay(1000);

            foreach (int areaID in _referenceAtlas.DefaultAreas)
                _referenceAtlas.Ontology.ID2Node(areaID).SetVisibility(false);

            //Now let's load a sided area
            _root = _referenceAtlas.Ontology.ID2Node(8);
            await _root.LoadMesh(OntologyNode.OntologyNodeSide.Left);
            _root.SetMaterial(_sideMaterialTest);
            _root.SetColor(_sideColor);

            // Create a new custom transform that tilts up 90 degrees
            AtlasTransform custom = new CustomAffineTransform(Vector3.one * 1.1f, Vector3.zero);
            BrainAtlasManager.ActiveAtlasTransform = custom;

            await Task.Delay(1000);

            custom = new CustomAffineTransform(Vector3.one, new Vector3(0f, -90f, 0f));
            BrainAtlasManager.ActiveAtlasTransform = custom;

            await Task.Delay(1000);

            _root.ResetAtlasTransform();
            AtlasTransform nullT = new NullTransform();
            BrainAtlasManager.ActiveAtlasTransform = nullT;
            _root.SetVisibility(false, OntologyNode.OntologyNodeSide.Left);
            _root.SetVisibility(true, OntologyNode.OntologyNodeSide.Right);

            await Task.Delay(1000);
        }

        // Test Texture3D loading
        if (true)
        {
            _referenceAtlas.LoadAnnotationTexture();
            await _referenceAtlas.AnnotationTextureTask;

            AnnotationTexture = _referenceAtlas.AnnotationTexture;
        }

        // Test coordinate transformations
        if (false)
        {
            CoordinateSpace bgSpace = new BGAtlasSpace("temp", BrainAtlasManager.ActiveReferenceAtlas.Dimensions);
            Debug.Log(bgSpace.ZeroOffset);

            Vector3 cornerU = Vector3.zero;
            Vector3 otherU = new Vector3(13.2f, 11.4f, 8f);

            Vector3 cornerWorld = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(cornerU);
            Vector3 otherWorld = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(otherU);

            Debug.Log((cornerU, cornerWorld));
            Debug.Log((otherU, otherWorld));

            var cornerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cornerGO.transform.position = cornerWorld;
            cornerGO.GetComponent<Renderer>().material.color = Color.red;

            var otherGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            otherGO.transform.position = otherWorld;
            otherGO.GetComponent<Renderer>().material.color = Color.red;
        }

        // Test the actual annotations
        if (false)
        {
            _referenceAtlas.LoadAnnotations();
            await _referenceAtlas.AnnotationsTask;

            int[,,] annotations = _referenceAtlas.AnnotationsTask.Result;

            APSlice = new Texture2D(annotations.GetLength(1), annotations.GetLength(2), TextureFormat.RGB24, false);
            APSlice.wrapMode = TextureWrapMode.Clamp;

            int i = 100;

            for (int j = 0; j < annotations.GetLength(1); j++)
                for (int k = 0; k < annotations.GetLength(2); k++)
                {
                    APSlice.SetPixel(j, k, _referenceAtlas.Ontology.ID2Color(annotations[i,j,k]));
                }
        }

        // Test the coordinate transforms
        if (false)
        {
            BrainAtlasManager.SetReferenceCoord(new Vector3(5.2f, 5.7f, 0.33f));

            Vector3 worldU = BrainAtlasManager.ActiveReferenceAtlas.AtlasSpace.ZeroOffset;
            GameObject.Find("WorldU").transform.position = worldU;

            Vector3 atlasU = BrainAtlasManager.ActiveReferenceAtlas.World2Atlas(worldU);
            Vector3 coordT = BrainAtlasManager.ActiveAtlasTransform.U2T(atlasU);
            Vector3 atlasT = BrainAtlasManager.ActiveAtlasTransform.T2U_Vector(coordT);
            Vector3 worldT = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(atlasT);

            Debug.Log((atlasU, coordT, atlasT, worldT));

            Vector3 worldT2 = BrainAtlasManager.WorldU2WorldT(worldU);
            GameObject.Find("WorldT").transform.position = worldT2;
        }
    }

    private void ApplyTransform()
    {
        _root.ApplyAtlasTransform(x => BrainAtlasManager.WorldU2WorldT(x, false));
    }
}
