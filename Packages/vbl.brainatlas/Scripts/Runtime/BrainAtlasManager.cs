using BrainAtlas.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BrainAtlas
{
    public class BrainAtlasManager : MonoBehaviour
    {
        #region Static
        public static BrainAtlasManager Instance;
        #endregion

        #region Exposed
        [SerializeField] List<Material> _brainRegionMaterials;
        [SerializeField] List<string> _brainRegionMaterialNames;
        #endregion

        #region Variables
        private AtlasMetaData _atlasMetaData;

        public List<string> AtlasNames { get { return _atlasMetaData.AtlasNames; } }
        public Dictionary<string, Material> BrainRegionMaterials;
        #endregion

        #region Unity

        private void Awake()
        {
            if (Instance != null)
                throw new Exception("Only one instance of BrainAtlasManager can be in the scene");
            Instance = this;

            BrainRegionMaterials = new();
            for (int i = 0; i < _brainRegionMaterials.Count; i++)
                BrainRegionMaterials.Add(_brainRegionMaterialNames[i], _brainRegionMaterials[i]);
        }

        private async void Start()
        {
            var metaDataTask = AddressablesRemoteLoader.LoadAtlasMetaData();
            await metaDataTask;
            _atlasMetaData = metaDataTask.Result;

            await LoadAtlas(_atlasMetaData.AtlasNames[2]);

            _referenceAtlas.Ontology.ID2Node(10000).LoadMesh(OntologyNode.OntologyNodeSide.Full);

            //var areaTask = LoadArea(10000);
            //await areaTask;
            //Instantiate(areaTask.Result, _referenceAtlas.ParentT);
        }
        #endregion

        #region Active atlas
        private ReferenceAtlas _referenceAtlas;
        private GameObject _parentGO;

        public async Task<bool> LoadAtlas(string atlasName)
        {
#if UNITY_EDITOR
            Debug.Log($"(BAM) Loading {atlasName}");
#endif
            // Load the parent transform and place it in the scene, as a child of this GO
            var parentTransformTask = AddressablesRemoteLoader.LoadReferenceAtlasParentGO(atlasName);
            await parentTransformTask;

            _parentGO = Instantiate(parentTransformTask.Result, transform);

            // Load the atlas metadata
            var referenceAtlasDataTask = AddressablesRemoteLoader.LoadReferenceAtlasData(atlasName);
            await referenceAtlasDataTask;

            //Build the active atlas
            Material defaultMaterial = BrainRegionMaterials["default"];
            _referenceAtlas = new ReferenceAtlas(referenceAtlasDataTask.Result, _parentGO.transform, ref defaultMaterial);

            return true;
        }

        public async Task<GameObject> LoadArea(string acronym, bool sided = false)
        {
            var handle = LoadArea(Instance._referenceAtlas.Ontology.Acronym2ID(acronym), sided);
            await handle;
            return handle.Result;
        }

        public async Task<GameObject> LoadArea(int areaID, bool sided = false)
        {
#if UNITY_EDITOR
            Debug.Log($"(BAM) Loading {Instance._referenceAtlas.Name}:{areaID}");
#endif
            if (Instance._referenceAtlas == null) { Debug.LogError("Atlas is not loaded"); return null; }

            var areaTask = AddressablesRemoteLoader.LoadMeshPrefab(sided ? $"{areaID}L" : areaID.ToString());
            await areaTask;
            return areaTask.Result;
        }
        #endregion

        #region Active atlas private helpers

        #endregion
    }
}
