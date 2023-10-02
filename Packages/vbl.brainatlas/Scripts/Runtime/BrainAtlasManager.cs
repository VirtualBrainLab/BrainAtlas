using BrainAtlas.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent LoadedEvent;
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

            LoadMetaData();

            BrainRegionMaterials = new();
            for (int i = 0; i < _brainRegionMaterials.Count; i++)
                BrainRegionMaterials.Add(_brainRegionMaterialNames[i], _brainRegionMaterials[i]);
        }
        #endregion

        #region Active atlas
        private ReferenceAtlas _referenceAtlas;
        private GameObject _parentGO;

        public ReferenceAtlas ActiveReferenceAtlas { get { return _referenceAtlas; } }
        public AtlasTransform ActiveAtlasTransform;

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
            _referenceAtlas = new ReferenceAtlas(referenceAtlasDataTask.Result, _parentGO.transform, defaultMaterial);

            return true;
        }
        #endregion

        #region Active atlas private helpers

        #endregion

        #region Helpers
        private async void LoadMetaData()
        {
            var metaDataTask = AddressablesRemoteLoader.LoadAtlasMetaData();
            await metaDataTask;
            _atlasMetaData = metaDataTask.Result;

            LoadedEvent.Invoke();
        }
        #endregion
    }
}
