using System.Runtime.InteropServices.ComTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.UI;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;

public enum VIP_OPTION
{
    DETECTING = 1,
    ENTER_ZONE = 2,
    EXIT_ZONE = 3,
    ILLEGAL_PLAYERS = 4,
    TOKEN_INVALID = 5
}

public enum DETECTION_RESULT
{
    NONE = 0,
    FAIL = -1,
    SUCCESS = 1,
    NO_WALLET = 2
}

public class VIPZoneManager : ManagerInstance<VIPZoneManager>, IManager, IPVPManager, IUGCManager
{
    public Vector3 vipAreaSrcPosition = new Vector3(0, 0, 0);
    public Vector3 doorSrcPosition = new Vector3(0, 0, -2.815f);
    public Vector3 checkSrcPosition = new Vector3(0, 0, -4.0f);
    // 地图中所有VIP区域字典  uid <-> VIPZone Dict key:uid
    private Dictionary<int, VIPZoneBehaviour> vipZoneDict = new Dictionary<int, VIPZoneBehaviour>();
    // 地图中设置了DC的VIP区域的DC信息字典  dc mapId <-> DCInfo Dict key:mapId
    private Dictionary<string, DCInfo> vipZoneDcDict = new Dictionary<string, DCInfo>();
    // 玩家校验还在生效的区域列表
    private List<int> vipTokenAreaList = new List<int>();
    public CinemachineVirtualCamera cam;
    public GameObject player;
    private Vector3 camPos;
    private float zoomSpeed = 0.6f;
    private BudTimer showEffectTimer, hideEffectTimer;
    public bool isDetecting = false;
    private Dictionary<VIPComponentType, List<string>> ugcDic = new Dictionary<VIPComponentType, List<string>>();

    private List<VIPZoneBehaviour> allVIPZone = new List<VIPZoneBehaviour>();
    private const int COUNT_MAX = 99;
    private int curAreaId = -1;
    public bool IsInArea = false;
    public string selfId;

    public int resultState = (int)DETECTION_RESULT.NONE;
    private Dictionary<VIPZoneBehaviour, CheckTriggerData> vipTriggerData = new Dictionary<VIPZoneBehaviour, CheckTriggerData>();
    // 地图中所有VIP区域DC Icon 字典  url <-> icon Texture Dict key:url
    public Dictionary<string, Texture> dcIconDict = new Dictionary<string, Texture>();
    public void Init()
    {
        cam = CameraUtils.Inst.VirtualCamera;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.AddListener<bool>(MessageName.PosMove, PlayerMovePos);
    }

    public override void Release()
    {
        base.Release();
        allVIPZone.Clear();
        vipZoneDcDict.Clear();
        vipZoneDict.Clear();
        vipTokenAreaList.Clear();
        dcIconDict.Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.RemoveListener<bool>(MessageName.PosMove, PlayerMovePos);
    }

    private void HandlePackPanelShow(bool status)
    {
        for (int i = 0; i < allVIPZone.Count; i++)
        {
            allVIPZone[i].gameObject.SetActive(!status);
        }
    }

    private void OnChangeMode(GameMode mode)
    {
        for (int i = 0; i < allVIPZone.Count; i++)
        {
            allVIPZone[i].OnModeChange(mode);
        }

        if (mode == GameMode.Edit)
        {
            if (isDetecting)
            {
                SetDetectEffectVisible(selfId, false);

            }
            ExitDetectMode();

            ResetCamera();
            if (TokenDetectionPanel.Instance)
            {
                TokenDetectionPanel.Instance.OnClickClose();
            }

            if (PlayModePanel.Instance)
            {
                PlayModePanel.Instance.gameObject.SetActive(false);
            }
            //清楚检测台数据
            vipTriggerData.Clear();
            vipTokenAreaList.Clear();
            dcIconDict.Clear();
        }
        else
        {
            // 关闭没有选择DC的VIP区域限制
            CloseNoTokenVIPZoneLimit();
#if UNITY_EDITOR
            selfId = TestNetParams.testHeader.uid;
#else
            selfId = Player.Id;
#endif
        }
    }

    public void CloseNoTokenVIPZoneLimit()
    {
        foreach (var vipZone in allVIPZone)
        {
            // 未选择 DC
            if (IsVIPZoneNoLimit(vipZone))
            {
                vipZone.SignCanEnterDoor();
                vipZone.DisableAllWallCollider();
            }
        }
    }

    public bool IsVIPZoneNoLimit(VIPZoneBehaviour vipZoneBehaviour)
    {
        var vipZoneCom = vipZoneBehaviour.entity.Get<VIPZoneComponent>();
        var mapId = vipZoneCom.passId;
        if (mapId == null)
        {
            vipZoneBehaviour.SignCanEnterDoor();
            return true;
        }
        return false;
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour is VIPZoneBehaviour vipZoneBehaviour)
        {
            OnRemoveVIPZone(vipZoneBehaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour is VIPZoneBehaviour vipZoneBehaviour)
        {
            OnCreateNewVIPZone(vipZoneBehaviour);
        }
    }

