using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Texture3DAlpha : MonoBehaviour
{
    private Texture3D materialTexture;
    private Color blackTransparent = new Color(0f, 0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        Material mat = GetComponent<Renderer>().material;
        int[] texIDs = mat.GetTexturePropertyNameIDs();
        foreach (int texID in texIDs)
            Debug.Log(texID);

        materialTexture = (Texture3D) mat.mainTexture;

        for (int xi = 0; xi < materialTexture.width; xi++)
        {
            for (int yi = 0; yi < materialTexture.height; yi++)
            {
                for (int zi = 0; zi < materialTexture.depth; zi++)
                {
                    if (materialTexture.GetPixel(xi, yi, zi) == Color.black)
                        materialTexture.SetPixel(xi, yi, zi, blackTransparent);
                }
            }
        }
        materialTexture.Apply();
    }
}
