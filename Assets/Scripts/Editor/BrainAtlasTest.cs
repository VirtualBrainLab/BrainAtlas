using BrainAtlas;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BrainAtlasTest : MonoBehaviour
{
    [SerializeField] private RenderTexture _axialTexture;
    [SerializeField] private RenderTexture _sagittalTexture;
    [SerializeField] private RenderTexture _coronalTexture;

    [SerializeField] private BrainAtlasManager _BAM;

    [SerializeField] private Material _sideMaterialTest;
    [SerializeField] private Color _sideColor;

    public Texture3D AnnotationTexture;
    public Texture3D ReferenceTexture;

    private OntologyNode _root;

    public async void RunTest()
    {
        BrainAtlasManager.Instance.AtlasTransformChangedEvent.AddListener(ApplyTransform);

        var loadTask = BrainAtlasManager.LoadAtlas(BrainAtlasManager.AtlasNames[0]);
        await loadTask;

        Debug.Log("Mouse atlas loaded");

        // Load some nodes
        ReferenceAtlas _referenceAtlas = loadTask.Result;

        // Test node loading, transforms, materials, colors
        if (true)
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
            var annotationTask = _referenceAtlas.LoadAnnotationTexture();
            await annotationTask;

            AnnotationTexture = annotationTask.Result;
        }
    }

    private void ApplyTransform()
    {
        _root.ApplyAtlasTransform(BrainAtlasManager.WorldU2WorldT);
    }
}
