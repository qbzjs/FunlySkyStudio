using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using RTG;
using UnityEngine;

public class FirePropManager : ManagerInstance<FirePropManager>, IManager
{
    public const int mLimitedCount = 99;
    public bool IsLimitedCount => (Count >= mLimitedCount);
    public int Count => mNodes.Count;
    private Dictionary<int, NodeBaseBehaviour> mNodes = new Dictionary<int, NodeBaseBehaviour>();
    
    
    #region 创建流程
    public NodeBaseBehaviour CreateBySelected(Vector3 pos)
    {
        FirePropBehaviour nBev = SceneBuilder.Inst.CreateSceneNode<FirePropCreator, FirePropBehaviour>();
        FirePropData data = new FirePropData();
        data.id = (int)GameResType.FireProp;
        data.flare = 0;
        data.intensity = 1.5f;
        data.collision = 1;
        data.doDamage = 0;
        data.hpDamage = 20;
        data.lightRange = GetLightRangeFromScale(nBev.gameObject);

        nBev.transform.position = pos;
        
        //编辑创建素材时，需要添加的数据
        nBev.data = new NodeData()      
        {
            id = (int)GameResType.FireProp
        };

        FirePropCreator.SetData(nBev, data);
        AddNode(nBev);
        return nBev;
    }
    public void AddNode(NodeBaseBehaviour node)
    {
        GameObjectComponent compt = node.entity.Get<GameObjectComponent>();
        mNodes.Add(compt.uid, node);
    }
    public void RemoveNode(int uid)
    {
        if (mNodes.ContainsKey(uid))
        {
            mNodes.Remove(uid);
        }
    }
    public NodeBaseBehaviour GetNode(int uid)
    {
        NodeBaseBehaviour node = null;
        mNodes.TryGetValue(uid, out node);
        return node;
    }
    public bool IsCanClone(int count)
    {
        if (Count + count > mLimitedCount)
        {
            return false;
        }
        return true;
    }

