/// <summary>
/// Author:WeiXin
/// Description:方向盘管理器
/// Date: 2021-01-24
/// </summary>
/// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public class SteeringWheelManager : MonoManager<SteeringWheelManager>, IManager,IPVPManager,INetMessageHandler
{
    private PlayerBaseControl player;
    private SteeringWheelBehaviour playSteering;
    private steeringWheelStatus steeringStatus = new steeringWheelStatus();
    
    RigidbodyConstraints freezeConstraints = ~RigidbodyConstraints.FreezePositionY;
    RigidbodyConstraints moveConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

    private Dictionary<int, SteeringWheelBehaviour> carDic = new Dictionary<int, SteeringWheelBehaviour>();
    private Dictionary<string, Item> playerDic = new Dictionary<string, Item>(); //保存玩家当前在车上的数据

    private bool isMoving = false;

    public void Awake()
    {
    }
    
    public override void Init()
    {
        player = GameObject.Find("GameStart")?.GetComponent<GameController>().playerCom;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.PosMove, SendGetOffCar);
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
        MessageHelper.AddListener(MessageName.PlayerCreate, PlayerOnCreate);
    }

    public void Update()
    {
        if (player == null) return;
        foreach (var carkv in carDic)
        {
            var v = carkv.Value;
            if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel == v) continue;
            v.DriverFollowCar();
        }

        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            var trs = PlayerDriveControl.Inst.steeringWheel.follow;
            player.transform.position = trs.position;
            player.transform.Rotate(Vector3.up, offset, Space.Self);
            offset = 0;
        }
    }

    public void FixedUpdate()
    {
        OperatingCar(playSteering);
    }

    private float offset;
    public void SetOffset(float set)
    {
        offset = set;
    }

    private GameObject getCarRoot(GameObject obj)
    {
        var parent = obj.transform.parent;
        var cNode = parent.GetComponent<CombineBehaviour>();
        var mNode = parent.GetComponent<DoTweenBehaviour>();
        if (mNode != null)
        {
            obj = parent.gameObject;
        }else if (cNode != null)
        {
            if (parent.parent.GetComponent<DoTweenBehaviour>() != null)
            {
                obj = parent.parent.gameObject;
            }
            else
            {
                obj = parent.gameObject;
            }
        }

        return obj;
    }

    //Add by Shaocheng, 恢复人在车上的表现。原SetCar只用来恢复数据和车的位置
    private void UpdatePlayerOnCars(bool isOnCar, SteeringWhellData sd, int uid, SteeringWheelBehaviour car)
    {
        if (sd == null || car == null)
        {
            return;
        }
        LoggerUtils.Log($"SteeringWheelManager UpdatePlayerOnCars: {isOnCar}, {uid}");

        if (isOnCar)
        {
            if (Player.Id == sd.playerId)
            {
                PlayerOnSteering(uid);
            }
            else
            {
                OtherPlayerCtr op = ClientManager.Inst.GetOtherPlayerComById(sd.playerId);
                if (op != null)
                {
                    var trs = op.transform;
                    op.isAvoidFrame = false;
                    op.avoidLerpCount = 1;
                    DrivingCar(car, trs);
                    op.steeringWheel = car;
                    op.DrivingCar(true);
                }
            }
        }
        else
        {
            if (Player.Id == sd.playerId)
            {
                GetOffCar();
            }
            else
            {
                OtherPlayerCtr op = ClientManager.Inst.GetOtherPlayerComById(sd.playerId);
                if (op != null)
                {
                    var trs = op.transform;
                    op.isAvoidFrame = false;
                    op.avoidLerpCount = 1;
                    DrivingCar(car);
                    op.steeringWheel = null;
                    op.DrivingCar(false);
                }
            }
        }
    }

    private void SetCar(Item item)
    {
        if (item.type == (int)ItemType.STEERING_WHEEL)
        {
            var uid = item.id;
            var sd = JsonConvert.DeserializeObject<SteeringWhellData>(item.data);
            bool isOnCar = sd.status == 1;
            if (!isOnCar && sd.playerId == null)
            {
                sd.playerId = playerDic.FirstOrDefault(v => v.Value.id == uid).Key;
            }
            if (isOnCar)//上车
            {
                if (!playerDic.ContainsKey(sd.playerId))
                {
                    playerDic.Add(sd.playerId, item);
                }
                else
                {
                    //如果GetItems后又收到了广播，及时刷新数据
                    playerDic.Remove(sd.playerId);
                    playerDic.Add(sd.playerId, item);
                }
            }
            else if (!isOnCar && sd.playerId != null && playerDic.ContainsKey(sd.playerId))//下车
            {
                playerDic.Remove(sd.playerId);
            }
            if (!carDic.ContainsKey(uid))
            {
                return;
            }
            var car = carDic[uid];
            float px, py, pz, rx, ry, rz, rw;
            bool isMoved = false;
            string[] vals = sd.position.Split('|');
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
                var pos = new Vector3(px/SteeringWhellData.DataMultiple, 
                    py/SteeringWhellData.DataMultiple, 
                    pz/SteeringWhellData.DataMultiple);
                var rot = new Quaternion(rx/SteeringWhellData.DataMultiple, 
                    ry/SteeringWhellData.DataMultiple, 
                    rz/SteeringWhellData.DataMultiple, 
                    rw/SteeringWhellData.DataMultiple).normalized;
                if (isMoved) car.Anim(false);
                var root = getCarRoot(car.carTrs.gameObject);
                root.transform.SetPositionAndRotation(pos, rot);
            }
            
            UpdatePlayerOnCars(isOnCar, sd, uid, car);
        }
    }
    
    public void HandlePlayerCreated()
    {
        LoggerUtils.Log("SteeringWheelManager : HandlePlayerCreated");
        foreach (var playerId in playerDic.Keys)
        {
            if (playerDic[playerId] != null)
            {
                var item = playerDic[playerId];
                var uid = item.id;
                var sd = JsonConvert.DeserializeObject<SteeringWhellData>(item.data);
                bool isOnCar = sd.status == 1;
                if (carDic.ContainsKey(uid))
                {
                    var car = carDic[uid];
                    UpdatePlayerOnCars(isOnCar, sd, uid, car);
                }
                else
                {
                    LoggerUtils.Log($"CarsDic not contains uid:{uid}");
                }
            }
        }
    }

    public void AddCar(int uid, SteeringWheelBehaviour car)
    {
        car.uid = uid;
        if (!carDic.ContainsKey(uid))
        {
            carDic.Add(uid, car);
        }
    }
    
    private void OperatingCar(SteeringWheelBehaviour car)
    {
        if (car != null && car.carRgb != null)
        {
            var rgb = car.carRgb;
            var trs = rgb.transform;

            if (rgb.velocity.sqrMagnitude < 0.1f && isMoving)
            {
                isMoving = false;
                AKSoundManager.Inst.steeringWheelSound(car.gameObject, isMoving);
            }else if (rgb.velocity.sqrMagnitude > 0.1f && !isMoving)
            {
                isMoving = true;
                AKSoundManager.Inst.steeringWheelSound(car.gameObject, isMoving);
            }
            
            if (steeringStatus.vStatus != steeringVertical.None)
            {
                var v = Time.fixedDeltaTime * steeringStatus.acceleration;
                v = steeringStatus.vStatus == steeringVertical.forward ? v : -v;
                steeringStatus.speed += v;
            }
            else
            {
                var s = Mathf.Sign(steeringStatus.speed);
                steeringStatus.speed += -s * steeringStatus.deceleration * Time.fixedDeltaTime;
                steeringStatus.speed = (s < 0 == steeringStatus.speed < 0) ? steeringStatus.speed : 0;
            }
            if (steeringStatus.speed != 0 && steeringStatus.lStatus != steeringLandscape.None)
            {
                var v = steeringStatus.angularSpeed;
                if ((steeringStatus.lStatus == steeringLandscape.left && steeringStatus.speed > 0) || 
                    (steeringStatus.lStatus == steeringLandscape.right && steeringStatus.speed < 0))
                {
                    v = -v;
                }
                var ov = rgb.transform.rotation;
                var nv = trs.rotation * Quaternion.Euler(v * Time.fixedDeltaTime);
                rgb.MoveRotation(nv);
                if (player) player.transform.Rotate(Vector3.up, nv.eulerAngles.y - ov.eulerAngles.y, Space.Self);
            }
            var speed = trs.forward * steeringStatus.speed;
            speed.y = rgb.velocity.y;
            rgb.velocity = speed;
        }
    }

    public void SteeringWheelSound(bool play = false)
    {
        if (playSteering && isMoving)
        { 
            AKSoundManager.Inst.steeringWheelSound(playSteering.gameObject, play);
        }
    }

    public void OnRightClick(bool isDown)
    {
        steeringStatus.right = isDown;
    }

    public void OnLeftClick(bool isDown)
    {
        steeringStatus.left = isDown;
    }

    public void OnBackwardClick(bool isDown)
    {
        steeringStatus.backward = isDown;
    }

    public void OnForwardClick(bool isDown)
    {
        steeringStatus.forward = isDown;
    }

    public void PlayerSendOnSteering(int uid)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (carDic.ContainsKey(uid) && carDic[uid] != null)
            {
                SendDrivingCar(uid, true);
            }
        }
        else
        {
            PlayerOnSteering(uid);
        }
    }

    public void PlayerOnSteering(int uid)
    {
        playSteering = carDic[uid];
        if (PlayerDriveControl.Inst && playSteering == PlayerDriveControl.Inst.steeringWheel) return;
        BlackPanel.Show();
        BlackPanel.Instance.PlayTransitionAnimAct(null);

        var playerDriveCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerDriveControl>();
        if (!playerDriveCtrl)
        {
            //添加驾驶控制脚本
            playerDriveCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerDriveControl>();
        }

        if (PlayerDriveControl.Inst)
        {
            PlayerDriveControl.Inst.OnSteering(playSteering);
        }

        DrivingCar(playSteering, player.transform);
        PlayModePanel.Instance.Driving(true);

        PlayerBaseControl.Inst.PlayAnimation(AnimId.IsOnSteering, true);
        player.SetPosToNewPoint(playSteering.transform.position - player.initPos, playSteering.transform.rotation);
    }

    public void ResetPlayerLookSteering()
    {
        player.SetPosToNewPoint(playSteering.transform.position - player.initPos, playSteering.transform.rotation);
    }

    public void OnSteering(int hashCode, Transform transform)
    {
        if (transform != null)
        {
            OnSteeringSuccess();
        }
    }

    public void OnSteeringSuccess()
    {
    }

    private void OnChangeMode(GameMode mode)
    {
        foreach (var car in carDic.Values)
        {
            if (car != null)
            {
                car.OnModeChange(mode);
            }
        }
        if (mode == GameMode.Edit)
        {
            GetOffCar();
        }
        else
        {
            
        }
    }
    
    private void PlayerLeaveRoom(string id)
    {
        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
        if (otherCtr != null)
        {
            if (playerDic.ContainsKey(id))
            {
                if (carDic.ContainsKey(playerDic[id].id)) DrivingCar(carDic[playerDic[id].id]);
                playerDic.Remove(id);
            }
        }
    }
    
    private void PlayerOnCreate()
    {
        if (Global.Room.RoomInfo.PlayerList != null)
        {
            foreach (var playerInfo in Global.Room.RoomInfo.PlayerList)
            {
                var id = playerInfo.Id;
                if (playerDic.ContainsKey(id))
                {
                    
                }
            }
        }
    }

    public void CombineCar(SceneEntity entity)
    {
        var go = entity.Get<GameObjectComponent>().bindGo;
        var sb = go.GetComponentInChildren<SteeringWheelBehaviour>();
        if (sb != null)
        {
            var pTrs = go.transform;
            SetCarCenter(pTrs, sb.transform);
            sb.carMCS = pTrs.GetComponentsInChildren<MeshCollider>(true);
        }
    }

    private void SetCarCenter(Transform parent, Transform sb)
    {
        var pos = sb.InverseTransformPoint(parent.position);
        pos.y = 0;
        pos.z = 0;
        pos = sb.TransformPoint(pos);
        List<Transform> tlist = new List<Transform>();
        foreach (Transform child in parent)
        {
            tlist.Add(child);
        }

        parent.DetachChildren();
        parent.position = pos;
        parent.rotation = sb.rotation;
        tlist.ForEach((t) => t.SetParent(parent));
    }

    public Rigidbody AddPhysical(GameObject obj)
    {
        obj = getCarRoot(obj);
        Rigidbody rgb = obj.GetComponent<Rigidbody>();
        if (rgb == null)
        {
            rgb = obj.AddComponent<Rigidbody>();
            rgb.useGravity = false;
            rgb.constraints = freezeConstraints;
            rgb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rgb.constraints = moveConstraints;
            rgb.drag = 0.1f;
            rgb.angularDrag = Mathf.Infinity;
            rgb.isKinematic = false;
            rgb.useGravity = true;
        }

        return rgb;
    }

    private void ClearProperties(SceneEntity entity)
    {
        entity.Remove<RPAnimComponent>();
        entity.Remove<CollectControlComponent>();
        entity.Remove<MovementComponent>();
        ShowHideManager.Inst.OnCombineNode(entity);
        SwitchControlManager.Inst.OnCombineNode(entity);
        SwitchManager.Inst.OnCombineNode(entity);
        LockHideManager.Inst.RefreshLockList(entity.Get<GameObjectComponent>().uid, false);
        PickabilityManager.Inst.OnCombineNode(entity);
    }

    public void DrivingCar(SteeringWheelBehaviour car, Transform player = null)
    {
        var driving = player != null;
        if (driving)
        {
            // ClearProperties(car.entity);
            PlayerBaseControl.Inst.animCon.StopLoop();
            car.carRgb = AddPhysical(car.transform.gameObject);
            car.Anim(false);
            car.carBc.gameObject.layer = LayerMask.NameToLayer("Default");
            car.driverTrs = player;
            car.isMoved = true;
            car.carMCS = car.carRgb.gameObject.GetComponentsInChildren<MeshCollider>(true);
        }
        else if(car.carRgb != null)
        {
            car.driverTrs = null;
            car.Anim(false);
            // car.ResetPos();
            car.ResetLayer();
            // car.carRgb.constraints = freezeConstraints;
            car.carMCS = car.carRgb.gameObject.GetComponentsInChildren<MeshCollider>(true);
            car.carRgb.isKinematic = true;
            Destroy(car.carRgb);
            car.carRgb = null;
        }
        if (car.carMCS != null)
        {
            foreach (MeshCollider mc in car.carMCS)
            {
                mc.convex = driving;
            }
        }
    }

    public void SendDrivingCar(int uid, bool driving)
    {
        var carPos = carDic[uid].transform.position;
        var carRot = carDic[uid].transform.rotation;
        SteeringWhellData sd = new SteeringWhellData
        {
            status = driving ? 1 : 0,
            playerId = Player.Id,
            position = 
                $"{carPos.x * SteeringWhellData.DataMultiple:0}|" +
                $"{carPos.y * SteeringWhellData.DataMultiple:0}|" +
                $"{carPos.z * SteeringWhellData.DataMultiple:0}|" +
                $"{carRot.x * SteeringWhellData.DataMultiple:0}|" +
                $"{carRot.y * SteeringWhellData.DataMultiple:0}|" +
                $"{carRot.z * SteeringWhellData.DataMultiple:0}|" +
                $"{carRot.w * SteeringWhellData.DataMultiple:0}|" +
                $"{carDic[uid].isMoved}",
        };
        Item itemData = new Item()
        {
            id = uid,
            type = (int)ItemType.STEERING_WHEEL,
            data = JsonConvert.SerializeObject(sd),
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
        LoggerUtils.Log("SteeringWheel SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendDrivingCarCallBack);
    }
    private void SendDrivingCarCallBack(int code, string data)
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
                LoggerUtils.Log("SendDrivingCarCallBack =>" + ret.retcode);
                TipPanel.ShowToast("The steering wheel has been occupied by other player");
            }
        }
    }
    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("SteeringWheel OnReceiveServer==>" + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                SetCar(item);
            }
        }
        return true;
    }
    
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========SteeringWheelManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.LogError("[SteeringWheelManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                if (getItemsRsp.items == null)
                {
                    LoggerUtils.LogError("[SteeringWheelManager.OnGetItemsCallback]getItemsRsp.items is null");
                    return;
                }
                foreach (var item in getItemsRsp.items)
                {
                    SetCar(item);
                }
            }
        }
    }

    public void SendGetOffCar(bool isTrap = false)
    {
        if (isTrap)
        {
            steeringStatus.speed = 0;
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (playSteering && carDic.ContainsKey(playSteering.uid))
            {
                SendDrivingCar(playSteering.uid, false);
            }
        }
        else
        {
            GetOffCar();
        }
    }

    public void GetOffCar()
    {
        if (playSteering != null)
        {
            steeringStatus.Reset();
            BlackPanel.Show();
            PlayModePanel.Instance.Driving(false);

            if (PlayerDriveControl.Inst)
            {
                PlayerDriveControl.Inst.GetOffSteering();
            }
            DrivingCar(playSteering);
            AKSoundManager.Inst.steeringWheelSound(playSteering.gameObject);
            playSteering = null;

            PlayerBaseControl.Inst.PlayAnimation(AnimId.IsOnSteering, false);
            BlackPanel.Instance.PlayTransitionAnimAct(null);
            isMoving = false;
        }
    }
    //重置方向盘状态
    public void OnReset()
    {
        if (playSteering != null)
        {
            steeringStatus.Reset();
            PlayModePanel.Instance.Driving(false);

            if (PlayerDriveControl.Inst)
            {
                PlayerDriveControl.Inst.GetOffSteering();
            }
            DrivingCar(playSteering);
            AKSoundManager.Inst.steeringWheelSound(playSteering.gameObject);
            playSteering = null;
            PlayerBaseControl.Inst.PlayAnimation(AnimId.IsOnSteering, false);
            isMoving = false;
        }
        
        
        //分离其他玩家
        foreach (var otherPlayerData in ClientManager.Inst.otherPlayerDataDic)
        {
            PlayerLeaveRoom(otherPlayerData.Key);
            otherPlayerData.Value.steeringWheel = null;
            otherPlayerData.Value.PlayAnim("IsOnSteering", false);
        }

        foreach (var car in carDic.Values)
        {
            car.ResetPos();
        }
        
        playerDic.Clear();
    }

    public void OnPanelReset()
    {
        steeringStatus.Reset();
    }

    public void OnSteeringWheelDisable(int uid)
    {
        if (carDic.ContainsKey(uid))
        {
            if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
            {
                SendDrivingCar(uid, false);
            }
            else if (player != null && carDic[uid] != null && carDic[uid].driverTrs == player.transform)
            {
                GetOffCar();
            }
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        carDic.Clear();
        playerDic.Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.PosMove, SendGetOffCar);
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
        MessageHelper.RemoveListener(MessageName.PlayerCreate, PlayerOnCreate);
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }

    public void Clear()
    {
    }

}

