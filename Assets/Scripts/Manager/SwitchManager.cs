using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public enum SwitchAction
{
    Add,
    Remove
}

/// <summary>
/// Author:Shaocheng
/// Description:开关控制管理
/// Date: 2022-3-30 19:43:08
/// </summary>
public class SwitchManager : ManagerInstance<SwitchManager>, IManager,IPVPManager
{
    public int MaxCount = 99;

    public int CurrentNum;

    //key - switchId
    public Dictionary<int, NodeBaseBehaviour> switchBevs = new Dictionary<int, NodeBaseBehaviour>();

    #region NetMessage Listener

    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========SwitchManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null )
            {
                LoggerUtils.Log("[SwitchManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                if (getItemsRsp.items == null)
                {
                    LoggerUtils.Log("[SwitchManager.OnGetItemsCallback]getItemsRsp.items is null");
                    return;
                }

                for (int i = 0; i < getItemsRsp.items.Length; i++)
                {
                    Item item = getItemsRsp.items[i];
                    if (item.type == (int)ItemType.SWITCH)
                    {
                        var sid = GetSwitchSIDByUid(item.id);
                        if (sid != 0)
                        {
                            SwitchButtonBehaviour bev = switchBevs[sid] as SwitchButtonBehaviour;
                            SwitchPack sp = JsonConvert.DeserializeObject<SwitchPack>(item.data);
                            if (bev.isWork != (sp.status == 1))
                            {
                                OnHandleSwitchClicked(bev);
                                bev.isWork = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("SwitchButton OnReceiveServer==>" + msg);

        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.SWITCH)
                {
                    var sid = GetSwitchSIDByUid(item.id);
                    if (sid != 0)
                    {
                        SwitchPack sp = JsonConvert.DeserializeObject<SwitchPack>(item.data);
                        bool serverIsWork = sp.status == 1;

                        SwitchButtonBehaviour bev = switchBevs[sid] as SwitchButtonBehaviour;

                        if (serverIsWork == bev.isWork)
                        {
                            LoggerUtils.Log("SwitchButton OnReceiveServer 出现两次一样的值");
                            return true;
                        }
                        bev.isWork = serverIsWork;
                        OnHandleSwitchClicked(bev);
                    }
                    else
                    {
                        LoggerUtils.Log("[SwitchButton OnReceiveServer] not find switch, uid=" + item.id);
                    }
                }
            }
        }
        return true;
    }

    private void OnHandleSwitchClicked(SwitchButtonBehaviour bev)
    {
        var uids = bev.entity.Get<SwitchButtonComponent>().controllUids;
        foreach (var uid in uids)
        {
            ShowHideManager.Inst.OnSwitchClick(uid);
        }

        var moveUids = bev.entity.Get<SwitchButtonComponent>().moveControllUids;
        foreach (var uid in moveUids)
        {
            SwitchControlManager.Inst.OnSwitchClick(uid);
        }

        var soundUids = bev.entity.Get<SwitchButtonComponent>().soundControllUids;
        foreach (var uid in soundUids)
        {
            // 没有关闭 loading 页面则不需要播放之前的开关控制的声音播放
            if (GameManager.Inst == null || !GameManager.Inst.loadingPageIsClosed)
            {
                LoggerUtils.Log("===========SwitchManager===> OnHandleSwitchClicked loadingPage is not closed, could not play sound!!!!");
            }
            else
            {
                SwitchControlManager.Inst.OnSwitchPlaySound(uid);
            }
        }

        var animUids = bev.entity.Get<SwitchButtonComponent>().animControllUids;
        foreach (var uid in animUids)
        {
            SwitchControlManager.Inst.OnSwitchRefreshAnim(uid);
        }
    }

    #endregion

    public void ClearBevs()
    {
        if (switchBevs != null)
        {
            switchBevs.Clear();
        }
    }

    public bool IsOverMaxSwitchCount()//最大开关数量
    {
        if (CurrentNum >= MaxCount)
        {
            return true;
        }
        return false;
    }

    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        if (newBev.entity.HasComponent<SwitchButtonComponent>())
        {
            AddSwtich(newBev);
        }
    }

    public bool IsCanCloneSwitch(int count)
    {
        if (CurrentNum + count > MaxCount)
        {
            return false;
        }
        return true;
    }

    public int GetNewSwitchId()
    {
        return ++CurrentNum;
    }

    public void UpdateMaxSwitchId(int newId)
    {
        if (newId > CurrentNum)
        {
            CurrentNum = newId;
        }
    }

    public void AddSwtich(NodeBaseBehaviour b)
    {
        int sid = b.entity.Get<SwitchButtonComponent>().switchId;
        if (!switchBevs.ContainsKey(sid))
        {
            switchBevs.Add(sid, b);
            LoggerUtils.Log("switchCount:" + switchBevs.Count);
        }
        else
        {
            LoggerUtils.Log("[AddSwtich] sid already exist");
        }
    }

    public void OnRemoveNode(NodeBaseBehaviour b)
    {
        // LoggerUtils.Log("SwitchManager OnRemoveNode");
        GameObjectComponent goCmp = b.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.Switch)
        {
            int sid = b.entity.Get<SwitchButtonComponent>().switchId;
            switchBevs.Remove(sid);
        }

        foreach (var switchId in switchBevs.Keys)
        {
            var behav = switchBevs[switchId];
            var controllList = behav.entity.Get<SwitchButtonComponent>().controllUids;
            if (controllList.Contains(goCmp.uid))
            {
                controllList.Remove(goCmp.uid);
            }

            var moveControllList = behav.entity.Get<SwitchButtonComponent>().moveControllUids;
            if (moveControllList.Contains(goCmp.uid))
            {
                moveControllList.Remove(goCmp.uid);
            }

            var soundControllList = behav.entity.Get<SwitchButtonComponent>().soundControllUids;
            if (soundControllList.Contains(goCmp.uid))
            {
                soundControllList.Remove(goCmp.uid);
            }

            var animControllList = behav.entity.Get<SwitchButtonComponent>().animControllUids;
            if (animControllList.Contains(goCmp.uid))
            {
                animControllList.Remove(goCmp.uid);
            }
            var fireworkControllList = behav.entity.Get<SwitchButtonComponent>().fireworkControllUids;
            if (fireworkControllList.Contains(goCmp.uid))
            {
                fireworkControllList.Remove(goCmp.uid);
            }
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        OnRemoveNode(behaviour);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        //恢复开关关联的物体的开关属性
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.Switch)
        {
            AddSwtich(behaviour);

            SwitchButtonBehaviour b = behaviour as SwitchButtonBehaviour;
            b.ShowIndexNum();

            int switchUid = goCmp.uid;
            var showHideUids = behaviour.entity.Get<SwitchButtonComponent>().controllUids;
            foreach(int uid in showHideUids)
            {
                var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                if(entity !=null)
                {
                    ShowHideManager.Inst.AddSwitchId(entity,switchUid);
                }
            }

            var moveUids = behaviour.entity.Get<SwitchButtonComponent>().moveControllUids;
            foreach(int uid in moveUids)
            {
                var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                if(entity !=null)
                {
                    SwitchControlManager.Inst.AddSwitchId(entity,switchUid);
                }
            }

            var soundUids = behaviour.entity.Get<SwitchButtonComponent>().soundControllUids;
            foreach (int uid in soundUids)
            {
                var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                if (entity != null)
                {
                    SwitchControlManager.Inst.AddSoundSwitchId(entity, switchUid);
                }
            }

            var animUids = behaviour.entity.Get<SwitchButtonComponent>().animControllUids;
            foreach (int uid in animUids)
            {
                var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                if (entity != null)
                {
                    SwitchControlManager.Inst.AddAnimSwitchId(entity, switchUid);
                }
            }
            var fireworkUids = behaviour.entity.Get<SwitchButtonComponent>().fireworkControllUids;
            foreach (int uid in fireworkUids)
            {
                var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                if (entity != null)
                {
                    SwitchControlManager.Inst.AddFireworkSwitchId(entity, switchUid);
                }
            }
        }
    }

    public void OnCombineNode(SceneEntity entity)
    {
        foreach (var sBevs in switchBevs.Values)
        {
            if (sBevs.entity.HasComponent<SwitchButtonComponent>())
            {
                var controllList = sBevs.entity.Get<SwitchButtonComponent>().controllUids;
                var combineUid = entity.Get<GameObjectComponent>().uid;
                if (controllList.Contains(combineUid))
                {
                    controllList.Remove(combineUid);
                    LoggerUtils.Log("[OnCombineNode] reset switch controll uid=" + combineUid);
                }

                var moveControllList = sBevs.entity.Get<SwitchButtonComponent>().moveControllUids;
                var moveCombineUid = entity.Get<GameObjectComponent>().uid;
                if (moveControllList.Contains(moveCombineUid))
                {
                    moveControllList.Remove(moveCombineUid);
                    LoggerUtils.Log("[OnCombineNode] reset switch controll uid=" + moveCombineUid);
                }

                var soundControllList = sBevs.entity.Get<SwitchButtonComponent>().soundControllUids;
                var soundCombineUid = entity.Get<GameObjectComponent>().uid;
                if (soundControllList.Contains(soundCombineUid))
                {
                    soundControllList.Remove(soundCombineUid);
                    LoggerUtils.Log("[OnCombineNode] reset switch controll uid=" + soundCombineUid);
                }

                var animControllList = sBevs.entity.Get<SwitchButtonComponent>().animControllUids;
                var animCombineUid = entity.Get<GameObjectComponent>().uid;
                if (animControllList.Contains(animCombineUid))
                {
                    animControllList.Remove(animCombineUid);
                    LoggerUtils.Log("[OnCombineNode] reset switch controll uid=" + animCombineUid);
                }
                var fireworkControllList = sBevs.entity.Get<SwitchButtonComponent>().fireworkControllUids;
                var firworkCombineUid = entity.Get<GameObjectComponent>().uid;
                if (fireworkControllList.Contains(firworkCombineUid))
                {
                    fireworkControllList.Remove(firworkCombineUid);
                    LoggerUtils.Log("[OnCombineNode] reset switch controll uid=" + firworkCombineUid);
                }
            }
        }
    }

    public void Clear()
    {
    }

    public void AddControlledId(int sid, int controllUid, int type = (int)SwitchControlType.VISIBLE_CONTROL)
    {
        if (switchBevs.ContainsKey(sid))
        {
            var cmp = switchBevs[sid].entity.Get<SwitchButtonComponent>();
            var ctrUids = cmp.controllUids;

            switch (type)
            {
                case (int)SwitchControlType.MOVEMENT_CONTROL:
                    ctrUids = cmp.moveControllUids;
                    break;
                case (int)SwitchControlType.SOUNDPLAY_CONTROL:
                    ctrUids = cmp.soundControllUids;
                    break;
                case (int)SwitchControlType.ANIMATION_CONTROL:
                    ctrUids = cmp.animControllUids;
                    break;
                case (int)SwitchControlType.FIREWORK_CONTROL:
                    ctrUids = cmp.fireworkControllUids;
                    break;
            }
            if (!ctrUids.Contains(controllUid))
            {
                ctrUids.Add(controllUid);
                LoggerUtils.Log("[AddControlledIdToSwitch] ctrUids added:" + LogList(ctrUids));
            }
            else
            {
                LoggerUtils.Log("[AddControlledIdToSwitch] controllUid already exist");
            }
        }
        else
        {
            LoggerUtils.Log("[AddControlledIdToSwitch] sid not found =>" + sid);
        }
    }

    public void OnReset()
    {
        foreach (var switchBtn in switchBevs.Values)
        {
            if (switchBtn != null)
            {
                SwitchButtonBehaviour bev =switchBtn as SwitchButtonBehaviour;
                bev.isWork = false;
            }
        }
    }
    // public void AddMoveControlledId(int sid, int controllUid)
    // {
    //     if (switchBevs.ContainsKey(sid))
    //     {
    //         var ctrUids = switchBevs[sid].entity.Get<SwitchButtonComponent>().moveControllUids;
    //         if (!ctrUids.Contains(controllUid))
    //         {
    //             ctrUids.Add(controllUid);
    //             LoggerUtils.Log("[AddMoveControlledIdToSwitch] ctrUids added:" + LogList(ctrUids));
    //         }
    //         else
    //         {
    //             LoggerUtils.Log("[AddMoveControlledIdToSwitch] controllUid already exist");
    //         }
    //     }
    //     else
    //     {
    //         LoggerUtils.Log("[AddMoveControlledIdToSwitch] sid not found =>" + sid);
    //     }
    // }

    // public void AddSoundControlledId(int sid, int controllUid)
    // {
    //     if (switchBevs.ContainsKey(sid))
    //     {
    //         var ctrUids = switchBevs[sid].entity.Get<SwitchButtonComponent>().soundControllUids;
    //         if (!ctrUids.Contains(controllUid))
    //         {
    //             ctrUids.Add(controllUid);
    //             LoggerUtils.Log("[AddSoundControlledIdToSwitch] ctrUids added:" + LogList(ctrUids));
    //         }
    //         else
    //         {
    //             LoggerUtils.Log("[AddSoundControlledIdToSwitch] controllUid already exist");
    //         }
    //     }
    //     else
    //     {
    //         LoggerUtils.Log("[AddSoundControlledIdToSwitch] sid not found =>" + sid);
    //     }
    // }

    public void RemoveControlledId(int sid, int controllUid, int type = (int)SwitchControlType.VISIBLE_CONTROL)
    {
        if (switchBevs.ContainsKey(sid))
        {
            var cmp = switchBevs[sid].entity.Get<SwitchButtonComponent>();
            var ctrUids = cmp.controllUids;

            switch (type)
            {
                case (int)SwitchControlType.MOVEMENT_CONTROL:
                    ctrUids = cmp.moveControllUids;
                    break;
                case (int)SwitchControlType.SOUNDPLAY_CONTROL:
                    ctrUids = cmp.soundControllUids;
                    break;
                case (int)SwitchControlType.ANIMATION_CONTROL:
                    ctrUids = cmp.animControllUids;
                    break;
                case (int)SwitchControlType.FIREWORK_CONTROL:
                    ctrUids = cmp.fireworkControllUids;
                    break;
            }

            if (ctrUids.Contains(controllUid))
            {
                ctrUids.Remove(controllUid);
                LoggerUtils.Log("[RemoveControlledIdFromSwitch] ctrUids removed:" + LogList(ctrUids));
            }
            else
            {
                LoggerUtils.Log("[RemoveControlledIdFromSwitch] controllUid not found =>" + controllUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveControlledIdFromSwitch] sid not found =>" + sid);
        }
    }

    // public void RemoveMoveControlledId(int sid, int controllUid)
    // {
    //     if (switchBevs.ContainsKey(sid))
    //     {
    //         var ctrUids = switchBevs[sid].entity.Get<SwitchButtonComponent>().moveControllUids;
    //         if (ctrUids.Contains(controllUid))
    //         {
    //             ctrUids.Remove(controllUid);
    //             LoggerUtils.Log("[RemoveControlledIdFromSwitch] ctrUids removed:" + LogList(ctrUids));
    //         }
    //         else
    //         {
    //             LoggerUtils.Log("[RemoveControlledIdFromSwitch] controllUid not found =>" + controllUid);
    //         }
    //     }
    //     else
    //     {
    //         LoggerUtils.Log("[RemoveControlledIdFromSwitch] sid not found =>" + sid);
    //     }
    // }

    // public void RemoveSoundControlledId(int sid, int controllUid)
    // {
    //     if (switchBevs.ContainsKey(sid))
    //     {
    //         var ctrUids = switchBevs[sid].entity.Get<SwitchButtonComponent>().soundControllUids;
    //         if (ctrUids.Contains(controllUid))
    //         {
    //             ctrUids.Remove(controllUid);
    //             LoggerUtils.Log("[RemoveSoundControlledIdFromSwitch] ctrUids removed:" + LogList(ctrUids));
    //         }
    //         else
    //         {
    //             LoggerUtils.Log("[RemoveSoundControlledIdFromSwitch] controllUid not found =>" + controllUid);
    //         }
    //     }
    //     else
    //     {
    //         LoggerUtils.Log("[RemoveSoundControlledIdFromSwitch] sid not found =>" + sid);
    //     }
    // }

    public int GetSwtichUidBySId(int sid)
    {
        if (switchBevs.ContainsKey(sid))
        {
            return switchBevs[sid].entity.Get<GameObjectComponent>().uid;
        }
        LoggerUtils.Log("switch not found, sid =>" + sid);
        return 0;
    }

    public int GetSwitchSIDByUid(int uid)
    {
        foreach (var sid in switchBevs.Keys)
        {
            var b = switchBevs[sid];
            if (uid == b.entity.Get<GameObjectComponent>().uid)
            {
                return sid;
            }
        }
        return 0;
    }

    //todo:delete it
    private string LogList(List<int> list)
    {
        string s = "";

        for (int i = 0; i < list.Count; i++)
        {
            s = s + list[i] + ", ";
        }

        return s;
    }

}