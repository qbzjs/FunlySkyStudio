using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using SavingData;

public class UGCTexManager: CInstance<UGCTexManager>
{
    private Dictionary<string,Texture> ugcTexUrlsDic = new Dictionary<string, Texture>();
    private Dictionary<string,List<Action<Texture>>> loadingDic = new Dictionary<string, List<Action<Texture>>>();
    public void Init()
    {
        Clear();
    }
    public void ReplaceUGCTex(Material sharedMat)
    {
        var texId = GetUGCTexID(sharedMat.name);
        if(string.IsNullOrEmpty(texId))
        {
            return;
        }

        GetUGCTex(ParserUrl(sharedMat.name),(tex)=>{
            sharedMat.mainTexture = tex;
        });
    }
    public void ParserUmatList(Dictionary<string,UGCMatSaveData> UmatList)
    {
        if (UmatList != null)
        {
            GlobalFieldController.ugcMatData = UmatList;
        }
                
    }
    public void GetUGCTex(string uurl, Action<Texture> call)
    {
        if(string.IsNullOrEmpty(uurl))
        {
            call?.Invoke(null);
            LoggerUtils.LogError("uurl == null !!!");
            return;
        }
        var texId = GetUGCTexID(uurl);
        if(ugcTexUrlsDic.ContainsKey(texId))
        {
            call?.Invoke(ugcTexUrlsDic[texId]);
            loadingDic[texId].Clear();
            return;
        }
        if(loadingDic.ContainsKey(texId))
        {
            loadingDic[texId].Add(call);
            return;
        }
        loadingDic.Add(texId,new List<Action<Texture>>(){call});
        UGCTexLoader loader = null;
        loader = new UGCTexLoader(uurl,(baseAction, asset, err)=>{
            if (string.IsNullOrEmpty(err) && asset != null)
            {
                Texture2D tex = new Texture2D(512,512);

                var texture = (asset as byte[]);
                tex.LoadImage(texture);
                if (texture != null)
                {
                    ugcTexUrlsDic.Add(texId,tex);
                    foreach (var item in loadingDic[texId])
                    {
                        item?.Invoke(tex);
                    }
                }
                else
                {
                    LoggerUtils.LogError("mapRenderData == null 111");
                }
            }
        });
        loader.Do();
    } 
    public void GetUGCTexWithUMat(string umat, Action<Texture> call)
    {
        GetUGCTex(GetUGCTexUrl(umat), call);
    }
    public string GetUGCTexID(string matName)
    {
        var tex = ParserUrl(matName);
        if(tex == null)return Path.GetFileName(matName);    
        return Path.GetFileName(tex);
    }
    private string ParserUrl(string matName)
    {
        var matNames = matName.Split('_');
        if(matNames.Length == 5)
        {
            return GetUGCTexUrl(matNames[4]);
        }
        else
        {
            return null;
        }
    }
    public string GetUGCTexUrl(string matId)
    {
        GlobalFieldController.ugcMatData.TryGetValue(matId, out var value);
        if(value != null)return value.uurl;
        return null;
    }
    private void Clear()
    {
        ugcTexUrlsDic?.Clear();
    }
    public override void Release()
    {
        Clear();
        base.Release();
    }
}