using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author:Shaocheng
/// Description: 红点管理. 目前只有某功能上新，icon上显示对应一次性的红点
/// Date: 2022-6-30 15:51:36
/// </summary>
public class RedDotKey
{
    public const string REDDOT_CAMERA_MODE = "REDDOT_CAMERA_MODE";
    public const string REDDOT_NEW_EMO = "REDDOT_NEW_EMO";
    public const string SPAWN_UPGRADE = "SPAWN_UPGRADE";
    public const string TRAP_BOX_UPGRADE = "TRAP_BOX_UPGRADE";
    public const string ICE_CUBE_UPGRADE = "ICE_CUBE_UPGRADE";
    public const string FIREWORK_UPGRADE = "FIREWORK";
}


public class RedDotManager : CInstance<RedDotManager>
{

    Dictionary<string, RedDotItem> RedDotItemDict = new Dictionary<string,RedDotItem>();
    public void Init()
    {
    }

    public void RegisterRedDot(string redDotKey, RedDotItem item)
    {
       if(!RedDotItemDict.ContainsKey(redDotKey))
       {
            RedDotItemDict.Add(redDotKey,item);
       }
    }

    public void UnRegisterRedDot(string redDotKey)
    {
        if (string.IsNullOrEmpty(redDotKey))
        {
            return;
        }
        if(RedDotItemDict.ContainsKey(redDotKey))
        {
            RedDotItemDict.Remove(redDotKey);
        }
    }

    #region 一次性红点的保存和刷新

    public void RefreshOnceRedDot(string key, GameObject redDotGameObject)
    {
        if (string.IsNullOrEmpty(key) || redDotGameObject == null)
        {
            return;
        }

        var v = PlayerPrefs.GetInt(key, 0);
        redDotGameObject.SetActive(v == 0);
    }

    public void SaveOnceRedDot(string savedKey, GameObject redDotGameObject)
    {
        if (string.IsNullOrEmpty(savedKey))
        {
            return;
        }
        PlayerPrefs.SetInt(savedKey, 1);
        PlayerPrefs.Save();

        RefreshOnceRedDot(savedKey, redDotGameObject);
    }

    public void SaveOnceRedDot(string redDotKey)
    {
        if(!RedDotItemDict.ContainsKey(redDotKey))
        {
            return;
        }

        RedDotItem dotItem = RedDotItemDict[redDotKey];
        SaveOnceRedDot(redDotKey,dotItem.gameObject);
    }

    #endregion


    public GameObject CreateRedItemNode(string redDotKey,GameObject parent,Vector2 vec2 = default)
    {
        if(parent == null || string.IsNullOrEmpty(redDotKey))
        {
            return null;
        }

        Transform redTransfrom = parent.transform.Find("RedDotItem");
        if( redTransfrom != null)
        {
            RefreshOnceRedDot(redDotKey,redTransfrom.gameObject);
            return redTransfrom.gameObject;
        }
        var redDotPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/RedDot/RedDotItem");
        var redDotNode = GameObject.Instantiate(redDotPrefab, parent.transform);
        redDotNode.GetComponent<RedDotItem>().Init(redDotKey,vec2);

        RefreshOnceRedDot(redDotKey,redDotNode);

        return redDotNode;
    }

    public override void Release()
    {
        base.Release();
        RedDotItemDict.Clear();
    }
}