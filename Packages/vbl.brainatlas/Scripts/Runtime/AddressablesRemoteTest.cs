using System.Threading.Tasks;
using UnityEngine;

public class AddressablesRemoteTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        AsyncTest();
    }

    public async void AsyncTest()
    {
        Task<Mesh> handle = BrainAtlasManager.LoadCCFMesh("8.obj");
        await handle;

        Debug.Log("Loaded 8.obj");

        Task<Texture3D> handleTex = BrainAtlasManager.LoadAnnotationTexture();
        await handleTex;

        Debug.Log("Loaded texture");

        Task<byte[]> volumeHandle = BrainAtlasManager.LoadVolumeIndexes();
        await volumeHandle;

        Debug.Log("Loaded volume indices");
    }
}
