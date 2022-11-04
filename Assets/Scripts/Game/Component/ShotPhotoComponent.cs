using Newtonsoft.Json;
using System;
using SavingData;

[Serializable]
public struct ShotPhotoData
{
    public string pUrl;
    public int type;
}

/// <summary>
/// Author: 熊昭
/// Description: 3D相册道具数据类
/// Date: 2022-02-06 18:00:27
/// </summary>
public class ShotPhotoComponent : IComponent
{
    public string photoUrl = "";
    public SavePhotoType type = SavePhotoType.CheckInPhoto;

    public IComponent Clone()
    {
        ShotPhotoComponent component = new ShotPhotoComponent();
        component.photoUrl = photoUrl;
        component.type = type;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        ShotPhotoData data = new ShotPhotoData
        {
            pUrl = photoUrl,
            type = (int)type
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.ShotPhoto,
            v = JsonConvert.SerializeObject(data)
        };
    }
}