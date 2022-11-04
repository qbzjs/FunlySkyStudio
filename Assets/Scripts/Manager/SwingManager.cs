using System.Collections.Generic;
using System.Linq;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public class SwingManager : ManagerInstance<SwingManager>, IManager, IPVPManager, IUGCManager
{
    private int MaxCount = 99;
    private Dictionary<int, SwingBehaviour> swingDict = new Dictionary<int, SwingBehaviour>();
    private Dictionary<string, SwingBehaviour> curSwingDict = new Dictionary<string, SwingBehaviour>();
    private List<string> ugcIds = new List<string>();
    PlayerBaseControl player;
    private readonly Vector3 playerInitPos = new Vector3(0, -0.95f, 0);

    private readonly int touchLayer = LayerMask.NameToLayer("Touch");

    public void Init()
    {
        player = PlayerManager.Inst.selfPlayer;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.PosMove, PlayerForceStopSiwing);
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
    }

    private void PlayerLeaveRoom(string id)
    {
        LeaveSwing(id);
    }

    public void PlayerForceStopSiwing(bool isTrap = false)
    {
        if (player == null)
        {
            return;
        }

        PlayerSendStop();
    }

    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            if (StateManager.IsOnSwing)
            {
                SelfStop();
            }
        }

        foreach (var v in swingDict.Values)
        {
            v.OnModeChange(mode);
        }

        curSwingDict.Clear();
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<SwingComponent>())
        {
            SwingBehaviour sb = behaviour as SwingBehaviour;
            if (sb != null)
            {
                sb.OnRemoveNode();
                swingDict.Remove(sb.GetHashCode());
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }

    public void Clear()
    {
    }

    public void OnReset()
    {
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
    }

    public bool IsOverMaxCount()
    {
        if (GetFireworkCount() >= MaxCount)
        {
            return true;
        }

        return false;
    }
    
    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponent<NodeBaseBehaviour>().entity.HasComponent<SwingComponent>())
        {
            if (swingDict.Count >= MaxCount)
            {
                TipPanel.ShowToast("Oops! Exceed limit:(");
                return false;
            }
        }

        return true;
    }

    private int GetFireworkCount()
    {
        return swingDict.Count;
    }

    public void ShowSwingMutexToast()
    {
        TipPanel.ShowToast("Please quit swing first.");
    }

    public void PlayerSendPlay()
    {
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }

        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SwingBehaviour sb = null;
            if (curSwingDict.TryGetValue(Player.Id, out sb))
            {
                SendSwingReq(sb, 2);
            }
        }
        else
        {
            SelfPlay();
        }
    }

    private void SelfPlay()
    {
        if (StateManager.IsOnSwing)
        {
            SwingBehaviour sb;
            if (curSwingDict.TryGetValue(Player.Id, out sb))
            {
                sb.Play();
            }
        }
    }

    public void PlayerSendStop()
    {
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }

        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SwingBehaviour sb = null;
            if (curSwingDict.TryGetValue(Player.Id, out sb))
            {
                SendSwingReq(sb, 3);
            }
        }
        else
        {
            SelfStop();
        }
    }

    private void SelfStop()
    {
        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(true);
        }

        LeaveSwing(Player.Id);
    }

    private void ForceLeaveSwing(string id)
    {
        SwingBehaviour sb;
        if (curSwingDict.TryGetValue(id, out sb))
        {
            sb.ForceStop();
            curSwingDict.Remove(id);
        }
    }

    private void LeaveSwing(string id)
    {
        SwingBehaviour sb;
        if (curSwingDict.TryGetValue(id, out sb))
        {
            sb.Leave(() =>
            {
                if (id == Player.Id)
                {
                    PlayModePanel.Instance.SetOnSwing(false);
                    // player.playerAnim.transform.localPosition = playerInitPos;
                    if (PlayerOnSwingControl.Inst)
                    {
                        PlayerOnSwingControl.Inst.LeaveSwing();
                    }
                }

                curSwingDict.Remove(id);
            });
        }
    }

    private bool ContainsPlayerTrans(Transform trs)
    {
        // return curSwingDict.ContainsValue(trs.GetComponent<PlayerBaseControl>());
        return true;
    }

    public bool CanUseSwing()
    {
        if (StateManager.IsOnSwing)
        {
            return false;
        }
        
        if (StateManager.IsOnSeesaw)
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

    public void PlayerSendOnSwing(int hashCode)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SwingBehaviour sb = GetSwingBehaviourBySeatCode(hashCode);
            if (sb != null)
            {
                SendSwingReq(sb, 1);
            }
        }
        else
        {
            PlayerOnSwing(hashCode);
        }
    }

    private void PlayerOnSwing(int hashCode)
    {
        var psc = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerOnSwingControl>();
        if (!psc)
        {
            psc = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerOnSwingControl>();
        }

        if (PlayerOnSwingControl.Inst)
        {
            PlayerOnSwingControl.Inst.OnSwing();
        }

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(false);
        }

        PlayModePanel.Instance.SetOnSwing(true);
        OnSwing(hashCode, Player.Id);
    }

    private void SendSwingReq(SwingBehaviour sb, int opType)
    {
        var uid = sb.entity.Get<GameObjectComponent>().uid;
        SwingData sd = new SwingData()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            itemId = uid,
            playerId = Player.Id,
            angle = sb.startPos.x,
            targetAngle = sb.nowSpeed,
            time = 0,
        };

        SwingSeverData ssd = new SwingSeverData()
        {
            opType = opType,
            data = sd,
        };

        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int) RecChatType.Swing,
            data = JsonConvert.SerializeObject(ssd),
        };
        LoggerUtils.Log("Swing SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendSwingCallBack);
    }

    private void SendSwingCallBack(int code, string data)
    {
        if (code != 0) //底层错误，业务层不处理
        {
            return;
        }

        SeesawErrorData ret = JsonConvert.DeserializeObject<SeesawErrorData>(data);

        // if (ret.retcode == )
        // {
        //     LoggerUtils.Log("SendSwingCallBack =>" + ret.retcode);
        //     TipPanel.ShowToast("The Swing has been occupied by other player.");
        // }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("Swing OnReceiveServer==>" + msg);
        SwingSeverData ssd = JsonConvert.DeserializeObject<SwingSeverData>(msg);
        string mapId = ssd.data.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            SetSwing(ssd, senderPlayerId);
        }

        return true;
    }

    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========SwingManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[SwingManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.playerCustomDatas != null)
            {
                List<string> p = new List<string>();
                foreach (var pcd in getItemsRsp.playerCustomDatas)
                {
                    if (pcd.activitiesData != null)
                    {
                        foreach (var ad in pcd.activitiesData)
                        {
                            if (ad != null && ad.activityId == ActivityID.Swing)
                            {
                                SwingData data = JsonConvert.DeserializeObject<SwingData>(ad.data);
                                if (data != null && data.mapId == GlobalFieldController.CurMapInfo.mapId)
                                {
                                    LoggerUtils.Log("GetItems Swing 恢复");
                                    p.Add(data.playerId);
                                    RestoreSwing(data);
                                }
                            }
                        }
                    }
                }
                foreach (var k in curSwingDict.Keys)
                {
                    if (!p.Contains(k))
                    {
                        ForceLeaveSwing(k);
                    }
                }
            }
        }
    }

    private void RestoreSwing(SwingData data)
    {
        var id = data.itemId;
        int hashCode = GetSwingHashCode(id);
        SwingBehaviour sb;
        if (hashCode != 0 && swingDict.TryGetValue(hashCode, out sb))
        {
            if (curSwingDict.ContainsValue(sb))
            {
                var pid = curSwingDict.FirstOrDefault(v => v.Value == sb).Key;
                if (!string.IsNullOrEmpty(pid))
                {
                    ForceLeaveSwing(pid);
                }
            }
            SetPlayerOnSwing(data, data.playerId);
            if (data.time > 0)
            {
                sb.InitSwingState(data.angle, data.targetAngle, data.time);
            }
        }
    }

    private void SetSwing(SwingSeverData ssd, string playerId)
    {
        switch (ssd.opType)
        {
            case 1: //上
                SetPlayerOnSwing(ssd.data, playerId);
                break;
            case 2: //玩
                setPlayerPlay(ssd.data, playerId);
                break;
            case 3: //下
                SetPlayerLeaveSwing(playerId);
                break;
            default:
                LoggerUtils.LogError("Swing OnReceiveServer Type Error==>" + ssd.opType);
                break;
        }
    }

    private void setPlayerPlay(SwingData sd, string playerId)
    {
        if (Player.Id == playerId)
        {
            if (StateManager.IsOnSwing)
            {
                SelfPlay();
            }
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherCtr != null)
            {
                SwingBehaviour sb = null;
                if (curSwingDict.TryGetValue(playerId, out sb))
                {
                    sb.Play();
                    // otherCtr.PushSeesaw();
                }
            }
        }
    }

    private void SetPlayerLeaveSwing(string playerId)
    {
        if (Player.Id == playerId)
        {
            SelfStop();
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherCtr != null)
            {
                SwingBehaviour sb = null;
                if (curSwingDict.TryGetValue(playerId, out sb))
                {
                    sb.Leave((() => curSwingDict.Remove(playerId)));
                    otherCtr.isAvoidFrame = false;
                }
            }
        }
    }

    private void SetPlayerOnSwing(SwingData sd, string playerId)
    {
        if (curSwingDict.ContainsKey(playerId))
        {
            ForceLeaveSwing(playerId);
        }

        var hashCode = GetSwingHashCode(sd.itemId);
        if (playerId == Player.Id)
        {
            PlayerOnSwing(hashCode);
        }
        else
        {
            OnSwing(hashCode, playerId, sd);
        }
    }

    public int GetSwingHashCode(int uid)
    {
        foreach (var kv in swingDict)
        {
            if (uid == kv.Value.entity.Get<GameObjectComponent>().uid)
            {
                return kv.Key;
            }
        }

        return 0;
    }

    public void OnSwing(int hashCode, string id, SwingData sd = null)
    {
        Transform trs = null;
        if (id == Player.Id)
        {
            trs = player.playerAnim.transform;
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
            if (otherCtr != null)
            {
                trs = otherCtr.transform;
            }

            if (curSwingDict.ContainsKey(id))
            {
                LeaveSwing(id);
            }
            else
            {
                otherCtr.isAvoidFrame = true;
            }
        }

        if (trs != null)
        {
            SwingBehaviour sb;
            swingDict.TryGetValue(hashCode, out sb);
            if (sb != null)
            {
                sb.PlayerOnBoard(trs, id == Player.Id);
                curSwingDict[id] = sb;
            }
        }
    }

    private SwingBehaviour GetSwingBehaviourBySeatCode(int hashCode)
    {
        SwingBehaviour bv = null;
        swingDict.TryGetValue(hashCode, out bv);
        return bv;
    }

    public void AddSwing(SwingBehaviour sb)
    {
        int hashCode = sb.GetHashCode();
        if (!swingDict.ContainsKey(hashCode))
        {
            swingDict.Add(hashCode, sb);
        }
    }

    public void AddSwingSeat(NodeBaseBehaviour behaviour, NodeData data)
    {
        var sb = behaviour.transform.parent.GetComponent<SwingBehaviour>();
        if (sb != null)
        {
            behaviour.gameObject.layer = touchLayer;
            sb.SetBoard(behaviour.transform);
        }
    }

    public bool IsOtherPlayerOnSwing(OtherPlayerCtr op)
    {
        return curSwingDict.ContainsKey(op.gameObject.GetComponent<PlayerData>().playerInfo.Id);
    }

    public List<string> GetAllUGC()
    {
        return ugcIds;
    }

    public void SaveRid(string rId)
    {
        if (!ugcIds.Contains(rId))
        {
            ugcIds.Add(rId);
        }
    }

    public void SetView()
    {
        SwingBehaviour sb;
        if (curSwingDict.TryGetValue(Player.Id, out sb))
        {
            sb.SetView();
        }
    }
    
    public void ReSetView()
    {
        SwingBehaviour sb;
        if (curSwingDict.TryGetValue(Player.Id, out sb))
        {
            sb.ReSetView();
        }
    }
    
    public void Selfie()
    {
        if (StateManager.IsOnSwing)
        {
            SwingBehaviour sb;
            if (curSwingDict.TryGetValue(Player.Id, out sb))
            {
                sb.Selfie();
            }
        }
    }

    public void ExitSelfie()
    {
        if (StateManager.IsOnSwing)
        {
            SwingBehaviour sb;
            if (curSwingDict.TryGetValue(Player.Id, out sb))
            {
                sb.ExitSelfie();
            }
        }
    }
}