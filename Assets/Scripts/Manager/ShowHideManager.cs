using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class ShowHideManager : ManagerInstance<ShowHideManager>, IManager,IPVPManager
{
    //key - uid 
    public Dictionary<int, SceneEntity> allShowHideEntity = new Dictionary<int, SceneEntity>();

    public Action<GameObject> afterSwitchClick { get; set; }

    public void ClearBevs()
    {
        allShowHideEntity.Clear();
    }

    public void OnHandleClone(SceneEntity sourceEntity, SceneEntity newEntity)
    {
        if (newEntity.HasComponent<ShowHideComponent>())
        {
            AddShowHideEntityToDict(newEntity);

            var showHideCmp = newEntity.Get<ShowHideComponent>();
            var newSwitchIds = showHideCmp.switchUids;
            var newUid = newEntity.Get<GameObjectComponent>().uid;

            if (newSwitchIds != null && newSwitchIds.Count > 0)
            {
                foreach (var suid in newSwitchIds)
                {
                    int sid = SwitchManager.Inst.GetSwitchSIDByUid(suid);
                    SwitchManager.Inst.AddControlledId(sid, newUid);
                }
            }
        }
    }

    //组合时重置默认显隐状态，以及重置被开关控制的关系
    public void OnCombineNode(SceneEntity entity)
    {
        if (entity != null && entity.HasComponent<ShowHideComponent>())
        {
            var uid = entity.Get<GameObjectComponent>().uid;
            if (allShowHideEntity.ContainsKey(uid))
            {
                allShowHideEntity.Remove(uid);
            }
            entity.Remove<ShowHideComponent>();
        }
    }

    public void AddShowHideEntityToDict(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<ShowHideComponent>())
        {
            LoggerUtils.Log("[AddShowHideEntityToDict] ShowHideComponent is exist");
            if (!allShowHideEntity.ContainsKey(uid))
            {
                allShowHideEntity.Add(uid, entity);
                LoggerUtils.Log("[AddShowHideEntityToDict] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddShowHideEntityToDict] ShowHideComponent not exist");
        }
    }

    public void OnRemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        int uid = goCmp.uid;

        if (allShowHideEntity.ContainsKey(uid))
        {
            allShowHideEntity.Remove(uid);
        }

        if (goCmp.modelType == NodeModelType.Switch)
        {
            foreach (var e in allShowHideEntity.Values)
            {
                var switchUids = e.Get<ShowHideComponent>().switchUids;
                if (switchUids.Contains(uid))
                {
                    switchUids.Remove(uid);
                }
            }
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        OnRemoveNode(behaviour);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        //恢复与关联开关的关系
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if(behaviour.entity.HasComponent<ShowHideComponent>())
        {
            AddShowHideEntityToDict(behaviour.entity);

            var showHideCmp = behaviour.entity.Get<ShowHideComponent>();
            var switchUids = showHideCmp.switchUids;
            //找到ShowHideComponent关联的开关，并将自己注册回去
            foreach(int switchId in switchUids)
            {
                var switchEntity = SceneBuilder.Inst.GetEntityByUid(switchId);
                if(switchEntity != null && switchEntity.HasComponent<SwitchButtonComponent>())
                {
                    var controllList = switchEntity.Get<SwitchButtonComponent>().controllUids;
                    if (!controllList.Contains(goCmp.uid))
                    {
                        controllList.Add(goCmp.uid);
                    }
                }
            }
        }   
    }

    public void Clear()
    {
        
    }

    public override void Release()
    {
        base.Release();
        afterSwitchClick = null;
    }

    //没有被开关控制，defaultShow = true时去除Component
    public void UpdateShowHideCmpInEntity(SceneEntity curEntity)
    {
        if (curEntity.HasComponent<ShowHideComponent>())
        {
            var switchUids = curEntity.Get<ShowHideComponent>().switchUids;
            if (switchUids == null || switchUids.Count <= 0)
            {
                if (curEntity.Get<ShowHideComponent>().defaultShow == 0)
                {
                    curEntity.Remove<ShowHideComponent>();
                    allShowHideEntity.Remove(curEntity.Get<GameObjectComponent>().uid);
                    LoggerUtils.Log("没有被开关控制且默认显示，移除ShowHideCmp");
                }
            }
        }
    }

    public void AddSwitchId(SceneEntity curEntity, int switchUid)
    {
        var switchUids = curEntity.Get<ShowHideComponent>().switchUids;
        if (!switchUids.Contains(switchUid))
        {
            switchUids.Add(switchUid);
            AddShowHideEntityToDict(curEntity);
            LoggerUtils.Log("[AddSwitchId] Entity switchIds :" + switchUid);
        }
        else
        {
            LoggerUtils.Log("[AddSwitchId] switchUid already exist");
        }
    }

    public void RemoveSwitchId(SceneEntity curEntity, int switchUid)
    {
        if (curEntity.HasComponent<ShowHideComponent>())
        {
            var switchUids = curEntity.Get<ShowHideComponent>().switchUids;
            if (switchUids.Contains(switchUid))
            {
                switchUids.Remove(switchUid);
                LoggerUtils.Log("[RemoveSwitchId]移除curEntity开关id=>" + switchUid);

                UpdateShowHideCmpInEntity(curEntity);
            }
            else
            {
                LoggerUtils.Log("[RemoveSwitchId] switchUid not found =>" + switchUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveSwitchId] ShowHideComponent not found");
        }
    }


    public void EnterPlayMode()
    {
        if (allShowHideEntity == null)
        {
            return;
        }

        foreach (var entity in allShowHideEntity.Values)
        {
            var goCmp = entity.Get<GameObjectComponent>();
            if (entity.Get<ShowHideComponent>().defaultShow == 1)
            {
                goCmp.bindGo.SetActive(false);
            }
            else
            {
                goCmp.bindGo.SetActive(true);
            }
        }

        BloodPropManager.Inst.SetDefaultModeShow(false);
        AttackWeaponManager.Inst.SetDefaultModeShow(false);
        ShootWeaponManager.Inst.SetDefaultModeShow(false);
        FreezePropsManager.Inst.SetDefaultModeShow(false);
    }

    public void EnterEditMode()
    {
        if (allShowHideEntity == null)
        {
            return;
        }
        foreach (var entity in allShowHideEntity.Values)
        {
            var goCmp = entity.Get<GameObjectComponent>();
            goCmp.bindGo.SetActive(true);
        }
    }

    public void OnReset()
    {
        EnterPlayMode();
    }

    public void OnSwitchClick(int uid)
    {
        if (allShowHideEntity.ContainsKey(uid))
        {
            var go = allShowHideEntity[uid].Get<GameObjectComponent>().bindGo;
            var gcomp = allShowHideEntity[uid].Get<GameObjectComponent>();
            var propStar = go.GetComponentInChildren<PropStarBehaviour>(true);
            if (propStar != null && !propStar.CheckCanClick())
            {
                //星星被收集之后 不会再被开关控制
                return;
            }

            var behv = BloodPropManager.Inst.GetBloodPropByUid(uid);
            if (behv != null && BloodPropManager.Inst.IsBloodPropUsed(behv))
            {
                // 回血道具被使用后，不会被控制
                return;
            }
            var freezeNode = FreezePropsManager.Inst.GetNodeByUid(uid);
            if (freezeNode != null && FreezePropsManager.Inst.IsPropUsed(freezeNode))
            {
                // 冻结道具被使用后，不会被控制
                return;
            }
            var fireworkBehv = go.GetComponentInChildren<FireworkBehaviour>(true);
            if (fireworkBehv != null)
            {
                //烟花道具未添加实体模型 不可被控制
                return;
            }

            behv = PickabilityManager.Inst.GetBaseBevByUid(uid);
            if (behv != null && AttackWeaponManager.Inst.IsAttackPropOutOfControl(behv))
            {
                return;
            }
                // 攻击道具被损毁后，不能被控制
            if (!PickabilityManager.Inst.IsCanBeControlled(gcomp.uid))
            {
                //道具被拾取之后 不会再被开关控制
                return;
            }
            if (!EdibilityManager.Inst.CheckCanBeControl(gcomp.uid))
            {
                //被吃完的道具不能被控制
                return;
            }

            ActiveData activeData = new ActiveData()
            {
                uid = uid,
                status = !go.activeSelf
            };
            go.SetActive(!go.activeSelf);
            SceneSystem.Inst.RestoreSystemState();

            afterSwitchClick?.Invoke(go);

        }
        else
        {
            LoggerUtils.Log("[OnSwitchClick] uid not found = " + uid);
        }
    }

}