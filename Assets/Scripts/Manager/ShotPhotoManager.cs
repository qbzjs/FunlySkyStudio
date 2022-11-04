using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Author: 熊昭
/// Description: 3D相册道具管理类：管理场景中所有3D相册共享数据、功能
/// Date: 2022-02-06 18:00:27
/// </summary>
public class ShotPhotoManager : ManagerInstance<ShotPhotoManager>, IManager
{
    private List<ShotPhotoBehaviour> photos = new List<ShotPhotoBehaviour>();
    //图片缓存池：存放已下载过的图片
    public Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
    public int LoadingQueue = 1; //下载队列长度
    public int LoadingCount; //当前下载数
    private const int MaxCount = 50;
    public const string MAX_COUNT_TIP = "Oops! Exceed limit:(";

    public override void Release()
    {
        base.Release();
        Clear();
    }

    public void Clear()
    {
        if (photos != null)
        {
            photos.Clear();
        }
        LoadingCount = 0;
        ClearTexturesPool();
    }

    private void ClearTexturesPool()
    {
        if (Textures != null)
        {
            foreach (var key in Textures.Keys)
            {
                Object.Destroy(Textures[key]);
            }
            Textures.Clear();
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        RemovePhoto(behaviour as ShotPhotoBehaviour);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.ShotPhoto)
        {
            AddPhoto(behaviour as ShotPhotoBehaviour); 
        }
    }

    public void InitPhotosVisiable()
    {
        if (GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            foreach (var photo in photos)
            {
                photo.SetTextureVisiable(photo.isHasPhoto);
            }
        }
        else
        {
            foreach (var photo in photos)
            {
                photo.SetTextureVisiable(true);
            }
        }
    }

    public bool IsCanEnterLoadQueue()
    {
        if (LoadingCount < LoadingQueue)
        {
            LoadingCount++;
            return true;
        }
        return false;
    }

    public void AddPhoto(ShotPhotoBehaviour pBehav)
    {
        if (!photos.Contains(pBehav))
        {
            photos.Add(pBehav);
        }
    }

    public void RemovePhoto(ShotPhotoBehaviour pBehav)
    {
        if (photos.Contains(pBehav))
        {
            photos.Remove(pBehav);
        }
    }

    public String[] getUrlArray()
    {
        HashSet<string> set = new HashSet<string>();
        foreach (var spb in photos)
        {
            var url = spb.lastUrl;
            if (!string.IsNullOrEmpty(url))
            {
                set.Add(url);
            }
        }

        string[] arr = new string[set.Count];
        set.CopyTo(arr);
        return arr;
    }
    
    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<ShotPhotoBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<ShotPhotoBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + photos.Count > MaxCount)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }
        return true;
    }
    
    public bool IsOverMaxCount()
    {
        if (photos.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }
}