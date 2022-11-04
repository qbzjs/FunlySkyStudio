using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
public class FaceSaveCanvas : MonoBehaviour
{
    public RawImage image;
    public Camera f_camera;
    [HideInInspector]
    public RenderTexture rt;
    public void SetRawTexture(List<RenderTexture> renderTextures , List<RenderTexture> alphaTextures, List<UGCData> uGCDatas)
    {
        rt = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
        rt.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        rt.Create();
        f_camera.targetTexture = rt;
        f_camera.Render();

        var mat = image.material;
        if (mat==null)
        {
            return;
        }
        for (int i = 0; i < renderTextures.Count; i++)
        {
            mat.SetTexture(uGCDatas[i].faceMatTextureName, renderTextures[i]);
        }
        for (int i = 0; i < alphaTextures.Count; i++)
        {
            mat.SetTexture(uGCDatas[i].faceMatTextureName + "_mask", alphaTextures[i]);
        }
        
    }
    public byte[] GetTexture()
    {

        Rect rect =new Rect(0,0,1024,1024);
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        var screenBytes = screenShot.EncodeToPNG();
        Object.Destroy(screenShot);
        return screenBytes;
    }
    
}
