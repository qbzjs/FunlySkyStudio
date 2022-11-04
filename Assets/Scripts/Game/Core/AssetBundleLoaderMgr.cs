/// <summary>
/// Author:WeiXin
/// Description:AssetBundle加载脚本
/// Date: 2022/7/20 17:19:21
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.U2D;

public class AssetBundleLoaderMgr : CInstance<AssetBundleLoaderMgr>
{
    private const string dotAB = ".ab";

    private Dictionary<string, AssetBundle> SkyboxDic = new Dictionary<string, AssetBundle>();
    private Dictionary<string, Texture2D> SkyboxTexture = new Dictionary<string, Texture2D>();
    private string SkyboxPath;

    private Dictionary<string, AssetBundle> ABDic = new Dictionary<string, AssetBundle>();

    private Dictionary<string, Dictionary<string, Texture>> TextureDic =
        new Dictionary<string, Dictionary<string, Texture>>()
        {
            {"Character", new Dictionary<string, Texture>()},
            {"BaseTexture", new Dictionary<string, Texture>()},
            {"UGCClothes", new Dictionary<string, Texture>()},
        };

    private Dictionary<string, string> abPath;

    private Dictionary<string, SpriteAtlas> atlasDic = new Dictionary<string, SpriteAtlas>();
    private Dictionary<string, Material> matDic = new Dictionary<string, Material>();
    private Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
    private Dictionary<string, AnimationClip> clipDic = new Dictionary<string, AnimationClip>();

    private string animationABPath = Path.Combine(Application.streamingAssetsPath, "assetbundle/animation/");
    
    private static object obj = new object();

    public void Init()
    {
        SkyboxPath = Path.Combine(Application.streamingAssetsPath, "assetbundle", "skybox");
        abPath = new Dictionary<string, string>()
        {
            {"Character", Path.Combine(Application.streamingAssetsPath, "assetbundle", "character") + dotAB},
            {"BaseTexture", Path.Combine(Application.streamingAssetsPath, "assetbundle", "ground") + dotAB},
            {"UGCClothes", Path.Combine(Application.streamingAssetsPath, "assetbundle", "ugcclothes") + dotAB},
        };
        getAB("BaseTexture");
    }

    public AssetBundle getAB(string name)
    {
        AssetBundle ab = null;
        try
        {
            if (ABDic.ContainsKey(name))
            {
                ab = ABDic[name];
            }
            else
            {
                ab = AssetBundle.LoadFromFile(abPath[name]);
                ABDic[name] = ab;
            }
        }
        catch
        {
            Debug.LogError("load AB error:"+name);
        }

        return ab;
    }

    private Texture getTextureFromAB(string abName, string name)
    {
        AssetBundle ab = getAB(abName);
        Texture t = null;

        var ts = TextureDic[abName];
        if (ts.ContainsKey(name))
        {
            t = ts[name];
        }
        else
        {
            t = ab.LoadAsset<Texture>(name);
            ts.Add(name, t);
        }

        return t;
    }

    public SpriteAtlas LoadSpriteAtlas(string assetName)
    {
        var name = Path.GetFileName(assetName);
        var dir = Path.GetDirectoryName(assetName);
        AssetBundle ab = getAB("Character");
        SpriteAtlas sa = null;

        if (dir.Contains("Atlas"))
        {
            if (atlasDic.ContainsKey(name))
            {
                sa = atlasDic[name];
            }
            else
            {
                sa = ab.LoadAsset<SpriteAtlas>(name);
                atlasDic.Add(name, sa);
            }
        }

        return sa;
    }

    public Texture LoadTexture(string assetName)
    {
        var name = Path.GetFileName(assetName);
        var dir = Path.GetDirectoryName(assetName);
        Texture t = null;

        if (dir.Contains("BaseTexture"))
        {
            t = getTextureFromAB("BaseTexture", name);
        }
        else if (dir.Contains("UGCClothes"))
        {
            t = getTextureFromAB("UGCClothes", name);
        }
        else if (dir.Contains("Character"))
        {
            t = getTextureFromAB("Character", name);
        }

        return t;
    }

    public Texture2D LoadSkybox(string assetName)
    {
        lock (obj)
        {
            var name = Path.GetFileName(assetName);
            var dir = Path.GetDirectoryName(assetName);
            Texture2D t = null;
            if (dir.Contains("Skybox"))
            {
                AssetBundle ab = null;
                if (SkyboxDic.ContainsKey(dir))
                {
                    ab = SkyboxDic[dir];
                }
                else
                {
                    var p = Path.Combine(SkyboxPath, Path.GetFileName(dir)) + dotAB;
                    ab = AssetBundle.LoadFromFile(p);
                    SkyboxDic.Add(dir, ab);
                }

                if (SkyboxTexture.ContainsKey(assetName))
                {
                    t = SkyboxTexture[assetName];
                }
                else
                {
                    t = ab.LoadAsset<Texture2D>(name);
                    SkyboxTexture.Add(assetName, t);
                }
            }

            return t;
        }
    }