public enum steeringVertical
{
    None = 0,
    forward,
    backward,
}

public enum steeringLandscape
{
    None = 0,
    left,
    right,
}

public class steeringWheelStatus
{
    public steeringVertical vStatus = steeringVertical.None;
    public steeringLandscape lStatus = steeringLandscape.None;
    private bool _forward = false;
    private bool _backward = false;
    private bool _left = false;
    private bool _right = false;

    private float _speed = 0;
    public float maxSpeed = 20f;
    public float acceleration = 15f;
    public float deceleration = 5f;
    public Vector3 angularSpeed = new Vector3(0, 90, 0);
    
    public float speed
    {
        set => _speed = Mathf.Clamp(value, -maxSpeed, maxSpeed);
        get => _speed;
    }
    
    public bool forward
    {
        set
        {
            _forward = value;
            SetVerticl();
        }
    }
    
    public bool backward
    {
        set
        {
            _backward = value;
            SetVerticl();
        }
    }
    
    public bool left
    {
        set
        {
            _left = value;
            SetLandscape();
        }
    }
    
    public bool right
    {
        set
        {
            _right = value;
            SetLandscape();
        }
    }

    public void Reset()
    {
        speed = 0;
        vStatus = steeringVertical.None;
        lStatus = steeringLandscape.None;
        _forward = false;
        _backward = false;
        _left = false;
        _right = false;
    }

    private void SetVerticl()
    {
        if (_forward == _backward)
        {
            vStatus = steeringVertical.None;
        }
        else
        {
            vStatus = _forward ? steeringVertical.forward : steeringVertical.backward;
        }
    }
    
    private void SetLandscape()
    {
        if (_left == _right)
        {
            lStatus = steeringLandscape.None;
        }
        else
        {
            lStatus = _left ? steeringLandscape.left : steeringLandscape.right;
        }
    }
}