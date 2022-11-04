using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SavingData;
using Newtonsoft.Json;
using UnityEngine;
using BudEngine.NetEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Author:WenJia
/// Description:跷跷板管理器
/// 主要管理跷跷板的创建和还原，以及跷跷板的联机相关表现
/// Date: 2022/9/9 17:10:51
/// </summary>

// 跷跷板音效播放
public enum SeesawState
{
    Seesaw_SitDown = 1,
    Seesaw_StandUp,
    Seesaw_PushDown,
}

public class SeesawPlayerInfo
{
    public Transform player;
    public bool isSelf;
    public OtherPlayerCtr otherPlayerCtr;
    public Transform beforeParentNode;
    public int seatIndex = -1;
    public Transform selfPlayerModelTrans;
}

public class SeesawInfo
{
    public Transform carryTrans;
    public SeesawBehaviour seesawBehaviour;
    public string lPlayerId;
    public string rPlayerId;
}

public class SeesawManager : ManagerInstance<SeesawManager>, IManager, IPVPManager,IUGCManager
{
    private List<SceneEntity> all = new List<SceneEntity>();
    private Dictionary<int, SeesawBehaviour> seesawDict = new Dictionary<int, SeesawBehaviour>();//当前地图全部跷跷板
    private Dictionary<int, NodeBaseBehaviour> seesawSeatDict = new Dictionary<int, NodeBaseBehaviour>();//当前地图全部跷跷板座椅
    private Dictionary<int, SeesawBehaviour> seatBoardDict = new Dictionary<int, SeesawBehaviour>();//当前地图座椅和跷跷板的对应
    public Dictionary<SeesawPlayerInfo, SeesawInfo> curSeesawDict = new Dictionary<SeesawPlayerInfo, SeesawInfo>();//当前与玩家绑定跷跷板
    private Dictionary<string, int> dataDict = new Dictionary<string, int>();//服务器下发与玩家绑定跷跷板
    PlayerBaseControl player;
    Vector3 playerPos = new Vector3(0, 0.95f, 0);
    private Vector3 playerInitPos = new Vector3(0, -0.95f, 0);
    private List<string> ugcIds = new List<string>();
    public const string SEAT_DEFAULT = "SEAT_ID_DEFAULT";
    private bool eatOrDrinkCtrPanelShow = false;
    private const int COUNT_MAX = 99;

    public void Init()
    {
        player = PlayerManager.Inst.selfPlayer;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.AddListener<bool>(MessageName.PosMove, PlayerForceLeaveSeesaw);
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
    }

    private void HandlePackPanelShow(bool show)
    {
        for (int i = 0; i < all.Count; i++)
        {
            all[i].Get<GameObjectComponent>().bindGo.SetActive(!show);
        }
    }

    public GameObject CreateSeeSaw()
    {
        if (all.Count >= COUNT_MAX)
        {
            TellMax();
            return null;
        }
        SeesawBehaviour seesawBehaviour = SceneBuilder.Inst.CreateSceneNode<SeeSawCreater, SeesawBehaviour>();
        SeesawSeatBehaviour leftSeat = SceneBuilder.Inst.CreateSceneNode<SeesawSeatCreater, SeesawSeatBehaviour>();
        SeesawSeatComponent leftc = leftSeat.entity.Get<SeesawSeatComponent>();
        leftc.index = 0;
        SeesawSeatBehaviour rightSeat = SceneBuilder.Inst.CreateSceneNode<SeesawSeatCreater, SeesawSeatBehaviour>();
        SeesawSeatComponent rightc = rightSeat.entity.Get<SeesawSeatComponent>();
        rightc.index = 1;
        rightSeat.Reverse();

        List<SceneEntity> seeSawEntities = new List<SceneEntity>();
        seeSawEntities.Add(seesawBehaviour.entity);
        seeSawEntities.Add(leftSeat.entity);
        seeSawEntities.Add(rightSeat.entity);
        var entity = SceneBuilder.Inst.CombineNode(seeSawEntities);
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.bindGo.GetComponent<CombineBehaviour>().isCanClick = false;
        gameComp.modId = (int)GameResType.SeeSaw;
        gameComp.handleType = NodeHandleType.SeesawCombine;
        entity.Get<SeesawComponent>();
        gameComp.bindGo.AddComponent<SpawnPointConstrainer>();

        AdjustCombinePos(entity);
        leftSeat.SetPos(seesawBehaviour.GetLeftSeatSrcPosition());
        rightSeat.SetPos(seesawBehaviour.GetRightSeatSrcPosition());

        SymmetrySeat symmetrySeat = rightSeat.gameObject.GetComponent<SymmetrySeat>();
        if (symmetrySeat == null)
        {
            symmetrySeat = rightSeat.gameObject.AddComponent<SymmetrySeat>();
        }
        if(!symmetrySeat.enabled)
        {
            symmetrySeat.enabled = true;
        }

        symmetrySeat.Init(leftSeat.transform);

        EditModeController.SetSelect(entity);
        OnAddNewSeeSaw(entity);
        return gameComp.bindGo;
    }

