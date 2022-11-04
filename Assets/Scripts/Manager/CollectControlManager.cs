/*
* @Author: YangJie
 * @LastEditors: wenjia
* @Description:道具收集控制管理类
* @Date: ${YEAR}-${MONTH}-${DAY} ${TIME}
* @Modify:
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public class CollectControlManager : CInstance<CollectControlManager>,IPVPManager
{
    //key - uid 

    private List<PropStarBehaviour> propStarBehaviours = new List<PropStarBehaviour>();

    private List<NodeBaseBehaviour> allCollectControlNodes = new List<NodeBaseBehaviour>() ;

    private Dictionary<string, List<int>> mapCollectStarEntities = new Dictionary<string, List<int>>();
    private List<CollectControlObj> collectControlObjs = new List<CollectControlObj>();

    private List<string> collectedUsers = new List<string>();
    
    private int collectEntityCount = 0;
    private int sendRetryCount = 0;
    private Coroutine timeOutCoroutine;

    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnModelChange);
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnModelChange);
    }

    public void ClearBevs()
    {
        propStarBehaviours.Clear();
        allCollectControlNodes?.Clear();
    }

    public void ClearRoomData()
    {
        mapCollectStarEntities.Clear();
        collectControlObjs.Clear();
    }


    void OnModelChange(GameMode mode)
    {
        LoggerUtils.Log("OnModelChange:" + mode);
        if (mode == GameMode.Play || mode == GameMode.Guest)
        {
            EnterPlayMode();
            foreach (var tmpPropStar in propStarBehaviours)
            {
                tmpPropStar.OnChangeMode(mode);
            }
        }
        else
        {
            foreach (var tmpPropStar in propStarBehaviours)
            {
                tmpPropStar.OnChangeMode(mode);
            }
            EnterEditMode();

        }
         

    }

    /// <summary>
    /// 暂时采用先进编辑模式再试玩来重置数据，代码顺序不调整
    /// </summary>
    public void OnReset()
    {
        foreach (var tmpPropStar in propStarBehaviours)
        {
            tmpPropStar.EndPropStar();
        }
        EnterEditMode();

        EnterPlayMode();
        foreach (var tmpPropStar in propStarBehaviours)
        {
            tmpPropStar.StartPropStar();
        }
    }


    public void OnGetItemsCallBack(string data)
    {
        LoggerUtils.Log("OnGetItems:" + data);
        var getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(data);

        CoroutineManager.Inst.StartCoroutine(getItemsRsp != null
            ? OnSyncDefaultRemoteRoomAttrs(getItemsRsp.roomAttrs)
            : OnSyncDefaultRemoteRoomAttrs(null));
    }

    /// <summary>
    /// 收到服务器下发 更新房间内物体控制信息
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool OnReceivedRoomAttr(string playerId, string data)
    {
        LoggerUtils.Log("OnReceivedRoomAttr:" + playerId + "," + data);
        var roomAttrReq = JsonConvert.DeserializeObject<SendRoomAttrReq>(data);
        var roomAttrs = roomAttrReq.roomAttrs;
        if (roomAttrs == null)
        {
            return true;
        }

        foreach (var roomAttr in roomAttrs)
        {
            if (roomAttr.type != (int) RoomAttrType.COLLECT_ENTITY) continue;
            if (playerId != Player.Id)
            {
                //var syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playerId);
                //var ugcUserInfo = GameManager.Inst.ugcUserInfo;
                //var nickName = syncPlayerInfo != null ? syncPlayerInfo.userName : ugcUserInfo != null ? ugcUserInfo.userName : Player.Name;
                //RoomChatPanel.Instance.SetRecChat(RecChatType.TextChat, nickName, "has collected all the collectibles!");
                SyncCollectRoomAttr(roomAttr);
            }

            if (!collectedUsers.Contains(playerId))
            {
                collectedUsers.Add(playerId); 
            }
   
        }

        return true;
    }
    
    /// <summary>
    /// 发生地图切换之后同步房间内受收集道具影响的数据
    /// </summary>
    IEnumerator OnSyncDefaultRemoteRoomAttrs(RoomAttr[] roomAttrs)
    {
        // 等待其他控制属性初始化完成
        yield return new WaitForSeconds(0.1f);
        if (roomAttrs == null)
        {
            yield break;
        }

        foreach (var roomAttr in roomAttrs)
        {
            if (roomAttr.type == (int) RoomAttrType.COLLECT_ENTITY)
            {
                LoggerUtils.Log("OnSyncDefaultRemoteRoomAttrs:");
                //var roomAttrCollectControl = JsonConvert.DeserializeObject<RoomAttrCollectControl>(roomAttr.data);
                //foreach (var collectedUser in roomAttrCollectControl.collectedUsers)
                //{
                //    var syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(collectedUser);
                //    if (syncPlayerInfo != null)
                //    {
                //        RoomChatPanel.Instance.SetRecChat(RecChatType.TextChat, syncPlayerInfo.userName, "has collected all the collectibles!");
                //    }
                //}

                SyncCollectRoomAttr(roomAttr);

                break;
            }
        }
    }


    private void SyncCollectRoomAttr(RoomAttr roomAttr)
    {
        var roomAttrCollectControl = JsonConvert.DeserializeObject<RoomAttrCollectControl>(roomAttr.data);
        var mapId = GlobalFieldController.CurMapInfo != null
            ? GlobalFieldController.CurMapInfo.mapId
            : "1465952717637517312_1638345577_1";

        foreach (var collectControlObj in roomAttrCollectControl.collectControlObjs)
        {
            var tmpCollectObj = collectControlObjs.Find(tmp =>
                tmp.mapId == collectControlObj.mapId && tmp.uid == collectControlObj.uid);


            if (collectControlObj.mapId == mapId)
            {
                var tmpNodeBehaviour = allCollectControlNodes.FirstOrDefault(tmp =>
                    tmp.entity.Get<GameObjectComponent>().uid == collectControlObj.uid);
                if (tmpNodeBehaviour != null)
                {
                    if (tmpCollectObj == null || tmpCollectObj.triggerCount != collectControlObj.triggerCount )
                    {
                        if (collectControlObj.triggerCount > 0 && tmpNodeBehaviour.entity.Get<CollectControlComponent>().triggerCount == 0)
                        {
                            if (tmpNodeBehaviour.entity.Get<CollectControlComponent>().isControl == 1)
                            {
                                var goComponent = tmpNodeBehaviour.entity.Get<GameObjectComponent>();

                                if (BloodPropManager.Inst.IsBloodPropUsed(tmpNodeBehaviour))
                                {
                                    // 回血道具被使用后，不会被控制
                                    return;
                                }
                                if (FreezePropsManager.Inst.IsPropUsed(tmpNodeBehaviour))
                                {
                                    // 冻结道具被使用后，不会被控制
                                    return;
                                }
                                var fire = goComponent.bindGo.GetComponentInChildren<FireworkBehaviour>(true);
                                if (fire != null)
                                {
                                    //烟花道具未添加实体模型 不可被控制
                                    return; ;
                                }
                                if (AttackWeaponManager.Inst.IsAttackPropOutOfControl(tmpNodeBehaviour))
                                {
                                    // 攻击道具被损毁后，不能被控制
                                    return;
                                }
                                if (!PickabilityManager.Inst.IsCanBeControlled(goComponent.uid))
                                {
                                    //道具被拾取之后 不会再被开关控制
                                    return;
                                }
                                goComponent.bindGo.SetActive(!goComponent.bindGo.activeSelf);
                                SceneSystem.Inst.RestoreSystemState();
                            }

                            var go = tmpNodeBehaviour.entity.Get<GameObjectComponent>().bindGo;

                            if (tmpNodeBehaviour.entity.Get<CollectControlComponent>().moveActive)
                            {
                                if (go.activeSelf && tmpNodeBehaviour.entity.HasComponent<MovementComponent>())
                                {
                                    var moveCom = tmpNodeBehaviour.entity.Get<MovementComponent>();
                                    moveCom.tempMoveState = ((moveCom.tempMoveState == 0) ? 1 : 0);
                                    SceneSystem.Inst.ExcuteMoveSystem();
                                }
                            }

                            if (tmpNodeBehaviour.entity.Get<CollectControlComponent>().animActive == (int)AnimControl.SUPPORT_CTRL_ANIM)
                            {
                                if (go.activeSelf && tmpNodeBehaviour.entity.HasComponent<RPAnimComponent>())
                                {
                                    var animCom = tmpNodeBehaviour.entity.Get<RPAnimComponent>();
                                    animCom.tempAnimState = ((animCom.tempAnimState == 0) ? 1 : 0);
                                    SceneSystem.Inst.ExcuteUpDownSystem();
                                }
                            }

                            if (tmpNodeBehaviour.entity.Get<CollectControlComponent>().playSound == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                            {
                                if (go.activeSelf && tmpNodeBehaviour.entity.HasComponent<GameObjectComponent>())
                                {
                                    var soundCom = tmpNodeBehaviour.entity.Get<SoundComponent>();
                                    // if (soundCom.isControl == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                                    // {
                                    var soundBev = go.GetComponentInChildren<SoundButtonBehaviour>();
                                    // 本地播放音乐
                                    soundBev.OnClickSound();
                                    // }
                                }
                            }
                        }

                        tmpNodeBehaviour.entity.Get<CollectControlComponent>().triggerCount =
                            collectControlObj.triggerCount;
                    }
                }
            }
            
            if (tmpCollectObj != null)
            {
                tmpCollectObj.triggerCount = collectControlObj.triggerCount;
            }
            else
            {
                collectControlObjs.Add(collectControlObj);
            }

            
  
        }
    }

    private void LocalCollectStarProp()
    {
        if (collectEntityCount < propStarBehaviours.Count) return;
        if (!collectedUsers.Contains(Player.Id))
        {
            collectedUsers.Add(Player.Id);
        }
        foreach (var tmpEntity in allCollectControlNodes)
        {
            SetEntityCollected(tmpEntity.entity);
        }
        //改为不广播
        //try
        //{
        //    var syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(Player.Id);
        //    var ugcUserInfo = GameManager.Inst.ugcUserInfo;
        //    var nickName = syncPlayerInfo != null ? syncPlayerInfo.userName :
        //        ugcUserInfo != null ? ugcUserInfo.userName : Player.Name;
        //    RoomChatPanel.Instance.SetRecChat(RecChatType.TextChat, nickName,"has collected all the collectibles!");
        //}
        //catch (Exception e)
        //{
        //    LoggerUtils.Log("LocalCollectStarProp Error:" + e.Message);
        //}
    }

    private void EnterEditMode()
    {
        foreach (var tmpEntity in allCollectControlNodes)
        {
            if (!tmpEntity.entity.HasComponent<CollectControlComponent>()) continue;
            if (tmpEntity.entity.Get<CollectControlComponent>().isControl == 1)
            {
                var goCmp = tmpEntity.entity.Get<GameObjectComponent>();
                tmpEntity.entity.Get<CollectControlComponent>().triggerCount = 0;
                goCmp.bindGo.SetActive(true);
            }

            if (tmpEntity.entity.Get<CollectControlComponent>().moveActive)
            {
                tmpEntity.entity.Get<CollectControlComponent>().triggerCount = 0;
            }

            if (tmpEntity.entity.Get<CollectControlComponent>().playSound == (int)SoundControl.SUPPORT_CTRL_MUSIC)
            {
                tmpEntity.entity.Get<CollectControlComponent>().triggerCount = 0;
            }

            if (tmpEntity.entity.Get<CollectControlComponent>().animActive == (int)AnimControl.SUPPORT_CTRL_ANIM)
            {
                tmpEntity.entity.Get<CollectControlComponent>().triggerCount = 0;
            }
            if (tmpEntity.entity.Get<CollectControlComponent>().playfirework == (int)FireworkControl.SUPPORT_CTRL_Firework)
            {
                tmpEntity.entity.Get<CollectControlComponent>().triggerCount = 0;
            }

        }

        foreach (var propStarBehaviour in propStarBehaviours)
        {
            
            propStarBehaviour.KillCollectAnim();
            propStarBehaviour.GetRootCombineOrSelf().gameObject.SetActive(true);
        }
        
        

        collectEntityCount = 0;

        ClearRoomData();
        ClearBevs();
        // propEntities.Clear();
    }
    private void EnterPlayMode()
    {
        collectEntityCount = 0;
        sendRetryCount = 0;
        //更新道具数量
        allCollectControlNodes = SceneSystem.Inst.FilterBehaviours<CollectControlComponent>(SceneBuilder.Inst.allControllerBehaviours);
        propStarBehaviours = SceneSystem.Inst.FilterNodeBehaviours<PropStarBehaviour>(SceneBuilder.Inst.allControllerBehaviours);

        
        LoggerUtils.Log("EnterPlayMode propEntities:" + propStarBehaviours.Count);
        LoggerUtils.Log("EnterPlayMode allCollectControlEntity:" + allCollectControlNodes.Count);


        CoroutineManager.Inst.StartCoroutine(DelayShowStarOrObjs());

        PlayModePanel.Instance.UpdateCollectText(collectEntityCount, propStarBehaviours.Count);
    }


    private  IEnumerator DelayShowStarOrObjs()
    {

        yield return new WaitForSeconds(0.1f);
        var mapId = GlobalFieldController.CurMapInfo != null
            ? GlobalFieldController.CurMapInfo.mapId
            : "1465952717637517312_1638345577_1";
        
        if (mapCollectStarEntities.ContainsKey(mapId))
        {
            var starEntities = mapCollectStarEntities[mapId];
            for (var i = 0; i < starEntities.Count; i++)
            {
                var propStarBehaviour = propStarBehaviours.Find(tmp => tmp.entity.Get<GameObjectComponent>().uid == starEntities[i]);
                propStarBehaviour.SetCollect();
                collectEntityCount++;
            }
        }
        
        foreach (var collectControlEntity in allCollectControlNodes)
        {
            if (!collectControlEntity.entity.HasComponent<CollectControlComponent>()) continue;
            var collectControlObj = collectControlObjs.Find(tmp => tmp.mapId == mapId);
            LoggerUtils.Log("collectControlObj:" + (collectControlObj == null) );
            if (collectControlObj != null)
            {        
                LoggerUtils.Log("collectControlObj:" + (collectControlObj.triggerCount) );
                if (collectControlEntity.entity.Get<CollectControlComponent>().isControl == 1)
                {
                    collectControlEntity.entity.Get<CollectControlComponent>().triggerCount = collectControlObj.triggerCount;
                    if (collectControlObj.triggerCount == 0 || !IsCanCollect(collectControlEntity.entity)) continue;
                    var bindGo = collectControlEntity.entity.Get<GameObjectComponent>().bindGo;
                    var gcomp = collectControlEntity.entity.Get<GameObjectComponent>();

                    if (BloodPropManager.Inst.IsBloodPropUsed(collectControlEntity))
                    {
                        // 回血道具被使用后，不会被控制
                        continue;
                    }
                    if (FreezePropsManager.Inst.IsPropUsed(collectControlEntity))
                    {
                        // 冻结道具被使用后，不会被控制
                        continue;
                    }
                    var fire = bindGo.GetComponentInChildren<FireworkBehaviour>(true);
                    if (fire != null)
                    {
                        //烟花道具未添加实体模型 不可被控制
                        continue;
                    }

                    if (AttackWeaponManager.Inst.IsAttackPropOutOfControl(collectControlEntity))
                    {
                        // 攻击道具被损毁后，不能被控制
                        continue;
                    }

                    if (!PickabilityManager.Inst.IsCanBeControlled(gcomp.uid))
                    {
                        //道具被拾取之后 不会再被开关控制
                        continue;
                    }
                    bindGo.SetActive(!bindGo.activeSelf);
                    SceneSystem.Inst.RestoreSystemState();
                }

                if (collectControlEntity.entity.Get<CollectControlComponent>().moveActive)
                {
                    if (collectControlEntity.entity.HasComponent<MovementComponent>())
                    {
                        var go = collectControlEntity.entity.Get<GameObjectComponent>().bindGo;
                        if (go.activeSelf)
                        {
                            var moveCom = collectControlEntity.entity.Get<MovementComponent>();
                            moveCom.tempMoveState = ((moveCom.tempMoveState == 0) ? 1 : 0);
                            SceneSystem.Inst.ExcuteMoveSystem();
                        }
                    }
                }

                if (collectControlEntity.entity.Get<CollectControlComponent>().playSound == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                {
                    var go = collectControlEntity.entity.Get<GameObjectComponent>().bindGo;
                    if (go.activeSelf)
                    {
                        var soundCom = collectControlEntity.entity.Get<SoundComponent>();
                        // if (soundCom.isControl == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                        // {
                        var soundBev = go.GetComponentInChildren<SoundButtonBehaviour>();
                        // 本地播放音乐
                        soundBev.OnClickSound();
                        // }
                    }
                }

                if (collectControlEntity.entity.Get<CollectControlComponent>().animActive == (int)AnimControl.SUPPORT_CTRL_ANIM)
                {
                    if (collectControlEntity.entity.HasComponent<RPAnimComponent>())
                    {
                        var go = collectControlEntity.entity.Get<GameObjectComponent>().bindGo;
                        if (go.activeSelf)
                        {
                            var animCom = collectControlEntity.entity.Get<RPAnimComponent>();
                            animCom.tempAnimState = ((animCom.tempAnimState == 0) ? 1 : 0);
                            SceneSystem.Inst.ExcuteUpDownSystem();
                        }
                    }
                }
            }
        }
        PlayModePanel.Instance.UpdateCollectText(collectEntityCount, propStarBehaviours.Count);
        
    }


    public void OnHandleClone(SceneEntity sourceEntity, SceneEntity newEntity)
    {
        
        var propStars = newEntity.Get<GameObjectComponent>().bindGo.GetComponentsInChildren<PropStarBehaviour>();
        
        
        foreach (var tmpPropStar in propStars)
        {
            if (!propStarBehaviours.Contains(tmpPropStar))
            {
                propStarBehaviours.Add(tmpPropStar);
            }
        }
        LoggerUtils.Log("OnHandleClone:" + propStarBehaviours.Count);
    }

    public void CollectEntities(SceneEntity[] entities)
    {
        LoggerUtils.Log("CollectEntities:" + entities.Length);
        collectEntityCount += entities.Length;
        var mapId = GlobalFieldController.CurMapInfo != null
            ? GlobalFieldController.CurMapInfo.mapId
            : "1465952717637517312_1638345577_1";
        for (var i = 0; i < entities.Length; i++)
        {
            if (!mapCollectStarEntities.ContainsKey(mapId))
            {
                mapCollectStarEntities.Add(mapId, new List<int>());
            }

            var uid = entities[i].Get<GameObjectComponent>().uid;
            if (!mapCollectStarEntities[mapId].Contains(uid))
            {
                mapCollectStarEntities[mapId].Add(uid);
            }
        }

        PlayModePanel.Instance.UpdateCollectText(collectEntityCount, propStarBehaviours.Count);
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            LocalCollectStarProp();
            if (!Global.IsInRoom()) return;
            if (collectEntityCount >= propStarBehaviours.Count)
            {
                SendRoomAttr();
            }
        }
        else
        {
            LocalCollectStarProp();
        }
    }

    private bool IsCanCollect(SceneEntity entity)
    {
        var root = entity.Get<GameObjectComponent>().bindGo.GetComponent<CombineBehaviour>();
        if (root == null)
        {
            root = entity.Get<GameObjectComponent>().bindGo.transform.parent.GetComponent<CombineBehaviour>();
        }
        if (root == null) return true;
        var starProp = root.GetComponentInChildren<PropStarBehaviour>(true);
        return starProp == null || starProp.CheckCanClick();

    }

    private void SetEntityCollected(SceneEntity sceneEntity)
    {
        var mapId = GlobalFieldController.CurMapInfo != null
            ? GlobalFieldController.CurMapInfo.mapId
            : "1465952717637517312_1638345577_1";

        LoggerUtils.Log("mapId" + mapId);
        if (!sceneEntity.HasComponent<CollectControlComponent>()) return;
        var collectControl = sceneEntity.Get<CollectControlComponent>();
        // if (collectControl.isControl != 1) return;
        if (collectControl.triggerCount == 0)
        {
            if (IsCanCollect(sceneEntity))
            {
                if (collectControl.isControl == 1)
                {
                    var gComp = sceneEntity.Get<GameObjectComponent>();
                    var bindGo = gComp.bindGo;

                    var behv = BloodPropManager.Inst.GetBloodPropByUid(gComp.uid);
                    if (behv != null && BloodPropManager.Inst.IsBloodPropUsed(behv))
                    {
                        // 回血道具被使用后，不会被控制
                        return;
                    }
                    var freezeNode = FreezePropsManager.Inst.GetNodeByUid(gComp.uid);
                    if (freezeNode != null && FreezePropsManager.Inst.IsPropUsed(freezeNode))
                    {
                        // 冻结道具被使用后，不会被控制
                        return;
                    }
                    var fire = bindGo.GetComponentInChildren<FireworkBehaviour>(true);
                    if (fire != null)
                    {
                        //烟花道具未添加实体模型 不可被控制
                        return;
                    }
                    behv = PickabilityManager.Inst.GetBaseBevByUid(gComp.uid);
                    if (AttackWeaponManager.Inst.IsAttackPropOutOfControl(behv))
                    {
                        // 攻击道具被损毁后，不能被控制
                        return;
                    }

                    if (!PickabilityManager.Inst.IsCanBeControlled(gComp.uid))
                    {
                        //道具被拾取之后 不会再被开关控制
                        return;
                    }
                    bindGo.SetActive(!bindGo.activeSelf);
                    SceneSystem.Inst.RestoreSystemState();
                }

                var go = sceneEntity.Get<GameObjectComponent>().bindGo;

                if (collectControl.moveActive)
                {
                    if (go.activeSelf && sceneEntity.HasComponent<MovementComponent>())
                    {
                        var moveCom = sceneEntity.Get<MovementComponent>();
                        moveCom.tempMoveState = ((moveCom.tempMoveState == 0) ? 1 : 0);
                        SceneSystem.Inst.ExcuteMoveSystem();
                    }
                }

                if (collectControl.playSound == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                {
                    if (go.activeSelf)
                    {
                        var soundCom = sceneEntity.Get<SoundComponent>();
                        // if (soundCom.isControl == (int)SoundControl.SUPPORT_CTRL_MUSIC)
                        // {
                        var soundBev = go.GetComponentInChildren<SoundButtonBehaviour>();
                        // 本地播放音乐
                        soundBev.OnClickSound();
                        // }
                    }
                }

                if (collectControl.animActive == (int)AnimControl.SUPPORT_CTRL_ANIM)
                {
                    if (go.activeSelf && sceneEntity.HasComponent<RPAnimComponent>())
                    {
                        var moveCom = sceneEntity.Get<RPAnimComponent>();
                        moveCom.tempAnimState = ((moveCom.tempAnimState == 0) ? 1 : 0);
                        SceneSystem.Inst.ExcuteUpDownSystem();
                    }
                }

                if (collectControl.playfirework == (int)FireworkControl.SUPPORT_CTRL_Firework)
                {
                    if (go.activeSelf && sceneEntity.HasComponent<FireworkComponent>())
                    {
                        var fireBev = go.GetComponentInChildren<NodeBaseBehaviour>();
                        FireworkManager.Inst.OnTriggerFirework(fireBev);
                    }
                }
            }
        }

        collectControl.triggerCount++;
        var uid = sceneEntity.Get<GameObjectComponent>().uid;
        var collectObj = collectControlObjs.FirstOrDefault(tmp =>
            tmp.mapId == mapId && tmp.uid == uid);
        if (collectObj == null)
        {
            collectObj = new CollectControlObj
            {
                mapId = mapId,
                uid = uid,
            };
            collectObj.triggerCount++;
            collectControlObjs.Add(collectObj);
        }
        else
        {
            collectObj.triggerCount++;
        }
    }


    private IEnumerator RoomAttrCheck()
    {
        yield return new WaitForSeconds(10.0f);
        timeOutCoroutine = null;
        if (sendRetryCount >= 5)
        {
            LoggerUtils.Log("重试已经超过5次");
        }
        else
        {
            SendRoomAttr();
        }
    }

    private void SendRoomAttr()
    {
        timeOutCoroutine = CoroutineManager.Inst.StartCoroutine(RoomAttrCheck());
        sendRetryCount++;
        if (!collectedUsers.Contains(Player.Id))
        {
            collectedUsers.Add(Player.Id);
        }

        var roomAttrCollectControl = new RoomAttrCollectControl
        {
            collectedUsers = collectedUsers,
            collectControlObjs = collectControlObjs
        };

        var roomChatData = new RoomChatData()
        {
            msgType = (int) RecChatType.RoomAttrs,
            data = JsonConvert.SerializeObject(new SendRoomAttrReq()
            {
                roomId = Global.Room.RoomInfo.Id,
                roomAttrs = new List<RoomAttr>
                {
                    new RoomAttr
                    {
                        type = (int) RoomAttrType.COLLECT_ENTITY,
                        data = JsonConvert.SerializeObject(roomAttrCollectControl)
                    }
                }
            })
        };
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), (i, s) =>
        {
            if (timeOutCoroutine != null)
            {
                CoroutineManager.Inst.StopCoroutine(timeOutCoroutine);
                timeOutCoroutine = null;
            }

            if (i != ErrCode.EcOk)
            {
                SendRoomAttr();
            }
            else
            {
                sendRetryCount = 0;
            }
        });
    }

    //是否地图是添加了星星收集道具
    public bool IsCanCollectStar()
    {
        if (propStarBehaviours != null && propStarBehaviours.Count > 0)
        {
            return true;
        }

        return false;
    }
}