using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:感应盒管理器，用以管理感应盒以及感应盒所控制物体的数据和表现
/// </summary> 

public class SensorBoxManager : ManagerInstance<SensorBoxManager>, IManager,IPVPManager
{
    //感应盒部分
    public Dictionary<int, NodeBaseBehaviour> sensorBoxDict = new Dictionary<int, NodeBaseBehaviour>();
    public int MaxCount = 999;
    public int CurrentNum;

    //被感应盒控制部分
    public Dictionary<int, SceneEntity> visibleCtrDict = new Dictionary<int, SceneEntity>();
    public Dictionary<int, SceneEntity> moveCtrDict = new Dictionary<int, SceneEntity>();
    public Dictionary<int, SceneEntity> soundCtrDict = new Dictionary<int, SceneEntity>();
    public Dictionary<int, SceneEntity> animCtrDict = new Dictionary<int, SceneEntity>();
    public Dictionary<int, SceneEntity> fireworkCtrDict = new Dictionary<int, SceneEntity>();

    public Action<GameObject> afterSwitchClick { get; set; }

    public void AddSensorBox(NodeBaseBehaviour behaviour)
    {
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        if (!sensorBoxDict.ContainsKey(uid))
        {
            SensorBoxBehaviour b = behaviour as SensorBoxBehaviour;
            sensorBoxDict.Add(uid, behaviour);
            LoggerUtils.Log("###AddSensorBox uid :" +uid+ "  SensorStatus"+ b.SensorStatus);
        }
    }

    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("###===========SensorBoxManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[###SensorBoxManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                if (getItemsRsp.items == null)
                {
                    LoggerUtils.Log("[###SensorBoxManager.OnGetItemsCallback]getItemsRsp.items is null");
                    return;
                }

                for (int i = 0; i < getItemsRsp.items.Length; i++)
                {
                    Item item = getItemsRsp.items[i];
                    if (item.type == (int)ItemType.SENSOR_BOX)
                    {
                        var uid = item.id;
                        if (sensorBoxDict.ContainsKey(uid))
                        {
                            SensorBoxBehaviour bev = sensorBoxDict[uid] as SensorBoxBehaviour;
                            SensorBoxProtoData sp = JsonConvert.DeserializeObject<SensorBoxProtoData>(item.data);
                            LoggerUtils.Log("#####本地感应盒状态："+bev.SensorStatus + "当前服务器状态：" +sp.status);
                            if (bev.SensorStatus != sp.status)
                            {
                                HandleSensorBoxTouch(bev);
                                bev.SensorStatus = sp.status;
                            }
                            bev.UsedTimes = sp.count;
                        }else{
                            LoggerUtils.Log("##OnGetItemsCallback sensorBox is not exist："+uid);
                        }
                    }
                }
            }
        }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("SensorBoxManager OnReceiveServer==>" + msg);

        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.SENSOR_BOX)
                {
                    var uid = item.id;
                    if (sensorBoxDict.ContainsKey(uid))
                    {
                        SensorBoxProtoData sp = JsonConvert.DeserializeObject<SensorBoxProtoData>(item.data);
                        SensorBoxBehaviour bev = sensorBoxDict[uid] as SensorBoxBehaviour;
                        if (sp.status == bev.SensorStatus)
                        {
                            LoggerUtils.Log("##SensorBox OnReceiveServer 出现两次一样的值");
                            return true;
                        }

                        LoggerUtils.Log("##SensorBoxManager OnReceiveServer Item 数据==>status:" + sp.status + "  times:" + sp.times + " count:" + sp.count);
                        bev.SensorStatus = sp.status;
                        HandleSensorBoxTouch(bev);
                    }
                    else
                    {
                        LoggerUtils.Log("[SensorBox OnReceiveServer] not find switch, uid=" + item.id);
                    }
                }
            }
        }
        return true;
    }

    public void HandleSensorBoxTouch(SensorBoxBehaviour bev)
    {
        var visibleCtrlUids = bev.entity.Get<SensorBoxComponent>().visibleCtrlUids;
        foreach (var uid in visibleCtrlUids)
        {
            OnHandleVisible(uid);
        }

        var moveCtrlUids = bev.entity.Get<SensorBoxComponent>().moveCtrlUids;
        foreach (var uid in moveCtrlUids)
        {
            OnHandleMove(uid);
        }

        var soundCtrlUids = bev.entity.Get<SensorBoxComponent>().soundCtrlUids;
        foreach (var uid in soundCtrlUids)
        {
            OnHandleSound(uid);
        }

        var animCtrlUids = bev.entity.Get<SensorBoxComponent>().animCtrlUids;
        foreach (var uid in animCtrlUids)
        {
            OnHandleAnim(uid);
        }
    }
    public void LocalHandeFireworkTouch(SensorBoxBehaviour bev)
    {
        var fireworkCtrlUids = bev.entity.Get<SensorBoxComponent>().fireworkCtrlUids;//当前感应盒控制的所有烟花
        foreach (var uid in fireworkCtrlUids)
        {
            OnHandleFirework(uid);
        }
    }
    public void OnHandleVisible(int uid)
    {
        if (visibleCtrDict.ContainsKey(uid))
        {
            var go = visibleCtrDict[uid].Get<GameObjectComponent>().bindGo;
            var gcomp = visibleCtrDict[uid].Get<GameObjectComponent>();
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
            if (freezeNode != null &&FreezePropsManager.Inst.IsPropUsed(freezeNode))
            {
                // 冻结道具被使用后，不会被控制
                return;
            }
            var fire = go.GetComponentInChildren<FireworkBehaviour>(true);
            if (fire != null)
            {
                //烟花道具未添加实体模型 不可被控制
                return;
            }
            behv = PickabilityManager.Inst.GetBaseBevByUid(uid);
            if (behv != null && AttackWeaponManager.Inst.IsAttackPropOutOfControl(behv))
            {
                // 攻击道具被损毁后，不能被控制
                return;
            }

            if (!PickabilityManager.Inst.IsCanBeControlled(gcomp.uid))
            {
                //道具被拾取之后 不会再被开关控制
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
            LoggerUtils.Log("[OnHandleVisible] uid not found = " + uid);
        }
    }

    public void OnHandleMove(int uid)
    {
        if (moveCtrDict.ContainsKey(uid))
        {
            var entity = moveCtrDict[uid];
            var go = entity.Get<GameObjectComponent>().bindGo;

            var propStar = go.GetComponentInChildren<PropStarBehaviour>(true);
            if (propStar != null && !propStar.CheckCanClick())
            {
                //星星被收集之后 不会再被开关控制
                return;
            }


            if (!go.activeSelf)
            {
                // 当物体不可见时，移动开关不生效
                return;
            }
            if (!entity.HasComponent<MovementComponent>())
            {
                return;
            }
            var moveCom = entity.Get<MovementComponent>();
            moveCom.tempMoveState = ((moveCom.tempMoveState == 0) ? 1 : 0);
            SceneSystem.Inst.ExcuteMoveSystem();
        }
        else
        {
            LoggerUtils.Log("[OnHandleMove] uid not found = " + uid);
        }
    }

    public void OnHandleSound(int uid)
    {
        if (soundCtrDict.ContainsKey(uid))
        {
            var entity = soundCtrDict[uid];
            var go = entity.Get<GameObjectComponent>().bindGo;

            var propStar = go.GetComponentInChildren<PropStarBehaviour>(true);
            if (propStar != null && !propStar.CheckCanClick())
            {
                //星星被收集之后 不会再被开关控制
                return;
            }


            if (!go.activeSelf)
            {
                // 当物体不可见时，移动开关不生效
                return;
            }

            if (!entity.HasComponent<SoundComponent>())
            {
                return;
            }

            var soundCom = entity.Get<SoundComponent>();
            // if (soundCom.isControl == (int)SoundControl.SUPPORT_CTRL_MUSIC)
            // {
            var soundBev = go.GetComponentInChildren<SoundButtonBehaviour>();
            // 本地播放音乐
            soundBev?.OnClickSound();
            // }
        }
        else
        {
            LoggerUtils.Log("[OnHandleSound] uid not found = " + uid);
        }
    }

    public void OnHandleAnim(int uid)
    {
        if (animCtrDict.ContainsKey(uid))
        {
            var entity = animCtrDict[uid];
            var go = entity.Get<GameObjectComponent>().bindGo;

            var propStar = go.GetComponentInChildren<PropStarBehaviour>(true);
            if (propStar != null && !propStar.CheckCanClick())
            {
                //星星被收集之后 不会再被开关控制
                return;
            }


            if (!go.activeSelf)
            {
                // 当物体不可见时，移动开关不生效
                return;
            }
            if (!entity.HasComponent<RPAnimComponent>())
            {
                return;
            }
            var animCom = entity.Get<RPAnimComponent>();
            animCom.tempAnimState = ((animCom.tempAnimState == 0) ? 1 : 0);
            SceneSystem.Inst.ExcuteUpDownSystem();
        }
        else
        {
            LoggerUtils.Log("[OnHandleAnim] uid not found = " + uid);
        }
    }
    public void OnHandleFirework(int uid)
    {
        if (fireworkCtrDict.ContainsKey(uid))
        {
            var entity = fireworkCtrDict[uid];
            var go = entity.Get<GameObjectComponent>().bindGo;
            if (!go.activeSelf)
            {
                // 当物体不可见时，移动开关不生效
                return;
            }
            if (entity.HasComponent<FireworkComponent>())
            {
                var fireworkBev = go.GetComponentInChildren<NodeBaseBehaviour>();
                FireworkManager.Inst.OnTriggerFirework(fireworkBev);
            }
        }
        else
        {
            LoggerUtils.Log("[OnHandleSound] uid not found = " + uid);
        }
    }

    public bool IsOverMaxCount()//最大开关数量
    {
        if (CurrentNum >= MaxCount)
        {
            return true;
        }
        return false;
    }

    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        if (newBev.entity.HasComponent<SensorBoxComponent>())
        {
            SensorBoxBehaviour sensorBoxBehaviour = newBev as SensorBoxBehaviour;
            sensorBoxBehaviour.RefreshIndex();
            AddSensorBox(newBev);
        }

        if (newBev.entity.HasComponent<SensorControlComponent>())
        {
            var newEntity = newBev.entity;
            var newUid = newEntity.Get<GameObjectComponent>().uid;
            var sComp = newBev.entity.Get<SensorControlComponent>();
            var visibleSensorUids = sComp.visibleSensorUids;
            var moveSensorUids = sComp.moveSensorUids;
            var soundSensorUids = sComp.soundSensorUids;
            var animSensorUids = sComp.animSensorUids;
            var fireworkSensorUids = sComp.fireworkSensorUids;


            if (visibleSensorUids != null && visibleSensorUids.Count > 0)
            {
                foreach (var boxUid in visibleSensorUids)
                {
                    AddControlledId(boxUid, newUid, (int)PropControlType.VISIBLE_CONTROL);
                }
                AddVisibleCtrEntity(newEntity);

            }

            if (moveSensorUids != null && moveSensorUids.Count > 0)
            {
                foreach (var boxUid in moveSensorUids)
                {
                    AddControlledId(boxUid, newUid, (int)PropControlType.MOVEMENT_CONTROL);
                }
                AddMoveCtrEntity(newEntity);
            }

            if (soundSensorUids != null && soundSensorUids.Count > 0)
            {
                foreach (var boxUid in soundSensorUids)
                {
                    AddControlledId(boxUid, newUid, (int)PropControlType.SOUNDPLAY_CONTROL);
                }
                AddSoundCtrEntity(newEntity);
            }

            if (animSensorUids != null && animSensorUids.Count > 0)
            {
                foreach (var boxUid in animSensorUids)
                {
                    AddControlledId(boxUid, newUid, (int)PropControlType.ANIMATION_CONTROL);
                }
                AddAnimCtrEntity(newEntity);
            }

            if (fireworkSensorUids != null && fireworkSensorUids.Count > 0)
            {
                foreach (var boxUid in fireworkSensorUids)
                {
                    AddControlledId(boxUid, newUid, (int)PropControlType.FIREWORK_CONTROL);
                }
                AddFireworkCtrEntity(newEntity);
            }
        }
    }

    public bool IsCanClone(int count)
    {
        if (CurrentNum + count > MaxCount)
        {
            return false;
        }
        return true;
    }

    public int GetNewIndex()
    {
        return ++CurrentNum;
    }

    public void UpdateMaxIndex(int index)
    {
        if (index > CurrentNum)
        {
            CurrentNum = index;
        }
    }

    //关联物体的移动属性 到 感应盒
    public void BindEntityToMove(SceneEntity curEntity, int boxUid)
    {
        var moveSensorUids = curEntity.Get<SensorControlComponent>().moveSensorUids;
        if (!moveSensorUids.Contains(boxUid))
        {
            moveSensorUids.Add(boxUid);
            AddMoveCtrEntity(curEntity);
            LoggerUtils.Log("[BindEntityToMove] Entity boxUid :" + boxUid);
        }
        else
        {
            LoggerUtils.Log("[BindEntityToMove] boxUid already exist");
        }
    }

    public void BindEntityToVisible(SceneEntity curEntity, int boxUid)
    {
        var visibleSensorUids = curEntity.Get<SensorControlComponent>().visibleSensorUids;
        if (!visibleSensorUids.Contains(boxUid))
        {
            visibleSensorUids.Add(boxUid);
            AddVisibleCtrEntity(curEntity);
            LoggerUtils.Log("[BindEntityToVisible] Entity boxUid :" + boxUid);
        }
        else
        {
            LoggerUtils.Log("[BindEntityToVisible] boxUid already exist");
        }
    }

    public void BindEntityToSound(SceneEntity curEntity, int boxUid)
    {
        var soundSensorUids = curEntity.Get<SensorControlComponent>().soundSensorUids;
        if (!soundSensorUids.Contains(boxUid))
        {
            soundSensorUids.Add(boxUid);
            AddSoundCtrEntity(curEntity);
            LoggerUtils.Log("[BindEntityToSound] Entity boxUid :" + boxUid);
        }
        else
        {
            LoggerUtils.Log("[BindEntityToSound] boxUid already exist");
        }
    }

    public void BindEntityToAnim(SceneEntity curEntity, int boxUid)
    {
        var animSensorUids = curEntity.Get<SensorControlComponent>().animSensorUids;
        if (!animSensorUids.Contains(boxUid))
        {
            animSensorUids.Add(boxUid);
            AddAnimCtrEntity(curEntity);
            LoggerUtils.Log("[BindEntityToAnim] Entity boxUid :" + boxUid);
        }
        else
        {
            LoggerUtils.Log("[BindEntityToAnim] boxUid already exist");
        }
    }
    public void BindEntityToFirework(SceneEntity curEntity, int boxUid)
    {
        var fireworkSensorUids = curEntity.Get<SensorControlComponent>().fireworkSensorUids;
        if (!fireworkSensorUids.Contains(boxUid))
        {
            fireworkSensorUids.Add(boxUid);
            AddFireworkCtrEntity(curEntity);
            LoggerUtils.Log("[BindEntityToAnim] Entity boxUid :" + boxUid);
        }
        else
        {
            LoggerUtils.Log("[BindEntityToAnim] boxUid already exist");
        }
    }



    public void UnBindEntityToMove(SceneEntity curEntity, int boxUid)
    {
        if (!curEntity.HasComponent<SensorControlComponent>())
        {
            LoggerUtils.Log("[UnBindEntityToMove] SensorControlComponent not found");
            return;
        }

        var moveSensorUids = curEntity.Get<SensorControlComponent>().moveSensorUids;
        if (moveSensorUids.Contains(boxUid))
        {
            moveSensorUids.Remove(boxUid);
            CheckRemoveSensorControll(curEntity);
        }

        var compUid = curEntity.Get<GameObjectComponent>().uid;
        if (moveSensorUids.Count <= 0 && moveCtrDict.ContainsKey(compUid))
        {
            moveCtrDict.Remove(compUid);
            LoggerUtils.Log("[UnBindEntityToMove] moveCtrDict remove to dict ,uid=>" + compUid);
        }
    }

    public void UnBindEntityToVisible(SceneEntity curEntity, int boxUid)
    {
        if (!curEntity.HasComponent<SensorControlComponent>())
        {
            LoggerUtils.Log("[UnBindEntityToVisible] SensorControlComponent not found");
            return;
        }

        var visibleSensorUids = curEntity.Get<SensorControlComponent>().visibleSensorUids;
        if (visibleSensorUids.Contains(boxUid))
        {
            visibleSensorUids.Remove(boxUid);
            CheckRemoveSensorControll(curEntity);
        }

        var compUid = curEntity.Get<GameObjectComponent>().uid;
        if (visibleSensorUids.Count <= 0 && visibleCtrDict.ContainsKey(compUid))
        {
            visibleCtrDict.Remove(compUid);
            LoggerUtils.Log("[UnBindEntityToVisible] Remove to dict ,uid=>" + compUid);
        }
    }

    public void UnBindEntityToSound(SceneEntity curEntity, int boxUid)
    {
        if (!curEntity.HasComponent<SensorControlComponent>())
        {
            LoggerUtils.Log("[UnBindEntityToSound] SensorControlComponent not found");
            return;
        }

        var soundSensorUids = curEntity.Get<SensorControlComponent>().soundSensorUids;
        if (soundSensorUids.Contains(boxUid))
        {
            soundSensorUids.Remove(boxUid);
            CheckRemoveSensorControll(curEntity);
        }

        var compUid = curEntity.Get<GameObjectComponent>().uid;
        if (soundSensorUids.Count <= 0 && soundCtrDict.ContainsKey(compUid))
        {
            soundCtrDict.Remove(compUid);
            LoggerUtils.Log("[UnBindEntityToSound] Remove to dict ,uid=>" + compUid);
        }
    }

    public void UnBindEntityToAnim(SceneEntity curEntity, int boxUid)
    {
        if (!curEntity.HasComponent<SensorControlComponent>())
        {
            LoggerUtils.Log("[UnBindEntityToAnim] SensorControlComponent not found");
            return;
        }

        var animSensorUids = curEntity.Get<SensorControlComponent>().animSensorUids;
        if (animSensorUids.Contains(boxUid))
        {
            animSensorUids.Remove(boxUid);
            CheckRemoveSensorControll(curEntity);
        }

        var compUid = curEntity.Get<GameObjectComponent>().uid;
        if (animSensorUids.Count <= 0 && animCtrDict.ContainsKey(compUid))
        {
            animCtrDict.Remove(compUid);
            LoggerUtils.Log("[UnBindEntityToAnim] Remove to dict ,uid=>" + compUid);
        }
    }
    public void UnBindEntityToFirework(SceneEntity curEntity, int boxUid)
    {
        if (!curEntity.HasComponent<SensorControlComponent>())
        {
            LoggerUtils.Log("[UnBindEntityToAnim] SensorControlComponent not found");
            return;
        }

        var fireworkSensorUids = curEntity.Get<SensorControlComponent>().fireworkSensorUids;
        if (fireworkSensorUids.Contains(boxUid))
        {
            fireworkSensorUids.Remove(boxUid);
            CheckRemoveSensorControll(curEntity);
        }

        var compUid = curEntity.Get<GameObjectComponent>().uid;
        if (fireworkSensorUids.Count <= 0 && fireworkCtrDict.ContainsKey(compUid))
        {
            fireworkCtrDict.Remove(compUid);
            LoggerUtils.Log("[UnBindEntityToAnim] Remove to dict ,uid=>" + compUid);
        }
    }

    //没有被感应盒控制时,去除 SensorControlComponent
    public void CheckRemoveSensorControll(SceneEntity curEntity)
    {
        if (curEntity.HasComponent<SensorControlComponent>())
        {
            var moveSensorUids = curEntity.Get<SensorControlComponent>().moveSensorUids;
            var visibleSensorUids = curEntity.Get<SensorControlComponent>().visibleSensorUids;
            var soundSensorUids = curEntity.Get<SensorControlComponent>().soundSensorUids;
            var animSensorUids = curEntity.Get<SensorControlComponent>().animSensorUids;
            var fireworkSensorUids = curEntity.Get<SensorControlComponent>().fireworkSensorUids;


            if (moveSensorUids.Count <= 0 && visibleSensorUids.Count <= 0
            && soundSensorUids.Count <= 0 && animSensorUids.Count <= 0 && fireworkSensorUids.Count <= 0)
            {
                curEntity.Remove<SensorControlComponent>();
                LoggerUtils.Log("not controll by SensorBox,remove SensorControlComponent");
            }
        }
    }


    public void AddMoveCtrEntity(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<SensorControlComponent>())
        {
            if (!moveCtrDict.ContainsKey(uid))
            {
                moveCtrDict.Add(uid, entity);
                LoggerUtils.Log("[AddMoveCtrEntity] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddMoveCtrEntity] SensorControlComponent not exist");
        }
    }

    public void AddVisibleCtrEntity(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<SensorControlComponent>())
        {
            if (!visibleCtrDict.ContainsKey(uid))
            {
                visibleCtrDict.Add(uid, entity);
                LoggerUtils.Log("[AddVisibleCtrEntity] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddVisibleCtrEntity] SensorControlComponent not exist");
        }
    }

    public void AddSoundCtrEntity(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<SensorControlComponent>())
        {
            if (!soundCtrDict.ContainsKey(uid))
            {
                soundCtrDict.Add(uid, entity);
                LoggerUtils.Log("[AddSoundCtrEntity] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddSoundCtrEntity] SensorControlComponent not exist");
        }
    }

    public void AddAnimCtrEntity(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<SensorControlComponent>())
        {
            if (!animCtrDict.ContainsKey(uid))
            {
                animCtrDict.Add(uid, entity);
                LoggerUtils.Log("[AddAnimCtrEntity] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddAnimCtrEntity] SensorControlComponent not exist");
        }
    }
    public void AddFireworkCtrEntity(SceneEntity entity)
    {
        int uid = entity.Get<GameObjectComponent>().uid;
        if (entity.HasComponent<SensorControlComponent>())
        {
            if (!fireworkCtrDict.ContainsKey(uid))
            {
                fireworkCtrDict.Add(uid, entity);
                LoggerUtils.Log("[AddFireworkCtrEntity] add to dict ,uid=>" + uid);
            }
        }
        else
        {
            LoggerUtils.Log("[AddFireworkCtrEntity] SensorControlComponent not exist");
        }
    }

    public void AddControlledId(int boxUid, int controllUid, int type = (int)PropControlType.VISIBLE_CONTROL)
    {
        if (sensorBoxDict.ContainsKey(boxUid))
        {
            var cmp = sensorBoxDict[boxUid].entity.Get<SensorBoxComponent>();
            var ctrUids = cmp.visibleCtrlUids;

            switch (type)
            {
                case (int)PropControlType.MOVEMENT_CONTROL:
                    ctrUids = cmp.moveCtrlUids;
                    break;
                case (int)PropControlType.SOUNDPLAY_CONTROL:
                    ctrUids = cmp.soundCtrlUids;
                    break;
                case (int)PropControlType.ANIMATION_CONTROL:
                    ctrUids = cmp.animCtrlUids;
                    break;
                case (int)PropControlType.FIREWORK_CONTROL:
                    ctrUids = cmp.fireworkCtrlUids;
                    break;
            }

            if (!ctrUids.Contains(controllUid))
            {
                ctrUids.Add(controllUid);
                LoggerUtils.Log("[AddControlledId] ctrUids added:" + LogList(ctrUids));
            }
            else
            {
                LoggerUtils.Log("[AddControlledId] controllUid already exist");
            }
        }
        else
        {
            LoggerUtils.Log("[AddControlledId] sid not found =>" + boxUid);
        }
    }

    public void RemoveControlledId(int uid, int controllUid, int type = (int)PropControlType.VISIBLE_CONTROL)
    {
        if (sensorBoxDict.ContainsKey(uid))
        {
            var cmp = sensorBoxDict[uid].entity.Get<SensorBoxComponent>();
            var ctrUids = cmp.visibleCtrlUids;

            switch (type)
            {
                case (int)PropControlType.MOVEMENT_CONTROL:
                    ctrUids = cmp.moveCtrlUids;
                    break;
                case (int)PropControlType.SOUNDPLAY_CONTROL:
                    ctrUids = cmp.soundCtrlUids;
                    break;
                case (int)PropControlType.ANIMATION_CONTROL:
                    ctrUids = cmp.animCtrlUids;
                    break;
                case (int)PropControlType.FIREWORK_CONTROL:
                    ctrUids = cmp.fireworkCtrlUids;
                    break;
            }

            if (ctrUids.Contains(controllUid))
            {
                ctrUids.Remove(controllUid);
                LoggerUtils.Log("[RemoveControlledId] ctrUids removed:" + LogList(ctrUids));
            }
            else
            {
                LoggerUtils.Log("[RemoveControlledId] controllUid not found =>" + controllUid);
            }
        }
        else
        {
            LoggerUtils.Log("[RemoveControlledId] sid not found =>" + uid);
        }
    }

    public int GetBoxUidByIndex(int index)
    {
        foreach (var uid in sensorBoxDict.Keys)
        {
            var b = sensorBoxDict[uid];
            if (index == b.entity.Get<SensorBoxComponent>().boxIndex)
            {
                return uid;
            }
        }
        return 0;
    }

    public int GetBoxIndexByUid(int uid)
    {
        if (sensorBoxDict.ContainsKey(uid))
        {
            var b = sensorBoxDict[uid];
            int index = b.entity.Get<SensorBoxComponent>().boxIndex;
            return index;
        }
        return 0;
    }

    public Dictionary<int, string> GetIndexDict()
    {
        Dictionary<int, string> tempDic = new Dictionary<int, string>();
        foreach (var behaviour in sensorBoxDict.Values)
        {
            int index = behaviour.entity.Get<SensorBoxComponent>().boxIndex;
            if (!tempDic.ContainsKey(index))
            {
                tempDic.Add(index, index.ToString());
            }
        }
        return tempDic;
    }

    public List<int> GetIndexList()
    {
        List<int> tempList = new List<int>();
        foreach (var behaviour in sensorBoxDict.Values)
        {
            int index = behaviour.entity.Get<SensorBoxComponent>().boxIndex;
            if (!tempList.Contains(index))
            {
                tempList.Add(index);
            }
        }
        tempList.Sort();
        return tempList;
    }

    private string LogList(List<int> list)
    {
        string s = "";

        for (int i = 0; i < list.Count; i++)
        {
            s = s + list[i] + ", ";
        }

        return s;
    }

    public void EnterPlayMode()
    {

    }

    public void OnReset()
    {
        EnterEditMode();
    }

    public void EnterEditMode()
    {
        if (visibleCtrDict != null && visibleCtrDict.Count > 0)
        {
            foreach (var entity in visibleCtrDict.Values)
            {
                entity.Get<GameObjectComponent>().bindGo.SetActive(true);
            }
        }

        foreach (var behaviour in sensorBoxDict.Values)
        {
            SensorBoxBehaviour boxBev = behaviour as SensorBoxBehaviour;
            boxBev.SensorStatus = 0;
            boxBev.UsedTimes = 0;
        }

    }

    
    
    private void RemoveControlledEntity(SceneEntity entity)
    {
        if (!entity.HasComponent<SensorControlComponent>()) return;
        GameObjectComponent goCmp = entity.Get<GameObjectComponent>();
        foreach (var sensorUid in sensorBoxDict.Keys)
        {
            var behav = sensorBoxDict[sensorUid];
            var sensorBoxComp = behav.entity.Get<SensorBoxComponent>();
            var visibleCtrlUids = sensorBoxComp.visibleCtrlUids;
            if (visibleCtrlUids.Contains(goCmp.uid))
            {
                visibleCtrlUids.Remove(goCmp.uid);
            }

            var moveCtrlUids = sensorBoxComp.moveCtrlUids;
            if (moveCtrlUids.Contains(goCmp.uid))
            {
                moveCtrlUids.Remove(goCmp.uid);
            }

            var soundCtrlUids = sensorBoxComp.soundCtrlUids;
            if (soundCtrlUids.Contains(goCmp.uid))
            {
                soundCtrlUids.Remove(goCmp.uid);
            }

            var animCtrlUids = sensorBoxComp.animCtrlUids;
            if (animCtrlUids.Contains(goCmp.uid))
            {
                animCtrlUids.Remove(goCmp.uid);
            }
            var firworkCtrlUids = sensorBoxComp.fireworkCtrlUids;
            if (firworkCtrlUids.Contains(goCmp.uid))
            {
                firworkCtrlUids.Remove(goCmp.uid);
            }
        }

        if (visibleCtrDict.ContainsKey(goCmp.uid))
        {
            visibleCtrDict.Remove(goCmp.uid);
        }
        if (moveCtrDict.ContainsKey(goCmp.uid))
        {
            moveCtrDict.Remove(goCmp.uid);
        }
        if (soundCtrDict.ContainsKey(goCmp.uid))
        {
            soundCtrDict.Remove(goCmp.uid);
        }
        if (animCtrDict.ContainsKey(goCmp.uid))
        {
            animCtrDict.Remove(goCmp.uid);
        }
        if (fireworkCtrDict.ContainsKey(goCmp.uid))
        {
            fireworkCtrDict.Remove(goCmp.uid);
        }

    }
    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        LoggerUtils.Log("SensorBoxManager OnRemoveNode");
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.SensorBox)
        {
            int boxUid = goCmp.uid;
            sensorBoxDict.Remove(boxUid);

            var sensorBoxComp = behaviour.entity.Get<SensorBoxComponent>();

            foreach (var uid in sensorBoxComp.visibleCtrlUids)
            {
                if (visibleCtrDict.ContainsKey(uid))
                {
                    var entity = visibleCtrDict[uid];
                    UnBindEntityToVisible(entity, boxUid);
                }
            }

            foreach (var uid in sensorBoxComp.moveCtrlUids)
            {
                if (moveCtrDict.ContainsKey(uid))
                {
                    var entity = moveCtrDict[uid];
                    UnBindEntityToMove(entity, boxUid);
                }
            }

            foreach (var uid in sensorBoxComp.soundCtrlUids)
            {
                if (soundCtrDict.ContainsKey(uid))
                {
                    var entity = soundCtrDict[uid];
                    UnBindEntityToSound(entity, boxUid);
                }
            }

            foreach (var uid in sensorBoxComp.animCtrlUids)
            {
                if (animCtrDict.ContainsKey(uid))
                {
                    var entity = animCtrDict[uid];
                    UnBindEntityToAnim(entity, boxUid);
                }
            }
            foreach (var uid in sensorBoxComp.fireworkCtrlUids)
            {
                if (fireworkCtrDict.ContainsKey(uid))
                {
                    var entity = fireworkCtrDict[uid];
                    UnBindEntityToFirework(entity, boxUid);
                }
            }
        }

        if (behaviour.entity.HasComponent<SensorControlComponent>())
        {
            RemoveControlledEntity(behaviour.entity);
        }

    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        //恢复开关关联的物体的开关属性
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.SensorBox)
        {
            AddSensorBox(behaviour);

            SensorBoxBehaviour b = behaviour as SensorBoxBehaviour;
            b.RefreshIndex();

            if (behaviour.entity.HasComponent<SensorBoxComponent>())
            {
                var sensorBoxComp = behaviour.entity.Get<SensorBoxComponent>();
                int boxUid = goCmp.uid;
                var visibleCtrlUids = sensorBoxComp.visibleCtrlUids;
                foreach (int uid in visibleCtrlUids)
                {
                    var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                    if (entity != null)
                    {
                        SensorBoxManager.Inst.BindEntityToVisible(entity, boxUid);
                    }
                }

                var moveCtrlUids = sensorBoxComp.moveCtrlUids;
                foreach (int uid in moveCtrlUids)
                {
                    var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                    if (entity != null)
                    {
                        SensorBoxManager.Inst.BindEntityToMove(entity, boxUid);
                    }
                }

                var soundCtrlUids = sensorBoxComp.soundCtrlUids;
                foreach (int uid in soundCtrlUids)
                {
                    var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                    if (entity != null)
                    {
                        SensorBoxManager.Inst.BindEntityToSound(entity, boxUid);
                    }
                }

                var animCtrlUids = sensorBoxComp.animCtrlUids;
                foreach (int uid in animCtrlUids)
                {
                    var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                    if (entity != null)
                    {
                        SensorBoxManager.Inst.BindEntityToAnim(entity, boxUid);
                    }
                }
                var fireworkCtrlUids = sensorBoxComp.fireworkCtrlUids;
                foreach (int uid in fireworkCtrlUids)
                {
                    var entity = SceneBuilder.Inst.GetEntityByUid(uid);
                    if (entity != null)
                    {
                        SensorBoxManager.Inst.BindEntityToFirework(entity, boxUid);
                    }
                }
            }
        }

        if (behaviour.entity.HasComponent<SensorControlComponent>())
        {
            var sensorCtrEntity =  behaviour.entity;
            var sensorCtrComp = behaviour.entity.Get<SensorControlComponent>();
            var visibleSensorUids = sensorCtrComp.visibleSensorUids;
            var moveSensorUids = sensorCtrComp.moveSensorUids;
            var soundSensorUids = sensorCtrComp.soundSensorUids;
            var animSensorUids = sensorCtrComp.animSensorUids;
            var fireworkSensorUids = sensorCtrComp.fireworkSensorUids;

            //找到SensorControlComponent关联的感应盒，并将自己注册回去
            foreach (int boxUid in visibleSensorUids)
            {
                var boxEntity = SceneBuilder.Inst.GetEntityByUid(boxUid);
                if (boxEntity != null && boxEntity.HasComponent<SensorBoxComponent>())
                {
                    AddControlledId(boxUid,goCmp.uid,(int)PropControlType.VISIBLE_CONTROL);
                    AddVisibleCtrEntity(sensorCtrEntity);
                }
            }

            foreach (int boxUid in moveSensorUids)
            {
                var boxEntity = SceneBuilder.Inst.GetEntityByUid(boxUid);
                if (boxEntity != null && boxEntity.HasComponent<SensorBoxComponent>())
                {
                    AddControlledId(boxUid,goCmp.uid,(int)PropControlType.MOVEMENT_CONTROL);
                    AddMoveCtrEntity(sensorCtrEntity);
                }
            }

            foreach (int boxUid in soundSensorUids)
            {
                var boxEntity = SceneBuilder.Inst.GetEntityByUid(boxUid);
                if (boxEntity != null && boxEntity.HasComponent<SensorBoxComponent>())
                {
                    AddControlledId(boxUid,goCmp.uid,(int)PropControlType.SOUNDPLAY_CONTROL);
                    AddSoundCtrEntity(sensorCtrEntity);
                }
            }

            foreach (int boxUid in animSensorUids)
            {
                var boxEntity = SceneBuilder.Inst.GetEntityByUid(boxUid);
                if (boxEntity != null && boxEntity.HasComponent<SensorBoxComponent>())
                {
                    AddControlledId(boxUid, goCmp.uid, (int)PropControlType.ANIMATION_CONTROL);
                    AddAnimCtrEntity(sensorCtrEntity);
                }
            }
            foreach (int boxUid in fireworkSensorUids)
            {
                var boxEntity = SceneBuilder.Inst.GetEntityByUid(boxUid);
                if (boxEntity != null && boxEntity.HasComponent<SensorBoxComponent>())
                {
                    AddControlledId(boxUid, goCmp.uid, (int)PropControlType.FIREWORK_CONTROL);
                    AddFireworkCtrEntity(sensorCtrEntity);
                }
            }
        }
    }

    public void OnCombineNode(SceneEntity entity)
    {
        if (entity == null) return;
        //找出所有与该组合节点有关的感应盒，清除对应的数据
        if (entity.HasComponent<SensorControlComponent>())
        {
            RemoveControlledEntity(entity);
            entity.Remove<SensorControlComponent>();
        }
    }

    public void Clear()
    {
        LoggerUtils.Log("SensorBoxManager Clear");
        if (sensorBoxDict != null)
        {
            sensorBoxDict.Clear();
        }

        if(visibleCtrDict !=null)
        {
            visibleCtrDict.Clear();
        }

        if(moveCtrDict !=null)
        {
            moveCtrDict.Clear();
        }

        if(soundCtrDict!=null)
        {
            soundCtrDict.Clear();
        }

        if (animCtrDict != null)
        {
            animCtrDict.Clear();
        }
    }

    public override void Release()
    {
        base.Release();
        afterSwitchClick = null;
    }


}