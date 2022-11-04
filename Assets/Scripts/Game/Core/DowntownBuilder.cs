using System.Collections.Generic;
using HLODSystem;
using Newtonsoft.Json;
using UnityEngine;

public partial class SceneBuilder : CInstance<SceneBuilder>
{
    public void DowntownParseAndBuild(string json)
    {
        allControllerBehaviours.Clear();
        ParsePropWithTipsManager.Inst.DestroyGameObj();
        NodeBehaviourManager.Inst.ClearManagers();
        SnowfieldMapData subMapData = JsonConvert.DeserializeObject<SnowfieldMapData>(json);

        var mData = ResManager.Inst.LoadJsonRes<SnowfieldMapData>("Prefabs/DownTown/SnowfieldJson");

        //UGCBehaviorManager.Inst.InitLocalOfflineRes(mData);
        var terColor = DataUtils.DeSerializeColor(mData.ter.cols);
        HLOD.Inst.Init();
        GetSkyboxCreater().CreateSkybox(mData.sky);
        SetDirLight(mData.dir);
        SetBGMusic(mData.bgmusic);
        SpawnPointManager.Inst.SetSpawnPointByData(mData.spawns);
        SetWeatherWithSaveParams(mData.weather);
        BuildAllNodes(mData.pref);

        SetSnowfieldTransferData(subMapData.subMaps);

        PostProcessCreater.SetData(PostProcessBehaviour, mData.postprocess);
        SceneBuilderUtils.Inst.ClearAll();
    }

    private void SetSnowfieldTransferData(List<string> subMaps)
    {
        LoggerUtils.Log("subMaps = " + JsonConvert.SerializeObject(subMaps));
        DowntownTransferManager.Inst.SetTransferData(subMaps);
    }
}
