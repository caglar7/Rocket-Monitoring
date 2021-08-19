using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    private Renderer planeRenderer;

    void Start()
    {
        planeRenderer = GetComponent<Renderer>();
    }

    // wait half a second then set texture for mesh
    public void SetTileTexture(byte[] data)
    {
        StartCoroutine(WaitAndSetTexture(data));
    }

    IEnumerator WaitAndSetTexture(byte[] data)
    {
        yield return new WaitForSeconds(0.5f);

        // texture settings, test 256 etc.
        Texture2D texture = new Texture2D(0, 0, TextureFormat.RGB24, true);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.LoadImage(data);
        planeRenderer.material.mainTexture = texture;
    }
    
}