    public Material LoadMaterial(string assetName)
    {
        var name = Path.GetFileName(assetName);
        var dir = Path.GetDirectoryName(assetName);
        Material m = null;
        if (dir.Contains("Ground"))
        {
            AssetBundle ab = getAB("BaseTexture");
            if (matDic.ContainsKey(name))
            {
                m = matDic[name];
            }
            else
            {
                m = ab.LoadAsset<Material>(name);
                matDic.Add(name, m);
            }
        }

        return m;
    }
    
    public Sprite LoadSprite(string assetName)
    {
        var name = Path.GetFileName(assetName);
        var dir = Path.GetDirectoryName(assetName);
        Sprite s = null;
        if (dir.Contains("UGCClothes"))
        {
            AssetBundle ab = getAB("UGCClothes");
            if (spriteDic.ContainsKey(name))
            {
                s = spriteDic[name];
            }
            else
            {
                s = ab.LoadAsset<Sprite>(name);
                spriteDic.Add(name, s);
            }
        }

        return s;
    }

    public AnimationClip LoadAnimationClip(string animationName)
    {
        AnimationClip clip = null;
        if (clipDic.ContainsKey(animationName))
        {
            clip = clipDic[animationName];
        }
        else
        {
            string path = animationABPath + animationName.ToLower();
            AssetBundle ab = AssetBundle.LoadFromFile(path);

            if (ab != null)
            {
                clip = LoadAnimClip(ab, animationName);
                clipDic.Add(animationName, clip);

                ab.Unload(false);
            }
        }

        return clip;
    }

    private AnimationClip LoadAnimClip(AssetBundle ab, string animationName)
    {
        AnimationClip clip = ab.LoadAsset<AnimationClip>(animationName);

        if (clip == null)
        {
            foreach (var asset in ab.LoadAllAssets())
            {
                if (asset is AnimationClip && animationName.Equals(asset.name))
                {
                    clip = asset as AnimationClip;
                    break;
                }
            }
        }

        return clip;
    }

    public T LoadRes<T>(string path) where T : UnityEngine.Object
    {
        lock (obj)
        {
            var name = typeof(T).Name;
            switch (name)
            {
                case "Texture":
                    Texture t = LoadTexture(path);
                    if (t) return t as T;
                    break;
                case "SpriteAtlas":
                    SpriteAtlas sa = LoadSpriteAtlas(path);
                    if (sa) return sa as T;
                    break;
                case "Material":
                    Material m = Inst.LoadMaterial(path);
                    if (m) return m as T;
                    break;
                case "Sprite":
                    Sprite s = Inst.LoadSprite(path);
                    if (s) return s as T;
                    break;
                default:
                    break;
            }

            return null;
        }
    }

    public void Clear()
    {
        foreach (KeyValuePair<string,AssetBundle> kv in SkyboxDic)
        {
            if (kv.Value != null)
            {
                kv.Value.Unload(true);
            }
        }
        SkyboxDic.Clear();
        foreach (KeyValuePair<string,AssetBundle> kv in ABDic)
        {
            if (kv.Value != null)
            {
                kv.Value.Unload(true);
            }
        }
        ABDic.Clear();
        
        foreach (KeyValuePair<string,Dictionary<string,Texture>> kv in TextureDic)
        {
            foreach (var tkv in kv.Value)
            {
                if (tkv.Value != null)
                {
                    GameObject.Destroy(tkv.Value);
                }
            }
            kv.Value.Clear();
        }
        
        foreach (var kv in SkyboxTexture)
        {
            if (kv.Value != null)
            {
                GameObject.Destroy(kv.Value);
            }
        }
        SkyboxTexture.Clear();
        foreach (var kv in atlasDic)
        {
            if (kv.Value != null)
            {
                GameObject.Destroy(kv.Value);
            }
        }
        atlasDic.Clear();
        foreach (var kv in matDic)
        {
            if (kv.Value != null)
            {
                GameObject.Destroy(kv.Value);
            }
        }
        matDic.Clear();
        foreach (var kv in spriteDic)
        {
            if (kv.Value != null)
            {
                GameObject.Destroy(kv.Value);
            }
        }
        spriteDic.Clear();

        foreach (var kv in clipDic)
        {
            if (kv.Value != null)
            {
                GameObject.Destroy(kv.Value);
            }
        }
        clipDic.Clear();
    }
    
    public override void Release()
    {
        base.Release();
        Clear();
    }
}