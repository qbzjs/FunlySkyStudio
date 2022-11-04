/// <summary>
/// Author:Mingo-LiZongMing
/// Description:拾起道具Manager
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;

public struct PickablityData
{
    public SceneEntity entity;
    public Vector3 oriPos;
    public Vector3 oriRot;
    public Vector3 oriScale;
    public Vector3 anchorsPos;
}

public struct PickPropSyncData
{
    public int status;
    public string playerId;
    public int uid;
    public string position;
    public string curHoldItem;
    public int propType; // 道具类型  0:普通道具；1.攻击道具；2.射击道具
    public int isOpenDurability; // 是否开启道具耐力值 1-开启 0-关闭
    public float curDurability; // 当前道具耐力值
    public int opType;//1.按下开火键/2.抬起开火键/3.开始换弹/4.结束换弹 ⚠️仅type=2003用到
    public int hasCapacity;//是否开启弹夹
    public int capacity;//弹匣容量 
    public int curBullet;//当前子弹数
}

public enum PROP_TYPE
{
    NORMAL = 0,
    ATTACK = 1,
    SHOOT = 2,
    FOOD = 3,
    FISHING = 4,
}

public enum PICK_STATE
{
    DROP = 0,
    CATCH = 1,
}

public struct ActiveData
{
    public int uid;
    public bool status;
}

public class PickabilityManager : ManagerInstance<PickabilityManager>, IManager, IPVPManager, INetMessageHandler
{
    /// <summary>
    /// PickabilityDic<道具Uid,PickablityData>
    /// </summary>
    public Dictionary<int, PickablityData> PickabilityDic = new Dictionary<int, PickablityData>();
    /// <summary>
    /// PlayerPickDic<playerId,道具uid>
    /// </summary>
    public Dictionary<string, int> PlayerPickDic = new Dictionary<string, int>();
    /// <summary>
    /// 用来记录道具的MovementComponent,切换到编辑模式下时还原数据
    /// </summary>
    public Dictionary<int, MovementComponent> mCompsDic = new Dictionary<int, MovementComponent>();
    /// <summary>
    /// RPAnimComponent,切换到编辑模式下时还原数据
    /// </summary>
    public Dictionary<int, RPAnimComponent> rpCompsDic = new Dictionary<int, RPAnimComponent>();
    /// <summary>
    /// 记录可拾取道具的父节点
    /// </summary>
    public Dictionary<int, Transform> PropParentDic = new Dictionary<int, Transform>();
    public string selfUid;
    /// <summary>
    /// 由于创建人物形象和GetItems同时发起的，又都是异步回调，可能出现人还没创建出来时，GetItem已经回包的问题
    /// </summary>
    public List<PickPropSyncData> recordDataList = new List<PickPropSyncData>();
    /// <summary>
    /// 缓动队列List，用于统一管理
    /// </summary>
    private List<Sequence> DOTweenSequenceList = new List<Sequence>();
    private PlayerBaseControl playerCom;
    private bool IsInitListener = false;
    
    public const int MAX_PICK_COUNT = 99; //可拾取道具最大数量，和武器类型共用
    public const string MAX_COUNT_TIP = "Up to 99 objects can be set as pickable";

    public bool isSelfPicking = false; //当前是否正在拾取中
    public Coroutine isSelfPickRollbackCor; 
    public Action OnResetPickComplete;
    private bool isGetItemCallBack = false; //断线重连回来时不执行背包逻辑
    private Vector3 oriPickNodeRot = new Vector3(3.614f, -107.98f, 247.07f);
    private void SetIsSelfPicking(bool value)
    {
        isSelfPicking = value;
    }
    
    private void SetIsSelfPicking(string playerId, bool value)
    {
        if (playerId == selfUid)
        {
            isSelfPicking = value;
            if (isSelfPickRollbackCor != null)
            {
                CoroutineManager.Inst.StopCoroutine(isSelfPickRollbackCor);
                isSelfPickRollbackCor = null;
            }
        }
    }
    
    //避免断网时没有回包导致标志位未重置
    private void SetIsSelfPickingAutoRollback(bool value)
    {
        SetIsSelfPicking(value);
        if (isSelfPickRollbackCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(isSelfPickRollbackCor);
            isSelfPickRollbackCor = null;
        }
        isSelfPickRollbackCor = CoroutineManager.Inst.StartCoroutine(IsSelfPickingAutoRollback());
    }

    private IEnumerator IsSelfPickingAutoRollback()
    {
        yield return new WaitForSeconds(2.0f);
        SetIsSelfPicking(false);
    }


    public void OnChangeMode(GameMode mode)
    {
        switch (mode){
            case GameMode.Edit:
                EnterEditMode();
                break;
            case GameMode.Play:
                RefreshPickablityData();
                break;
            case GameMode.Guest:
                EnterGuestMode();
                break;
        }
    }

    private void EnterGuestMode()
    {
        InitData();
    }

    private void EnterEditMode()
    {
        InitData();
        ResetPickabilityDic();
        ResetPlayerPickState();
    }

   

