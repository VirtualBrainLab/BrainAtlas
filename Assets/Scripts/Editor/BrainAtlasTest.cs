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

    private OntologyNode _root;

    public async void RunTest()
    {
        BrainAtlasManager.Instance.AtlasTransformChangedEvent.AddListener(ApplyTransform);

        var loadTask = BrainAtlasManager.LoadAtlas(BrainAtlasManager.AtlasNames[0]);
        await loadTask;

        Debug.Log("Mouse atlas loaded");

        // Load some nodes
        ReferenceAtlas _referenceAtlas = loadTask.Result;

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
        AtlasTransform custom = new CustomAffineTransform(Vector3.one * 1.1f, new Vector3(0f, 90f, 0f));
        BrainAtlasManager.ActiveAtlasTransform = custom;
    }

    private void ApplyTransform()
    {
        _root.ApplyAtlasTransform(BrainAtlasManager.WorldU2WorldT);
    }
}
