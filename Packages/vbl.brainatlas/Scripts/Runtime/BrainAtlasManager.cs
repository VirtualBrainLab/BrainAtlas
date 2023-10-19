using BrainAtlas.ScriptableObjects;
using BrainAtlas.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using BrainAtlas.Remote;

namespace BrainAtlas
{
    public class BrainAtlasManager : MonoBehaviour
    {
        #region Static
        public static BrainAtlasManager Instance;
        public static List<string> AtlasNames { get { return Instance._atlasMetaData.AtlasNames; } }
        public static Dictionary<string, Material> BrainRegionMaterials;
        public static List<AtlasTransform> AtlasTransforms;
        #endregion

        #region Exposed
        [SerializeField] List<Material> _brainRegionMaterials;
        [SerializeField] List<string> _brainRegionMaterialNames;
        #endregion

        #region Variables
        private AtlasMetaData _atlasMetaData;

        private TaskCompletionSource<bool> _metaLoadedSource;
        #endregion

        #region Events
        /// <summary>
        /// Fires when the metadata has been loaded (i.e. it's safe to load an atlas)
        /// </summary>
        public UnityEvent MetaLoadedEvent;

        public UnityEvent<Vector3> ReferenceCoordChangedEvent;

        /// <summary>
        /// Fires when the AtlasTransform is set externally
        /// </summary>
        public UnityEvent AtlasTransformChangedEvent;
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

            AtlasTransforms = new();
            AtlasTransforms.Add(new NullTransform());

            _metaLoadedSource = new();
        }

        private async void Start()
        {
            await _metaLoadedSource.Task;
            MetaLoadedEvent.Invoke();
        }
        #endregion

        #region Active atlas
        private ReferenceAtlas _referenceAtlas;
        private AtlasTransform _atlasTransform;
        private GameObject _parentGO;

        public static ReferenceAtlas ActiveReferenceAtlas { get { return Instance._referenceAtlas; } }
        public static AtlasTransform ActiveAtlasTransform { get { return Instance._atlasTransform; } set
            {
                Instance._atlasTransform = value;
                Instance.AtlasTransformChangedEvent.Invoke();
            }
        }


        public static async Task<ReferenceAtlas> LoadAtlas(string atlasName)
        {
#if UNITY_EDITOR
            Debug.Log($"(BAM) Loading {atlasName}");
#endif
            // Load the parent transform and place it in the scene, as a child of this GO
            var parentTransformTask = AddressablesRemoteLoader.LoadReferenceAtlasParentGO(atlasName);
            await parentTransformTask;

            Instance._parentGO = Instantiate(parentTransformTask.Result, Instance.transform);

            // Load the atlas metadata
            var referenceAtlasDataTask = AddressablesRemoteLoader.LoadReferenceAtlasData(atlasName);
            await referenceAtlasDataTask;

            //Build the active atlas
            Material defaultMaterial = BrainRegionMaterials["default"];
            Instance._referenceAtlas = new ReferenceAtlas(referenceAtlasDataTask.Result, Instance._parentGO.transform, defaultMaterial);

            // Set the null transform
            ActiveAtlasTransform = AtlasTransforms[0];

            return Instance._referenceAtlas;
        }
        #endregion

        #region Active atlas transform functions

        /// <summary>
        /// Convert a world coordinate into the corresponding world coordinate after transformation
        /// </summary>
        /// <param name="coordWorld"></param>
        /// <returns></returns>
        public static Vector3 WorldU2WorldT(Vector3 coordWorld, bool useReference = false)
        {
            return Instance._referenceAtlas.Atlas2World(Instance._atlasTransform.T2U_Vector(
                Instance._atlasTransform.U2T(Instance._referenceAtlas.World2Atlas(coordWorld, useReference))), useReference);
        }

        public static Vector3 WorldT2WorldU(Vector3 coordWorldT, bool useReference = false)
        {
            return Instance._referenceAtlas.Atlas2World(Instance._atlasTransform.T2U(
                Instance._atlasTransform.U2T_Vector(Instance._referenceAtlas.World2Atlas(coordWorldT, useReference))), useReference);
        }

        public static Vector3 WorldT2WorldU_Vector(Vector3 vectorWorldT)
        {
            return Instance._referenceAtlas.Atlas2World_Vector(Instance._atlasTransform.T2U(
                Instance._atlasTransform.U2T_Vector(Instance._referenceAtlas.World2Atlas_Vector(vectorWorldT))));
        }

        /// <summary>
        /// Helper function
        /// Convert a world coordinate into a transformed coordinate using the reference coordinate and the axis change
        /// </summary>
        /// <param name="vectorWorld"></param>
        /// <returns></returns>
        public static Vector3 World2T_Vector(Vector3 vectorWorld)
        {
            return Instance._atlasTransform.U2T_Vector(Instance._referenceAtlas.World2Atlas_Vector(vectorWorld));
        }

        public static Vector3 T2World_Vector(Vector3 vectorT)
        {
            return Instance._referenceAtlas.Atlas2World_Vector(Instance._atlasTransform.T2U_Vector(vectorT));
        }

        public static void SetReferenceCoord(Vector3 refCoordU)
        {
            if (ActiveReferenceAtlas != null)
            {
                ActiveReferenceAtlas.AtlasSpace.ReferenceCoord = refCoordU;
                Instance.ReferenceCoordChangedEvent.Invoke(refCoordU);
            }
        }
        #endregion

        #region Active atlas private helpers

        #endregion

        #region Helpers
        public async void LoadMetaData()
        {
            var metaDataTask = AddressablesRemoteLoader.LoadAtlasMetaData();
            await metaDataTask;
            _atlasMetaData = metaDataTask.Result;

            _metaLoadedSource.SetResult(true);
        }
        #endregion
    }
}
