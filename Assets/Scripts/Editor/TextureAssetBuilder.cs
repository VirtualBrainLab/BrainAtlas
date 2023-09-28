using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TextureAssetBuilder : MonoBehaviour
{
    [SerializeField] private bool rebuildTexture;

    // Start is called before the first frame update
    void Start()
    {
        AsyncStartBuildGPUTexture();
    }


    private async void AsyncStartBuildGPUTexture()
    {
        Task<Texture3D> textureTask = AddressablesRemoteLoader.LoadAnnotationTexture();
        await textureTask;

        Texture3D originalTexture = textureTask.Result;

        // Build the 3D annotation dataset texture
        Texture3D newTexture = new Texture3D(528, 320, 456, TextureFormat.RGBA32, false);
        newTexture.filterMode = FilterMode.Point;
        newTexture.wrapMode = TextureWrapMode.Clamp;

        Color transparentBlack = new Color(0f, 0f, 0f, 0f);

        Debug.Log("Converting annotation texture to alpha format");
        for (int ap = 0; ap < 528; ap++)
        {
            for (int dv = 0; dv < 320; dv++)
                for (int ml = 0; ml < 456; ml++)
                    if (originalTexture.GetPixel(ap, dv, ml) == Color.black)
                        newTexture.SetPixel(ap, dv, ml, transparentBlack);
                    else
                        newTexture.SetPixel(ap, dv, ml, originalTexture.GetPixel(ap, dv, ml));
        }
        newTexture.Apply();

        //if (Application.isEditor)
        //    AssetDatabase.CreateAsset(newTexture, "Assets/AddressableAssets/Textures/AnnotationDatasetTexture3DAlpha.asset");
    }
}
