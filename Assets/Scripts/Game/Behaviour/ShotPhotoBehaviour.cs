using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Author: 熊昭
/// Description: 3D相册道具功能行为类
/// Date: 2022-02-06 18:00:27
/// </summary>
public class ShotPhotoBehaviour : NodeBaseBehaviour
{
    private static MaterialPropertyBlock mpb;
    private Color[] oldColor;
    private BoxCollider collid;
    private Renderer render;

    public bool isLoading;
    public bool isHasPhoto;
    public Action<Texture2D> onLoadSuc;
    public Action onLoadFai;
    private Coroutine loadCor;
    private float ratio = 1.7778f;

    private float photoWidth = 1.6f;
    private float photoHeight = 0.9f;
    private MeshFilter meshFilter;
    private Mesh orginMesh, newMesh;
    private Vector3 orginSize, orginCenter;

    [HideInInspector]
    public string lastUrl;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        render = GetComponentInChildren<Renderer>();
        collid = GetComponentInChildren<BoxCollider>();
        meshFilter = GetComponentInChildren<MeshFilter>();
        orginMesh = meshFilter.mesh;
        orginSize = collid.size;
        orginCenter = collid.center;
    }

    public override void OnReset()
    {
        base.OnReset();
        ResetTexture();

        isLoading = false;
        lastUrl = string.Empty;

        if (loadCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(loadCor);
        }
        meshFilter.mesh = orginMesh;
    }

    private void SetTexture(Texture tex)
    {
        SetTextureVisiable(true);
        mpb.SetTexture("_MainTex", tex);
        render.SetPropertyBlock(mpb);
        isHasPhoto = true;
    }

    private void ResetTexture()
    {
        SetTextureVisiable(true);
        mpb.Clear();
        render.SetPropertyBlock(mpb);
        isHasPhoto = false;
    }

    public void SetTextureVisiable(bool state)
    {
        render.enabled = state;
        collid.enabled = state;
    }

    public Texture GetCurrentTexture()
    {
        //MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        render.GetPropertyBlock(mpb);
        return mpb.GetTexture("_MainTex");
    }

    private Texture2D CutTexture(Texture2D tex)
    {
        Vector2 nRect = tex.width / tex.height > ratio ? new Vector2(tex.height * ratio, tex.height) : new Vector2(tex.width, tex.width / ratio);
        //压缩比例设置为0.6
        return DataUtils.TextureCompress(tex, nRect, 0.6f);
    }

    public void LoadPhoto()
    {
        var shotCom = entity.Get<ShotPhotoComponent>();
        string url = shotCom.photoUrl;
        if (string.IsNullOrEmpty(url) || url == lastUrl)
        {
            return;
        }

        isLoading = true;
        ResetTexture();
        loadCor = CoroutineManager.Inst.StartCoroutine(LoadTexture(url, (tex) =>
        {
            isLoading = false;
            Texture2D nTex = tex;
            if (shotCom.type == SavePhotoType.CheckInPhoto)
            {
                ResetOrgin();
                nTex = CutTexture(tex);
            }
            else
            {
                AutoSize(tex);
            }
            SetTexture(nTex);
            lastUrl = url;
            ShotPhotoManager.Inst.Textures.Add(url, nTex);
            onLoadSuc?.Invoke(nTex);
        }, (nTex) =>
        {
            isLoading = false;
            if (shotCom.type == SavePhotoType.CheckInPhoto)
            {
                ResetOrgin();
            }
            else
            {
                AutoSize(nTex);
            }
            SetTexture(nTex);
            lastUrl = url;
            onLoadSuc?.Invoke(nTex);
        }, () =>
        {
            isLoading = false;
            lastUrl = string.Empty;
            LoggerUtils.LogError("Shot Photo --> Load Texture Failed");
            onLoadFai?.Invoke();
        }));
    }

    private IEnumerator LoadTexture(string url, Action<Texture2D> onSuccess, Action<Texture2D> onLoaded, Action onFail)
    {
        yield return new WaitUntil(ShotPhotoManager.Inst.IsCanEnterLoadQueue);
        if (ShotPhotoManager.Inst.Textures.ContainsKey(url))
        {
            onLoaded.Invoke(ShotPhotoManager.Inst.Textures[url]);
            ShotPhotoManager.Inst.LoadingCount--;
            yield break;
        }
        //下载新图片
        UnityWebRequest www = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 15;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadTextureError" + www.error);
            onFail();
        }
        else
        {
            texDl.texture.Compress(true);
            onSuccess.Invoke(texDl.texture);
        }
        ShotPhotoManager.Inst.LoadingCount--;
        texDl.Dispose();
        www.Dispose();
    }

    public void OnLoadingClone()
    {
        if (isLoading)
        {
            OnReset();
            LoadPhoto();
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref oldColor);
    }

    private void ResetOrgin()
    {
        meshFilter.mesh = orginMesh;
        collid.size = orginSize;
    }

    private void AutoSize(Texture2D tex)
    {
        meshFilter.mesh = GetNewMesh(tex);
        var size = orginSize;
        size.y = photoHeight;
        collid.size = size;
        var center = orginCenter;
        center.y = photoHeight / 2;
        collid.center = center;
    }

    private Mesh GetNewMesh(Texture2D tex)
    {
        float newRatio = (float)tex.height / tex.width;
        photoHeight = newRatio * photoWidth;
        Vector3[] newVertices = { new Vector3(-photoWidth/2, 0, 0), 
            new Vector3(-photoWidth/2, photoHeight, 0), 
            new Vector3(photoWidth/2, photoHeight, 0), 
            new Vector3(photoWidth/2, 0, 0) };
        Vector2[] newUV = { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0) };
        int[] newTriangles = {0,2,1,0,3,2};
        newMesh = newMesh == null ? new Mesh() : newMesh;
        newMesh.vertices = newVertices;
        newMesh.uv = newUV;
        newMesh.triangles = newTriangles;
        return newMesh;
    }
}
