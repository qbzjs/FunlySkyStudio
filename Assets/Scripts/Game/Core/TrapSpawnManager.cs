using System;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using BudEngine.NetEngine;

/// <summary>
/// Author: 熊昭
/// Description: 陷阱盒道具管理类：统一管理场景中所有陷阱盒及其传送点的共享数据、功能
/// Date: 2022-01-03 21:24:42
/// </summary>
public class TrapSpawnManager : ManagerInstance<TrapSpawnManager>, IManager
{
    public static readonly int MaxCount = 999;

    private int trapBoxMax;

    private Dictionary<int, SceneEntity> trapBoxs = new Dictionary<int, SceneEntity>();
    private Dictionary<int, SceneEntity> spawnPoints = new Dictionary<int, SceneEntity>();
    
    private CharBattleControl myHpCtr;

    private bool IsSelfDeath = false;

    public override void Release()
    {
        base.Release();
        ClearTrapList();
    }

    private void ClearTrapList()
    {
        trapBoxs.Clear();
        spawnPoints.Clear();
    }

    public bool IsOverMaxTrapCount()
    {
        if (trapBoxMax >= MaxCount)
        {
            LoggerUtils.Log("Max 999 no more");
            return true;
        }
        return false;
    }
    public bool IsOverMaxTrapCountWhenClone(int count)
    {
        if (trapBoxMax + count > MaxCount)
        {
            LoggerUtils.Log("Max 99 no more");
            return true;
        }
        return false;
    }

    public int GetNextId()
    {
        return trapBoxMax + 1;
    }

    public void AddTrapBox(int tId, SceneEntity go)
    {
        if (!trapBoxs.ContainsKey(tId))
        {
            trapBoxMax = Math.Max(trapBoxMax, tId);
            trapBoxs.Add(tId, go);
        }
    }

    public void AddTrapSpawn(int tId, SceneEntity go)
    {
        if (!spawnPoints.ContainsKey(tId))
        {
            spawnPoints.Add(tId, go);
        }


    }

    public void RemoveTrapSpawn(int tId)
    {
        if (spawnPoints.ContainsKey(tId))
        {
            spawnPoints.Remove(tId);
        }
    }

    public void OnHandleClone(SceneEntity oEntity, SceneEntity nEntity)
    {
        var tComp = nEntity.Get<TrapBoxComponent>();
        if (tComp.rePos == (int)TrapBoxTrans.CustomSpawn)
        {
            int oId = oEntity.Get<TrapBoxComponent>().tId;
            var oPoint = GetPointGo(oId);
            if (oPoint != null)
            {
                var pos = oPoint.Get<GameObjectComponent>().bindGo.transform.position;
                var pointBehav = SceneBuilder.Inst.CreateTrapSpawn(nEntity, pos);
                ModelHandlePanel.AddCloneRecord(pointBehav.gameObject);
            }
        }
    }

    private SceneEntity GetTrapGo(int tId)
    {
        if (trapBoxs.ContainsKey(tId))
        {
            return trapBoxs[tId];
        }
        return null;
    }

    public SceneEntity GetPointGo(int tId)
    {
        if (spawnPoints.ContainsKey(tId))
        {
            return spawnPoints[tId];
        }
        return null;
    }

    private CharBattleControl GetMyHpCtr()
    {
        if(myHpCtr != null)
        {
            return myHpCtr;
        }
        
        if(PlayerBaseControl.Inst == null)
        {
            return null;
        }

        myHpCtr = PlayerBaseControl.Inst.GetComponentInChildren<CharBattleControl>(true);
        
        if (myHpCtr == null)
        {
            myHpCtr = PlayerBaseControl.Inst.gameObject.AddComponent<CharBattleControl>();
            myHpCtr.ShowState(GameManager.Inst.ugcUserInfo.uid);
        }
        return myHpCtr;
    }