    #endregion
    #region 处理网络消息
    //发送和接收采用同一个结构体
    public class ReqAndRspData
    {
        public string playerId { get; set; }
        public int canDamage { get; set; }// 0 不开启，1 开启
        public float damage { get; set; }
        public float curBlood { get; set; }
        public int alive { get; set; }//1存活，2.死亡
    }
    public bool OnReceiveServer(string sendPlayerId, string msg)
    {
        LoggerUtils.Log($"FirePropManager OnReceiveServer ==> => senderPlayer:{sendPlayerId}, msg:{msg}");
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.FIREPROP)
                {
                    if (string.IsNullOrEmpty(item.data))
                    {
                        LoggerUtils.Log("[FirePropManager.OnReceiveServer.Items.item.Data is null");
                        continue;
                    }
                    ReqAndRspData itemData = JsonConvert.DeserializeObject<ReqAndRspData>(item.data);
                    NodeBaseBehaviour node = GetNode(item.id);
                    bool playAnimation = false;
                    if (node != null)
                    {
                        FirePropComponent compt = node.entity.Get<FirePropComponent>();
                        playAnimation = compt.collision == 1;
                    }

                    if (itemData != null)
                    {
                        NetBurn(itemData, playAnimation);
                    }
                }
            }
        }
        return true;
    }
    public void ReqFireDamge(SceneEntity entity)
    {
        int canDamage = 0;
        int damage = 0;
        if (entity.HasComponent<FirePropComponent>())
        {
            FirePropComponent compt = entity.Get<FirePropComponent>();
            canDamage = compt.doDamage;
            damage = compt.hpDamage;
        }
        ReqAndRspData itemData = new ReqAndRspData();
        itemData.playerId = Player.Id;
        itemData.canDamage = canDamage;
        itemData.damage = damage;
        var uid = entity.Get<GameObjectComponent>().uid;
        Item[] itemsArray =
        {
            new Item()
            {
                id = uid,
                type = (int) ItemType.FIREPROP,
                data = JsonConvert.SerializeObject(itemData),
            }
        };

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

        string jsonData = JsonConvert.SerializeObject(roomChatData);
        LoggerUtils.Log($"FirePropManager ReqFireDamge ==> =>:{jsonData}");
        ClientManager.Inst.SendRequest(jsonData, null);
    }
    public bool IsCanHurt()
    {
        bool isCanHurt = true;
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            isCanHurt = false;
        }

        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            isCanHurt = false;
        }
        if (PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder)
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
            return false;
        }
        return isCanHurt;
    }
    #endregion
    #region 响应燃烧
    public void EditorBurn(string playerId, float damage, bool isPlayAnimation, bool isDamage)
    {
        BurnEffect(playerId);
        BurnSound(playerId);

        bool isDead = false;
        if (isDamage)
        {
            LoalDamage(playerId, damage, out isDead);
        }
        //这里的isPlayAnimation决定是否开启打断其他行为
        if (isPlayAnimation && !isDead)
        {
            BurnAnimation(playerId);
        }
    }
    public void NetBurn(ReqAndRspData data, bool isPlayAnimation)
    {
        BurnEffect(data.playerId);
        BurnSound(data.playerId);
        if (IsOpenDamage() && data.canDamage == 1)
        {
            Damage(data.playerId, data.curBlood, data.alive);
        }
        if (isPlayAnimation && data.alive != 2)
        {
            BurnAnimation(data.playerId);
        }
    }
    public void HandlePlayerDeath(string playerId)
    {
        var battleCtr = PlayerInfoManager.GetBattleCtr(playerId);
        if ((battleCtr != null))
        {
            battleCtr.OnDeadEvent = () =>
            {
                OnDeath(playerId);
            };
            battleCtr.GetDeath(playerId);
            battleCtr.OnDeadEvent = null;
        }
    }
    public void OnDeath(string playerId)
    {
        //玩家已死亡不处理
        if (PlayerManager.Inst.GetPlayerDeathState(playerId))
        {
            return;
        }

        GameObject playerNode = null;
        if (playerId != Player.Id)
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
            PlayDeathPs(playerNode, new Vector3(0, -1, 0));
        }

        PlayerManager.Inst.OnPlayerDeath(playerId);
        AKSoundManager.Inst.PlayDeathSound(playerNode);
        ClearFaceAnim(playerNode);//死亡后恢复默认面部表情
    }
    public void ClearFaceAnim(GameObject playerNode)
    {
        AnimationController animCon = playerNode.GetComponentInChildren<AnimationController>();
        if (animCon)
        {
            animCon.RleasePrefab();
            animCon.CancelLastEmo();
        }
    }
    public void BurnEffect(string playerId)
    {
        Transform burnEffectRoot = null;
        Transform decalRoot = null;
        if (playerId == Player.Id)
        {
            burnEffectRoot = PlayerBaseControl.Inst.transform.transform.Find("Player/body");
            decalRoot = PlayerBaseControl.Inst.transform.transform.Find("Player/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head");
        }
        else
        {
            OtherPlayerCtr otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer != null)
            {
                burnEffectRoot = otherPlayer.transform.Find("body");
                decalRoot = otherPlayer.transform.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head");
            }
        }
        //着火特效
        ParticleManager.Inst.PlayEffect("Effect/FireBurn/FireBurnEffect", burnEffectRoot, 1.72f);
        //脸部烧焦贴花
        ParticleManager.Inst.PlayEffect("Effect/FireBurn/FireBurnDecal", decalRoot, 3.0f);

    }
    public void BurnSound(string playerId)
    {
        GameObject soundGameObject = null;
        if (playerId == Player.Id)
        {
            soundGameObject = PlayerBaseControl.Inst.playerAnim.gameObject;
        }
        else
        {
            OtherPlayerCtr otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer != null)
            {
                soundGameObject = otherPlayer.gameObject;
            }
        }
        AKSoundManager.Inst.PostEvent("Play_Fire_Burn", soundGameObject);
    }
    public void BurnAnimation(string playerId)
    {
        AnimationController animCon = null;
        if (playerId == Player.Id)
        {
            animCon = PlayerBaseControl.Inst.animCon;
            //打断降落伞
            if (StateManager.IsParachuteUsing)
            {
                PlayerParachuteControl.Inst.ForceStopParachute();
            }
            //打断钓鱼
            if (StateManager.IsFishing)
            {
                FishingManager.Inst.ForceStopFishing();
            }
            //过滤判断一下是否是冰冻状态
            if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
            {
                return;
            }
        }
        else
        {
            OtherPlayerCtr otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer != null)
            {
                animCon = otherPlayer.animCon;
                if (otherPlayer.steeringWheel != null || MagneticBoardManager.Inst.IsOtherPlayerOnBoard(otherPlayer))
                {
                    return;
                }
                if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(otherPlayer))
                {
                    return;
                }
            }
        }
        //播放动画
        if (animCon)
        {
            animCon.PlayFireHitAnim();
        }
    }
    public void LoalDamage(string playerId, float damage, out bool isDead)
    {
        isDead = false;
        CharBattleControl selfCon = GetMyHpCtr();
        if (selfCon != null)
        {
            selfCon.SubHp(damage,Player.Id);
            float myHpValue = selfCon.GetCurHp();
            myHpValue = Mathf.Max(myHpValue, 0);
            if (myHpValue == 0)
            {
                isDead = true;
                CoroutineManager.Inst.StartCoroutine(DelayShowDead());
            }
        }
        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.Hit();
        }
    }
    public IEnumerator DelayShowDead()
    {
        yield return null;
        ShowLocalDeath();
    }
    private void ShowLocalDeath()
    {
        PlayerBaseControl.Inst.SetPosToSpawnPoint();
        MessageHelper.Broadcast(MessageName.PosMove, true);
        GameObject playerNode = PlayerBaseControl.Inst.animCon.gameObject;
        PlayDeathPs(playerNode, new Vector3(0, -1, 0));
        AKSoundManager.Inst.PlayDeathSound(playerNode);
        SelfDeathOnPlay();
        ResetHpValue();
    }
    private void SelfDeathOnPlay()
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && GlobalFieldController.CurGameMode == GameMode.Play)
        {
            var comp = PVPWaitAreaManager.Inst.PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            if (comp.gameMode == (int)PVPServerTaskType.Survival && PVPSurvivalGamePlayPanel.Instance != null)
            {
                PVPSurvivalGamePlayPanel.Instance.SetWinner(PVPGameOverPanel.GameOverStateEnum.Loss);
            }
        }
    }
    public void PlayDeathPs(GameObject playerNode, Vector3 diffPos = default)
    {
        ParticleManager.Inst.PlayEffect("Effect/death_smoke/death_smoke", playerNode.transform, 1.0f);
    }
    public void Damage(string playerId, float curHp, int alive)
    {
        if (alive == 2)
        {
            HandlePlayerDeath(playerId);
        }
        PVPManager.Inst.UpdatePlayerHpShow(playerId, curHp);
        if (playerId == Player.Id && FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.Hit();
        }
    }
    private void ResetHpValue()
    {
        var selfCon = GetMyHpCtr();
        if (selfCon != null)
        {
            selfCon.ResetHpValue(Player.Id);
        }
    }
    private CharBattleControl GetMyHpCtr()
    {
        if (PlayerBaseControl.Inst == null)
        {
            return null;
        }

        CharBattleControl myHpCtr = PlayerBaseControl.Inst.GetComponentInChildren<CharBattleControl>(true);

        if (myHpCtr == null)
        {
            myHpCtr = PlayerBaseControl.Inst.gameObject.AddComponent<CharBattleControl>();
            myHpCtr.ShowState(GameManager.Inst.ugcUserInfo.uid);
        }
        return myHpCtr;
    }
    #endregion
    public void Clear()
    {

    }

    public override void Release()
    {
        base.Release();

        Clear();
    }
    public bool IsOpenDamage()
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return false;
        }
        List<int> dmgList = SceneParser.Inst.GetDamageSources();
        return dmgList.Contains((int)DamageSource.Fire);
    }
    public void RemoveNode(NodeBaseBehaviour node)
    {
        if (node == null)
            return;
        GameObjectComponent objCompt = node.entity.Get<GameObjectComponent>();
        int uid = objCompt.uid;
        RemoveNode(uid);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.FireProp)
        {
            //mCurCount++;
            AddNode(behaviour);
        }
    }

    public void SetLightRangeOnScale(GameObject target)
    {
        FirePropBehaviour[] behavs = target.GetComponentsInChildren<FirePropBehaviour>();
        foreach (FirePropBehaviour behav in behavs)
        {
            float range = GetLightRangeFromScale(behav.gameObject);
            behav.SetLightRange(range);
            behav.entity.Get<FirePropComponent>().lightRange = range;
        }
    }

    private float GetLightRangeFromScale(GameObject target)
    {
        return target.transform.lossyScale.x * 8f;
    }

    public void OnChangeMode(GameMode mode)
    {
        if(mode == GameMode.Play || mode == GameMode.Guest)
        {
            PlayAllSound();
        }
        if(mode == GameMode.Edit)
        {
            StopAllSound();
        }
    }

    private void PlayAllSound()
    {
        var fires = mNodes.Values;
        foreach(NodeBaseBehaviour b in fires)
        {
            (b as FirePropBehaviour)?.PlaySound();
        }
    }

    private void StopAllSound()
    {
        var fires = mNodes.Values;
        foreach (NodeBaseBehaviour b in fires)
        {
            (b as FirePropBehaviour)?.StopSound();
        }
    }
}