    private void TellMax()
    {
        TipPanel.ShowToast("Up to 99 Seesaws can be placed.");
    }

    private void AdjustCombinePos(SceneEntity entity)
    {
        Transform root = entity.Get<GameObjectComponent>().bindGo.transform;
        Transform anchor = GameUtils.FindChildByName(root, "seesaw_root01");
        Vector3 dir = root.transform.position - anchor.transform.position;
        for (int i = 0; i < root.childCount; i++)
        {
            root.GetChild(i).transform.localPosition += dir;
        }
        root.transform.Translate(Vector3.up * 0.4451f, Space.World);
    }

    public void AddSeeSawToCombine(NodeBaseBehaviour behaviour, NodeData data)
    {
        var seeSawKey = data.attr.Find(x => x.k == (int)BehaviorKey.SeeSaw);
        if (seeSawKey != null)
        {
            behaviour.isCanClick = false;
            
            SeesawData seesawData = GameUtils.GetAttr<SeesawData>((int)BehaviorKey.SeeSaw, data.attr);
            var gameComp = behaviour.entity.Get<GameObjectComponent>();
            gameComp.modId = (int)GameResType.SeeSaw;
            gameComp.handleType = NodeHandleType.SeesawCombine;

            SeesawComponent seesawComponent = behaviour.entity.Get<SeesawComponent>();
            seesawComponent.mat = seesawData.mat;
            seesawComponent.color = seesawData.color;
            seesawComponent.setLeftSitPoint = seesawData.setLeftSitPoint;
            seesawComponent.leftSitPoint = seesawData.leftSitPoint;
            seesawComponent.setRightSitPoint = seesawData.setRightSeatPoint;
            seesawComponent.rightSitPoint = seesawData.rightSitPoint;
            seesawComponent.tiling = seesawData.tiling;
            seesawComponent.setLeftSeatPos = seesawData.setLeftSeatPos;
            seesawComponent.setRightSeatPos = seesawData.setRightSeatPos;
            OnAddNewSeeSaw(behaviour.entity);

            SeesawBehaviour seesawBehaviour = behaviour.GetComponentInChildren<SeesawBehaviour>();
            seesawBehaviour.SetMat(seesawData.mat);
            seesawBehaviour.SetTiling(seesawData.tiling);
            seesawBehaviour.SetColor(DataUtils.DeSerializeColor(seesawData.color));
            
        }
    }

