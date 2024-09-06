using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using BrainAtlas.ScriptableObjects;

namespace BrainAtlas.Remote
{
    /// <summary>
    /// The RemoteLoader class handles accessing Prefabs, Textures, ScriptableObjects and other BrainAtlas resources stored on the remote server
    /// </summary>
    public class AddressablesRemoteLoader : MonoBehaviour
    {
        #region Static
        public static AddressablesRemoteLoader Instance { get; private set; }
        public static UnityEvent<AddressablesRemoteLoader> LoadCompleted;
        public static Task CatalogLoadedTask;

        public static string ActiveAtlas { get; private set; }
        #endregion

        #region Events
        public UnityEvent CatalogLoadedEvent;
        #endregion

        #region Exposed fields
        [SerializeField] private string _addressablesStorageRemotePath = "https://data.virtualbrainlab.org/BrainAtlas";
        [SerializeField] private string _buildVersion = "1.0.0";

        public bool localAddressables = false;
        #endregion

        #region Private vars
        private string _fileEnding = ".bin";
        private string _addressablesStorageTargetPath;

        private static TaskCompletionSource<bool> catalogLoadedSource;
        #endregion

        #region Unity
        void Awake()
        {
            if (Instance != null)
                Debug.LogError("(ARL) Only one instance of AddressablesRemoteLoader should exist");

            //Register to override WebRequests Addressables creates to download
            Addressables.WebRequestOverride = EditWebRequestURL;

            catalogLoadedSource = new TaskCompletionSource<bool>();
            CatalogLoadedTask = catalogLoadedSource.Task;

            LoadCatalog();
        }
        #endregion

        #region Public static loaders
        /// <summary>
        /// Load the parent prefab GameObject
        /// </summary>
        /// <param name="atlasName"></param>
        /// <param name="parentT">(optional) Transform to parent to the new atlas parent</param>
        /// <returns></returns>
        public static async Task<GameObject> LoadReferenceAtlasParentGO(string atlasName)
        {
#if UNITY_EDITOR
            Debug.Log($"(ARL) Loading reference atlas data for {atlasName}");
#endif
            await CatalogLoadedTask;

            // Catalog is loaded, load specified mesh file
            string path = $"Assets/AddressableAssets/{atlasName}/{atlasName}Parent.prefab";

            AsyncOperationHandle<GameObject> loadHandle = Addressables.LoadAssetAsync<GameObject>(path);
            await loadHandle.Task;

            return loadHandle.Result;
        }

        public static async Task<ReferenceAtlasData> LoadReferenceAtlasData(string atlasName)
        {
#if UNITY_EDITOR
            Debug.Log($"(ARL) Loading reference atlas data for {atlasName}");
#endif
            await CatalogLoadedTask;

            // Catalog is loaded, load specified mesh file
            string path = $"Assets/AddressableAssets/{atlasName}/{atlasName}.asset";

            AsyncOperationHandle<ReferenceAtlasData> loadHandle = Addressables.LoadAssetAsync<ReferenceAtlasData>(path);
            await loadHandle.Task;

            ActiveAtlas = atlasName;

            return loadHandle.Result;
        }

        public static async Task<AtlasMetaData> LoadAtlasMetaData()
        {
#if UNITY_EDITOR
            Debug.Log("(ARL) Loading atlas metadata");
#endif
            await CatalogLoadedTask;

            // Catalog is loaded, load specified mesh file
            string path = "Assets/AddressableAssets/metadataSO.asset";

            AsyncOperationHandle<AtlasMetaData> loadHandle = Addressables.LoadAssetAsync<AtlasMetaData>(path);
            await loadHandle.Task;

            return loadHandle.Result;
        }

        public static async Task<GameObject> LoadMeshPrefab(string areaID)
        {
#if UNITY_EDITOR
            Debug.Log($"(ARL) Loading area {areaID}");
#endif
            await CatalogLoadedTask;

            // Catalog is loaded, load specified mesh file
            string path = $"Assets/AddressableAssets/{ActiveAtlas}/meshes/{areaID}.prefab";

            AsyncOperationHandle<GameObject> loadHandle = Addressables.LoadAssetAsync<GameObject>(path);
            await loadHandle.Task;

            return loadHandle.Result;
        }

