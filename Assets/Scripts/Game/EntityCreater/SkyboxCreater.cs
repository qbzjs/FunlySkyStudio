using UnityEngine;
using Assets.Scripts.Game.Core;
using UnityEngine.Rendering;

/// <summary>
/// Author:Shaocheng
/// Description: 天空盒创建
/// Date: 2022-9-7 17:10:53
/// </summary>
public class SkyboxCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        return null;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public void CreateEmptyMapSkybox(int skyId)
    {
        SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxId = skyId;
        SceneBuilder.Inst.SkyboxBev.SetNormalSky(skyId);
    }

    public void CreateSkybox(SkyData data)
    {
        SetSkyboxData(data);

        switch ((SkyboxType) data.skyboxType)
        {
            case SkyboxType.Normal:
                CreateNormalSkybox(data);
                break;
            case SkyboxType.DayNight:
                CreateDayNightSkybox(data);
                break;
            default:
                break;
        }
    }

    //普通天空盒创建
    private void CreateNormalSkybox(SkyData data)
    {
        var skyId = data.skyId;
        SceneBuilder.Inst.SkyboxBev.SetNormalSky(skyId);

        var scolor = DataUtils.DeSerializeColor(data.scol);
        var eColor = DataUtils.DeSerializeColor(data.ecol);
        var gColor = DataUtils.DeSerializeColor(data.gcol);
        SkyboxBehaviour.SetSkyGradient((AmbientMode) data.type, scolor, eColor, gColor);
    }

    //昼夜天空盒创建
    private void CreateDayNightSkybox(SkyData data)
    {
        SceneBuilder.Inst.SkyboxBev.SetDayNightSkybox(data.skyId);
    }

    public static void SetSkyboxData(SkyData data)
    {
        var skyboxCmp = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>();
        skyboxCmp.skyboxId = data.skyId;
        skyboxCmp.type = data.type;
        skyboxCmp.scol = data.scol;
        skyboxCmp.ecol = data.ecol;
        skyboxCmp.gcol = data.gcol;
        skyboxCmp.skyboxType = (SkyboxType) data.skyboxType;
        skyboxCmp.dayLength = data.dayLength;
        skyboxCmp.daytimeHour = data.daytimeHour;
        skyboxCmp.daytimeMin = data.daytimeMin;
    }
}