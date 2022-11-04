using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:开关控制的物体集中管理器，主要管理由开关控制的物体
/// Date: 2022/1/20 16:10:17
/// </summary>

public class SwitchControlManager : ManagerInstance<SwitchControlManager>, IManager
{
    //key - uid 
    public Dictionary<int, SceneEntity> allControlEntity = new Dictionary<int, SceneEntity>();

    public void Clear()
    {
        allControlEntity.Clear();
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        OnRemoveNode(behaviour);
    }

    public void OnRemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        int uid = goCmp.uid;

        if (allControlEntity.ContainsKey(uid))
        {
            allControlEntity.Remove(uid);
        }

        if (goCmp.modelType == NodeModelType.Switch)
        {
            foreach (var e in allControlEntity.Values)
            {
                var switchUids = e.Get<SwitchControlComponent>().switchUids;
                if (switchUids.Contains(uid))
                {
                    switchUids.Remove(uid);
                }

                var switchSoundUids = e.Get<SwitchControlComponent>().switchSoundUids;
                if (switchSoundUids.Contains(uid))
                {
                    switchSoundUids.Remove(uid);
                }

                var switchAnimUids = e.Get<SwitchControlComponent>().switchAnimUids;
                if (switchAnimUids.Contains(uid))
                {
                    switchAnimUids.Remove(uid);
                }
            }
        }
    }

    public void OnHandleClone(SceneEntity sourceEntity, SceneEntity newEntity)
    {
        if (newEntity.HasComponent<SwitchControlComponent>())
        {
            AddSwitchControlEntityToDict(newEntity);

            var switchCmp = newEntity.Get<SwitchControlComponent>();
            var newSwitchIds = switchCmp.switchUids;
            var newSwitchSoundIds = switchCmp.switchSoundUids;
            var newSwitchAnimIds = switchCmp.switchAnimUids;
            var newSwitchFireworkIds = switchCmp.switchFireworkUids;
            var newUid = newEntity.Get<GameObjectComponent>().uid;

            if (newSwitchIds != null && newSwitchIds.Count > 0)
            {
                foreach (var suid in newSwitchIds)
                {
                    int sid = SwitchManager.Inst.GetSwitchSIDByUid(suid);
                    SwitchManager.Inst.AddControlledId(sid, newUid, (int)SwitchControlType.MOVEMENT_CONTROL);
                }
            }

            if (newSwitchSoundIds != null && newSwitchSoundIds.Count > 0)
            {
                foreach (var suid in newSwitchSoundIds)
                {
                    int sid = SwitchManager.Inst.GetSwitchSIDByUid(suid);
                    SwitchManager.Inst.AddControlledId(sid, newUid, (int)SwitchControlType.SOUNDPLAY_CONTROL);
                }
            }

            if (newSwitchAnimIds != null && newSwitchAnimIds.Count > 0)
            {
                foreach (var suid in newSwitchAnimIds)
                {
                    int sid = SwitchManager.Inst.GetSwitchSIDByUid(suid);
                    SwitchManager.Inst.AddControlledId(sid, newUid, (int)SwitchControlType.ANIMATION_CONTROL);
                }
            }
            if (newSwitchFireworkIds != null && newSwitchFireworkIds.Count > 0)
            {
                foreach (var suid in newSwitchFireworkIds)
                {
                    int sid = SwitchManager.Inst.GetSwitchSIDByUid(suid);
                    SwitchManager.Inst.AddControlledId(sid, newUid, (int)SwitchControlType.FIREWORK_CONTROL);
                }
            }
        }
    }

    //组合时重置默认显隐状态，以及重置被开关控制的关系
    public void OnCombineNode(SceneEntity entity)
    {
        if (entity != null && entity.HasComponent<SwitchControlComponent>())
        {
            var uid = entity.Get<GameObjectComponent>().uid;
            if (allControlEntity.ContainsKey(uid))
            {
                allControlEntity.Remove(uid);
            }
            entity.Remove<SwitchControlComponent>();
        }
    }

    public void AddSwitchControlEntityToDict(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<SwitchControlComponent>())
        {
            if (!allControlEntity.ContainsKey(uid))
            {
                allControlEntity.Add(uid, entity);
                LoggerUtils.Log("[AddSwitchControlEntityToDict] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddSwitchControlEntityToDict] SwitchControlComponent not exist");
        }
    }

    //没有被开关控制时,去除 SwitchControlComponent
    public void UpdateSwitchCmpInEntity(SceneEntity curEntity)
    {
        if (curEntity.HasComponent<SwitchControlComponent>())
        {
            var switchUids = curEntity.Get<SwitchControlComponent>().switchUids;
            var switchSoundUids = curEntity.Get<SwitchControlComponent>().switchSoundUids;
            var switchAnimUids = curEntity.Get<SwitchControlComponent>().switchAnimUids;
            var switchfireworkAnimUids = curEntity.Get<SwitchControlComponent>().switchFireworkUids;
            if ((switchUids == null || switchUids.Count <= 0)
            && (switchSoundUids == null || switchSoundUids.Count <= 0)
            && (switchAnimUids == null || switchAnimUids.Count <= 0)
            && (switchfireworkAnimUids == null || switchfireworkAnimUids.Count <= 0))
            {
                curEntity.Remove<SwitchControlComponent>();
                allControlEntity.Remove(curEntity.Get<GameObjectComponent>().uid);
                LoggerUtils.Log("没有被开关控制，移除 SwitchControlComponent");
            }
        }
    }

    public void AddSwitchId(SceneEntity curEntity, int switchUid)
    {
        var switchUids = curEntity.Get<SwitchControlComponent>().switchUids;
        if (!switchUids.Contains(switchUid))
        {
            switchUids.Add(switchUid);
            AddSwitchControlEntityToDict(curEntity);
            LoggerUtils.Log("[AddSwitchId] Entity switchIds :" + switchUid);
        }
        else
        {
            LoggerUtils.Log("[AddSwitchId] switchUid already exist");
        }
    }

    public void RemoveSwitchId(SceneEntity curEntity, int switchUid)
    {
        if (curEntity.HasComponent<SwitchControlComponent>())
        {
            var switchUids = curEntity.Get<SwitchControlComponent>().switchUids;
            if (switchUids.Contains(switchUid))
            {
                switchUids.Remove(switchUid);
                LoggerUtils.Log("[RemoveSwitchId]移除curEntity开关id=>" + switchUid);

                UpdateSwitchCmpInEntity(curEntity);
            }
            else
            {
                LoggerUtils.Log("[RemoveSwitchId] switchUid not found =>" + switchUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveSwitchId] SwitchControlComponent not found");
        }
    }

    public void AddSoundSwitchId(SceneEntity curEntity, int switchUid)
    {
        var switchUids = curEntity.Get<SwitchControlComponent>().switchSoundUids;
        if (!switchUids.Contains(switchUid))
        {
            switchUids.Add(switchUid);
            AddSwitchControlEntityToDict(curEntity);
            LoggerUtils.Log("[AddSoundSwitchId] Entity switchIds :" + switchUid);
        }
        else
        {
            LoggerUtils.Log("[AddSoundSwitchId] switchUid already exist");
        }
    }

    public void RemoveSoundSwitchId(SceneEntity curEntity, int switchUid)
    {
        if (curEntity.HasComponent<SwitchControlComponent>())
        {
            var switchUids = curEntity.Get<SwitchControlComponent>().switchSoundUids;
            if (switchUids.Contains(switchUid))
            {
                switchUids.Remove(switchUid);
                LoggerUtils.Log("[RemoveSoundSwitchId]移除curEntity开关id=>" + switchUid);

                UpdateSwitchCmpInEntity(curEntity);
            }
            else
            {
                LoggerUtils.Log("[RemoveSoundSwitchId] switchUid not found =>" + switchUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveSoundSwitchId] SwitchControlComponent not found");
        }
    }


    public void AddAnimSwitchId(SceneEntity curEntity, int switchUid)
    {
        var switchUids = curEntity.Get<SwitchControlComponent>().switchAnimUids;
        if (!switchUids.Contains(switchUid))
        {
            switchUids.Add(switchUid);
            AddSwitchControlEntityToDict(curEntity);
            LoggerUtils.Log("[AddAnimSwitchId] Entity switchIds :" + switchUid);
        }
        else
        {
            LoggerUtils.Log("[AddAnimSwitchId] switchUid already exist");
        }
    }

    public void RemoveAnimSwitchId(SceneEntity curEntity, int switchUid)
    {
        if (curEntity.HasComponent<SwitchControlComponent>())
        {
            var switchUids = curEntity.Get<SwitchControlComponent>().switchAnimUids;
            if (switchUids.Contains(switchUid))
            {
                switchUids.Remove(switchUid);
                LoggerUtils.Log("[RemoveAnimSwitchId]移除curEntity开关id=>" + switchUid);

                UpdateSwitchCmpInEntity(curEntity);
            }
            else
            {
                LoggerUtils.Log("[RemoveAnimSwitchId] switchUid not found =>" + switchUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveAnimSwitchId] SwitchControlComponent not found");
        }
    }

    public void AddFireworkSwitchId(SceneEntity curEntity, int switchUid)
    {
        var switchUids = curEntity.Get<SwitchControlComponent>().switchFireworkUids;
        if (!switchUids.Contains(switchUid))
        {
            switchUids.Add(switchUid);
            AddSwitchControlEntityToDict(curEntity);
            LoggerUtils.Log("[AddFireworkSwitchId] Entity switchIds :" + switchUid);
        }
        else
        {
            LoggerUtils.Log("[AddFireworkSwitchId] switchUid already exist");
        }
    }
    public void RemoveFireworkSwitchId(SceneEntity curEntity, int switchUid)
    {
        if (curEntity.HasComponent<SwitchControlComponent>())
        {
            var switchUids = curEntity.Get<SwitchControlComponent>().switchFireworkUids;
            if (switchUids.Contains(switchUid))
            {
                switchUids.Remove(switchUid);
                LoggerUtils.Log("[RemoveFireworkSwitchId]移除curEntity开关id=>" + switchUid);

                UpdateSwitchCmpInEntity(curEntity);
            }
            else
            {
                LoggerUtils.Log("[RemoveFireworkSwitchId] switchUid not found =>" + switchUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveFireworkSwitchId] SwitchControlComponent not found");
        }
    }

    public void OnSwitchClick(int uid)
    {
        if (allControlEntity.ContainsKey(uid))
        {
            var go = allControlEntity[uid].Get<GameObjectComponent>().bindGo;

            var propStar = go.GetComponentInChildren<PropStarBehaviour>(true);
            if (propStar != null && !propStar.CheckCanClick())
            {
                //星星被收集之后 不会再被开关控制
                return;
            }
            if (allControlEntity[uid].HasComponent<SwitchControlComponent>())
            {
                var switchCmp = allControlEntity[uid].Get<SwitchControlComponent>();
                // if (switchCmp.switchControlType == (int)SwitchControlType.MOVEMENT_CONTROL)
                // {
                if (!go.activeSelf)
                {
                    // 当物体不可见时，移动开关不生效
                    return;
                }

                var moveCom = allControlEntity[uid].Get<MovementComponent>();
                moveCom.tempMoveState = ((moveCom.tempMoveState == 0) ? 1 : 0);
                SceneSystem.Inst.ExcuteMoveSystem();
                // }
            }
        }
        else
        {
            LoggerUtils.Log("[OnSwitchClick] uid not found = " + uid);
        }
    }

    // 点击开关播放音乐
    public void OnSwitchPlaySound(int uid)
    {
        if (allControlEntity.ContainsKey(uid))
        {
            var go = allControlEntity[uid].Get<GameObjectComponent>().bindGo;

            if (allControlEntity[uid].HasComponent<SwitchControlComponent>())
            {
                var switchCmp = allControlEntity[uid].Get<SwitchControlComponent>();

                // if (switchCmp.controlPlaySound == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                // {
                var soundCom = allControlEntity[uid].Get<SoundComponent>();
                // if (soundCom.isControl == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                // {
                SwitchSoundPlay(go);
                // }
                // }
            }
        }
        else
        {
            LoggerUtils.Log("[OnSwitchPlaySound] uid not found = " + uid);
        }
    }

    // 点击开关刷新物体的旋转移动状态
    public void OnSwitchRefreshAnim(int uid)
    {
        if (allControlEntity.ContainsKey(uid))
        {
            var go = allControlEntity[uid].Get<GameObjectComponent>().bindGo;

            if (allControlEntity[uid].HasComponent<SwitchControlComponent>())
            {
                var switchCmp = allControlEntity[uid].Get<SwitchControlComponent>();

                if (!go.activeSelf)
                {
                    // 当物体不可见时，移动开关不生效
                    return;
                }

                var moveCom = allControlEntity[uid].Get<RPAnimComponent>();
                moveCom.tempAnimState = ((moveCom.tempAnimState == 0) ? 1 : 0);
                SceneSystem.Inst.ExcuteUpDownSystem();
            }
        }
        else
        {
            LoggerUtils.Log("[OnSwitchPlaySound] uid not found = " + uid);
        }
    }
    // 点击开关播放烟花
    public void OnSwitchPlayFirework(int uid)
    {
        if (allControlEntity.ContainsKey(uid))
        {
            var go = allControlEntity[uid].Get<GameObjectComponent>().bindGo;

            if (allControlEntity[uid].HasComponent<SwitchControlComponent>())
            {
                var switchCmp = allControlEntity[uid].Get<SwitchControlComponent>();
                if (!go.activeSelf)
                {
                    // 当物体不可见时，开关不生效
                    return;
                }
                var fireBev = go.GetComponentInChildren<NodeBaseBehaviour>();
                FireworkManager.Inst.OnTriggerFirework(fireBev);
            }
        }
        else
        {
            LoggerUtils.Log("[OnSwitchPlaySound] uid not found = " + uid);
        }
    }



    //开关控制音乐播放
    private void SwitchSoundPlay(GameObject go)
    {
        if (!go.activeSelf)
        {
            //当物体不可见时，音乐播放控制不生效
            return;
        }
        var soundBev = go.GetComponentInChildren<SoundButtonBehaviour>();
        // 本地播放音乐
        soundBev.OnClickSound();
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        //恢复与关联开关的关系
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if(behaviour.entity.HasComponent<SwitchControlComponent>())
        {
            AddSwitchControlEntityToDict(behaviour.entity);

            var switchCtrComp = behaviour.entity.Get<SwitchControlComponent>();
            var switchUids = switchCtrComp.switchUids;
            var switchSoundUids = switchCtrComp.switchSoundUids;
            var switchAnimUids = switchCtrComp.switchAnimUids;
            var switchFireworkUids = switchCtrComp.switchFireworkUids;

            //找到SwitchControlComponent关联的开关，并将自己注册回去
            foreach(int switchId in switchUids)
            {
                LoggerUtils.Log("SwitchControlManager RevertNode(): switch:"+switchId);
                var switchEntity = SceneBuilder.Inst.GetEntityByUid(switchId);

                if(switchEntity != null && switchEntity.HasComponent<SwitchButtonComponent>())
                {
                    var controllList = switchEntity.Get<SwitchButtonComponent>().moveControllUids;
                    if (!controllList.Contains(goCmp.uid))
                    {
                        controllList.Add(goCmp.uid);
                    }
                }
            }

            foreach (int switchId in switchSoundUids)
            {
                LoggerUtils.Log("SwitchControlManager RevertNode(): switch:" + switchId);
                var switchEntity = SceneBuilder.Inst.GetEntityByUid(switchId);

                if (switchEntity != null && switchEntity.HasComponent<SwitchButtonComponent>())
                {
                    var soundCtrlList = switchEntity.Get<SwitchButtonComponent>().soundControllUids;
                    if (!soundCtrlList.Contains(goCmp.uid))
                    {
                        soundCtrlList.Add(goCmp.uid);
                    }
                }
            }

            foreach (int switchId in switchAnimUids)
            {
                LoggerUtils.Log("SwitchControlManager RevertNode(): switch:" + switchId);
                var switchEntity = SceneBuilder.Inst.GetEntityByUid(switchId);

                if (switchEntity != null && switchEntity.HasComponent<SwitchButtonComponent>())
                {
                    var animCtrlList = switchEntity.Get<SwitchButtonComponent>().animControllUids;
                    if (!animCtrlList.Contains(goCmp.uid))
                    {
                        animCtrlList.Add(goCmp.uid);
                    }
                }
            }
            foreach (int switchId in switchFireworkUids)
            {
                LoggerUtils.Log("SwitchControlManager RevertNode(): switch:" + switchId);
                var switchEntity = SceneBuilder.Inst.GetEntityByUid(switchId);

                if (switchEntity != null && switchEntity.HasComponent<SwitchButtonComponent>())
                {
                    var fireworkCtrlList = switchEntity.Get<SwitchButtonComponent>().fireworkControllUids;
                    if (!fireworkCtrlList.Contains(goCmp.uid))
                    {
                        fireworkCtrlList.Add(goCmp.uid);
                    }
                }
            }
        }
        
    }
}