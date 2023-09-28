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

/// <summary>
/// The RemoteLoader class handles accessing Prefabs, Textures, ScriptableObjects and other BrainAtlas resources stored on the remote server
/// </summary>
public class AddressablesRemoteLoader : MonoBehaviour
{
    #region Static
    public static AddressablesRemoteLoader Instance { get; private set; }
    public static UnityEvent<AddressablesRemoteLoader> LoadCompleted;
    public static Task CatalogLoadedTask;
    #endregion

    #region Exposed fields
    [SerializeField] private string _addressablesStorageRemotePath = "https://data.virtualbrainlab.org/AddressablesStorage";
    [SerializeField] private string _buildVersion = "1.0.0";
    #endregion

    #region Private vars
    private string _fileEnding = ".json";
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

#if UNITY_EDITOR
        Test();
#endif
    }
    #endregion

    #region Public static loaders
    /// <summary>
    /// Load the parent prefab GameObject
    /// </summary>
    /// <param name="atlasName"></param>
    /// <param name="parentT">(optional) Transform to parent to the new atlas parent</param>
    /// <returns></returns>
    public async Task<GameObject> LoadParentPrefab(string atlasName, Transform parentT = null)
    {
        return null;
    }

    public static async Task<AtlasMetaData> LoadAtlasMetaData()
    {
#if UNITY_EDITOR
        Debug.Log("(ARL) Loading atlas metadata");
#endif

        // Wait for the catalog to load if this hasn't already happened
        await CatalogLoadedTask;

        // Catalog is loaded, load specified mesh file
        string path = "Assets/AddressableAssets/metadataSO.asset";

        AsyncOperationHandle<AtlasMetaData> loadHandle = Addressables.LoadAssetAsync<AtlasMetaData>(path);
        await loadHandle.Task;

        return loadHandle.Result;
    }
    #endregion

    #region Private helpers
    private async void LoadCatalog()
    {
#if UNITY_EDITOR
        _addressablesStorageTargetPath = Path.Join(Path.Join("C:\\proj\\VBL\\BrainAtlas", "ServerData", "StandaloneWindows64"), $"catalog_{_buildVersion}{_fileEnding}");
#else
        RuntimePlatform platform = Application.platform;
        if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
            _addressablesStorageTargetPath = _addressablesStorageRemotePath + "/StandaloneWindows64/catalog_" + _buildVersion + fileEnding;
        else if (platform == RuntimePlatform.WebGLPlayer)
            _addressablesStorageTargetPath = _addressablesStorageRemotePath + "/WebGL/catalog_" + _buildVersion + fileEnding;
        else if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
            _addressablesStorageTargetPath = _addressablesStorageRemotePath + "/StandaloneOSX/catalog_" + _buildVersion + fileEnding;
        else if (platform == RuntimePlatform.LinuxEditor || platform == RuntimePlatform.LinuxPlayer)
            _addressablesStorageTargetPath = _addressablesStorageRemotePath + "/StandaloneLinux64/catalog_" + _buildVersion + fileEnding;
        else
        {
            Debug.LogError(string.Format("Running on {0} we do NOT have a built Addressables Storage bundle", platform));
            return;
        }
#endif

#if UNITY_EDITOR
        Debug.Log("(AddressablesStorage) Loading catalog v" + _buildVersion);
#endif
        //Load a catalog and automatically release the operation handle.
        Debug.Log("(AddressablesStorage) Loading content catalog from: " + _addressablesStorageTargetPath);

        AsyncOperationHandle<IResourceLocator> catalogLoadHandle
            = Addressables.LoadContentCatalogAsync(_addressablesStorageTargetPath, true);

        await catalogLoadHandle.Task;

        catalogLoadedSource.SetResult(true);
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

#if UNITY_EDITOR
    private void Test()
    {

    }

#endif
    #endregion












    /// <summary>
    /// OLD CODE
    /// </summary>
    /// 


    // Server setup task
    //private TaskCompletionSource<bool> catalogLoadedSource;

    //// Catalog load task
    //private static Task catalogLoadedTask;

    //// Delaying the load allows you to set the catalog address
    //[SerializeField] private bool delayCatalogLoad = false;

    //// Start is called before the first frame update
    //void Awake()
    //{
    //    //Register to override WebRequests Addressables creates to download
    //    Addressables.WebRequestOverride = EditWebRequestURL;

    //    catalogLoadedSource = new TaskCompletionSource<bool>();
    //    catalogLoadedTask = catalogLoadedSource.Task;

    //    if (!delayCatalogLoad)
    //    {
    //        LoadCatalog();
    //    }
    //}


    //public void ChangeCatalogServer(string newAddressablesStorageRemotePath)
    //{
    //    _addressablesStorageRemotePath = newAddressablesStorageRemotePath;
    //}

    //    public async void LoadCatalog()
    //    {
    //        RuntimePlatform platform = Application.platform;
    //        if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
    //            addressablesStorageTargetPath = _addressablesStorageRemotePath + "/StandaloneWindows64/catalog_" + _buildVersion + fileEnding;
    //        else if (platform == RuntimePlatform.WebGLPlayer)
    //            addressablesStorageTargetPath = _addressablesStorageRemotePath + "/WebGL/catalog_" + _buildVersion + fileEnding;
    //        else if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
    //            addressablesStorageTargetPath = _addressablesStorageRemotePath + "/StandaloneOSX/catalog_" + _buildVersion + fileEnding;
    //        else
    //        {
    //            Debug.LogError(string.Format("Running on {0} we do NOT have a built Addressables Storage bundle", platform));
    //        }

    //#if UNITY_EDITOR
    //        Debug.Log("(AddressablesStorage) Loading catalog v" + _buildVersion);
    //#endif
    //        //Load a catalog and automatically release the operation handle.
    //        Debug.Log("(AddressablesStorage) Loading content catalog from: " + GetAddressablesPath());

    //        AsyncOperationHandle<IResourceLocator> catalogLoadHandle
    //            = Addressables.LoadContentCatalogAsync(GetAddressablesPath(), true);

    //        await catalogLoadHandle.Task;

    //        catalogLoadedSource.SetResult(true);
    //    }

    //    public Task GetCatalogLoadedTask()
    //    {
    //        return catalogLoadedTask;
    //    }


    public static async Task<Mesh> LoadCCFMesh(string objPath)
    {
#if UNITY_EDITOR
        Debug.Log("Loading mesh file: " + objPath);
#endif

        // Wait for the catalog to load if this hasn't already happened
        await CatalogLoadedTask;


        // Catalog is loaded, load specified mesh file
        string path = "Assets/AddressableAssets/AllenCCF/" + objPath;
        // Not sure why this extra path check is here, I think maybe some objects don't exist and so this hangs indefinitely for those?
        AsyncOperationHandle<IList<IResourceLocation>> pathHandle = Addressables.LoadResourceLocationsAsync(path);
        await pathHandle.Task;

        AsyncOperationHandle<Mesh> loadHandle = Addressables.LoadAssetAsync<Mesh>(path);
        await loadHandle.Task;

        // Copy the mesh so that we can modify it without modifying the original
        Mesh returnMesh = new Mesh();
        returnMesh.vertices = loadHandle.Result.vertices;
        returnMesh.triangles = loadHandle.Result.triangles;
        returnMesh.uv = loadHandle.Result.uv;
        returnMesh.normals = loadHandle.Result.normals;
        returnMesh.colors = loadHandle.Result.colors;
        returnMesh.tangents = loadHandle.Result.tangents;

        Addressables.Release(pathHandle);
        Addressables.Release(loadHandle);

        return returnMesh;
    }

    public static async Task<string> LoadAllenCCFOntology()
    {
#if UNITY_EDITOR
        Debug.Log("Loading Allen CCF");
#endif

        await CatalogLoadedTask;

        string path = "Assets/AddressableAssets/AllenCCF/ontology_structure_minimal.csv";

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<TextAsset>(path);
        await loadHandle.Task;

        string returnText = ((TextAsset)loadHandle.Result).text;
        Addressables.Release(loadHandle);

        return returnText;
    }

    public static async Task<Texture3D> LoadAnnotationTexture()
    {
#if UNITY_EDITOR
        Debug.Log("Loading Allen CCF annotation texture");
#endif

        // Wait for the catalog to load if this hasn't already happened
        await CatalogLoadedTask;

        // Catalog is loaded, load the Texture3D object
        string path = "Assets/AddressableAssets/Textures/AnnotationDatasetTexture3DAlpha.asset";

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<Texture3D>(path);
        await loadHandle.Task;

        Texture3D returnTexture = (Texture3D)loadHandle.Result;
        //Addressables.Release(loadHandle);

        return returnTexture;
    }

    public static async Task<byte[]> LoadVolumeIndexes()
    {
#if UNITY_EDITOR
        Debug.Log("Loading volume indexes");
#endif

        // Wait for the catalog to load if this hasn't already happened
        await CatalogLoadedTask;

        string volumePath = "Assets/AddressableAssets/Datasets/volume_indexes.bytes";

        AsyncOperationHandle loadHandle = Addressables.LoadAssetAsync<TextAsset>(volumePath);
        await loadHandle.Task;

        byte[] resultText = ((TextAsset)loadHandle.Result).bytes;
        Addressables.Release(loadHandle);

        return resultText;
    }

    /// <summary>
    /// Loads the annotation data to be reconstructed by the VolumeDatasetManager
    /// </summary>
    /// <returns>List of TextAssets where [0] is the index and [1] is the map</returns>
    public static async Task<(byte[] index, byte[] map)> LoadAnnotationIndexMap()
    {
#if UNITY_EDITOR
        Debug.Log("Loading annotation index mapping");
#endif

        // Wait for the catalog to load if this hasn't already happened
        await CatalogLoadedTask;

        string annIndexPath = "Assets/AddressableAssets/Datasets/ann/annotation_indexes.bytes";
        AsyncOperationHandle indexHandle = Addressables.LoadAssetAsync<TextAsset>(annIndexPath);
        await indexHandle.Task;

        string annMapPath = "Assets/AddressableAssets/Datasets/ann/annotation_map.bytes";
        AsyncOperationHandle mapHandle = Addressables.LoadAssetAsync<TextAsset>(annMapPath);
        await mapHandle.Task;

        (byte[] index, byte[] map) data = (((TextAsset)indexHandle.Result).bytes, ((TextAsset)mapHandle.Result).bytes);
        Addressables.Release(indexHandle);
        Addressables.Release(mapHandle);

        return data;
    }
}