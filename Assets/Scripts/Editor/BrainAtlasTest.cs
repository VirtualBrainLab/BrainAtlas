using BrainAtlas;
using UnityEngine;

public class BrainAtlasTest : MonoBehaviour
{
    [SerializeField] private RenderTexture _axialTexture;
    [SerializeField] private RenderTexture _sagittalTexture;
    [SerializeField] private RenderTexture _coronalTexture;

    [SerializeField] private BrainAtlasManager _BAM;

    // Start is called before the first frame update
    async void Start()
    {
    }

    public async void RunTest()
    {

        var loadTask = _BAM.LoadAtlas(_BAM.AtlasNames[1]);
        await loadTask;

        Debug.Log("Mouse atlas loaded");

        // Load some nodes
        ReferenceAtlas _referenceAtlas = _BAM.ActiveReferenceAtlas;

        int[] defaults = { 1034, 1096, 1084, 1038, 1081, 1097, 1048, 1057, 1061, 1055, 1059, 1069, 1056, 1065, 1072, 1020, 1047, 58, 1044, 74, 1046, 56, 1045, 75, 1043 };
        //int[] defaults = { 184, 500, 453, 1057, 677, 247, 669, 31, 972, 44, 714, 95, 254, 22, 541, 922, 698, 895, 1089, 703, 623, 343, 512 };

        foreach (int areaID in defaults)
            _referenceAtlas.Ontology.ID2Node(areaID).LoadMesh(OntologyNode.OntologyNodeSide.Full);
    }
}