    public void Clear()
    {
        allVIPZone.Clear();
        vipZoneDcDict.Clear();
        vipZoneDict.Clear();
        vipTokenAreaList.Clear();
        dcIconDict.Clear();
    }

    public void OnReset()
    {
        ForceExitDetectMode();
        AllPlayersForceExitVIPZone();
        ResetVIPZone();
    }

    public void ForceExitDetectMode()
    {
        if (isDetecting)
        {
            SetDetectEffectVisible(selfId, false);
        }
        ExitDetectMode();

        ResetCamera();
        if (TokenDetectionPanel.Instance)
        {
            TokenDetectionPanel.Instance.OnClickClose();
        }
    }
    
    public void ResetVIPZone()
    {
        foreach (var vipZone in allVIPZone)
        {
            // 未选择 DC
            if (IsVIPZoneNoLimit(vipZone))
            {
                vipZone.SignCanEnterDoor();
                vipZone.DisableAllWallCollider();
            }
        else
            {
                vipZone.ResumeDoorInterceptStatus();
            }
        }
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        if (!ugcCombBehaviour.entity.HasComponent<VIPCheckComponent>())
        {
            return;
        }
        var vipCheckBoundControl = ugcCombBehaviour.gameObject.GetComponent<VIPCheckBoundControl>();
        if (vipCheckBoundControl != null)
        {
            vipCheckBoundControl.UpdateEffectShow();
            //游玩模式刷新一下检测区域
            if (GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                VIPZoneBehaviour vipZoneBehaviour = ugcCombBehaviour.gameObject.GetComponentInParent<VIPZoneBehaviour>();
                if (vipZoneBehaviour != null)
                {
                    vipZoneBehaviour.RefreshCheckColliderSize(vipCheckBoundControl.center, vipCheckBoundControl.size);
                }
            }
        }
    }

    public GameObject CreateVIPZoneInEdit()
    {
        if (allVIPZone.Count >= COUNT_MAX)
        {
            TellMax();
            return null;
        }
        var vipZoneBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPZoneCreater, VIPZoneBehaviour>();
        VIPZoneCreater.SetDefaultData(vipZoneBehaviour);
        //创建下面的组件
        //碰撞盒
        var vipAreaBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPAreaCreater, VIPAreaBehaviour>();
        VIPAreaCreater.SetDefaultData(vipAreaBehaviour);
        vipAreaBehaviour.gameObject.transform.SetParent(vipZoneBehaviour.transform);
        vipAreaBehaviour.gameObject.transform.localPosition = vipAreaSrcPosition;
        //检测区域
        var vipCheckBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPCheckCreater, VIPCheckBehaviour>();
        VIPCheckCreater.SetDefaultData(vipCheckBehaviour);
        VIPCheckCreater.UpdateModel(vipCheckBehaviour, VIPZoneConstant.CHECK_ID_4);
        vipCheckBehaviour.gameObject.transform.SetParent(vipZoneBehaviour.transform);
        vipCheckBehaviour.transform.localPosition = checkSrcPosition;
        vipCheckBehaviour.GetComponent<VIPCheckBoundControl>().UpdateEffectShow();
        //门
        var vipDoorBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPDoorCreater, VIPDoorBehaviour>();
        VIPDoorCreater.SetDefaultData(vipDoorBehaviour);
        VIPDoorCreater.UpdateModel(vipDoorBehaviour, VIPZoneConstant.DOOR_ID_4);
        vipDoorBehaviour.gameObject.transform.SetParent(vipZoneBehaviour.transform);
        vipDoorBehaviour.gameObject.transform.localPosition = doorSrcPosition;
        //门父类
        var vipDoorWrapBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPDoorWrapCreater, VIPDoorWrapBehaviour>();
        VIPDoorWrapCreater.SetDefaultData(vipDoorWrapBehaviour);
        vipDoorWrapBehaviour.gameObject.transform.SetParent(vipZoneBehaviour.transform);
        vipDoorWrapBehaviour.gameObject.transform.localPosition = doorSrcPosition;
        //门特效
        var vipDoorEffectBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPDoorEffectCreater, VIPDoorEffectBehaviour>();
        VIPDoorEffectCreater.SetDefaultData(vipDoorEffectBehaviour);
        VIPDoorEffectCreater.UpdateModel(vipDoorEffectBehaviour, (int)GameResType.VIPDoorEffect4);
        vipDoorEffectBehaviour.gameObject.transform.SetParent(vipZoneBehaviour.transform);
        vipDoorEffectBehaviour.gameObject.transform.localPosition = doorSrcPosition;
        EditModeController.SetSelect(vipZoneBehaviour.entity);
        return vipZoneBehaviour.gameObject;
    }

    public void HideOtherVIPZone(VIPZoneBehaviour current)
    {
        foreach (var zone in allVIPZone)
        {
            if (zone == current)
            {
                continue;
            }
            zone.gameObject.SetActive(false);
        }
    }

    public void ShowAllVIPZone()
    {
        foreach (var zone in allVIPZone)
        {
            zone.gameObject.SetActive(true);
        }
    }