        /// <summary>
        /// Load the annotation or reference Texture3D
        /// </summary>
        /// <param name="annotationTexture"></param>
        /// <returns></returns>
        public static async Task<Texture3D> LoadTexture(bool annotationTexture = true)
        {
#if UNITY_EDITOR
            Debug.Log($"(ARL) Loading {(annotationTexture ? "annotationTexture" : "referenceTexture")}");
#endif

            // Wait for the catalog to load if this hasn't already happened
            await CatalogLoadedTask;

            // Catalog is loaded, load the Texture3D object
            string path = $"Assets/AddressableAssets/{ActiveAtlas}/{(annotationTexture ? "annotationTexture" : "referenceTexture")}.asset";

            AsyncOperationHandle<Texture3D> loadHandle = Addressables.LoadAssetAsync<Texture3D>(path);
            await loadHandle.Task;

            return loadHandle.Result;
        }

        public static async Task<int[,,]> LoadAnnotationIDs((int x, int y, int z) dimensions)
        {
#if UNITY_EDITOR
            Debug.Log("Loading annotation ID values");
#endif

            // Wait for the catalog to load if this hasn't already happened
            await CatalogLoadedTask;

            // Catalog is loaded, load the Texture3D object
            string path = $"Assets/AddressableAssets/{ActiveAtlas}/annotations.asset";

            AsyncOperationHandle<AnnotationData> loadHandle = Addressables.LoadAssetAsync<AnnotationData>(path);
            await loadHandle.Task;

            int[,,] annotationIDs = new int[dimensions.x, dimensions.y, dimensions.z];

            int z = 0;
            for (int i = 0; i < dimensions.x; i++)
                for (int j = 0; j < dimensions.y; j++)
                    for (int k = 0; k < dimensions.z; k++)
                        annotationIDs[i, j, k] = loadHandle.Result.Annotations[z++];

            Addressables.Release(loadHandle);

            return annotationIDs;
        }
        #endregion

        #region Private helpers
        private async void LoadCatalog()
        {
//#if UNITY_EDITOR
//            _addressablesStorageTargetPath = Path.Join(Path.Join("C:\\proj\\VBL\\BrainAtlas", "ServerData", "StandaloneWindows64"), $"catalog_{_buildVersion}{_fileEnding}");
//#else
            string buildTarget;

            RuntimePlatform platform = Application.platform;
            if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
                buildTarget = "StandaloneWindows64";
            else if (platform == RuntimePlatform.WebGLPlayer)
                buildTarget = "WebGL";
            else if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
                buildTarget = "StandaloneOSX";
            else if (platform == RuntimePlatform.LinuxEditor || platform == RuntimePlatform.LinuxPlayer)
                buildTarget = "StandaloneLinux64";
            else
            {
                Debug.LogError(string.Format("Running on {0} we do NOT have a built Addressables Storage bundle", platform));
                return;
            }

            _addressablesStorageTargetPath = $"{_addressablesStorageRemotePath}/{_buildVersion}/{buildTarget}/catalog_{_buildVersion}{_fileEnding}";

//#endif

#if UNITY_EDITOR
            Debug.Log("(AddressablesStorage) Loading catalog v" + _buildVersion);
#endif
            //Load a catalog and automatically release the operation handle.
            Debug.Log("(AddressablesStorage) Loading content catalog from: " + _addressablesStorageTargetPath);

            AsyncOperationHandle<IResourceLocator> catalogLoadHandle
                = Addressables.LoadContentCatalogAsync(_addressablesStorageTargetPath, true);

            await catalogLoadHandle.Task;

            catalogLoadedSource.SetResult(true);
            CatalogLoadedEvent.Invoke();
        }

        /// <summary>
        /// This function guarantees that Addressable WebRequests use SSL, this was an issue in 2021.3.10f1, not sure if it's still necessary
        /// </summary>
        /// <param name="request"></param>
        private void EditWebRequestURL(UnityWebRequest request)
        {
            if (request.url.Contains("http://"))
            {
#if UNITY_EDITOR
                Debug.LogWarning("(ARL) Web request SSL override is still required");
#endif
                request.url = request.url.Replace("http://", "https://");
            }
        }
        #endregion
    }
}