    public void OnReset()
    {

        //需要延迟一帧避免表情卡住
        CoroutineManager.Inst.StartCoroutine(DelayReset());
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.SetButtonVisible(true);
        }
    }

    public void AddCompleteListener(Action complete)
    {
        OnResetPickComplete = complete;
    }
    IEnumerator DelayReset()
    {
        yield return 0;
        //所有拾取了道具的玩家，丢弃手中的道具
        ResetPickabilityDic();
        ResetPlayerPickState();
        OnResetPickComplete?.Invoke();
        OnResetPickComplete = null;
    }

    public void OnDealPortal()
    {
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.SetButtonVisible(true);
        }
        ResetPickabilityDic();
        ResetPlayerPickState();
    }

    //初始化PlayerController
    private void InitData()
    {
        if (!IsInitListener)
        {
            MessageHelper.AddListener<string>(MessageName.PlayerLeave, OnPlayerLeave);
            IsInitListener = true;
        }
        if (playerCom == null)
        {
            playerCom = GameObject.Find("GameStart").GetComponent<GameController>().playerCom;
        }
        if(GameManager.Inst.ugcUserInfo != null)
        {
#if UNITY_EDITOR
            selfUid = TestNetParams.testHeader.uid;
#else
            selfUid = GameManager.Inst.ugcUserInfo.uid;
#endif
        }
    }

    /// <summary>
    /// 设置道具的可拾取属性，并且记录到dic当中
    /// </summary>
    /// <param name="entity"></param>
    public void AddPickablityProp(SceneEntity entity, Vector3 pos)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        if (!PickabilityDic.ContainsKey(uid))
        {
            var pCom = entity.Get<PickablityComponent>();
            pCom.canPick = (int)PickableState.Pickable;
            pCom.anchors = pos;
            PickablityData pickablityData = GetPickablityData(entity);
            PickabilityDic.Add(uid, pickablityData);
        }
    }

    /// <summary>
    /// 移除道具的可拾取属性，并且从dic中移除
    /// </summary>
    /// <param name="entity"></param>
    public void RemovePickablityProp(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        if (PickabilityDic.ContainsKey(uid))
        {
            entity.Remove<PickablityComponent>();
            PickabilityDic.Remove(uid);
        }
    }

    /// <summary>
    /// 判断是否还能够设置可拾起属性，最大只能够设置99个
    /// </summary>
    /// <returns></returns>
    public bool CheckCanSetPickability()
    {
        if (PickabilityDic.Count >= MAX_PICK_COUNT)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 获取PickablityData,该结构体中存储道具entity和原始坐标信息
    /// </summary>
    /// <param name="entity"></param>
    private PickablityData GetPickablityData(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        PickablityData pickablityData = new PickablityData();
        pickablityData.entity = entity;
        pickablityData.oriPos = gComp.bindGo.transform.localPosition;
        pickablityData.oriRot = gComp.bindGo.transform.localEulerAngles;
        pickablityData.oriScale = gComp.bindGo.transform.localScale;
        return pickablityData;
    }

    /// <summary>
    /// 刷新道具的坐标信息
    /// </summary>
    private void RefreshPickablityData()
    {
        List<SceneEntity> tempList = new List<SceneEntity>();
        foreach(var pickData in PickabilityDic.Values)
        {
            tempList.Add(pickData.entity);
        }
        for(int i = 0;i < tempList.Count; i++)
        {
            var entity = tempList[i];
            var uid = entity.Get<GameObjectComponent>().uid;
            var pickData = GetPickablityData(tempList[i]);
            if (PickabilityDic.ContainsKey(uid))
            {
                PickabilityDic[uid] = pickData;
            }
        }
    }

    /// <summary>
    /// 判断该道具中是否包含不可拾取的物体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool CheckCanSetPickability(SceneEntity entity)
    {
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        var nodeBehvs = bindGo.GetComponentsInChildren<NodeBaseBehaviour>();
        for (var i = 0; i < nodeBehvs.Length; i++)
        {
            //判断是否包含特殊属性
            if (entity.HasComponent<BloodPropComponent>()
                || entity.HasComponent<FreezePropsComponent>()
                || entity.HasComponent<ParachuteComponent>()
                || entity.HasComponent<FireworkComponent>())
            {
                return false;
            }
            ResType resType = nodeBehvs[i].entity.Get<GameObjectComponent>().type;
            NodeModelType modeType = nodeBehvs[i].entity.Get<GameObjectComponent>().modelType;
            if (modeType != NodeModelType.BaseModel
                 && modeType != NodeModelType.DText
                 && modeType != NodeModelType.NewDText
                 && resType != ResType.UGC
                 && resType != ResType.PGC
                 && resType != ResType.CommonCombine
                 && modeType != NodeModelType.PGCPlant
                 && modeType != NodeModelType.PointLight
                 && modeType != NodeModelType.SpotLight
                 && modeType != NodeModelType.FishingModel
                 && modeType != NodeModelType.FishingHook
                 && modeType != NodeModelType.FishingRod
                 && modeType != NodeModelType.FlashLight
                 )
            {
                return false;
            }
        }
        return true;
    }
    
    public Vector3 GetAnchors(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        Vector3 pos = Vector3.zero;
        if (PickabilityDic.ContainsKey(uid))
        {
            var pCom = entity.Get<PickablityComponent>();
            pos = pCom.anchors;
        }
        return pos;
    }
    
    public void SetAnchors(SceneEntity entity, Vector3 pos)
    {
        if(!entity.HasComponent<PickablityComponent>())return;
        var pCom = entity.Get<PickablityComponent>();
        pCom.anchors = pos;
        PickablityData pickablityData = GetPickablityData(entity);
        pickablityData.anchorsPos = pos;
    }

    /// <summary>
    /// 判断道具是否具有可拾取属性
    /// </summary>
    /// <param name="entity"></param>
    public bool CheckPickability(SceneEntity entity)
    {
        var isPickable = false;
        if (entity.HasComponent<PickablityComponent>())
        {
            var pCom = entity.Get<PickablityComponent>();
            if(pCom.canPick == (int)PickableState.Pickable)
            {
                isPickable = true;
            }
        }
        return isPickable;
    }

    /// <summary>
    /// 将PickabilityDic中的可拾取道具的坐标信息重置并且显示 只在编辑模式下用到
    /// </summary>
    public void ResetPickabilityDic()
    {
        ClearDOTweenSequenceList();
        HandlePlayerDropProp(selfUid, false);
        foreach (var pickablityData in PickabilityDic.Values)
        { 
            var entity = pickablityData.entity;
            var gCom = entity.Get<GameObjectComponent>();
            var uid = gCom.uid;
            var target = gCom.bindGo;
            var targetTrans = target.transform;
            //还原可拾取道具和移动节点的属性
            RestoreMovementComp(entity);
            //还原可拾起道具的父节点
            if (PropParentDic.ContainsKey(uid))
            {
                var oParent = PropParentDic[uid];
                targetTrans.SetParent(oParent);
            }
            SetComponentEnable(target, true);
            //重置道具位置
            if (targetTrans.parent.name == "moveNode")
            {
                target.transform.localPosition = Vector3.zero;
            }
            else {
                target.transform.localPosition = pickablityData.oriPos;
            }
            target.transform.localEulerAngles = pickablityData.oriRot;
            target.transform.localScale = pickablityData.oriScale;
        }
        PropParentDic.Clear();
        mCompsDic.Clear();
        rpCompsDic.Clear();
    }

    /// <summary>
    /// 清除目前场景中拾起缓动队列
    /// </summary>
    private void ClearDOTweenSequenceList()
    {
        foreach (var sequence in DOTweenSequenceList)
        {
            sequence.Kill();
        }
        DOTweenSequenceList.Clear();
    }

    private RoleController GetRoleComByPlayerId(string playerId)
    {
        var gameMode = GlobalFieldController.CurGameMode;
        RoleController roleCom = null;
        switch (gameMode)
        {
            case GameMode.Edit:
            case GameMode.Play:
                roleCom = playerCom.transform.GetComponentInChildren<RoleController>(true);
                break;
            case GameMode.Guest:
            default:
                if (Global.Room != null)
                {
                    if (playerId == selfUid)
                    {
                        roleCom = playerCom.transform.GetComponentInChildren<RoleController>(true);
                    }
                    else
                    {
                        var playerCom = ClientManager.Inst.GetAnimControllerById(playerId);
                        if (playerCom != null)
                        {
                            roleCom = playerCom.transform.GetComponentInChildren<RoleController>(true);
                        }
                    }
                }
                else if (playerId == selfUid)
                {
                    roleCom = playerCom.transform.GetComponentInChildren<RoleController>(true);
                }
                break;
        }
        return roleCom;
    }

    public NodeBaseBehaviour GetBaseBevByPlayerId(string playerId)
    {
        var propUid = GetPropUidByPlayerId(playerId);
        if (propUid == -1) return null;
        var baseBev = GetBaseBevByUid(propUid);
        return baseBev;
    }

    private int GetPropUidByPlayerId(string playerId)
    {
        if (PlayerPickDic.ContainsKey(playerId))
        {
            return PlayerPickDic[playerId];
        }
        return -1;
    }

    public string GetPlayerIdByPropUid(int propUid)
    {
        foreach (var playerId in PlayerPickDic.Keys)
        {
            if (PlayerPickDic[playerId] == propUid)
            {
                return playerId;
            }
        }
        return null;
    }

    public NodeBaseBehaviour GetBaseBevByUid(int propUid)
    {
        if (PickabilityDic.ContainsKey(propUid))
        {
            var entity = PickabilityDic[propUid].entity;
            var gComp = entity.Get<GameObjectComponent>().bindGo;
            var nBevh = gComp.GetComponent<NodeBaseBehaviour>();
            return nBevh;
        }
        return null;
    }

    public void OnHandleClone(SceneEntity oEntity, SceneEntity nEntity)
    {
        if (CheckPickability(oEntity))
        {
            if (CheckCanSetPickability())
            {
                AddPickablityProp(nEntity, nEntity.Get<PickablityComponent>().anchors);
            }
            else
            {
                nEntity.Remove<PickablityComponent>();
            }
        }
    }

    /// <summary>
    /// 检查是否克隆，超过数量限制弹toast
    /// </summary>
    /// <returns></returns>
    public bool CheckCanClone()
    {
        if (!CheckCanSetPickability())
        {
            TipPanel.ShowToast(MAX_COUNT_TIP);
            return false;
        }

        return true;
    }

    public void OnCombineNode(SceneEntity entity)
    {
        if (entity == null) return;
        RemovePickablityProp(entity);
    }

    /// <summary>
    /// 当道具被拾取后，将身上的脚本设为Unable，不可交互
    /// </summary>
    /// <param name="target"></param>
    public void SetComponentEnable(GameObject target,bool isEnable)
    {
        var baseBevList = target.GetComponentsInChildren<NodeBaseBehaviour>();
        var colliderList = target.GetComponentsInChildren<Collider>();
        foreach (var bevComp in baseBevList)
        {
            bevComp.enabled = isEnable;
        }
        foreach (var coComp in colliderList)
        {
            coComp.enabled = isEnable;
        }
    }

    public void RemoveMovementComp(NodeBaseBehaviour nBevh) {
        nBevh.transform.DOKill();
        var entity = nBevh.entity;
        var gCom = entity.Get<GameObjectComponent>();
        var uid = gCom.uid;
        if (entity.HasComponent<MovementComponent>())
        {
            if (!mCompsDic.ContainsKey(uid))
            {
                var oMCom = entity.Get<MovementComponent>();
                var nMCom = entity.Get<MovementComponent>().Clone() as MovementComponent;
                // oMCom.moveState = 1;
                oMCom.tempMoveState = 1;
                mCompsDic.Add(uid, nMCom);
            }
        }

        if (entity.HasComponent<RPAnimComponent>())
        {
            if (!rpCompsDic.ContainsKey(uid))
            {
                var oRPCom = entity.Get<RPAnimComponent>();
                var nRPCom = entity.Get<RPAnimComponent>().Clone() as RPAnimComponent;
                // oRPCom.animState = 1;
                oRPCom.tempAnimState = 1;
                nRPCom.tempAnimState = 1;
                rpCompsDic.Add(uid, nRPCom);
            }
        }

        SceneSystem.Inst.RestoreSystemState();
    }

    private void RestoreMovementComp(SceneEntity entity)
    {
        var gCom = entity.Get<GameObjectComponent>();
        var pCom = entity.Get<PickablityComponent>();
        pCom.isPicked = false;
        pCom.alreadyPicked = false;
        var uid = gCom.uid;
        if (mCompsDic.ContainsKey(uid))
        {
            var mCom = mCompsDic[uid];
            var nCom = entity.Get<MovementComponent>();
            nCom.turnAround = mCom.turnAround;
            nCom.speedId = mCom.speedId;
            nCom.pathPoints = mCom.pathPoints;
            // nCom.moveState = mCom.moveState;
            nCom.tempMoveState = mCom.moveState;
        }
        if (rpCompsDic.ContainsKey(uid))
        {
            var rpCom = rpCompsDic[uid];
            var nCom = entity.Get<RPAnimComponent>();
            nCom.rSpeed = rpCom.rSpeed;
            nCom.uSpeed = rpCom.uSpeed;
            // nCom.animState = rpCom.animState;
            nCom.tempAnimState = rpCom.animState;
        }
    }


    /// <summary>
    /// 记录玩家拾取状态
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="baseBev"></param>
    public void RecordPlayerPick(string playerId,int uid)
    {
        if (!PlayerPickDic.ContainsKey(playerId))
        {
            PlayerPickDic.Add(playerId, uid);
        }
    }

    /// <summary>
    /// 记录玩家丢弃状态
    /// </summary>
    /// <param name="playerId"></param>
    public void RecordPlayerDrop(string playerId)
    {
        if (PlayerPickDic.ContainsKey(playerId))
        {
            PlayerPickDic.Remove(playerId);
        }
    }

    /// <summary>
    /// 判断玩家拾取状态
    /// </summary>
    public bool GetPlayerPickState(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return false;
        if (PlayerPickDic.ContainsKey(playerId))
        {
            return true;
        }
        return false;
    }

    public bool GetPropPickState(int uid)
    {
        var isPicked = PlayerPickDic.ContainsValue(uid) ? true : false;
        return isPicked;
    }

    /// <summary>
    /// 重置拾取状态
    /// </summary>
    public void ResetPlayerPickState()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.isPickedProp = false;
        }
        if (PlayerBaseControl.Inst)
        {
            RestPlayerAnimClips();
        }
        PlayerPickDic.Clear();
    }

    private void RestPlayerAnimClips()
    {
        ChangeAnimClips(selfUid, PICK_STATE.DROP);
        if(Global.Room != null)
        {
            foreach(var playerId in ClientManager.Inst.otherPlayerDataDic.Keys)
            {
                ChangeAnimClips(playerId, PICK_STATE.DROP);
            }
        }
    }

    /// <summary>
    /// 切换人物模型的动画状态机
    /// </summary>
    /// <param name="playerId"></param>
    public void ChangeAnimClips(string playerId,PICK_STATE state)
    {
        if (playerId == selfUid)
        {
            if(PlayerControlManager.Inst != null)
            {
                PlayerControlManager.Inst.isPickedProp = (state == PICK_STATE.CATCH);
                PlayerControlManager.Inst.ChangeAnimClips();
                if (SceneParser.Inst.GetBaggageSet() == 1)
                {
                    PlayerControlManager.Inst.isPickedProp = !BaggageManager.Inst.IsSelfBaggageNull();
                    PlayerBaseControl.Inst.SetBagPlayerRoleActive();
                    PlayerBaseControl.Inst.UpdateAnimatorCon();
                    return;
                }
                if (!PlayerBaseControl.Inst.isTps)
                {
                    PlayerBaseControl.Inst.UpdateAnimatorCon();
                    string animName = "";
                    if (state == PICK_STATE.CATCH)
                    {
                        animName = "collect";
                    }
                    CoroutineManager.Inst.StartCoroutine(PlayerBaseControl.Inst.SetPlayerRoleActive(0, (state == PICK_STATE.CATCH), animName));
                }
            }
        }
        else
        {
            var otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer)
            {
                if(state == PICK_STATE.CATCH)
                {
                    otherPlayer.SwitchPickupAnimClips();
                }
                else if(state == PICK_STATE.DROP)
                {
                    otherPlayer.SwitchNormalAnimClips();
                }
            }
        }
    }

    public void HandleCatchProp(NodeBaseBehaviour baseBev)
    {
        if (baseBev == null) return;
        //相机模式中，不允许拾取射击道具
        if (baseBev.entity.HasComponent<ShootWeaponComponent>() && 
            CameraModeManager.Inst != null && CameraModeManager.Inst.GetCurrentCameraMode() == CameraModeEnum.FreePhotoCamera)
        {
            TipPanel.ShowToast("Please exit camera mode before picking up shooting items.");
            return;
        }
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            TipPanel.ShowToast("You could not promote while picking up something");
        }
        playerCom.Move(Vector3.zero);
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.joyStick.JoystickReset();
        }

        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            SetIsSelfPicking(true);
            HandlePlayerCatchProp(baseBev, selfUid);
        }
        else if(GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            SetIsSelfPickingAutoRollback(true);
            SendMsgToSever(selfUid, baseBev, PICK_STATE.CATCH);
        }
    }

    public void HandleDropProp()
    {
        playerCom.Move(Vector3.zero);
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.joyStick.JoystickReset();
        }

        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            HandlePlayerDropProp(selfUid, true);
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            var baseBev = GetBaseBevByPlayerId(selfUid);
            if(baseBev != null)
            {
                SendMsgToSever(selfUid, baseBev, PICK_STATE.DROP);
            }
        }
    }

    public void LongPressHandleDropProp()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            HandlePlayerDropProp(selfUid);
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            var baseBev = GetBaseBevByPlayerId(selfUid);
            if (baseBev != null)
            {
                SendMsgToSever(selfUid, baseBev, PICK_STATE.DROP);
            }
        }
    }

    /// <summary>
    /// 根据所要拾起的NodeBaseBehaviour和PlayerId来确定是哪个玩家拾起了哪个道具
    /// </summary>
    /// <param name="baseBev"></param>
    /// <param name="playerId"></param>
    public void HandlePlayerCatchProp(NodeBaseBehaviour baseBev, string playerId, bool needAnim = true)
    {
        baseBev.transform.DOKill();
        var entity = baseBev.entity;
        var newBagBev = baseBev;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        var target = gComp.bindGo;
        var roleCom = GetRoleComByPlayerId(playerId);
        var pComp = entity.Get<PickablityComponent>();
        Transform targetTrans = target.transform;
        Transform pickPos;
        Transform pickNode;
        Transform backNode;

        ChangeAnimClips(playerId,PICK_STATE.CATCH);

        if (roleCom != null)
        {
            var animCom = roleCom.animCom;
            animCom.Play("collect");
            Sequence sequence = DOTween.Sequence();
            DOTweenSequenceList.Add(sequence);
            sequence.AppendInterval(0.16f);
            pickPos = roleCom.GetBandNode((int)BodyNode.PickPos);
            pickNode = roleCom.GetBandNode((int)BodyNode.PickNode);
            backNode = roleCom.GetBandNode((int)BodyNode.BackNode);
            pComp.isPicked = true;
            pComp.alreadyPicked = true;
            RemoveMovementComp(baseBev);
            SetComponentEnable(target, false);
            RecordPlayerPick(playerId, uid);
            if (SceneParser.Inst.GetBaggageSet() == 1 && isGetItemCallBack == false)
            {
                BaggageManager.Inst.AddNewItem(playerId, gComp.uid, gComp.resId);
                CheckHandleItem(playerId);
                baseBev = GetBagHandleItemBevByPlayerId(playerId);
                ChangeAnimClips(playerId, PICK_STATE.CATCH);
            }
            SetPickNodeRot(playerId);
            ParachuteManager.Inst.PlayPickEffect(playerId, entity, PICK_STATE.CATCH);
            if (StateManager.IsSnowCubeSkating)
            {
                PlayerSnowSkateControl.Inst.ForceStopSkating();
            }
            if (needAnim)
            {
                Tween tween = targetTrans.DOMove(pickPos.position, 0.2f).SetEase(Ease.Linear);
                sequence.Append(tween);
                tween.onComplete += () =>
                {
                    if (!PropParentDic.ContainsKey(uid))
                    {
                        PropParentDic.Add(uid, targetTrans.parent);
                    }
                    targetTrans.SetParent(pickNode);
                    targetTrans.localPosition = Vector3.zero;
                    targetTrans.localEulerAngles = GetOriQuaternion(uid);
                    targetTrans.position = targetTrans.TransformPoint(-pComp.anchors);
                    if (playerId == selfUid)
                    {
                        CatchPanel.Instance.SetCatchState(true);
                    }
                    if (SceneParser.Inst.GetBaggageSet() == 1 && isGetItemCallBack == true)
                    {
                        SetIsSelfPicking(playerId, false);
                        ParachuteManager.Inst.OnPickParacute(playerId, newBagBev, backNode, pickNode);
                        return;
                    }
                    WeaponSystemController.Inst.HandleWeaponPick(baseBev, true, playerId);
                    EdibilitySystemController.Inst.HandlePickableFood(baseBev, true, playerId);
                    SetIsSelfPicking(playerId, false);
                    ParachuteManager.Inst.OnPickParacute(playerId,newBagBev, backNode, pickNode);
                    FishingManager.Inst.HandlePickup(baseBev, true, playerId);
                };
            }
            else
            {
                if (!PropParentDic.ContainsKey(uid))
                {
                    PropParentDic.Add(uid, targetTrans.parent);
                }
                targetTrans.SetParent(pickNode);
                targetTrans.localPosition = Vector3.zero;
                targetTrans.localEulerAngles = GetOriQuaternion(uid);
                targetTrans.position = targetTrans.TransformPoint(-pComp.anchors);
                if (playerId == selfUid)
                {
                    CatchPanel.Instance.SetCatchState(true);
                }
                if (SceneParser.Inst.GetBaggageSet() == 1 && isGetItemCallBack == true)
                {
                    SetIsSelfPicking(playerId, false);
                    ParachuteManager.Inst.OnPickParacute(playerId, newBagBev, backNode, pickNode);
                    return;
                }
                WeaponSystemController.Inst.HandleWeaponPick(baseBev, true, playerId);
                EdibilitySystemController.Inst.HandlePickableFood(baseBev, true, playerId);
                SetIsSelfPicking(playerId, false);
                ParachuteManager.Inst.OnPickParacute(playerId,newBagBev, backNode, pickNode);
                FishingManager.Inst.HandlePickup(baseBev, true, playerId);
            }
        }
    }

    /// <summary>
    /// 根据PlayerId确定哪个玩家需要丢弃手中的道具
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="needAnim"></param>
    public void HandlePlayerDropProp(string playerId, bool needAnim = false, string curHoldItem = "")
    {
        if (!GetPlayerPickState(playerId)) return;
        var baseBev = GetBaseBevByPlayerId(playerId);
        if (baseBev == null) return;
        var entity = baseBev.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var pComp = entity.Get<PickablityComponent>();
        pComp.isPicked = false;
        var uid = gComp.uid;
        var roleCom = GetRoleComByPlayerId(playerId);
        var target = baseBev.gameObject;
        var targetTrans = target.transform;
        RecordPlayerDrop(playerId);
        SetComponentEnable(target, true);
        var isBaggageRemoved = false; //如果背包模块已经处理了，并且切换过animation,则拾取不做动画片段处理
        if (!(SceneParser.Inst.GetBaggageSet() == 1 && isGetItemCallBack == true))
        {
            WeaponSystemController.Inst.HandleWeaponPick(baseBev, false, playerId);
            EdibilitySystemController.Inst.HandlePickableFood(baseBev, false, playerId);
            FishingManager.Inst.HandlePickup(baseBev, false, playerId);
        }
        if (SceneParser.Inst.GetBaggageSet() == 1 && isGetItemCallBack == false)
        {
            BaggageManager.Inst.RemoveItem(playerId, uid, curHoldItem);
            if (!BaggageManager.Inst.IsPlayerBaggageNull(playerId))
            {
                isBaggageRemoved = true;
            }
        }
        ParachuteManager.Inst.PlayPickEffect(playerId,entity, PICK_STATE.DROP);
        SetPickNodeRot(playerId);
        if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        if (roleCom != null)
        {
            var animCom = roleCom.animCom;
            if (needAnim)
            {
                animCom.Play("discard");
                Sequence sequence = DOTween.Sequence();
                DOTweenSequenceList.Add(sequence);
                Tween tween2 = targetTrans.DOScale(targetTrans.localScale, 0.35f);
                sequence.Append(tween2);
                tween2.onComplete += () =>
                {
                    if (PropParentDic.ContainsKey(uid))
                    {
                        var oParent = PropParentDic[uid];
                        targetTrans.SetParent(oParent);
                        PropParentDic.Remove(uid);
                    }
                    else
                    {
                        targetTrans.SetParent(SceneBuilder.Inst.StageParent);
                    }
                    targetTrans.localEulerAngles = GetOriQuaternion(uid);
                    targetTrans.localScale = GetOriScale(uid);

                    float offset = 1.5f;
                    Vector3 dir = targetTrans.parent.TransformDirection(roleCom.transform.forward);
                    Vector3 moveDir = dir.normalized * offset;
                    moveDir.y = 0;
                    var endPos = targetTrans.position + moveDir;
                    Tween tween3 = targetTrans.DOMove(endPos, 0.17f).SetEase(Ease.Linear);
                    tween3.onComplete += () =>
                    {
                        ChangeAnimClips(playerId, PICK_STATE.DROP);
                    };

                    ParachuteManager.Inst.OnUnPickBag(playerId, baseBev);
                };
            }
            else
            {
                if (isBaggageRemoved == false)
                {
                    ChangeAnimClips(playerId, PICK_STATE.DROP);
                }
                if (PropParentDic.ContainsKey(uid))
                {
                    var oParent = PropParentDic[uid];
                    targetTrans.SetParent(oParent);
                    PropParentDic.Remove(uid);
                }
                else
                {
                    targetTrans.SetParent(SceneBuilder.Inst.StageParent);
                }
                targetTrans.localEulerAngles = GetOriQuaternion(uid);
                targetTrans.localScale = GetOriScale(uid);
                ParachuteManager.Inst.OnUnPickBag(playerId, baseBev);
            }
            if (entity.HasComponent<AttackWeaponComponent>())
            {
                var attackCmp = entity.Get<AttackWeaponComponent>();
                if (attackCmp.openDurability == 1 && attackCmp.curHits <= 0)
                {
                    target.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 处理物体被收集道具控制显隐时的操作
    /// </summary>
    /// <param name="data"></param>
    private void OnPickablePropActiveChange(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        ActiveData activeData = JsonConvert.DeserializeObject<ActiveData>(data);
        if (GetPropPickState(activeData.uid) && activeData.status == false)
        {
            var playerId = GetPlayerIdByPropUid(activeData.uid);
            if(playerId == selfUid)
            {
                HandleDropProp();
            }
            HandlePlayerDropProp(playerId, false);
        }
    }

    /// <summary>
    /// 通知联机服务器，用户拾取或丢弃道具
    /// </summary>
    /// <param name="sendPlayerId">该玩家Uid</param>
    /// <param name="baseBev"></param>
    /// <param name="pickState"></param>
    private void SendMsgToSever(string sendPlayerId, NodeBaseBehaviour baseBev, PICK_STATE pickState)
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            return;
        }
        var entity = baseBev.entity;
        var gComp = entity.Get<GameObjectComponent>();
        Vec3 curPos = new Vec3(baseBev.transform.position.x, baseBev.transform.position.y, baseBev.transform.position.z);

        PickPropSyncData pickPropSyncData = GetPickSyncData(entity);
        pickPropSyncData.status = (int)pickState;
        pickPropSyncData.playerId = sendPlayerId;
        pickPropSyncData.uid = gComp.uid;
        pickPropSyncData.position = JsonConvert.SerializeObject(curPos);

        Item itemData = new Item()
        {
            id = entity.Get<GameObjectComponent>().uid,
            type = (int)ItemType.PICK_PROP,
            data = JsonConvert.SerializeObject(pickPropSyncData),
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
        LoggerUtils.Log("PickabilityManager SendMsgToSever =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendPickPropCallBack);
    }

    /// <summary>
    /// 将不同道具的特有字段在协议数据中拼装，并获取协议数据
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>拼装好的协议数据</returns>
    public PickPropSyncData GetPickSyncData(SceneEntity entity)
    {
        var weaponType = WeaponSystemController.Inst.GetWeaponTypeInEntity(entity);
        PickPropSyncData pickPropSyncData = new PickPropSyncData();
        pickPropSyncData.propType = (int)PROP_TYPE.NORMAL;
        switch (weaponType)
        {
            case WeaponType.Attack:
                //拼装攻击道具特有信息
                var attackComp = entity.Get<AttackWeaponComponent>();
                pickPropSyncData.propType = (int)PROP_TYPE.ATTACK;
                pickPropSyncData.isOpenDurability = attackComp.openDurability;
                pickPropSyncData.curDurability = attackComp.curHits;
                break;
            case WeaponType.Shoot:
                // 拼装射击道具特有信息
                var shootComp = entity.Get<ShootWeaponComponent>();
                pickPropSyncData.propType = (int)PROP_TYPE.SHOOT;
                pickPropSyncData.hasCapacity = shootComp.hasCap;
                pickPropSyncData.capacity = shootComp.capacity;
                pickPropSyncData.curBullet = shootComp.curBullet;
                break;
            default:
                break;
        }
        if (entity.HasComponent<EdibilityComponent>())
        {
            pickPropSyncData.propType = (int)PROP_TYPE.FOOD;
        }
        return pickPropSyncData;
    }

    private void SendPickPropCallBack(int code, string data)
    {
        if (code != 0)//底层错误，业务层不处理
        {
            return;
        }
        SyncItemsReq ret = JsonConvert.DeserializeObject<SyncItemsReq>(data);
        int errorCode = 0;
        if (int.TryParse(ret.retcode, out errorCode))
        {
            //正常 是20000
            if (errorCode == (int)PropOptErrorCode.Exception)
            {
                LoggerUtils.Log("SendPickPropCallBack =>" + ret.retcode);
                TipPanel.ShowToast("This weapon has been picked up by other player");
                SetIsSelfPicking(false);
            }
        } 
    }
    /// <summary>
    /// 收到联机服务器的回包，有玩家进行拾取或丢弃的操作
    /// </summary>
    /// <param name="senderPlayerId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("PickabilityManager OnReceiveServer " + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type != (int)ItemType.PICK_PROP) continue;
                if(string.IsNullOrEmpty(item.data)) continue;
                PickPropSyncData data = JsonConvert.DeserializeObject<PickPropSyncData>(item.data);
                List<PickPropSyncData> tempRecordList = new List<PickPropSyncData>();
                tempRecordList.Add(data);
                HandleRecordData(tempRecordList);
            }
        }
        return true;
    }

    /// <summary>
    /// 获取房间内拾取道具的状态
    /// </summary>
    /// <param name="dataJson"></param>
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========PickabilityManager===>OnGetItemsCallback:" + dataJson);
        if (string.IsNullOrEmpty(dataJson)) return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
        if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
        {
            LoggerUtils.Log("[PickabilityManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
            return;
        }

        if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            HandleGetItems(getItemsRsp.items);
        }
    }

    private void HandleGetItems(Item[] items)
    {
        if (items == null)
        {
            LoggerUtils.Log("[PickabilityManager.OnGetItemsCallback]getItemsRsp.items is null");
            return;
        }
        recordDataList.Clear();
        for (int i = 0; i < items.Length; i++)
        {
            Item item = items[i];
            if (string.IsNullOrEmpty(item.data))
            {
                LoggerUtils.Log("[PickabilityManager.OnGetItemsCallback]getItemsRsp.item.Data is null");
                continue;
            }
            PickPropSyncData data = JsonConvert.DeserializeObject<PickPropSyncData>(item.data);
            data.uid = item.id;
            if (item.type != (int)ItemType.PICK_PROP) {
                continue;
            }
            //缓存GetItems道具信息
            recordDataList.Add(data);
        }
        isGetItemCallBack = true;
        HandleRecordData(recordDataList);
        isGetItemCallBack = false;
    }

    private void HandleRecordData(List<PickPropSyncData> recordDataList,bool needAnim = false)
    {
        //处理道具丢弃
        foreach (var data in recordDataList)
        {
            if (data.status == (int)PICK_STATE.DROP)
            {
                var uid = data.uid;
                var curHoldItem = data.curHoldItem;
                var playerId = GetPlayerIdByPropUid(uid);
                var baseBev = GetBaseBevByUid(uid);
                if (baseBev == null)
                    continue;
                baseBev.gameObject.SetActive(true);

                WeaponSystemController.Inst.UpdateWeaponInfo(baseBev, data);

                if (!string.IsNullOrEmpty(playerId))
                {
                    //情况2:当前有玩家拾起该道具 - 处理该玩家丢出道具
                    HandlePlayerDropProp(playerId, false, curHoldItem);
                }
                //情况1:当前没有玩家拾起该道具 - 直接更新该道具的位置
                var entity = baseBev.entity;
                var pComp = entity.Get<PickablityComponent>();
                pComp.isPicked = false;
                pComp.alreadyPicked = true;

                RemoveMovementComp(baseBev);
                var target = baseBev.gameObject;
                if (!string.IsNullOrEmpty(data.position))
                {
                    if (!data.position.Contains("|"))
                    {
                        Vec3 pos = JsonConvert.DeserializeObject<Vec3>(data.position);
                        if (pos != null)
                        {
                            target.transform.position = pos;
                        }
                    }
                    else
                    {
                        target.transform.position = GetPositionFromData(data.position);
                    }

                    if (PropParentDic.ContainsKey(uid))
                    {
                        target.transform.SetParent(PropParentDic[uid]);
                    }
                }
            }
        }
        //处理道具拾起
        foreach (var data in recordDataList)
        {
            if (data.status == (int)PICK_STATE.CATCH)
            {
                var uid = data.uid;//道具Uid
                var curPlayerId = data.playerId;//对应应该拾起道具的playerId
                var curPlayerIsPicked = GetPlayerPickState(curPlayerId);//对应玩家是否处于已拾起的状态
                var curPropOwner = GetPlayerIdByPropUid(uid);//当前道具的主人
                var curBaggage = SceneParser.Inst.GetBaggageSet() == 1? true:false; //当前背包是否开启
                var baseBev = GetBaseBevByUid(uid);
                if(curPlayerId == selfUid)
                {
                    needAnim = true;
                }
                if (baseBev == null)
                    continue;

                WeaponSystemController.Inst.UpdateWeaponInfo(baseBev, data);

                if (curPlayerIsPicked == false && string.IsNullOrEmpty(curPropOwner))
                {
                    //情况1:当前该道具没有被玩家拾起，处于空闲状态 - 直接让对应的玩家拾起该道具
                    HandlePlayerCatchProp(baseBev, curPlayerId, needAnim);
                }
                else if (curBaggage && curPlayerIsPicked == true && string.IsNullOrEmpty(curPropOwner))
                {
                    //背包情况1:当前该道具没有被玩家拾起，处于空闲状态 - 直接让对应的玩家拾起该道具
                    HandlePlayerCatchProp(baseBev, curPlayerId, false);
                }
                else if (curBaggage && curPlayerIsPicked == true && !string.IsNullOrEmpty(curPropOwner))
                {
                    //背包情况2:当前对应的玩家没有拾起道具，但是当前道具被其他玩家所拾起。则先让其他玩家丢下该道具，再让对应玩家拾起该道具
                    HandlePlayerDropProp(curPropOwner, false);
                    HandlePlayerCatchProp(baseBev, curPlayerId, false);
                }
                else if (curPlayerIsPicked == false && !string.IsNullOrEmpty(curPropOwner))
                {
                    //情况2:当前对应的玩家没有拾起道具，但是当前道具被其他玩家所拾起。则先让其他玩家丢下该道具，再让对应玩家拾起该道具
                    HandlePlayerDropProp(curPropOwner, false);
                    HandlePlayerCatchProp(baseBev, curPlayerId, needAnim);
                }
                else if (curPlayerIsPicked == true && string.IsNullOrEmpty(curPropOwner))
                {
                    //情况3:对应拾起的玩家手中已经拾起了道具，但是当前道具处于空闲状态，则先让此玩家丢弃老道具，再捡起新道具
                    HandlePlayerDropProp(curPlayerId, false);
                    HandlePlayerCatchProp(baseBev, curPlayerId, needAnim);
                }
                else if (curPlayerIsPicked == true && !string.IsNullOrEmpty(curPropOwner))
                {
                    //情况4:对应拾起的玩家手中已经拾起了道具，但是当前道具又被其他玩家所持有，则先让其他玩家丢下该道具，并且丢下对应玩家手中的道具，在让对应玩家拾起该道具
                    HandlePlayerDropProp(curPropOwner, false);
                    HandlePlayerDropProp(curPlayerId, false);
                    HandlePlayerCatchProp(baseBev, curPlayerId, needAnim);
                }
            }
        }
    }

    public void HandlePlayerCreated()
    {
        //TODO::此处在拉取人物时调用，新版拉取人物形象后不需要调用，调用会引起recordDataList被清空
        //HandleRecordData(recordDataList);
        //recordDataList.Clear();
    }

    private Vector3 GetPositionFromData(string data)
    {
        float DataMultiple = 10000f;
        float px, py, pz, rx, ry, rz, rw;
        bool isMoved = false;
        string[] vals = data.Split('|');
        if (float.TryParse(vals[0], out px) &&
            float.TryParse(vals[1], out py) &&
            float.TryParse(vals[2], out pz) &&
            float.TryParse(vals[3], out rx) &&
            float.TryParse(vals[4], out ry) &&
            float.TryParse(vals[5], out rz) &&
            float.TryParse(vals[6], out rw) &&
            bool.TryParse(vals[7], out isMoved)
           )
        {
            return new Vector3(px / DataMultiple, py / DataMultiple, pz / DataMultiple);
        }
        return Vector3.zero;
    }

    private void DealPortal()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            if (Global.Room != null)
            {
                DestroyOtherPlayerPick();
            }
        }
    }

    public void OnPlayerLeaveCurMap()
    {
        if(SceneParser.Inst.GetBaggageSet() == 1)
        {
            return;
        }
        HandleDropProp();
        HandlePlayerDropProp(selfUid, false);
    }

    public void OnBaggageLevelRoom(string playerId)
    {
        var itemList = BaggageManager.Inst.playerBaggageDic[playerId];
        RecordPlayerDrop(playerId);
        for (int i = itemList.Count - 1; i >= 0 ; i--)
        {
            RecordPlayerPick(playerId, itemList[i]);
            HandleDropProp();
        }
    }

    private void DestroyOtherPlayerPick()
    {
        var otherPlayerDic = ClientManager.Inst.otherPlayerDataDic;
        foreach(var other in otherPlayerDic)
        {
            var baseBev = GetBaseBevByPlayerId(other.Key);
            if (baseBev != null)
            {
                GameObject.Destroy(baseBev.gameObject);
            }
        }
    }

    public bool CheckSelfPickState()
    {
        if (playerCom != null)
        {
            var roleComp = playerCom.transform.GetComponentInChildren<RoleController>(true);
            if (roleComp != null)
            {
                var pickNode = roleComp.GetBandNode((int)BodyNode.PickNode);
                if (pickNode != null && pickNode.childCount > 0)//pickNode != null &&
                {
                    return true;
                }
            }
        }
        if((PlayerShootControl.Inst != null) && (PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null))
        {
            return true;
        }
        return false;
    }

    private void OnPlayerLeave(string playerId)
    {
        if(SceneParser.Inst.GetBaggageSet() == 1)
        {
            ClearPlayerBag(playerId);
        }
        else
        {
            HandlePlayerDropProp(playerId, false);
        }
    }

    public Vector3 GetOriQuaternion(int uid)
    {
        if (PickabilityDic.ContainsKey(uid))
        {
            var data = PickabilityDic[uid];
            return data.oriRot;
        }
        return Vector3.zero;
    }

    public Vector3 GetOriScale(int uid)
    {
        if (PickabilityDic.ContainsKey(uid))
        {
            var data = PickabilityDic[uid];
            return data.oriScale;
        }
        return Vector3.one;
    }

    public NodeBaseBehaviour GetBagHandleItemBevByPlayerId(string playerId)
    {
        var propUid = GetPropUidByPlayerId(playerId);
        if (propUid == -1) return null;
        var baseBev = GetBaseBevByUid(propUid);
        return baseBev;
    }

    public bool IsCanBeControlled(int uid)
    {
        var behav = GetBaseBevByUid(uid);
        if (behav!=null && behav.entity.HasComponent<PickablityComponent>())
        {
            if (behav.entity.Get<PickablityComponent>().isPicked)
            {
                //道具被拾取之后 不会被控制
                return false;
            }
        }
        return true;
    }

    //切换到手中道具
    public void CheckHandleItem(string playerId)
    {
        var itemList = BaggageManager.Inst.playerBaggageDic[playerId];
        for (int i = 0; i < itemList.Count; i++)
        {
            var tempBev = GetBaseBevByUid(itemList[i]);
            if (tempBev != null)
            {
                WeaponSystemController.Inst.HandleWeaponPick(tempBev, false, playerId);
                EdibilitySystemController.Inst.HandlePickableFood(tempBev, false, playerId);
                FishingManager.Inst.HandlePickup(tempBev, false, playerId);
                tempBev.gameObject.SetActive(false);
                BaggageManager.Inst.SetAttacheItem(tempBev);
            }
        }
        int itemUid = -1;
        var behav = GetBagHandleItemBevByPlayerId(playerId);
        if (behav != null)
        {
            behav.gameObject.SetActive(BaggageManager.Inst.IsCanShowItem(behav));
            var comp = behav.entity.Get<GameObjectComponent>();
            itemUid = comp.uid;
        }
        if (playerId == selfUid)
        {
            BaggagePanel.Instance.SetSelect(playerId, itemUid);
        }
    }

    public void SetBaggageChangeAnim(string playerId,int itemId,NodeBaseBehaviour behav)
    {
        RecordPlayerDrop(playerId);
        RecordPlayerPick(playerId, itemId);
        CheckHandleItem(playerId);
        ChangeAnimClips(playerId, PICK_STATE.CATCH);
        WeaponSystemController.Inst.HandleWeaponPick(behav, true, playerId);
        EdibilitySystemController.Inst.HandlePickableFood(behav, true, playerId);
        FishingManager.Inst.HandlePickup(behav, true, playerId);
    }

    public void ClearPlayerBag(string playerId)
    {
        if (!BaggageManager.Inst.playerBaggageDic.ContainsKey(playerId))
        {
            return;
        }
        RecordPlayerDrop(playerId);
        var itemList = BaggageManager.Inst.playerBaggageDic[playerId];
        for (int i = itemList.Count - 1; i >= 0 ; i--)
        {
            ClearSimpleItem(playerId, itemList[i]);
        }
        BaggageManager.Inst.playerBaggageDic.Remove(playerId);
        if (GameManager.Inst.ugcUserInfo.uid == playerId&& BaggagePanel.Instance)
        {
            BaggagePanel.Instance.ResetBaggage();
        }
    }

    private void ClearSimpleItem(string playerId, int uid)
    {
        var behav = GetBaseBevByUid(uid);
        if (behav == null) return;
        var entity = behav.entity;
        var pComp = entity.Get<PickablityComponent>();
        pComp.isPicked = false;
        var target = behav.gameObject;
        SetComponentEnable(target, true);
        ChangeAnimClips(playerId, PICK_STATE.DROP);
        target.SetActive(true);
        WeaponSystemController.Inst.HandleWeaponPick(behav, false, playerId);
        EdibilitySystemController.Inst.HandlePickableFood(behav, false, playerId);
        FishingManager.Inst.HandlePickup(behav, false, playerId);

        if (PropParentDic.ContainsKey(uid))
        {
            var oParent = PropParentDic[uid];
            target.transform.SetParent(oParent);
            PropParentDic.Remove(uid);
        }
        else
        {
            target.transform.SetParent(SceneBuilder.Inst.StageParent);
        }
        target.transform.localEulerAngles = GetOriQuaternion(uid);
    }

    public void OnBaggageDealPortal()
    {
        var selfId = GameManager.Inst.ugcUserInfo.uid;
        if (!string.IsNullOrEmpty(selfId))
        {
            if (CatchPanel.Instance)
            {
                CatchPanel.Instance.SetButtonVisible(true);
            }
            ClearPlayerBag(selfId);
            ChangeAnimClips(selfId, PICK_STATE.DROP);
        }
    }

    public void RePickItemPos(NodeBaseBehaviour behav)
    {
        var entity = behav.entity;
        if (!entity.HasComponent<PickablityComponent>())
        {
            return;
        }
        var gComp = entity.Get<GameObjectComponent>();
        var pComp = entity.Get<PickablityComponent>();
        var targetTrans = behav.transform;
        targetTrans.localPosition = Vector3.zero;
        targetTrans.localEulerAngles = GetOriQuaternion(gComp.uid);
        targetTrans.position = targetTrans.TransformPoint(-pComp.anchors);
    }
    /// <summary>
    /// 不同拾取道具设置不同拾取点的转向
    /// </summary>
    /// <param name="playerId"></param>
    public void SetPickNodeRot(string playerId)
    {
        var roleCom = GetRoleComByPlayerId(playerId);
        var pickNode = roleCom.GetBandNode((int)BodyNode.PickNode);
        var propBehav = GetBaseBevByPlayerId(playerId);
        if (propBehav != null)
        {
            if (propBehav.entity.HasComponent<ParachuteComponent>())
            {
                ParachuteManager.Inst.SetPickNodeRot(pickNode);
            }
            else if(propBehav.entity.Get<GameObjectComponent>().modelType == NodeModelType.FishingModel)
            {
                pickNode.localEulerAngles = Vector3.zero;
            }
            else
            {
                pickNode.localEulerAngles = oriPickNodeRot;
            }
        }
    }

    #region BaseFunc
    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var entity = behaviour.entity;
        if (entity != null)
        {
            var gComp = entity.Get<GameObjectComponent>();
            var uid = gComp.uid;
            if (PickabilityDic.ContainsKey(uid))
            {
                PickabilityDic.Remove(uid);
            }
        }
    }

    public void Clear()
    {
        DealPortal();
        PickabilityDic.Clear();
        PlayerPickDic.Clear();
        mCompsDic.Clear();
        rpCompsDic.Clear();
        PropParentDic.Clear();
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, OnPlayerLeave);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if(behaviour.entity.HasComponent<PickablityComponent>())
        {
           AddPickablityProp(behaviour.entity, behaviour.entity.Get<PickablityComponent>().anchors);
        }
    }

    // 道具损毁
    public void OnAttackPropDestroy()
    {
        HandleDropProp();
    }
    #endregion
}