    public bool IsOpenTrapDamage()
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return false;
        }

        List<int> dmgList = SceneParser.Inst.GetDamageSources();
        bool isTrapBoxHit = dmgList.Contains((int)DamageSource.TrapBox);        
        return isTrapBoxHit;
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.TrapBox)
        {
            var tComp = behaviour.entity.Get<TrapBoxComponent>();
            tComp.tempRePos = 0;
            LoggerUtils.Log("TrapSpawnManager RemoveNode TrapBox:"+tComp.tId);
            var pointEntity = GetPointGo(tComp.tId);
            if (pointEntity != null)
            {
                tComp.tempRePos = 1;
                var spawnComp = pointEntity.Get<TrapSpawnComponent>();
                RemoveTrapSpawn(spawnComp.tId);
                SecondCachePool.Inst.DestroyEntity(pointEntity.Get<GameObjectComponent>().bindGo);
            }
        }

        if (goCmp.modelType == NodeModelType.TrapSpawn)
        {
            LoggerUtils.Log("TrapSpawnManager RemoveNode TrapSpawn");
            var spawnComp = behaviour.entity.Get<TrapSpawnComponent>();
            var gameComp = behaviour.entity.Get<GameObjectComponent>();
            var trapEntity = GetTrapGo(spawnComp.tId);
            if (trapEntity != null)
            {
                if(trapEntity.Get<TrapBoxComponent>().pId != gameComp.uid){
                    //清除undo/redo栈时可能触发：删除的复活点跟当前记录复活点ID不一致
                    LoggerUtils.Log("TrapSpawnManager Remove TrapSpawn is not the current Node:"+trapEntity.Get<TrapBoxComponent>().pId + "  gameComp.uid:"+gameComp.uid);
                    return;
                }
                RemoveTrapSpawn(spawnComp.tId);
                if(trapEntity.Get<TrapBoxComponent>().rePos == (int)TrapBoxTrans.CustomSpawn)
                {
                    trapEntity.Get<TrapBoxComponent>().rePos = (int)TrapBoxTrans.MapSpawn;
                }
                // trapEntity.Get<TrapBoxComponent>().rePos = 0;
                var tBehav = trapEntity.Get<GameObjectComponent>().bindGo.GetComponent<TrapBoxBehaviour>();
                if (tBehav)
                {
                    tBehav.SetTextVisiable(false);
                }
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.TrapSpawn)
        {
            LoggerUtils.Log("TrapSpawnManager RevertNode TrapSpawn");
            var spawnComp = behaviour.entity.Get<TrapSpawnComponent>();
            AddTrapSpawn(spawnComp.tId,behaviour.entity);

            var trapEntity = GetTrapGo(spawnComp.tId);
            if (trapEntity != null)
            {
                var tComp = trapEntity.Get<TrapBoxComponent>();
                tComp.rePos = (int)TrapBoxTrans.CustomSpawn;
                tComp.pId = goCmp.uid;
                var tBehav = trapEntity.Get<GameObjectComponent>().bindGo.GetComponent<TrapBoxBehaviour>();
                if (tBehav)
                {
                    tBehav.SetTextVisiable(true);
                }
            }
        }  

        if (goCmp.modelType == NodeModelType.TrapBox)
        {
            var tComp = behaviour.entity.Get<TrapBoxComponent>();
            AddTrapBox(tComp.tId,behaviour.entity);
            var tBehav = behaviour as TrapBoxBehaviour;
            tBehav.RefreshShowId();

            if(tComp.pId > 0 && tComp.tempRePos == 1)
            {
                LoggerUtils.Log("TrapSpawnManager RevertNode TrapSpawn:"+tComp.pId);
                GameObject pointGo = SecondCachePool.Inst.GetGameObjectByUid(tComp.pId);
                if(pointGo != null)
                {
                    SecondCachePool.Inst.RevertEntity(pointGo);
                    TrapSpawnBehaviour pointBehav = pointGo.GetComponent<TrapSpawnBehaviour>();
                    if(pointBehav != null){
                        var trapSpawnComp = pointBehav.entity.Get<TrapSpawnComponent>();
                        AddTrapSpawn(trapSpawnComp.tId,pointBehav.entity);
                        tComp.rePos = (int)TrapBoxTrans.CustomSpawn;
                    }
                    
                }else
                {
                    LoggerUtils.Log("TrapSpawnManager RevertNode TrapSpawn Can not find:"+tComp.pId);
                }
            }
        }
    }

    public void Clear()
    {
        ClearTrapList();
    }

    public void SendRequest(TrapBoxBehaviour behaviour)
    {
        var entity = behaviour.entity;
        var trapComp = entity.Get<TrapBoxComponent>();
        TrapBoxAffectData dataList = new TrapBoxAffectData();
        TrapBoxAffectPlayerData affectData = new TrapBoxAffectPlayerData
        {
            playerId = Player.Id,
            canDamage = trapComp.hitState,
            damage = trapComp.hitValue
        };

        dataList.affectPlayers = new[]
        {
            affectData,
        };
 
        Item itemData = new Item()
        {
            id = entity.Get<GameObjectComponent>().uid,
            type = (int)ItemType.TRAP_BOX,
            data = JsonConvert.SerializeObject(dataList),
        };

        Item[] itemsArray = { itemData };
        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log("TrapSpawnManager SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("TrapSpawnManage OnReceiveServer==>" + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        foreach (var item in itemsReq.items)
        {
            if (item.type == (int)ItemType.TRAP_BOX)
            {
                TrapBoxAffectData affectData = JsonConvert.DeserializeObject<TrapBoxAffectData>(item.data);
                OnReceiveTrapBoxHit(affectData);
                break;
            }
        }
        
        return true;
    }

    public void OnReceiveTrapBoxHit(TrapBoxAffectData affectData)
    {
        if(affectData == null || affectData.affectPlayers == null || affectData.affectPlayers.Length <= 0)
        {
            return;
        }

        if(!IsOpenTrapDamage())
        {
            LoggerUtils.Log("出生点没有开启陷阱盒伤害");
            return;
        }

        for(int i = 0;i < affectData.affectPlayers.Length;i++ )
        {
            TrapBoxAffectPlayerData affectPlayerData = affectData.affectPlayers[i];
            HandleTrapBoxTouch(affectPlayerData);
          
        }
    }

    public void HandleTrapBoxTouch(TrapBoxAffectPlayerData affectPlayerData)
    {
        if(affectPlayerData.canDamage == 0)
        {
            LoggerUtils.Log("该阱盒无伤害");
            return;
        }

        //陷阱盒强打断降落伞
        if (affectPlayerData.playerId == Player.Id && StateManager.IsParachuteUsing)
        {
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
        else if (affectPlayerData.playerId == Player.Id && StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        //打断钓鱼
        else if (affectPlayerData.playerId == Player.Id && StateManager.IsFishing)
        {
            FishingManager.Inst.ForceStopFishing();
        }
        HandlePlayerDeath(affectPlayerData);
        HandlePlayerHp(affectPlayerData);

        string playerId = affectPlayerData.playerId;
        if(playerId != Player.Id)
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if (otherComp != null)
            {
                if (otherComp.steeringWheel != null)
                {
                    return;
                }

                if (MagneticBoardManager.Inst.IsOtherPlayerOnBoard(otherComp))
                {
                    return;
                }
                
                if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(otherComp))
                {
                    return ;
                }

                PlayTrapHitAnim(otherComp.gameObject);
                PlayTrapEffect(otherComp.gameObject);
                AKSoundManager.Inst.PlayTrapHitSound(otherComp.gameObject);
            }
        }
    }


    public void HandlePlayerHp(TrapBoxAffectPlayerData affectData)
    {
        var playerId = affectData.playerId;
        var curBlood = affectData.curBlood;
        PVPManager.Inst.UpdatePlayerHpShow(playerId, curBlood);
        if(playerId == Player.Id && FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.Hit();
        }
    }

    public void HandlePlayerDeath(TrapBoxAffectPlayerData affectData)
    {
        var playerId = affectData.playerId;
        var alive = affectData.alive;
        if (alive == 2)
        {
            var battleCtr = PlayerInfoManager.GetBattleCtr(playerId);
            if ((battleCtr != null))
            {
                battleCtr.OnDeadEvent = ()=>{
                    OnDeath(playerId);
                };
                battleCtr.GetDeath(playerId);
                battleCtr.OnDeadEvent = null;
            }
        }
    }

   
    public void OnDeath(string playerId)
    {
        LoggerUtils.Log("TrapSpwnManager OnDeath:"+playerId);
        //玩家已死亡不处理
        if (PlayerManager.Inst.GetPlayerDeathState(playerId))
        {
            return;
        }
       
        GameObject playerNode = null;
        if(playerId != Player.Id)
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if (otherComp != null)
            {
                playerNode = otherComp.gameObject;
            }
            PlayDeathPs(playerNode);
        }
        else
        {
            playerNode = PlayerBaseControl.Inst.animCon.gameObject;
            PlayDeathPs(playerNode,new Vector3(0,-1,0));
        }
        
        PlayerManager.Inst.OnPlayerDeath(playerId);
        AKSoundManager.Inst.PlayDeathSound(playerNode);
        ClearFaceAnim(playerNode);//死亡后恢复默认面部表情
    }

    public void PlayTrapHitAnim(GameObject playerNode)
    {
        AnimationController animCon = playerNode.GetComponentInChildren<AnimationController>();
        if(animCon)
        {
            animCon.PlayTrapHitAnim();
        }
    }

    public void ClearFaceAnim(GameObject playerNode)
    {
        AnimationController animCon = playerNode.GetComponentInChildren<AnimationController>();
        if(animCon)
        {
            animCon.RleasePrefab();
            animCon.CancelLastEmo();
        }
    }

    public void PlayTrapEffect(GameObject playerNode,Vector3 diffPos = default)
    {
        ParticleSystem effectParticle = null;
        Transform effectTrans = playerNode.transform.Find("TrapHitEffect");
        if(effectTrans == null)
        {
            GameObject hitEffect = ResManager.Inst.LoadRes<GameObject>("Effect/trap_hit/Trap_hit");
            effectTrans = GameObject.Instantiate(hitEffect, playerNode.transform).transform;
            effectTrans.name = "TrapHitEffect";
        }

        effectTrans.localPosition = diffPos;
        effectParticle = effectTrans.GetComponentInChildren<ParticleSystem>();
        ParticleSystemListener listenerComp = effectParticle.gameObject.GetComponent<ParticleSystemListener>();
        if(listenerComp == null)
        {
            listenerComp = effectParticle.gameObject.AddComponent<ParticleSystemListener>();
        }
    
        effectParticle.Play();
        ParticleSystem.MainModule mainModule= effectParticle.main;
        mainModule.loop = false;
        mainModule.stopAction = ParticleSystemStopAction.Callback;
        listenerComp.CompleteAction = ()=>{
            if(effectTrans != null)
            {
                GameObject.Destroy(effectTrans.gameObject);
            }
        };
    }

    public void PlayDeathPs(GameObject playerNode,Vector3 diffPos = default)
    {
        ParticleSystem effectParticle = null;
        Transform effectTrans = playerNode.transform.Find("DeathEffect");
        if(effectTrans == null)
        {
            GameObject hitEffect = ResManager.Inst.LoadRes<GameObject>("Effect/death_smoke/death_smoke");
            effectTrans = GameObject.Instantiate(hitEffect, playerNode.transform).transform;
            effectTrans.name = "DeathEffect";
        }

        effectTrans.localPosition = diffPos;
        effectParticle = effectTrans.GetComponentInChildren<ParticleSystem>();
        ParticleSystemListener listenerComp = effectParticle.gameObject.GetComponent<ParticleSystemListener>();
        if(listenerComp == null)
        {
            listenerComp = effectParticle.gameObject.AddComponent<ParticleSystemListener>();
        }
    
        effectParticle.Play();
        ParticleSystem.MainModule mainModule= effectParticle.main;
        mainModule.loop = false;
        mainModule.stopAction = ParticleSystemStopAction.Callback;
        listenerComp.CompleteAction = ()=>{
            if(effectTrans != null)
            {
                GameObject.Destroy(effectTrans.gameObject);
            }
        };
    }


    public bool IsCanHurt()
    {
        bool isCanHurt = true;
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            isCanHurt = false;
        }
        if(StateManager.IsOnLadder)
        {
            isCanHurt = false;
        }

        if (StateManager.IsOnSeesaw)
        {
            isCanHurt = false;
        }
        
        if (StateManager.IsOnSwing)
        {
            isCanHurt = false;
        }
        
        if (StateManager.IsOnSlide)
        {
            isCanHurt = false;
        }
        //驾驶方向盘时，不能响应牵手
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            isCanHurt =  false;
        }
        return isCanHurt;
    }


    public void HandleLocalShow(TrapBoxBehaviour bev)
    {   
        var tComp = bev.entity.Get<TrapBoxComponent>();

        if(IsOpenTrapDamage() == false || tComp.hitState == 0)
        {
            return;
        }

        if(PlayerBaseControl.Inst != null)
        {
            GameObject playerNode = PlayerBaseControl.Inst.animCon.gameObject;
            if(IsCanHurt() == true)
            {
                PlayTrapHitAnim(playerNode);
                PlayTrapEffect(playerNode,new Vector3(0,-1,0));
                AKSoundManager.Inst.PlayTrapHitSound(playerNode);
            }   
        }


        //TODO:暂时修改陷阱盒和磁力板不受伤
        if(IsCanHurt() == false) 
        {
            return;
        }
        
        
        var selfCon = GetMyHpCtr();
        if(selfCon == null)
        {
            return;
        }

        float myHpValue = selfCon.GetCurHp() - tComp.hitValue;
        if(myHpValue <=0)
        {
            myHpValue = 0;
        }
        UpdateLocalHp(myHpValue);
 
        //试玩模式下才播放本地死亡动作
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            if(myHpValue == 0 && IsSelfDeath == false)
            {
                IsSelfDeath = true;
                CoroutineManager.Inst.StartCoroutine(DelayShowDead());
                // ShowLocalDeath();
            } 
        }
       
    }

    //试玩模式下
    private void UpdateLocalHp(float hpValue)
    {
        var selfCon = GetMyHpCtr();
        if(selfCon!=null)
        {
            selfCon.UpdateHpValue(hpValue,Player.Id);
        }
        
        if(FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetBlood(hpValue);
            FPSPlayerHpPanel.Instance.Hit();
        }
    }

    private void ResetHpValue()
    {
        var selfCon = GetMyHpCtr();
        if(selfCon!=null)
        {
            selfCon.ResetHpValue(Player.Id);
        }
        IsSelfDeath = false;
    }

    private void ShowLocalDeath()
    {
        PlayerBaseControl.Inst.SetPosToSpawnPoint();
        MessageHelper.Broadcast(MessageName.PosMove, true);
        GameObject playerNode = PlayerBaseControl.Inst.animCon.gameObject;
        PlayDeathPs(playerNode,new Vector3(0,-1,0));
        AKSoundManager.Inst.PlayDeathSound(playerNode);
        SelfDeathOnPlay();
        ResetHpValue();
    }

    public IEnumerator DelayShowDead()
    {
        yield return 0;
        ShowLocalDeath();
    }

   
    private void SelfDeathOnPlay()
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && GlobalFieldController.CurGameMode == GameMode.Play)
        {
            var comp = PVPWaitAreaManager.Inst.PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            if (comp.gameMode == (int) PVPServerTaskType.Survival && PVPSurvivalGamePlayPanel.Instance != null)
            {
                PVPSurvivalGamePlayPanel.Instance.SetWinner(PVPGameOverPanel.GameOverStateEnum.Loss);
            }
        }
    }


    public void OnChangeMode(GameMode mode)
    {
        switch (mode){
            case GameMode.Edit:
                EnterEditMode();
                break;
            case GameMode.Play:
                EnterPlayMode();
                break;
            case GameMode.Guest:
                EnterGuestMode();
                break;
        }
    }

    public void EnterEditMode()
    {
        ResetHpValue();
        PVPWaitAreaManager.Inst.IsSelfDeath = false;
    }
    
    public void EnterPlayMode()
    {

    }


    public void EnterGuestMode()
    {

    }
}