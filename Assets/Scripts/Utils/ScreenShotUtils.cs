using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public static class ScreenShotUtils
{
    public static byte[] ScreenShot(Camera camera, Rect rect, bool isHighQuality = false)
    {
        GlobalFieldController.isScreenShoting = true;
        camera.RemoveLayer(LayerMask.NameToLayer("PVPArea"));
        camera.RemoveLayer(LayerMask.NameToLayer("ShotExclude"));
        camera.RemoveLayer(LayerMask.NameToLayer("SpecialModel"));
        camera.RemoveLayer(LayerMask.NameToLayer("Ignore Raycast"));
        byte[] shot = new byte[0];
        try
        {
            shot = TakeShot(camera, rect, isHighQuality);
        }
        catch
        {
            shot = null;
        }
        camera.AddLayer(LayerMask.NameToLayer("PVPArea"));
        camera.AddLayer(LayerMask.NameToLayer("ShotExclude"));
        camera.AddLayer(LayerMask.NameToLayer("SpecialModel"));
        camera.AddLayer(LayerMask.NameToLayer("Ignore Raycast"));
        GlobalFieldController.isScreenShoting = false;
        return shot;
    }

    public static byte[] ResScreenShot(Camera camera, Rect rect)
    {
        GlobalFieldController.isScreenShoting = true;
        camera.RemoveLayer(LayerMask.NameToLayer("ShotExclude"));
        camera.RemoveLayer(LayerMask.NameToLayer("SpecialModel"));
        camera.RemoveLayer(LayerMask.NameToLayer("Terrain"));
        camera.clearFlags = CameraClearFlags.SolidColor;
        Color oriBgColor = Camera.main.backgroundColor;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        var comBuffers = Camera.main.GetCommandBuffers(CameraEvent.BeforeImageEffects);
        Camera.main.RemoveCommandBuffers(CameraEvent.BeforeImageEffects);

        byte[] shot = new byte[0];
        try
        {
            shot = ResTakeShot(camera, rect);
        }
        catch
        {
            shot = null;
        }

        camera.AddLayer(LayerMask.NameToLayer("ShotExclude"));
        camera.AddLayer(LayerMask.NameToLayer("SpecialModel"));
        camera.AddLayer(LayerMask.NameToLayer("Terrain"));
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.backgroundColor = oriBgColor;
        foreach (var buffer in comBuffers)
        {
            Camera.main.AddCommandBuffer(CameraEvent.BeforeImageEffects, buffer);
        }
        GlobalFieldController.isScreenShoting = false;

        return shot;
    }

    public static byte[] ScreenShotAnim(Camera camera, Rect rect, UnityAction<Texture2D> callback)
    {
        camera.RemoveLayer(LayerMask.NameToLayer("PVPArea"));
        camera.RemoveLayer(LayerMask.NameToLayer("ShotExclude"));
        camera.RemoveLayer(LayerMask.NameToLayer("Ignore Raycast"));
        byte[] shot = new byte[0];
        try
        {
            shot = TakeShotAnim(camera, rect, callback);
        }
        catch
        {
            return null;
        }
        camera.AddLayer(LayerMask.NameToLayer("PVPArea"));
        camera.AddLayer(LayerMask.NameToLayer("ShotExclude"));
        camera.AddLayer(LayerMask.NameToLayer("Ignore Raycast"));
        return shot;
    }

    public static byte[] TakeShot(Camera camera, Rect rect, bool isHighQuality = false)
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);
        var screenBytes = isHighQuality ? screenShot.EncodeToPNG() : screenShot.EncodeToJPG();
        Object.Destroy(screenShot);
        return screenBytes;
    }

    public static byte[] ResTakeShot(Camera camera, Rect rect)
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);
        var screenBytes = screenShot.EncodeToPNG();
        Object.Destroy(screenShot);
        return screenBytes;
    }

    public static byte[] TakeShotAnim(Camera camera, Rect rect, UnityAction<Texture2D> callback)
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        //callback
        callback(screenShot);
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);
        var screenBytes = screenShot.EncodeToPNG();
        Object.Destroy(screenShot);
        return screenBytes;
    }

    public static void RemoveLayer(this Camera cam, int target)
    {
        cam.cullingMask = cam.cullingMask & ~(1 << target);
    }

    public static void AddLayer(this Camera cam, int target)
    {
        cam.cullingMask = cam.cullingMask | (1 << target);
    }
	
    public static byte[] TakeProfileShot(Camera camera, Rect rect)
    {
        RenderTexture.active = camera.targetTexture;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        RenderTexture.active = null;
        var screenBytes = screenShot.EncodeToPNG();
        Object.Destroy(screenShot);
        return screenBytes;
    }

    public static byte[] TakeUGCClothShot(Camera camera, Rect rect)
    {
        RenderTexture.active = camera.targetTexture;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        RenderTexture.active = null;
        var screenBytes = screenShot.EncodeToPNG();
        Object.Destroy(screenShot);
        return screenBytes;
    }
}