    public void AddSeeSawSeatToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        var seeSawKey = data.attr.Find(x => x.k == (int)BehaviorKey.SeeSawSeat);
        if (seeSawKey != null)
        {
            SeesawSeatCreater.SetData(behaviour, data);
        }
    }

    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            if (StateManager.IsOnSeesaw)
            {
                PlayerLeaveSeesaw(true);
            }
        }

        if (seesawDict.Count > 0)
        {
            foreach (var seesawBehaviour in seesawDict.Values)
            {
                if (seesawBehaviour != null)
                {
                    seesawBehaviour.OnModeChange(mode);
                }
            }

            dataDict.Clear();
            curSeesawDict.Clear();
        }
    }

    public void SendPlayerPushDownSeeSaw()
    {
        // 冻结状态下不允许操作跷跷板
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            var seesawPlayerInfo = GetSeesawPlayerInfo(player.transform);
            var seesawInfo = GetSeesawInfo(player.transform);
            if (seesawInfo != null)
            {
                bool isRight = seesawPlayerInfo.seatIndex == 1;
                SendSeesawReq(seesawInfo.seesawBehaviour.entity.Get<GameObjectComponent>().uid, (int)SeesawState.Seesaw_PushDown, isRight);
            }
        }
        else
        {
            PushSeesaw();
        }
    }


    public void PlayerForceLeaveSeesaw(bool isTrap = false)
    {
        if (player == null)
        {
            return;
        }

        var info = GetSeesawInfo(player.transform);
        if (info != null)
        {
            var bv = info.seesawBehaviour;
            int uid = bv.entity.Get<GameObjectComponent>().uid;
            PlayerLeaveSeesaw(true);
            if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
            {
                SendUnBindSeesaw(uid);
            }
        }
    }
    private void PlayerLeaveRoom(string id)
    {
        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
        if (otherCtr != null)
        {
            LeaveSeesaw(otherCtr.transform);
            if (dataDict.ContainsKey(id))
            {
                dataDict.Remove(id);
            }
        }
    }

    public void AddSeesaw(SeesawBehaviour behaviour)
    {
        int hashCode = behaviour.GetHashCode();
        Debug.Log(seesawDict.Count + "    " + hashCode);
        if (!seesawDict.ContainsKey(hashCode))
        {
            seesawDict.Add(hashCode, behaviour);
        }
    }

    public void RemoveSeesaw(SeesawBehaviour behaviour)
    {
        if (behaviour == null) return;
        if (seesawDict.ContainsValue(behaviour))
        {
            seesawDict.Remove(behaviour.GetHashCode());
        }
    }


    public void AddSeesawSeat(NodeBaseBehaviour behaviour)
    {
        int hashCode = behaviour.GetHashCode();
        Debug.Log(seesawSeatDict.Count + "    " + hashCode);
        if (!seesawSeatDict.ContainsKey(hashCode))
        {
            seesawSeatDict.Add(hashCode, behaviour);
        }
    }

    public void RemoveSeesawSeat(NodeBaseBehaviour behaviour)
    {
        if (behaviour == null) return;
        if (seesawSeatDict.ContainsValue(behaviour))
        {
            seesawSeatDict.Remove(behaviour.GetHashCode());
        }
    }


    public void AddSeatBoard(NodeBaseBehaviour behaviour, SeesawBehaviour seesawBehaviour)
    {
        var hashCode = behaviour.GetHashCode();
        if (!seatBoardDict.ContainsKey(hashCode))
        {
            seatBoardDict.Add(hashCode, seesawBehaviour);
        }
    }

    public void RemoveSeatBoard(NodeBaseBehaviour behaviour)
    {
        if (behaviour == null) return;
        var hashCode = behaviour.GetHashCode();
        if (seatBoardDict.ContainsKey(hashCode))
        {
            seatBoardDict.Remove(hashCode);
        }
    }

    public void OnReset()
    {
        PlayModePanel.Instance.SetOnSeesaw(false);
        if (StateManager.IsOnSeesaw)
        {
            PlayerForceLeaveSeesaw();
        }
        AllLeaveSeesaw();
        dataDict.Clear();
        curSeesawDict.Clear();
    }

    public void PlayerSendOnSeesaw(int hashCode, bool isRight)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SeesawBehaviour bv = GetSeesawBehaviourBySeatCode(hashCode);
            if (bv)
            {
                SendBindSeesaw(bv, isRight);
            }
        }
        else
        {
            PlayerOnSeesaw(hashCode, isRight);
        }
    }

    public void PlayerOnSeesaw(int hashCode, bool isRight)
    {
        if (ContainsPlayerTrans(player.transform))
        {
            var seesawInfo = GetSeesawInfo(player.transform);
            if (seesawInfo != null)
            {
                if (seesawInfo.seesawBehaviour.GetHashCode() != hashCode)
                {
                    PlayerLeaveSeesaw();
                }
            }
        }
        // BlackPanel.Show();
        // BlackPanel.Instance.PlayTransitionAnim();

        var playerOnSeesawCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerOnSeesawControl>();
        if (!playerOnSeesawCtrl)
        {
            // 添加跷跷板控制脚本
            playerOnSeesawCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerOnSeesawControl>();
        }
        if (PlayerOnSeesawControl.Inst)
        {
            PlayerOnSeesawControl.Inst.OnSeesaw(isRight);
        }
        PlayModePanel.Instance.SetOnSeesaw(true);

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(false);
        }

        OnSeesaw(hashCode, player.transform, isRight);
    }

    public SeesawBehaviour GetSeesawBehaviourBySeatCode(int hashCode)
    {
        SeesawBehaviour bv = null;
        seatBoardDict.TryGetValue(hashCode, out bv);
        return bv;
    }

    public void OnSeesaw(int hashCode, Transform transform, bool isRight)
    {
        if (transform)
        {
            SeesawBehaviour bv;
            seesawDict.TryGetValue(hashCode, out bv);
            if (bv == null)
            {
                seatBoardDict.TryGetValue(hashCode, out bv);
            }

            if (bv != null)
            {
                OtherPlayerCtr otherCtr = transform.GetComponent<OtherPlayerCtr>();
                var index = 0;
                if (isRight)
                {
                    index = 1;
                }
                var beforeParentNode = transform.parent;
                if (otherCtr == null)
                {
                    beforeParentNode = transform;
                }

                SeesawPlayerInfo playerInfo = new SeesawPlayerInfo()
                {
                    player = transform,
                    otherPlayerCtr = otherCtr,
                    isSelf = otherCtr == null,
                    beforeParentNode = beforeParentNode,
                    seatIndex = index,
                };
                string lPId = "", rPId = "";
                Transform carryNodeTran;
                PlayerData playerDataCom = transform.GetComponentInChildren<PlayerData>(true);
                var playerId = playerDataCom.syncPlayerInfo.uid;
                bv.SetPlayerNode(isRight);
                if (isRight)
                {
                    rPId = playerId;
                    carryNodeTran = bv.carryTranR;
                }
                else
                {
                    lPId = playerId;
                    carryNodeTran = bv.carryTranL;
                }
                SeesawInfo seesawInfo = new SeesawInfo()
                {
                    carryTrans = carryNodeTran,
                    seesawBehaviour = bv,
                    lPlayerId = lPId,
                    rPlayerId = rPId
                };
                Transform playerModelTrans = player.playerAnim.transform;
                if (otherCtr != null)
                {
                    playerModelTrans = transform;
                }
                if (carryNodeTran && playerModelTrans)
                {
                    playerModelTrans.SetParent(carryNodeTran);
                }
                var pos = Vector3.zero;
                playerModelTrans.localPosition = pos;
                var rot = Quaternion.Euler(0, 90, 0);
                if (isRight)
                {
                    rot = Quaternion.Euler(0, -90, 0);
                }
                playerModelTrans.localRotation = rot;
                curSeesawDict.Add(playerInfo, seesawInfo);
                bv.RefreshSeatState(index, true);
                if (transform == player.transform)
                {
                    bv.OnSeesawSuccess();

                    if (playerModelTrans)
                    {
                        playerInfo.selfPlayerModelTrans = playerModelTrans;
                        var playerTransPos = playerModelTrans.parent.TransformPoint(playerModelTrans.localPosition);
                        var posY = player.transform.localPosition.y;
                        player.transform.localPosition = playerTransPos;
                    }
                }
            }
        }
    }

    public void PlayerSendLeaveSeesaw()
    {
        //冻结状态不允许下跷跷板
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            var seesawInfo = GetSeesawInfo(player.transform);
            if (seesawInfo != null)
            {
                SendUnBindSeesaw(seesawInfo.seesawBehaviour);
            }
        }
        else
        {
            PlayerLeaveSeesaw();
        }
    }

    public void PlayerLeaveSeesaw(bool isForceLeave = false)
    {
        // BlackPanel.Show();
        // BlackPanel.Instance.PlayTransitionAnim();
        PlayModePanel.Instance.SetOnSeesaw(false);

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(true);
        }

        if (PlayerOnSeesawControl.Inst)
        {
            if (isForceLeave)
            {
                PlayerOnSeesawControl.Inst.ForceLeaveSeesaw();
            }
            else
            {
                PlayerOnSeesawControl.Inst.LeaveSeesaw();
            }
            
        }
        
        LeaveSeesaw(player.transform);
    }

    public void LeaveSeesaw(Transform transform)
    {
        if (transform && ContainsPlayerTrans(transform))
        {
            var seesawPlayerInfo = GetSeesawPlayerInfo(transform);
            var seesawBehaviour = curSeesawDict[seesawPlayerInfo].seesawBehaviour;

            if (seesawPlayerInfo.selfPlayerModelTrans)
            {
                seesawPlayerInfo.selfPlayerModelTrans.SetParent(seesawPlayerInfo.beforeParentNode);
                seesawPlayerInfo.selfPlayerModelTrans.localPosition = playerInitPos;
                seesawPlayerInfo.selfPlayerModelTrans.localRotation = Quaternion.identity;
            }
            else
            {
                seesawPlayerInfo.player.SetParent(seesawPlayerInfo.beforeParentNode);
            }

            if (seesawBehaviour)
            {
                seesawBehaviour.LeaveSeesawSuccess();
                seesawBehaviour.RefreshSeatState(seesawPlayerInfo.seatIndex, false);
            }
            curSeesawDict.Remove(seesawPlayerInfo);
        }
    }

    public bool ContainsPlayerTrans(Transform trans)
    {
        foreach (var item in curSeesawDict)
        {
            if (item.Key.player == trans)
            {
                return true;
            }
        }
        return false;
    }



    public bool ContainsCarryTrans(Transform trans)
    {
        foreach (var item in curSeesawDict)
        {
            if (item.Value.carryTrans == trans)
            {
                return true;
            }
        }
        return false;
    }
    public bool ContainsOtherCtr(OtherPlayerCtr ctr)
    {
        if (curSeesawDict.Count == 0)
        {
            return false;
        }
        foreach (var item in curSeesawDict.Keys)
        {
            if (item.otherPlayerCtr == ctr)
            {
                return true;
            }
        }
        return false;
    }


    public SeesawInfo GetSeesawInfo(Transform trans)
    {
        foreach (var item in curSeesawDict)
        {
            if (item.Key.player == trans)
            {
                return item.Value;
            }

        }
        return null;
    }
    public SeesawPlayerInfo GetSeesawPlayerInfo(Transform trans)
    {
        foreach (var item in curSeesawDict)
        {
            if (item.Key.player == trans)
            {
                return item.Key;
            }

        }
        return null;
    }

    /// <summary>
    /// 跷跷板互斥提示
    /// </summary>
    public void ShowSeesawMutexToast()
    {
        TipPanel.ShowToast("Please quit seesaw first.");
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<SeesawComponent>())
        {
            OnRemoveSeeSaw(behaviour.entity);
        }
    }

    private void OnAddNewSeeSaw(SceneEntity entity)
    {
        all.Add(entity);
        SeesawBehaviour seesawBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponentInChildren<SeesawBehaviour>();
        if (seesawBehaviour != null)
        {
            if (!seesawDict.ContainsValue(seesawBehaviour))
            {
                seesawDict.Add(seesawBehaviour.GetHashCode(), seesawBehaviour);
            }

            NodeBaseBehaviour leftSeatBehaviour = seesawBehaviour.GetLeftSeatBehaviour();
            if (!seesawSeatDict.ContainsValue(leftSeatBehaviour))
            {
                seesawSeatDict.Add(leftSeatBehaviour.GetHashCode(),leftSeatBehaviour);
            }

            NodeBaseBehaviour rightSeatBehaviour = seesawBehaviour.GetRightSeatBehaviour();
            if (!seesawSeatDict.ContainsValue(rightSeatBehaviour))
            {
                seesawSeatDict.Add(rightSeatBehaviour.GetHashCode(),rightSeatBehaviour);
            }
            
            if (!seatBoardDict.ContainsKey(leftSeatBehaviour.GetHashCode()))
            {
                seatBoardDict.Add(leftSeatBehaviour.GetHashCode(), seesawBehaviour);
            }
            
            if (!seatBoardDict.ContainsKey(rightSeatBehaviour.GetHashCode()))
            {
                seatBoardDict.Add(rightSeatBehaviour.GetHashCode(), seesawBehaviour);
            }
        }
    }

    private void OnRemoveSeeSaw(SceneEntity entity)
    {
        all.Remove(entity);
        SeesawBehaviour seesawBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponentInChildren<SeesawBehaviour>();
        if (seesawBehaviour != null)
        {
            seesawDict.Remove(seesawBehaviour.GetHashCode());
            
            NodeBaseBehaviour leftSeatBehaviour = seesawBehaviour.GetLeftSeatBehaviour();
            seesawSeatDict.Remove(leftSeatBehaviour.GetHashCode());

            NodeBaseBehaviour rightSeatBehaviour = seesawBehaviour.GetRightSeatBehaviour();
            seesawSeatDict.Remove(rightSeatBehaviour.GetHashCode());
            
            seatBoardDict.Remove(leftSeatBehaviour.GetHashCode());
            seatBoardDict.Remove(rightSeatBehaviour.GetHashCode());
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<SeesawComponent>())
        {
            OnAddNewSeeSaw(behaviour.entity);
        }
    }

    public void Clear()
    {
        curSeesawDict.Clear();
        seesawDict.Clear();
        seatBoardDict.Clear();
        dataDict.Clear();
        all.Clear();
    }

    public override void Release()
    {
        base.Release();
        curSeesawDict.Clear();
        seesawDict.Clear();
        seatBoardDict.Clear();
        dataDict.Clear();
        all.Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.RemoveListener<bool>(MessageName.PosMove, PlayerForceLeaveSeesaw);
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
    }

    private void SendBindSeesaw(SeesawBehaviour bv, bool isRight)
    {
        SendSeesawReq(bv.entity.Get<GameObjectComponent>().uid, (int)SeesawState.Seesaw_SitDown, isRight);
    }
    private void SendUnBindSeesaw(SeesawBehaviour bv)
    {
        SendSeesawReq(bv.entity.Get<GameObjectComponent>().uid, (int)SeesawState.Seesaw_StandUp);
    }
    private void SendUnBindSeesaw(int uid)
    {
        SendSeesawReq(uid, (int)SeesawState.Seesaw_StandUp);
    }

    private void SendSeesawReq(int uid, int state, bool isRight = false)
    {
        SeesawSendData seesawData = new SeesawSendData();
        seesawData.mapId = GlobalFieldController.CurMapInfo.mapId;
        seesawData.itemId = uid;
        seesawData.side = isRight ? 1 : 0;
        seesawData.option = state;
        int hashCode = GetSeesawHashCode(uid);

        SeesawBehaviour behaviour;
        if (seesawDict.TryGetValue(hashCode, out behaviour))
        {
            seesawData.angle = behaviour.angleZ;
            seesawData.speed = behaviour.curSpeed;
            seesawData.minAngle = behaviour.curAngleMin;
            seesawData.maxAngle = behaviour.curAngleMax;
        }

        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Seesaw,
            data = JsonConvert.SerializeObject(seesawData),
        };
        LoggerUtils.Log("Seesaw SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendSeesawCallBack);
    }

    private void SendSeesawCallBack(int code, string data)
    {
        if (code != 0)//底层错误，业务层不处理
        {
            return;
        }
        SeesawErrorData ret = JsonConvert.DeserializeObject<SeesawErrorData>(data);
        
        if (ret.retcode == (int)PropOptErrorCode.SeeSawError)
        {
            LoggerUtils.Log("SendSeesawCallBack =>" + ret.retcode);
            TipPanel.ShowToast("The seesaw has been occupied by other player.");
        }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("Seesaw OnReceiveServer==>" + msg);
        SeesawSendData seesawSendData = JsonConvert.DeserializeObject<SeesawSendData>(msg);
        string mapId = seesawSendData.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            var id = seesawSendData.itemId;
            SetSeesaw(seesawSendData, senderPlayerId);
        }
        return true;
    }

    public void SetView()
    {
        var seesawInfo = GetSeesawInfo(player.transform);
        var carryNodeTran = seesawInfo.carryTrans;
        var seesawPlayerInfo = GetSeesawPlayerInfo(player.transform);
        var rot = Quaternion.Euler(0, 90, 0);
        if (seesawPlayerInfo.seatIndex == 1)
        {
            rot = Quaternion.Euler(0, -90, 0);
        }
        if (seesawPlayerInfo.selfPlayerModelTrans)
        {
            seesawPlayerInfo.selfPlayerModelTrans.localRotation = rot;
        }

        PlayerControlManager.Inst.ChangeAnimClips();
    }

    public void SaveRid(string rId)
    {
        if (rId == SEAT_DEFAULT)
        {
            return;
        }
        if (ugcIds.Contains(rId))
        {
            return;
        }
        ugcIds.Add(rId);
    }

    public List<string> GetAllUGC()
    {
        return ugcIds;
    }

    public Transform FindLeftSeat(NodeBaseBehaviour right)
    {
        return right.gameObject.transform.parent.GetComponentInChildren<SeesawBehaviour>().FindSeat(0);
    }

    public SeesawBehaviour FindSeesawBehaviour(NodeBaseBehaviour seat)
    {
        return seat.transform.parent.GetComponentInChildren<SeesawBehaviour>();
    }

    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========SeesawManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[SeesawManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            ActivityData[] activityDatas = getItemsRsp.activityDatas;
            if (activityDatas != null)
            {
                for (int i = 0; i < activityDatas.Length; i++)
                {
                    ActivityData activityData = activityDatas[i];
                    if (activityData != null && activityData.activityId == ActivityID.SeesawItem)
                    {
                        SeesawSendData info = JsonConvert.DeserializeObject<SeesawSendData>(activityData.data);
                        if (info != null && info.mapId == GlobalFieldController.CurMapInfo.mapId)
                        {
                            LoggerUtils.Log("GetItems Seesaw 恢复");
                            RestoreSeesaw(info);
                        }
                    }
                }
            }
        }
    }

    private void RestoreSeesaw(SeesawSendData data)
    {
        var id = data.itemId;

        int hashCode = GetSeesawHashCode(id);

        SeesawBehaviour behaviour;
        if (seesawDict.TryGetValue(hashCode, out behaviour))
        {
            RefreshSeesawState(data);
        }
        if (hashCode != 0)
        {
            if (!string.IsNullOrWhiteSpace(data.uidLeft))
            {
                SetPlayerOnSeesaw(hashCode, data.uidLeft, false);
            }
            else
            {
                var playerTrans = behaviour.FindPlayerOnSeesaw(false);
                if (playerTrans)
                {
                    var playerData = playerTrans.GetComponent<PlayerData>();
                    if (playerData != null)
                    {
                        var playerId = playerData.syncPlayerInfo.uid;
                        SetPlayerLeaveSeesaw(playerId);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(data.uidRight))
            {
                SetPlayerOnSeesaw(hashCode, data.uidRight, true);
            }
            else
            {
                var playerTrans = behaviour.FindPlayerOnSeesaw(true);
                if (playerTrans)
                {
                    var playerData = playerTrans.GetComponent<PlayerData>();
                    if (playerData != null)
                    {
                        var playerId = playerData.syncPlayerInfo.uid;
                        SetPlayerLeaveSeesaw(playerId);
                    }
                }
            }
        }
    }

    private void SetPlayerOnSeesaw(int hashCode, string playerId, bool isRight)
    {
        if (Player.Id == playerId)
        {
            if (ContainsPlayerTrans(player.transform))
            {
                var seesawInfo = GetSeesawInfo(player.transform);
                if (seesawInfo != null && seesawInfo.seesawBehaviour != null)
                {
                    if (seesawInfo.seesawBehaviour.GetHashCode() != hashCode)
                    {
                        PlayerLeaveSeesaw();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            PlayerOnSeesaw(hashCode, isRight);
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherCtr != null)
            {
                Transform otherplayertrans = otherCtr.transform;
                if (ContainsOtherCtr(otherCtr))
                {
                    var seesawInfo = GetSeesawInfo(otherplayertrans);
                    if (seesawInfo != null && seesawInfo.seesawBehaviour != null)
                    {
                        if (seesawInfo.seesawBehaviour.GetHashCode() != hashCode)
                        {
                            LeaveSeesaw(otherplayertrans);
                        }
                        else
                        {
                            otherCtr.isAvoidFrame = true;
                            return;
                        }
                    }
                }

                OnSeesaw(hashCode, otherplayertrans, isRight);
                otherCtr.isAvoidFrame = true;
                // otherCtr.SwitchSeesawAnimClips();
            }
        }
    }

    private void SetPlayerLeaveSeesaw(string playerId)
    {
        if (Player.Id == playerId)
        {
            PlayerLeaveSeesaw();
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherCtr != null)
            {
                Transform otherplayertrans = otherCtr.transform;
                LeaveSeesaw(otherplayertrans);

                otherCtr.isAvoidFrame = false;
            }
        }
    }

    private void SetPlayerPushSeesaw(string playerId, SeesawSendData data)
    {
        if (Player.Id == playerId)
        {
            if (StateManager.IsOnSeesaw)
            {
                PlayerOnSeesawControl.Inst.PushSeesaw();
            }
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherCtr != null)
            {
                otherCtr.PushSeesaw();
            }
        }
    }

    private void SetSeesaw(SeesawSendData data, string playerId)
    {
        var id = data.itemId;
        var isRight = data.side == 1;
        var status = data.option;

        int hashCode = GetSeesawHashCode(id);

        if (hashCode != 0)
        {
            Debug.Log("Seesaw OnReceiveServer ==>  id " + id + " playerId " + playerId + " status " + status);
            SetDataSeesaw(status, playerId, hashCode);
            if (status == (int)SeesawState.Seesaw_SitDown)//上板子
            {
                SetPlayerOnSeesaw(hashCode, playerId, isRight);
            }
            else if (status == (int)SeesawState.Seesaw_StandUp)//下板子
            {
                SetPlayerLeaveSeesaw(playerId);
            }
            else if (status == (int)SeesawState.Seesaw_PushDown) // 给跷跷板施力
            {
                SetPlayerPushSeesaw(playerId, data);
            }
            SeesawBehaviour behaviour;
            if (seesawDict.TryGetValue(hashCode, out behaviour))
            {
                RefreshSeesawState(data);
            }
        }
    }

    public int GetSeesawHashCode(int uid)
    {
        foreach (var sid in seesawDict)
        {
            if (uid == sid.Value.entity.Get<GameObjectComponent>().uid)
            {
                return sid.Key;
            }
        }
        return 0;
    }

    public void SetDataSeesaw(int state, string playerId, int Code)
    {
        if (state == (int)SeesawState.Seesaw_SitDown && !dataDict.ContainsKey(playerId))//上板子
        {
            dataDict.Add(playerId, Code);
        }
        else if (state == (int)SeesawState.Seesaw_StandUp && dataDict.ContainsKey(playerId))//下板子
        {
            dataDict.Remove(playerId);
        }
    }

    public void RefreshSeesawState(Transform transform, SeesawSendData data)
    {
        var seesawInfo = GetSeesawInfo(transform);
        if (seesawInfo != null)
        {
            seesawInfo.seesawBehaviour.RefreshSeesawState(data);
        }
    }

    public void RefreshSeesawState(SeesawSendData data)
    {
        int hashCode = GetSeesawHashCode(data.itemId);
        if (hashCode != 0)
        {
            SeesawBehaviour behaviour;
            if (seesawDict.TryGetValue(hashCode, out behaviour))
            {
                behaviour.RefreshSeesawState(data);
            }
        }
    }

    public void PushSeesaw()
    {
        if (StateManager.IsOnSeesaw)
        {
            PlayerOnSeesawControl.Inst.PushSeesaw();
        }

        var seesawPlayerInfo = GetSeesawPlayerInfo(player.transform);
        var seesawInfo = GetSeesawInfo(player.transform);
        if (seesawInfo != null)
        {
            bool isRight = seesawPlayerInfo.seatIndex == 1;
            seesawInfo.seesawBehaviour.PushSeesaw(isRight);
        }
    }

    private void AllLeaveSeesaw()
    {
        foreach (var seesawInfo in curSeesawDict)
        {
            if (seesawInfo.Value.seesawBehaviour != null)
            {
                seesawInfo.Value.seesawBehaviour.LeaveSeesawSuccess();
                seesawInfo.Value.seesawBehaviour.ResetAllSeat();
            }
        }
        curSeesawDict.Clear();
    }
    public bool IsOtherPlayerOnSeesaw(OtherPlayerCtr otherCtr)
    {
        var seesawPlayerInfo = GetSeesawPlayerInfo(otherCtr.transform);
        return seesawPlayerInfo != null;
    }

    /// <summary>
    /// 当前状态不显示跷跷板操作按钮
    /// </summary>
    /// <returns></returns>
    public bool CanUseSeesaw()
    {
        if(StateManager.IsOnSeesaw)
        {
            return false;
        }
        if(StateManager.IsOnSwing)
        {
            return false;
        }
        if (PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder)
        {
            return false;
        }

        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            return false;
        }

        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            return false;
        }
        //降落伞和跷跷板互斥
        if (StateManager.IsParachuteUsing || StateManager.IsParachuteFalling)
        {
            return false;
        }

        if (StateManager.IsOnSlide)
        {
            return false;
        }
        return true;
    }

    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponent<NodeBaseBehaviour>().entity.HasComponent<SeesawComponent>())
        {
            if (all.Count >= COUNT_MAX)
            {
                TellMax();
                return false;
            }
        }

        return true;
    }

    public void OnClone(GameObject go)
    {
        if (go.GetComponent<CombineBehaviour>() == null)
        {
            return;
        }
        if (!go.GetComponent<CombineBehaviour>().entity.HasComponent<SeesawComponent>())
        {
            return;
        }
        OnAddNewSeeSaw(go.GetComponent<CombineBehaviour>().entity);
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        if (!ugcCombBehaviour.entity.HasComponent<SeesawSeatComponent>())
        {
            return;
        }
        SetToTouchLayer(ugcCombBehaviour.transform);
    }
    
    public void SetToTouchLayer(Transform seat)
    {
        var boxCollider = seat.GetComponentInChildren<Collider>();
        if (boxCollider != null)
        {
            boxCollider.gameObject.layer = LayerMask.NameToLayer("Touch");
            boxCollider.enabled = true;
        }
    }
}
