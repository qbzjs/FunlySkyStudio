using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:视频道具创建
/// Date: 2022-3-30 19:43:08
/// </summary>
public class VideoNodeCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.Video);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.Video;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Video;
        gameComponent.modelType = NodeModelType.Video;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static bool CanCloneTarget(GameObject target)
    {
        return false;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var oBehav = oBehaviour as VideoNodeBehaviour;
        var nBehav = nBehaviour as VideoNodeBehaviour;
        VideoNodeManager.Inst.AddVideoNode(nBehav);
        if (oBehav.currentStatus == VideoLoadStatus.UrlLoading)
        {
            //加载url时复制，nBehav需要开启加载
            var url = oBehav.entity.Get<VideoNodeComponent>().videoUrl;
            nBehav.StartLoadVideoUrl(url);
        }
        else
        {
            nBehav.SetVideoStatus(oBehav.currentStatus);
        }
        bool isActive = oBehav.GetThumbnailObject().gameObject.activeInHierarchy;
        nBehav.GetThumbnailObject().gameObject.SetActive(isActive);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ?? SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        SetData(behaviour as VideoNodeBehaviour, GameUtils.GetAttr<VideoNodeData>((int)BehaviorKey.VideoNode, data.attr));
    }

    public static void SetData(VideoNodeBehaviour behaviour, VideoNodeData data)
    {
        var mComp = behaviour.entity.Get<VideoNodeComponent>();
        mComp.videoUrl = data.vUrl;
        mComp.soundRange = data.sRange;
        behaviour.InitSoundRange();
        VideoNodeManager.Inst.AddVideoNode(behaviour);
    }
}