    public void FindComponentsInChild(GameObject go,
        Action<NodeBaseBehaviour, IComponent> OnFindDoor = null,
        Action<NodeBaseBehaviour, IComponent> OnFindDoorEffect = null,
        Action<NodeBaseBehaviour, IComponent> OnFindCheck = null)
    {
        VIPDoorBehaviour doorBehaviour = go.GetComponentInChildren<VIPDoorBehaviour>();
        if (doorBehaviour != null)
        {
            VIPDoorComponent c = doorBehaviour.entity.Get<VIPDoorComponent>();
            OnFindDoor?.Invoke(doorBehaviour, c);
        }
        VIPCheckBehaviour checkBehaviour = go.GetComponentInChildren<VIPCheckBehaviour>();
        if (checkBehaviour != null)
        {
            VIPCheckComponent c = checkBehaviour.entity.Get<VIPCheckComponent>();
            OnFindCheck?.Invoke(checkBehaviour, c);
        }
        VIPDoorEffectBehaviour effectBehaviour = go.GetComponentInChildren<VIPDoorEffectBehaviour>();
        if (effectBehaviour != null)
        {
            VIPDoorEffectComponent c = effectBehaviour.entity.Get<VIPDoorEffectComponent>();
            OnFindDoorEffect?.Invoke(effectBehaviour, c);
        }

        UGCCombBehaviour[] ugcs = go.GetComponentsInChildren<UGCCombBehaviour>();
        foreach (var ugcCombBehaviour in ugcs)
        {
            if (ugcCombBehaviour.entity.HasComponent<VIPDoorComponent>())
            {
                VIPDoorComponent c = ugcCombBehaviour.entity.Get<VIPDoorComponent>();
                OnFindDoor?.Invoke(ugcCombBehaviour, c);
            }
            else if (ugcCombBehaviour.entity.HasComponent<VIPCheckComponent>())
            {
                VIPCheckComponent c = ugcCombBehaviour.entity.Get<VIPCheckComponent>();
                OnFindCheck?.Invoke(ugcCombBehaviour, c);
            }
            else if (ugcCombBehaviour.entity.HasComponent<VIPDoorEffectComponent>())
            {
                VIPDoorEffectComponent c = ugcCombBehaviour.entity.Get<VIPDoorEffectComponent>();
                OnFindDoorEffect?.Invoke(ugcCombBehaviour, c);
            }
        }
    }

    public NodeBaseBehaviour FindDoor(NodeBaseBehaviour doorWrap)
    {
        NodeBaseBehaviour door = doorWrap.GetComponentInChildren<VIPDoorBehaviour>();
        if (door == null)
        {
            UGCCombBehaviour[] ugcs = doorWrap.GetComponentsInChildren<UGCCombBehaviour>();
            foreach (var ugc in ugcs)
            {
                if (ugc.entity.HasComponent<VIPDoorComponent>())
                {
                    door = ugc;
                    break;
                }
            }
        }
        return door;
    }

