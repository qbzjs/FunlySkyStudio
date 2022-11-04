
/// <summary>
/// Author:zhouzihan
/// Description:磁力版管理器
/// Date: #CreateTime#
/// /// <summary>
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src;
public struct PlayerMagneticInfo
{
    public bool isSelf;
    public OtherPlayerCtr OtherPlayerCtr;
}
public class MagneticBoardManager : MonoManager<MagneticBoardManager>,IPVPManager
{
    private Dictionary<int, MagneticBoardBehaviour> boardsDict = new Dictionary<int, MagneticBoardBehaviour>();//当前地图全部磁力版
    private Dictionary<Transform, Transform> curBoardDict = new Dictionary<Transform, Transform>();//当前与玩家绑定磁力版
    private Dictionary<string, int> dataDict = new Dictionary<string, int>();//服务器下发与玩家绑定磁力版
    private Dictionary<Transform, PlayerMagneticInfo> playerRotInfoDict = new Dictionary<Transform, PlayerMagneticInfo>();//获得其他玩家转向
    PlayerBaseControl player;
    public void Awake()
    {
       
    }
    public void Init(GameController controller)
    {
        player = controller.playerCom;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.PosMove, PlayerForceDownBoard);
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
        MessageHelper.AddListener(MessageName.PlayerCreate, PlayerOnCreate);
    }
    public void Update()
    {
        if (curBoardDict.Count>0)
        {
            if (PlayerOnBoardControl.Inst != null && PlayerOnBoardControl.Inst.isOnBoard)
            {
                curBoardDict[player.transform].Rotate(Vector3.up, offset, Space.Self);
                offset = 0;
            }
            foreach (var item in curBoardDict)
            {
                if (item.Key!=null&&item.Value!=null)
                {
                    item.Key.position = item.Value.position;
                    var ctr = GetOtherPlayerCtr(item.Key);
                    if (ctr!=null)
                    {
                        item.Value.eulerAngles = ctr.m_PlayerRot.eulerAngles;
                        item.Value.localEulerAngles = new Vector3(0, item.Value.localEulerAngles.y, 0);
                    }

                    item.Key.rotation = item.Value.rotation;
                }
         
            }
            
        }

    }
    
    private OtherPlayerCtr GetOtherPlayerCtr(Transform trans)
    {
        if (playerRotInfoDict.ContainsKey(trans))
        {
            if (playerRotInfoDict[trans].isSelf == false)
            {
                return playerRotInfoDict[trans].OtherPlayerCtr;
            }
            else
            {
                return null;
            }
           
        }
        else
        {
            var ctr = trans.GetComponent<OtherPlayerCtr>();
           
            if (ctr ==null)
            {
                PlayerMagneticInfo info = new PlayerMagneticInfo() {
                    isSelf = true,
                };
                playerRotInfoDict.Add(trans, info);
            }
            else
            {
                PlayerMagneticInfo info = new PlayerMagneticInfo()
                {
                    isSelf = false,
                    OtherPlayerCtr = ctr
                };
                playerRotInfoDict.Add(trans, info);
            }
            
            return ctr;
        }
    }
    private void SendBindBoard(MagneticBoardBehaviour bv)
    {
        
        SendBoard(bv.entity.Get<GameObjectComponent>().uid, 1);
    }
    private void SendUnBindBoard(MagneticBoardBehaviour bv)
    {
        SendBoard(bv.entity.Get<GameObjectComponent>().uid, 0);
    }
    private void SendUnBindBoard(int uid)
    {
        SendBoard(uid, 0);
    }
    private void SendBoard(int uid,int isOnBoard)
    {
       
        MagneticData s = new MagneticData
        {
            status = isOnBoard,
            playerId = Player.Id,
        };
        Item itemData = new Item()
        {
            id = uid,
            type = (int)ItemType.MAGNETIC_BOARD,
            data = JsonConvert.SerializeObject(s),
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
        LoggerUtils.Log("Magnetic SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendBoardCallBack);
    }
    private void SendBoardCallBack(int code,string data)
    {
        if (code!=0)//底层错误，业务层不处理
        {
            return;
        }
        SyncItemsReq ret= JsonConvert.DeserializeObject<SyncItemsReq>(data);
        int errorCode = 0;
        if (int.TryParse(ret.retcode, out errorCode))
        {
            if (errorCode == (int)PropOptErrorCode.Exception)
            {
                LoggerUtils.Log("SendBoardCallBack =>" + ret.retcode);
                TipPanel.ShowToast("The adhesive surface has been occupied by other player");
            }
        }
    }
    public void SendPlayerJumpOnBoard()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            CustomData data = new CustomData();
            data.type = (int)CustomType.Jump;

            RoomChatData roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Custom,
                data = JsonConvert.SerializeObject(data),
            };
            LoggerUtils.Log("Jump SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }
        else
        {
            JumpOnBoard(player.transform);

            if (PlayerOnBoardControl.Inst)
            {
                PlayerOnBoardControl.Inst.PlayJumpOnBoard();
            }
        }

        //CustomData data = new CustomData();
        //data.type = (int)CustomType.Jump;

        //RoomChatData roomChatData = new RoomChatData()
        //{
        //    msgType = (int)RecChatType.Custom,
        //    data = JsonConvert.SerializeObject(data),
        //};
        //LoggerUtils.Log("Jump SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        //ClientManager.Inst.RoomChat(JsonConvert.SerializeObject(roomChatData));

    }
    public bool OnPlayerJumpOnBoard(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("Jump  =>" + msg);
        CustomData data = JsonConvert.DeserializeObject<CustomData>(msg);
        if (data!=null&&data.type == (int)CustomType.Jump)
        {

            if (Player.Id == senderPlayerId)
            {
                JumpOnBoard(player.transform);

                if (PlayerOnBoardControl.Inst)
                {
                    PlayerOnBoardControl.Inst.PlayJumpOnBoard();
                }
            }
            else {

                OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(senderPlayerId);
                if (otherCtr!=null)
                {
                    Transform otherplayertrans = otherCtr.transform;
                    JumpOnBoard(otherplayertrans);
                    otherplayertrans.GetComponent<OtherPlayerCtr>().PlayJumpOnBoard();
                }
            }
           
        }
       
        return true;
    }
    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("Magnetic OnReceiveServer==>" + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                SetBoard(item);
            }
        }

        return true;

    }


    private void SetBoard(Item item)
    {
        if (item.type == (int)ItemType.MAGNETIC_BOARD)
        {
            int hashCode = GetMagneticBoardHashCode(item.id);
            if (hashCode != 0)
            {
                
                MagneticBoardBehaviour bv = boardsDict[hashCode];
                LoggerUtils.Log("Magnetic OnReceiveServer==>" + item.data);
                MagneticData ma = JsonConvert.DeserializeObject<MagneticData>(item.data);
                bool isOnBoard = ma.status == 1;
                SetDataBoard(ma,hashCode);
                if (isOnBoard && !curBoardDict.ContainsValue(bv.carryTran))//上板子
                {
                    if (Player.Id == ma.playerId)
                    {
                        PlayerOnBoard(hashCode);
                    
                    }
                    else
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(ma.playerId);
                        if (otherCtr != null)
                        {
                            Transform otherplayertrans = otherCtr.transform;
                            otherplayertrans.GetComponent<OtherPlayerCtr>().isAvoidFrame = true;
                            otherplayertrans.GetComponent<OtherPlayerCtr>().avoidLerpCount = 1;
                            OnBoard(hashCode, otherplayertrans);
                          
                        }
                    }
                }
                else if (!isOnBoard && curBoardDict.ContainsValue(bv.carryTran))//下板子
                {
                    if (Player.Id == ma.playerId)
                    {
                        PlayerDownBoard();
                    }
                    else
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(ma.playerId);
                        if (otherCtr != null)
                        {
                            Transform otherplayertrans = otherCtr.transform;
                            otherplayertrans.GetComponent<OtherPlayerCtr>().isAvoidFrame = false;
                            otherplayertrans.GetComponent<OtherPlayerCtr>().avoidLerpCount = 1;
                            DownBoard(otherplayertrans);
                            
                        }

                    }
                }

            }

        }
    }
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========MagneticBoardManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[MagneticBoardManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                if (getItemsRsp.items == null)
                {
                    LoggerUtils.Log("[MagneticBoardManager.OnGetItemsCallback]getItemsRsp.items is null");
                    return;
                }

                for (int i = 0; i < getItemsRsp.items.Length; i++)
                {
                    Item item = getItemsRsp.items[i];
                    SetBoard(item);
                   
                }
            }
        }
    }
    public void SetDataBoard(MagneticData data, int Code)
    {
        if (data.status == 1 && !dataDict.ContainsKey(data.playerId))//上板子
        {
            dataDict.Add(data.playerId, Code);
        }
        else if (data.status == 0 && dataDict.ContainsKey(data.playerId))//下板子
        {
            dataDict.Remove(data.playerId);
        }
    }



    public void AddBoard(int hashCode, MagneticBoardBehaviour boardBehaviour)
    {
        if (!boardsDict.ContainsKey(hashCode))
        {
            boardsDict.Add(hashCode, boardBehaviour);
        }
    }

    public void RemoveBoard(MagneticBoardBehaviour boardBehaviou)
    {
        if (boardBehaviou == null) return;
        if (boardsDict.ContainsValue(boardBehaviou))
        {
            boardsDict.Remove(boardBehaviou.GetHashCode());
        }
    }

    public void OnBoard(int hashCode,Transform transform)
    {
        if (transform!=null)
        {
            MagneticBoardBehaviour bv = boardsDict[hashCode];
            if (bv!=null&&!curBoardDict.ContainsValue(bv.carryTran))
            {
                curBoardDict.Add(transform, bv.carryTran);
                bv.OnBoardSuccess();
            }
        }
    }
    public void DownBoard(Transform transform)
    {
        if (transform!=null&& curBoardDict.ContainsKey(transform))
        {

            MagneticBoardBehaviour magneticBoardBehaviour = curBoardDict[transform].GetComponentInParent<MagneticBoardBehaviour>();
            if (magneticBoardBehaviour!=null)
            {
                magneticBoardBehaviour.DownBoardSuccess();
            }
            curBoardDict.Remove(transform);
            if (playerRotInfoDict.ContainsKey(transform))
            {
                playerRotInfoDict.Remove(transform);
            }
        }
    }
    public void PlayerSendDownBoard()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (curBoardDict.ContainsKey(player.transform))
            {
                SendUnBindBoard(curBoardDict[player.transform].GetComponentInParent<MagneticBoardBehaviour>());
            }
        }
        else
        {
            PlayerDownBoard();
        }

        //if (curBoardDict.ContainsKey(player.transform))
        //{
        //    SendUnBindBoard(curBoardDict[player.transform].GetComponentInParent<MagneticBoardBehaviour>());
        //}
    }
    public void OnBoardDisable(Transform carry,int uid)
    {
        if (curBoardDict!=null&& player!=null)
        {
            if (curBoardDict.ContainsKey(player.transform) && curBoardDict[player.transform] == carry)
            {
                PlayerDownBoard();
                if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
                {
                    SendUnBindBoard(uid);
                }
                // SendUnBindBoard(uid);
            }
        }
       
    }
    public void PlayerForceDownBoard(bool isTrap = false)
    {
        if (player == null)
        {
            return;
        }
        if (curBoardDict.ContainsKey(player.transform))
        {
            MagneticBoardBehaviour bv =  curBoardDict[player.transform].GetComponentInParent<MagneticBoardBehaviour>();
            int uid = bv.entity.Get<GameObjectComponent>().uid;
            PlayerDownBoard();
            if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
            {
                SendUnBindBoard(uid);
            }
           // SendUnBindBoard(uid);
        }
      
    }
    private void PlayerLeaveRoom(string id)
    {
        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
        if (otherCtr != null)
        {
            DownBoard(otherCtr.transform);
            if (dataDict.ContainsKey(id))
            {
                dataDict.Remove(id);
            }
        }
       
       
    }
    private void PlayerOnCreate()
    {
        if (Global.Room.RoomInfo.PlayerList!=null)
        {
            for (int i = 0; i < Global.Room.RoomInfo.PlayerList.Count; i++)
            {
                string id = Global.Room.RoomInfo.PlayerList[i].Id;
                if (dataDict.ContainsKey(id))
                {
                    if (id != Player.Id)
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
                        if (otherCtr != null)
                        {
                            if (!curBoardDict.ContainsKey(otherCtr.transform))
                            {
                                otherCtr.isAvoidFrame = true;
                                otherCtr.avoidLerpCount = 1;
                                OnBoard(dataDict[id], otherCtr.transform);
                            }
                        }
                    }
                    else
                    {
                        if (!curBoardDict.ContainsKey(player.transform))
                        {
                            PlayerOnBoard(dataDict[id]);
                        }
                    }
                }
                else
                {
                    if (Player.Id == id)
                    {
                        if (curBoardDict.ContainsKey(player.transform))
                        {
                            PlayerDownBoard();
                        }
                        
                    }
                    else
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
                        
                        if (otherCtr != null)
                        { 
                            if (curBoardDict.ContainsKey(otherCtr.transform))
                            {
                                otherCtr.isAvoidFrame = false;
                                otherCtr.avoidLerpCount = 1;
                                DownBoard(otherCtr.transform);
                            }

                        }

                    }
                }
            }

        }
       
        
    }
    public void PlayerDownBoard()
    {
        BlackPanel.Show();
        BlackPanel.Instance.PlayTransitionAnim();
        PlayModePanel.Instance.SetOnBoard(false);

        if (PlayerOnBoardControl.Inst)
        {
            PlayerOnBoardControl.Inst.UnBindBoard();
        }

        DownBoard(player.transform);
    }
    public void PlayerSendOnBoard(int hashCode)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (boardsDict.ContainsKey(hashCode) && boardsDict[hashCode] != null)
            {
                SendBindBoard(boardsDict[hashCode]);
            }
        }
        else
        {
            PlayerOnBoard(hashCode);
        }


        //if (boardsDict.ContainsKey(hashCode) && boardsDict[hashCode] != null)
        //{
        //    SendBindBoard(boardsDict[hashCode]);
        //}

    }
    public void PlayerOnBoard(int hashCode)
    {
        BlackPanel.Show();
        BlackPanel.Instance.PlayTransitionAnim();

        var playerOnBoardCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerOnBoardControl>();
        if (!playerOnBoardCtrl)
        {
            // 添加磁力板控制脚本
            playerOnBoardCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerOnBoardControl>();
        }
        if (PlayerOnBoardControl.Inst)
        {
            PlayerOnBoardControl.Inst.OnBoard();
        }
        PlayModePanel.Instance.SetOnBoard(true);
        OnBoard(hashCode, player.transform);
        boardsDict[hashCode].SetPlayerNode();
    }


    private void JumpOnBoard(Transform transform)
    {
        if (curBoardDict.ContainsKey(transform))
        {
            curBoardDict[transform].GetComponentInParent<MagneticBoardBehaviour>().JumpOnBoard();
        }
    }
    public void LandOnBoard(Transform carry)
    {
        if (!curBoardDict.ContainsKey(player.transform))
        {
            return;
        }
        if (curBoardDict[player.transform] == carry)
        {
            if (PlayerOnBoardControl.Inst)
            {
                PlayerOnBoardControl.Inst.LandOnBoard();
            }
        }
    }
    public void OnChangeMode(GameMode mode)
    {
        if (boardsDict.Count > 0)
        {
            dataDict.Clear();
            curBoardDict.Clear();
            playerRotInfoDict.Clear();
            foreach (var boardBehaviour in boardsDict.Values)
            {
                if (boardBehaviour != null)
                {
                    boardBehaviour.OnModeChange(mode);
                }
            }
        }
      
    }

    public void OnReset()
    {
        
        if (boardsDict.Count > 0)
        {
            foreach (var boardBehaviour in boardsDict.Values)
            {
                if (boardBehaviour != null)
                {
                    boardBehaviour.OnRestart();
                }
            }
            foreach (var otherPlayer in curBoardDict.Keys)
            {
                if (otherPlayer != null)
                {
                    OtherPlayerCtr otherplayertrans = otherPlayer.transform.GetComponent<OtherPlayerCtr>();
                    if (otherplayertrans != null)
                    {
                        otherplayertrans.isAvoidFrame = false;

                    }
                }
            }
            dataDict.Clear();
            curBoardDict.Clear();
            playerRotInfoDict.Clear();
        }
        PlayModePanel.Instance.SetOnBoard(false);

        if (PlayerOnBoardControl.Inst)
        {
            PlayerOnBoardControl.Inst.UnBindBoard();
        }
        //DownBoard(player.transform);
        AllDownBoard();
    }

    private void AllDownBoard()
    {
        foreach (var pBoard in curBoardDict)
        {
            
            MagneticBoardBehaviour magneticBoardBehaviour = pBoard.Key.GetComponentInParent<MagneticBoardBehaviour>();
            if (magneticBoardBehaviour != null)
            {
                magneticBoardBehaviour.DownBoardSuccess();
            }
        }
        curBoardDict.Clear();
        playerRotInfoDict.Clear();
    }



    public int GetMagneticBoardHashCode(int uid)
    {
        foreach (var sid in boardsDict)
        {
            
            if (uid == sid.Value.entity.Get<GameObjectComponent>().uid)
            {
                return sid.Key;
            }
        }
        return 0;
    }



    private float offset;
    public void SetOffset(float set)
    {
        offset = set;
    }
    public void SetView(Quaternion quaternion)
    {
        if (curBoardDict.ContainsKey(player.transform))
        {
            curBoardDict[player.transform].localRotation = quaternion;
        }
        
    }
    public bool IsOtherPlayerOnBoard(OtherPlayerCtr otherCtr)
    {
        return curBoardDict.ContainsKey(otherCtr.transform);
    }

    protected override void OnDestroy()
    {
        playerRotInfoDict.Clear();
        curBoardDict.Clear();
        boardsDict.Clear();
        dataDict.Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.PosMove, PlayerForceDownBoard);
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
        MessageHelper.RemoveListener(MessageName.PlayerCreate, PlayerOnCreate);
    }
}