    public void AddComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        VIPDoorCreater.SetData(behaviour, data, true);
        VIPCheckCreater.SetData(behaviour, data, true);
    }

    public void OnUgcUseSave(VIPComponentType type, string id)
    {
        if (!ugcDic.ContainsKey(type))
        {
            ugcDic.Add(type, new List<string>());
        }
        if (!ugcDic[type].Contains(id))
        {
            ugcDic[type].Add(id);
        }
    }

    public List<string> GetUseUgcs(VIPComponentType type)
    {
        if (ugcDic.ContainsKey(type))
        {
            return ugcDic[type];
        }
        return new List<string>();
    }

    public bool IsVIPZoneComponent(SceneEntity entity)
    {
        GameObjectComponent gameObjectComponent = entity.Get<GameObjectComponent>();
        NodeModelType modelType = gameObjectComponent.modelType;
        if (modelType == NodeModelType.VIPArea
            || modelType == NodeModelType.VIPZone
            || modelType == NodeModelType.VIPDoor
            || modelType == NodeModelType.VIPCheck
            || modelType == NodeModelType.VIPDoorEffect)
        {
            return true;
        }

        if (modelType == NodeModelType.CommonCombine)
        {
            if (entity.HasComponent<VIPDoorComponent>() || entity.HasComponent<VIPCheckComponent>())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCanSelectComponent(SceneEntity entity)
    {
        GameObjectComponent gameObjectComponent = entity.Get<GameObjectComponent>();
        NodeModelType modelType = gameObjectComponent.modelType;
        if (modelType == NodeModelType.VIPArea
            || modelType == NodeModelType.VIPDoor
            || modelType == NodeModelType.VIPCheck
            || modelType == NodeModelType.VIPDoorEffect)
        {
            return true;
        }

        if (modelType == NodeModelType.CommonCombine)
        {
            if (entity.HasComponent<VIPDoorComponent>() || entity.HasComponent<VIPCheckComponent>())
            {
                return true;
            }
        }

        return false;
    }

    public void OnPlayerEnterCheckArea(VIPZoneBehaviour vipZoneBehaviour)
    {
        var itemId = vipZoneBehaviour.entity.Get<GameObjectComponent>().uid;
        if (vipTokenAreaList.Contains(itemId))
        {
            return;
        }
        var vipZoneCom = vipZoneBehaviour.entity.Get<VIPZoneComponent>();
        // 未选择 DC
        if (vipZoneBehaviour.entity.HasComponent<VIPZoneComponent>())
        {
            if (IsVIPZoneNoLimit(vipZoneBehaviour))
            {
                return;
            }
        }
        EnterDetectMode();
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SendPlayerDetect(itemId);
        }
        else
        {
            vipTokenAreaList.Add(itemId);
            CoroutineManager.Inst.StartCoroutine(PlayerCanEnter(vipZoneBehaviour));
        }
        if (vipZoneBehaviour.entity.HasComponent<VIPZoneComponent>())
        {
            var mapId = vipZoneCom.passId;
            GetDCInfo(mapId);
        }
    }

    public IEnumerator PlayerCanEnter(VIPZoneBehaviour vipZoneBehaviour)
    {
        yield return new WaitForSeconds(2);
        if (TokenDetectionPanel.Instance && TokenDetectionPanel.Instance.gameObject.activeSelf)
        {
            TokenDetectionPanel.Instance.ShowDetectResult((int)DETECTION_RESULT.SUCCESS);
        }

        vipZoneBehaviour.SignCanEnterDoor();
    }

    public void OnPlayerReceiveDoor(VIPZoneBehaviour vipZoneBehaviour)
    {
        //未检测或检测未通过不能进入门
        if (!IsVIPZoneNoLimit(vipZoneBehaviour) && !IsInArea)
        {
            var itemId = vipZoneBehaviour.entity.Get<GameObjectComponent>().uid;
            if (!vipTokenAreaList.Contains(itemId))
            {
                vipZoneBehaviour.canEnter = false;
                return;
            }
        }

        vipZoneBehaviour.DisableFaceWallCollider();
    }

    public void OnPlayerLeaveDoor(VIPZoneBehaviour vipZoneBehaviour)
    {
        if (IsVIPZoneNoLimit(vipZoneBehaviour))
        {
            vipZoneBehaviour.SignCanEnterDoor();
            vipZoneBehaviour.DisableAllWallCollider();
            return;
        }
        
        vipZoneBehaviour.EnableFaceWallCollider();
        if (PlayerBaseControl.Inst == null)
        {
            return;
        }

        CheckPlayerInArea(vipZoneBehaviour);
    }

    public void CheckPlayerInArea(VIPZoneBehaviour vipZoneBehaviour)
    {
        var pos = PlayerBaseControl.Inst.transform.position;
        bool isInArea = vipZoneBehaviour.IsPlayerInArea(pos);
        if (isInArea == IsInArea)
        {
            return;
        }

        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            var itemId = vipZoneBehaviour.entity.Get<GameObjectComponent>().uid;
            if (IsInArea)
            {
                SendPlayerExitVIPZone(itemId);
            }
            else
            {
                SendPlayerEnterVIPZone(itemId);
            }
        }
        else
        {
            IsInArea = isInArea;
        }
    }

    //聚焦到Player正面
    public void SetZoomInPlayer(Transform target)
    {
        if (!NeedZoomInPlayer())
        {
            return;
        }

        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.playerAnim)
        {
            camPos = cam.transform.position;
            cam.LookAt = null;
            cam.Follow = null;
            var trs = PlayerBaseControl.Inst.playerAnim.transform;
            trs.localRotation = Quaternion.Euler(new Vector3(0, -180, 0));
            cam.transform.DOMove(trs.position + cam.transform.right.normalized * 1 + cam.transform.forward.normalized * -4 + cam.transform.up.normalized, 0.6f).SetAutoKill(false);
        }
    }

    public bool NeedZoomInPlayer()
    {
        if (CameraModeManager.Inst.GetCurrentCameraMode() == CameraModeEnum.FreePhotoCamera)
        {
            return false;
        }

        if (StateManager.IsInSelfieMode)
        {
            return false;
        }

        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps)
        {
            return false;
        }
        return true;
    }

    public void EnterDetectMode()
    {
        HideOrShowPanel(false);
        SetZoomInPlayer(PlayerManager.Inst.selfPlayer.transform);

        TokenDetectionPanel.Show();
        ClearTimer();
        showEffectTimer = TimerManager.Inst.RunOnce("showEffect", 0.2f, () =>
        {
            SetDetectEffectVisible(selfId, true);
        });
        isDetecting = true;
    }

    public void ExitDetectMode()
    {
        ZoomOutPlayer();
        HideOrShowPanel(true);

        isDetecting = false;
        ClearTimer();
    }


    public void HideOrShowPanel(bool Active)
    {
        if(GlobalFieldController.CurGameMode == GameMode.Edit )
        {
            return;
        }

        UIControlManager.Inst.CallUIControl(Active ? "detection_mode_exit" : "detection_mode_enter");

        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(Active);
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.gameObject.SetActive(Active);
        }

        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(Active);
        }

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(Active);
        }

        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(Active);
        }
        if(PromoteCtrPanel.Instance != null)
        {
            PromoteCtrPanel.Instance.SetBtnPanelVisible(Active);
        }
    }

    private void ZoomOutPlayer()
    {
        cam.transform.DOMove(camPos, zoomSpeed).OnComplete(() =>
        {
            ResetCamera();
        });
    }

    public void ResetCamera()
    {
        if (player == null)
        {
            player = GameObject.Find("Play Mode Camera Center");
        }
        if (player == null)
        {
            return;
        }
        cam.LookAt = player.transform;
        cam.Follow = player.transform;
        isDetecting = false;
    }

    // 播放玩家检测特效
    public void SetDetectEffectVisible(string playerId, bool visible, float delayTime = 5)
    {
        GameObject detectEffect = null;
        Transform detectEffectTrans = null;
        if (playerId == selfId)
        {
            detectEffectTrans = PlayerBaseControl.Inst.playerAnim.transform.Find("VIP_DetectEffect(Clone)");
            if (detectEffectTrans == null)
            {
                var detectEffectObj = ResManager.Inst.LoadRes<GameObject>("Effect/VIP_portal_mechanical/VIP_DetectEffect");
                detectEffect = UnityEngine.Object.Instantiate(detectEffectObj, PlayerBaseControl.Inst.playerAnim.transform);
            }
            else
            {
                detectEffect = detectEffectTrans.gameObject;
            }
        }
        else if (ClientManager.Inst.GetOtherPlayerComById(playerId))
        {
            var otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer)
            {
                detectEffectTrans = otherPlayer.transform.Find("VIP_DetectEffect(Clone)");
            }
            if (detectEffectTrans == null)
            {
                var detectEffectObj = ResManager.Inst.LoadRes<GameObject>("Effect/VIP_portal_mechanical/VIP_DetectEffect");
                detectEffect = UnityEngine.Object.Instantiate(detectEffectObj, otherPlayer.transform);
            }
            else
            {
                detectEffect = detectEffectTrans.gameObject;
            }
        }

        if (detectEffect)
        {
            //播放人物检测特效
            detectEffect.gameObject.SetActive(visible);
            ClearTimer();

            var name = visible ? "Play_VipArea_Scanning_Loop" : "Stop_VipArea_Scanning_Loop";
            PlayVIPAreaSound(name, detectEffect);

            if (visible)
            {
                // 检测特效隐藏
                hideEffectTimer = TimerManager.Inst.RunOnce("hideEffect", delayTime, () =>
                {
                    HideEffectNode(detectEffect);
                });
            }
        }
    }

    public void PlayVIPAreaSound(string name, GameObject gameObject)
    {
        if (GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            AKSoundManager.Inst.SetSwitch("", "", gameObject);
            AKSoundManager.Inst.PostEvent(name, gameObject);
        }
    }

    public void HideEffectNode(GameObject effectNode)
    {
        if (effectNode != null)
        {
            PlayVIPAreaSound("Stop_VipArea_Scanning_Loop", effectNode);
            effectNode.SetActive(false);
        }
        if (isDetecting)
        {
            ExitDetectMode();
        }
    }

    private void ClearTimer()
    {
        // TimerManager.Inst.Stop(hideEffectTimer);
        TimerManager.Inst.Stop(showEffectTimer);
    }

    // 发送玩家正在检测
    public void SendPlayerDetect(int itemId)
    {
        SendVIPZoneReq(itemId, 1);
    }

    //发送玩家进入 VIP 区域
    public void SendPlayerEnterVIPZone(int itemId)
    {
        SendVIPZoneReq(itemId, 2);
    }

    // 发送玩家离开 VIP 区域
    public void SendPlayerExitVIPZone(int itemId)
    {
        SendVIPZoneReq(itemId, 3);
    }

    // 更新玩家状态
    public void RefreshPlayerState(string sendPlayerId, VIPZoneSendData data)
    {
        switch (data.option)
        {
            case (int)VIP_OPTION.DETECTING:
                if (data.status == (int)DETECTION_RESULT.SUCCESS)
                {
                    //检测通过
                    if (data.item != null)
                    {
                        VIPZoneBehaviour area;
                        var areaId = data.item.id;
                        if (vipZoneDict.TryGetValue(areaId, out area))
                        {
                            area.SignCanEnterDoor();
                        }

                        vipTokenAreaList.Add(areaId);
                    }
                }
                resultState = data.status;
                if (TokenDetectionPanel.Instance && TokenDetectionPanel.Instance.gameObject.activeSelf && TokenDetectionPanel.Instance.IsCanShowResult)
                {
                    TokenDetectionPanel.Instance.ShowDetectResult(resultState);
                }
                break;
            case (int)VIP_OPTION.ENTER_ZONE:
                // 同步玩家进入区域
                if (data.item != null)
                {
                    PlayerEnterVIPZone(data.item.id);
                }
                break;
            case (int)VIP_OPTION.EXIT_ZONE:
                // 同步玩家离开区域
                IsInArea = false;
                curAreaId = 0;
                RefreshVIPZoneTokenState();
                break;
            case (int)VIP_OPTION.ILLEGAL_PLAYERS:
                RemoveIllegalPlayers(data.playerIds);
                break;
            case (int)VIP_OPTION.TOKEN_INVALID:
                //token 校验结果失效
                if (data.item != null)
                {
                    var areaId = data.item.id;
                    VIPZoneBehaviour area;
                    if ((!IsInArea || areaId != curAreaId) && vipZoneDict.TryGetValue(areaId, out area))
                    {
                        area.ResumeDoorInterceptStatus();
                    }
                    vipTokenAreaList.Remove(areaId);
                }
                break;
            default:
                break;
        }
    }

    public void PlayerEnterVIPZone(int itemId)
    {
        IsInArea = true;
        curAreaId = itemId;
        VIPZoneBehaviour area;
        if (vipZoneDict.TryGetValue(curAreaId, out area))
        {
            area.SignCanEnterDoor();
        }
    }

    // 移出非法玩家
    public void RemoveIllegalPlayers(List<string> playerIdList)
    {
        if (playerIdList == null || playerIdList.Count <= 0)
        {
            return;
        }

        foreach (var playerId in playerIdList)
        {
            if (playerId == selfId)
            {
                TipPanel.ShowToast("You don't have the DC Token of this VIP Zone :(");

                PlayerReturnSpawn();
                // 同步玩家离开区域
                IsInArea = false;
                curAreaId = 0;
                RefreshVIPZoneTokenState();
            }
        }
    }

    public void  PlayerReturnSpawn()
    {
        if (StateManager.IsParachuteUsing)
        {
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
        else if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        if (StateManager.IsOnSlide)
        {
            PlayerSlidePipeControl iCtrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
            if (iCtrl != null) iCtrl.ForceAbortSlideAction();
        }

        if(StateManager.PlayerIsMutual)
        {
            PlayerBaseControl.Inst.animCon.ReleaseHand();
            PlayerBaseControl.Inst.animCon.StopLoop();
            PlayerMutualControl.Inst.EndMutual();
        }

        MessageHelper.Broadcast(MessageName.PosMove, false);
        PlayerBaseControl.Inst.SetPosToSpawnPoint();
    }

    public void SendVIPZoneReq(int itemId, int option)
    {
        VIPZoneSendData vipZoneData = new VIPZoneSendData();
        vipZoneData.mapId = GlobalFieldController.CurMapInfo.mapId;
        vipZoneData.option = option;
        Item item = new Item();
        item.id = itemId;
        item.type = (int)ItemType.VIP_ZONE;
        vipZoneData.item = item;

#if UNITY_EDITOR
        vipZoneData.playerId = TestNetParams.testHeader.uid;
#else
            vipZoneData.playerId = Player.Id;
#endif
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.VIPZone,
            data = JsonConvert.SerializeObject(vipZoneData),
        };
        LoggerUtils.Log("VIPZoneManager SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendVIPZoneCallBack);
    }

    private void SendVIPZoneCallBack(int code, string data)
    {
        if (code != 0)//底层错误，业务层不处理
        {
            return;
        }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log($"VIPZoneManager OnReceiveServer ==> => senderPlayer:{senderPlayerId}, msg:{msg}");
        VIPZoneSendData vipZoneData = JsonConvert.DeserializeObject<VIPZoneSendData>(msg);
        var mapId = vipZoneData.mapId;
        var item = vipZoneData.item;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            if (item != null && item.type == (int)ItemType.VIP_ZONE)
            {
                if (string.IsNullOrEmpty(item.data))
                {
                    LoggerUtils.Log("[VIPZoneManager.OnGetItemsCallback]getItemsRsp.item.Data is null");
                }

                if (vipZoneData != null)
                {
                    LoggerUtils.Log("[VIPZoneManager.OnGetItemsCallback]getItemsRsp.itemData : " + vipZoneData);
                    // 处理联机表现
                    if (senderPlayerId == selfId)
                    {
                        RefreshPlayerState(senderPlayerId, vipZoneData);
                    }
                    else
                    {
                        if (vipZoneData.option == (int)VIP_OPTION.DETECTING)
                        {
                            SetDetectEffectVisible(senderPlayerId, true);
                        }
                        else if (vipZoneData.option == (int)VIP_OPTION.ILLEGAL_PLAYERS)
                        {
                            RemoveIllegalPlayers(vipZoneData.playerIds);
                        }
                    }
                }
            }
        }
        return true;
    }

    public void OnGetItemsCallback(string dataJson)
    {
        if (!string.IsNullOrEmpty(dataJson))
        {
            LoggerUtils.Log($"VIPZoneManager.OnGetItemsCallback=={dataJson}");
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[VIPZoneManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                PlayerCustomData[] playerCustomDatas = getItemsRsp.playerCustomDatas;
                var selfPlayerId = Player.Id;
#if UNITY_EDITOR
                selfPlayerId = TestNetParams.testHeader.uid;
#endif
                for (int i = 0; i < playerCustomDatas.Length; i++)
                {
                    PlayerCustomData playerData = playerCustomDatas[i];
                    ActivityData[] activitiesData = playerData.activitiesData;
                    if (activitiesData == null || playerData.playerId != selfPlayerId)
                    {
                        continue;
                    }
                    for (int n = 0; n < activitiesData.Length; n++)
                    {
                        ActivityData activeData = activitiesData[n];
                        if (activeData != null && activeData.activityId == ActivityID.VipZone)
                        {
                            VIPTokenData vipTokenData = JsonConvert.DeserializeObject<VIPTokenData>(activeData.data);
                            if (vipTokenData != null)
                            {
                                LoggerUtils.Log("[VIPZoneManager.OnGetItemsCallback]getItemsRsp.itemData : " + vipTokenData);
                                // 处理联机表现：更新生效区域
                                vipTokenAreaList = vipTokenData.areaIds;

                                if (vipTokenData.curAreaId > 0)
                                {
                                    PlayerEnterVIPZone(vipTokenData.curAreaId);
                                }
                            }
                            else
                            {
                                vipTokenAreaList.Clear();
                            }
                            RefreshVIPZoneTokenState();
                        }
                    }
                }
            }
        }
    }

    public void AddVIPZone(VIPZoneBehaviour behaviour)
    {
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        Debug.Log(vipZoneDict.Count + "    " + uid);
        if (!vipZoneDict.ContainsKey(uid))
        {
            vipZoneDict.Add(uid, behaviour);
        }
    }

    public void RemoveVipZone(VIPZoneBehaviour behaviour)
    {
        if (behaviour == null) return;
        if (vipZoneDict.ContainsValue(behaviour))
        {
            int uid = behaviour.entity.Get<GameObjectComponent>().uid;
            vipZoneDict.Remove(uid);
        }
    }

    // 全部玩家离开VIP 区域返回出生点，用于PVP重置
    public void AllPlayersForceExitVIPZone()
    {
        PlayerReturnSpawn();
        IsInArea = false;
        curAreaId = 0;
        vipTokenAreaList.Clear();
        RefreshVIPZoneTokenState();
    }

    public void PlayerMovePos(bool isTrap = false)
    {
        if (curAreaId != 0)
        {
            VIPZoneBehaviour area;
            if (vipZoneDict.TryGetValue(curAreaId, out area))
            {
                CheckPlayerInArea(area);
            }
        }
    }


    public void OnCreateNewVIPZone(VIPZoneBehaviour behaviour)
    {
        if (!allVIPZone.Contains(behaviour))
        {
            allVIPZone.Add(behaviour);
            // AddVIPZone(behaviour);
        }
    }

    private void OnRemoveVIPZone(VIPZoneBehaviour vipZoneBehaviour)
    {
        if (allVIPZone.Contains(vipZoneBehaviour))
        {
            allVIPZone.Remove(vipZoneBehaviour);
            RemoveVipZone(vipZoneBehaviour);
        }
    }

    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponent<VIPZoneBehaviour>() != null)
        {
            if (allVIPZone.Count >= COUNT_MAX)
            {
                TellMax();
                return false;
            }
        }

        return true;
    }

    private void TellMax()
    {
        TipPanel.ShowToast("Oops! exceed limit:(");
    }

    public void OnHandleClone(SceneEntity oldEntity, SceneEntity newEntity)
    {
        if (newEntity.HasComponent<VIPZoneComponent>())
        {
            VIPZoneBehaviour vipZoneBehaviour = newEntity.Get<GameObjectComponent>().bindGo.GetComponent<VIPZoneBehaviour>();
            if (vipZoneBehaviour != null)
            {
                OnCreateNewVIPZone(vipZoneBehaviour);
                vipZoneBehaviour.InitColliderGameObject(Vector3.one);
            }
        } else if (newEntity.HasComponent<VIPCheckComponent>())
        {
            VIPCheckBoundControl vipCheckBoundControl = newEntity.Get<GameObjectComponent>().bindGo.GetComponent<VIPCheckBoundControl>();
            vipCheckBoundControl.InitEffect();
        }
    }

    public bool IsVIPZone(int id)
    {
        if (id >= 1070 && id <= 1080)
        {
            return true;
        }
        return false;
    }

    public void GetDCInfo(string mapId)
    {
        if (mapId == null)
        {
            return;
        }
        if (vipZoneDcDict.ContainsKey(mapId))
        {
            var dcInfo = vipZoneDcDict[mapId];
            if (dcInfo != null)
            {
                if (TokenDetectionPanel.Instance)
                {
                    TokenDetectionPanel.Instance.SetDCInfo(dcInfo.itemCover, dcInfo.itemName);
                }
            }
            return;
        }
        string[] mapArr = { mapId };

        // 获取 DCInfo 并保存
        var httpMapDataInfo = new HttpBatchMapDataInfo
        {
            mapIds = mapArr,
            dataType = -1
        };

        MapLoadManager.Inst.GetBatchMapInfo(httpMapDataInfo, getMapInfo =>
        {
            LoggerUtils.Log($"GetDCInfo Success:{JsonConvert.SerializeObject(getMapInfo)}");

            var mapInfos = getMapInfo.mapInfos;
            if (mapInfos != null && mapInfos.Length > 0)
            {
                foreach (var mapInfo in mapInfos)
                {
                    if (mapInfo != null)
                    {
                        var dcInfo = mapInfo.dcInfo;
                        if (dcInfo != null)
                        {
                            if (!vipZoneDcDict.ContainsKey(mapId))
                            {
                                vipZoneDcDict.Add(mapId, dcInfo);
                            }
                            if (TokenDetectionPanel.Instance)
                            {
                                TokenDetectionPanel.Instance.SetDCInfo(dcInfo.itemCover, dcInfo.itemName);
                            }
                        }
                        break;
                    }
                }
            }
        }, error => { LoggerUtils.LogError($"GetDCInfo faild:{error}"); });
    }

    public void RefreshVIPZoneTokenState()
    {
        if (vipTokenAreaList.Count <= 0)
        {
            foreach (var vipZoneBehaviour in vipZoneDict.Values)
            {
                if (!IsVIPZoneNoLimit(vipZoneBehaviour))
                {
                    vipZoneBehaviour.ResumeDoorInterceptStatus();
                }
            }
            return;
        }

        foreach (var areaId in vipZoneDict.Keys)
        {
            var vipZoneBehaviour = vipZoneDict[areaId];
            if (vipZoneBehaviour == null) continue;
            if (vipTokenAreaList.Contains(areaId) || IsVIPZoneNoLimit(vipZoneBehaviour))
            {
                vipZoneBehaviour.SignCanEnterDoor();
            }
            else if (IsInArea && areaId != curAreaId)
            {
                vipZoneBehaviour.ResumeDoorInterceptStatus();
            }
        }
    }

    public bool CanEnter(VIPZoneBehaviour vipZoneBehaviour)
    {
        return vipZoneBehaviour.canEnter;
    }

    public void OnPlayerEnterCheckTrigger(VIPZoneBehaviour vipZoneBehaviour)
    {
        if (!vipTriggerData.ContainsKey(vipZoneBehaviour))
        {
            vipTriggerData[vipZoneBehaviour] = new CheckTriggerData();
        }

        vipTriggerData[vipZoneBehaviour].EnterTrigger();
    }

    public void OnPlayerEnterTriggerCenterSmallArea(VIPZoneBehaviour vipZoneBehaviour)
    {
        if (!vipTriggerData.ContainsKey(vipZoneBehaviour))
        {
            return;
        }

        if (vipTriggerData[vipZoneBehaviour].CanTriggerCheck())
        {
            vipTriggerData[vipZoneBehaviour].OnCheckTrigger();
            OnPlayerEnterCheckArea(vipZoneBehaviour);
        }
    }

    public void OnPlayerExitCheckTrigger(VIPZoneBehaviour vipZoneBehaviour)
    {
        if (!vipTriggerData.ContainsKey(vipZoneBehaviour))
        {
            return;
        }
        vipTriggerData[vipZoneBehaviour].ExitTrigger();
    }

    public void OnTransformChange(GameObject target, HandleMode handleMode)
    {
        if (IsVIPCheck(target))
        {
            VIPCheckBoundControl vipCheckBoundControl = target.GetComponent<VIPCheckBoundControl>();
            if (vipCheckBoundControl != null)
            {
                vipCheckBoundControl.UpdateEffectShow();
            }
        }else if (IsVIPArea(target))
        {
            if (handleMode == HandleMode.Scale)
            {
                VIPZoneBehaviour vipZoneBehaviour = target.GetComponentInParent<VIPZoneBehaviour>();
                Transform doorWrap = vipZoneBehaviour.FindDoorWrap();
                DoorScaleKeeper keeper = doorWrap.GetComponent<DoorScaleKeeper>();
                keeper.KeepScale();
            }
        }
    }

    private bool IsVIPArea(GameObject target)
    {
        VIPAreaBehaviour area = target.GetComponent<VIPAreaBehaviour>();
        return area != null;
    }

    private bool IsVIPCheck(GameObject target)
    {
        NodeBaseBehaviour nodeBaseBehaviour = target.GetComponent<NodeBaseBehaviour>();
        if (nodeBaseBehaviour != null && nodeBaseBehaviour.entity.HasComponent<VIPCheckComponent>())
        {
            return true;
        }

        return false;
    }

    public void AddDCIcon(string url, Texture iconTexture)
    {
        if(!dcIconDict.ContainsKey(url))
        {
            dcIconDict.Add(url, iconTexture);
        }
    }

    public void RemoveDCIcon(string url)
    {
        if(dcIconDict.ContainsKey(url))
        {
            dcIconDict.Remove(url);
        }
    }

    public Texture GetDCIcon(string url)
    {
        Texture icon = null;
        dcIconDict.TryGetValue(url, out icon);
        return icon;
    }
}

public enum VIPComponentType
{
    PassDC,
    Door,
    Check,
    DoorEffect
}
