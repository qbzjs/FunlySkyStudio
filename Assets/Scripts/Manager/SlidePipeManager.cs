using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;
using static SlidePipePanel;

/// <summary>
/// Author:JayWill
/// Description:滑梯管理器
/// </summary>

public class SlidePipeManager:ManagerInstance<SlidePipeManager>, IManager,IPVPManager
{
    public class SlidePipeGetItemInfo
    {
        public int status;
        public int Id;
        public string mapId;
        public int inPos;//0 正向，1 反向
    }
    public const int MaxCount = 99;
    public const int MaxItemCount = 99;
    public const string MAX_COUNT_TIP = "Up to 99 Sliders can be placed.";
    public const string MAX_ITEM_TIP = "Maximum 99 sections.";
    public const string MIN_ITEM_TIP = "At least 1 section.";
    public const string CHANGE_SHAPE_TIP = "You can only change the shape of the last section of Slider.";
    public const string LOCK_TIPS = "Please quit slide first.";
    private string ITEM_DEFALUT_COLOR = "3AC0FF";

    public int CurSlideType = (int)SlideResType.Forward;
    public int CurSpeedType = (int)ESpeedType.Medium;
    public SlideItemBehaviour CurSelectItemBehaviour = null;

    private Dictionary<int, SlidePipeBehaviour> mPipeDict = new Dictionary<int, SlidePipeBehaviour>();
    public void ShowLockTips()
    {
        TipPanel.ShowToast(LOCK_TIPS);
    }
    public SlidePipeManager()
    {
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.PosMove, PlayerForceAbortSlide);
        MessageHelper.AddListener(MessageName.ChangeTps, OnChangeTps);
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.PosMove, PlayerForceAbortSlide);
        MessageHelper.RemoveListener(MessageName.ChangeTps, OnChangeTps);
        Clear();
    }

    private void OnChangeTps()
    {
        var isTps = PlayerBaseControl.Inst && PlayerBaseControl.Inst.isTps;
        if (isTps)
        {
            if (PlayerControlManager.Inst != null)
            {
                IPlayerCtrlMgr iCtrl = PlayerControlManager.Inst.GetPlayerCtrlMgr(PlayerControlType.SlidePipe);
                if (iCtrl != null)
                {
                    PlayerSlidePipeControl ctrl = iCtrl as PlayerSlidePipeControl;
                    if (ctrl.mSlideMoveComponent!=null)
                    {
                        ctrl.mSlideMoveComponent.OnChangeTps();
                    }
                }
            }
        }
    }
    public void PlayerForceAbortSlide(bool isTrap=false)
    {
        if (PlayerControlManager.Inst!=null)
        {
            PlayerSlidePipeControl ctrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
            if (ctrl != null&& ctrl.IsOnSlide())
            {
                ctrl.ForceAbortSlideAction();
            }
        }
    }
    public void AddSlidePipe(SlidePipeBehaviour behaviour)
    {
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        if (!mPipeDict.ContainsKey(uid))
        {
            mPipeDict.Add(uid, behaviour);
        }
    }

    public void RemoveSlidePipe(SlidePipeBehaviour behaviour)
    {
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        if (mPipeDict.ContainsKey(uid))
        {
            mPipeDict.Remove(uid);
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.SlidePipe)
        {
            int uid = goCmp.uid;
            if (mPipeDict.ContainsKey(uid))
            {
                mPipeDict.Remove(uid);
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.SlidePipe)
        {
            SlidePipeBehaviour pipeBehaviour = behaviour as SlidePipeBehaviour;
            int uid = goCmp.uid;
            if (!mPipeDict.ContainsKey(uid))
            {
                mPipeDict.Add(uid,pipeBehaviour);
            }
        }  
    }
    public bool OnReceiveServer(string sendPlayerId, string msg)
    {
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        LoggerUtils.Log($"SlidePipeManager.OnReceiveServer={msg}");
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.SLIDE_PIPE)
                {
                    PlayerSlidePipeControl.PlayerItemData itemData = JsonConvert.DeserializeObject<PlayerSlidePipeControl.PlayerItemData>(item.data);
                    ESlideInPosType inPosType = (ESlideInPosType)itemData.inPos;
                    ESlideAction actionType = (ESlideAction)itemData.opType;
                    if (Player.Id == sendPlayerId)
                    {
                        PlayerSlidePipeControl ctrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
                        if (ctrl != null)
                        {
                            if (actionType == ESlideAction.Up)
                            {
                                bool isNegDir = inPosType == ESlideInPosType.Tail;
                                ctrl.OnUpSlidePipe(item.id, isNegDir);
                            }
                            else if(actionType == ESlideAction.Down)
                            {
                                //玩家可能已经被陷阱盒强制下滑梯了，需要判断一下是否在滑梯上
                                if (StateManager.IsOnSlide)
                                {
                                    ctrl.OnDownSlidePipe();
                                }
                            }
                            else if (actionType==ESlideAction.Start)
                            {
                                ctrl.OnStartSlide();
                            }
                        }
                    }
                    else
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(sendPlayerId);
                        if (otherCtr != null)
                        {
                            if (otherCtr.mSlideMovementCompt==null)
                            {
                                otherCtr.mSlideMovementCompt = new OtherPlayerSlideMoveCompt(otherCtr);
                                otherCtr.mSlideMovementCompt.Init();
                            }
                            if (actionType == ESlideAction.Up)
                            {
                                otherCtr.mSlideMovementCompt.GotoStart();
                            }
                            else if (actionType == ESlideAction.Down)
                            {
                                otherCtr.mSlideMovementCompt.GotoEnd();
                            }
                            else if(actionType==ESlideAction.Start)
                            {
                                otherCtr.mSlideMovementCompt.GotoState(ESlidePipeMoveState.Slide);
                            }
                            else if (actionType == ESlideAction.End)
                            {
                                otherCtr.mSlideMovementCompt.GotoState(ESlidePipeMoveState.EndIdle);
                            }
                        }
                    }
                    break;
                }
            }
        }
        return true;
    }
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========SlidePipeManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[SlidePipeManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            PlayerCustomData[] playerCustomDatas = getItemsRsp.playerCustomDatas;
            if (playerCustomDatas != null)
            {
                for (int i = 0; i < playerCustomDatas.Length; i++)
                {
                    PlayerCustomData playerData = playerCustomDatas[i];

                    ActivityData[] activitiesData = playerData.activitiesData;
                    if (activitiesData == null)
                    {
                        continue;
                    }
                    for (int n = 0; n < activitiesData.Length; n++)
                    {
                        ActivityData activeData = activitiesData[n];
                        if (activeData != null && activeData.activityId == ActivityID.SlidePipe)
                        {
                            SlidePipeGetItemInfo info = JsonConvert.DeserializeObject<SlidePipeGetItemInfo>(activeData.data);
                            if (info != null && info.mapId == GlobalFieldController.CurMapInfo.mapId)
                            {
                                ESlideState state = (ESlideState)info.status;
                                if (playerData.playerId==Player.Id)
                                {
                                    if (StateManager.IsOnSlide)
                                    {
                                        if (state == ESlideState.OutTheSlide)
                                        {
                                            IPlayerCtrlMgr iCtrl = PlayerControlManager.Inst.GetPlayerCtrlMgr(PlayerControlType.SlidePipe);
                                            if (iCtrl == null)
                                            {
                                                iCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerSlidePipeControl>();
                                            }
                                            PlayerSlidePipeControl ctrl = iCtrl as PlayerSlidePipeControl;
                                            ctrl.ForceAbortSlideAction();
                                        }
                                    }
                                    if (state == ESlideState.InTheSlide|| state == ESlideState.Slide)
                                    {
                                        PlayerSlidePipeControl ctrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
                                        if (ctrl == null)
                                        {
                                            ctrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerSlidePipeControl>();
                                        }
                                        SlideControlPanel.Show();
                                        SlideControlPanel.Instance.SetrSlidePipeCtrl(ctrl);
                                        SlidePipeBehaviour slidePipe = GetSlidePipe(info.Id);
                                        if (slidePipe!=null)
                                        {
                                            if (ctrl.mSlideMoveComponent==null)
                                            {
                                                ctrl.CreateMovement(info.Id,(ESlideInPosType)info.inPos == ESlideInPosType.Tail);
                                            }
                                            if (state == ESlideState.Slide)
                                            {
                                                ctrl.mSlideMoveComponent.ExcuteMove();
                                            }
                                        }
                                        PlayModePanel.Instance.SetOnSlidePipeMode(true);
                                        PlayerBaseControl.Inst.AddNoAbilityFlag(EObjAbilityType.Emo);
                                    }
                                }
                                else
                                {
                                    OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerData.playerId);
                                    if (otherCtr != null)
                                    {
                                        if (otherCtr.mSlideMovementCompt == null)
                                        {
                                            otherCtr.mSlideMovementCompt = new OtherPlayerSlideMoveCompt(otherCtr);
                                            otherCtr.mSlideMovementCompt.Init();
                                        }
                                        if (state == ESlideState.InTheSlide)
                                        {
                                            otherCtr.mSlideMovementCompt.GotoState(ESlidePipeMoveState.StartIdle);
                                        }
                                        else if (state == ESlideState.Slide)
                                        {
                                            otherCtr.mSlideMovementCompt.GotoState(ESlidePipeMoveState.Slide);
                                        }
                                        else if (state == ESlideState.OutTheSlide)
                                        {
                                            otherCtr.mSlideMovementCompt.OnMoveStateFinished();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
    }
    private string mSlideCentreClipName = "SlidePipe.huati_centre";
    private string mIdleClipName = "SlidePipe.huati_idle";
    public void HandleFrameState(OtherPlayerCtr otherPlayerCtr, OtherPlayerAnimStateManager animStateManager, Animator playerAnim, FrameStateType stateType)
    {
        if (otherPlayerCtr == null || animStateManager == null || playerAnim == null)
        {
            return;
        }
    }
    public SlideItemBehaviour CreateSlideItem(SlidePipeBehaviour pipeBehaviour,bool isFirstItem = false)
    {
        SlideItemBehaviour itemBehaviour = SceneBuilder.Inst.CreateSceneNode<SlideItemCreater,SlideItemBehaviour>();
        NodeData itemData = new NodeData() { uid = 0, id = (int)SlideResType.Forward };
        SlideItemData slideItemData = new SlideItemData()
        {
            index = pipeBehaviour.GetMaxIndex(),
            mat = 0,
            color = ITEM_DEFALUT_COLOR,
            tile = DataUtils.Vector2ToString(new Vector2(1, 1)),
            speedtype = (int)SlidePipePanel.ESpeedType.Medium,
        };
        var tailItem = pipeBehaviour.GetTailItem();
        if (tailItem)
        {
            SlideItemComponent tailItemCompt =  tailItem.entity.Get<SlideItemComponent>();
            slideItemData.mat = tailItemCompt.MatId;
            slideItemData.speedtype = tailItemCompt.SpeedType;
            slideItemData.color = DataUtils.ColorRGBAToString( tailItemCompt.Color);
            slideItemData.tile = DataUtils.Vector2ToString(tailItemCompt.Tile);
            GameObjectComponent objCompt = tailItem.entity.Get<GameObjectComponent>();
            itemData.id = objCompt.modId;
        }
        
        itemBehaviour.mRoot = pipeBehaviour;
        SlideItemCreater.SetData(itemBehaviour,slideItemData,itemData);
        if(tailItem)
        {
            var endNode = GameUtils.FindChildByName(tailItem.transform,"EndNode");
            itemBehaviour.transform.SetPositionAndRotation(endNode.position,endNode.rotation);
        }
        itemBehaviour.transform.SetParent(pipeBehaviour.transform);
        if(isFirstItem)
        {
            //默认的第一节微调localPosition
            itemBehaviour.transform.localPosition = new Vector3(0,0,-1);
        }
        else
        {   
            //默认的第一节不用添加Create记录，后续的需要
            EditModeController.AddCreateRecord(itemBehaviour.gameObject);
        }
        
        return itemBehaviour;
    }

    public void RemoveSlideItem(SlidePipeBehaviour pipeBehaviour)
    {
        var tailItem = pipeBehaviour.GetTailItem();
        if(tailItem)
        {
            tailItem.SetSelect(false);
            EditModeController.AddDestroyRecord(tailItem.gameObject);
            SecondCachePool.Inst.DestroyEntity(tailItem.gameObject);
        }
    }

    public void ChangeSlideItemModel(int resType,SlideItemBehaviour itemBehaviour)
    {
        SlidePipeManager.Inst.CurSlideType = resType;
        ChangeSlideItemModel(itemBehaviour);
    }

    public void ChangeSlideItemModel(SlideItemBehaviour itemBehaviour)
    {
        if(itemBehaviour.assetObj != null)
        {
            var gameComp = itemBehaviour.entity.Get<GameObjectComponent>();
            ModelCachePool.Inst.Release(gameComp.modId, itemBehaviour.assetObj);
        }
        var model = ModelCachePool.Inst.Get(CurSlideType);
        itemBehaviour.UpdateModel(model,CurSlideType);
    }
    public void HideSlideItemSelect()
    {
        foreach (var pipe in mPipeDict.Values)
        {
            pipe.UnSelectAllItem();
        }
    }

    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        if(newBev == null || !newBev.entity.HasComponent<SlidePipeComponent>()) return;
        var comp = newBev.entity.Get<GameObjectComponent>();
        AddSlidePipe(newBev as SlidePipeBehaviour);
    }

    private void HandlePackPanelShow(bool isShow)
    {
        foreach(var pipe in mPipeDict.Values)
        {
            if(!LockHideManager.Inst.IsHidedEntity(pipe.entity))
            {
                pipe.gameObject.SetActive(!isShow);
            }
        }
    }

    public void OnDisSelectTarget(GameObject target)
    {
        if(target != null)
        {
           var pipeBehaviour = target.GetComponent<SlidePipeBehaviour>();
            if(pipeBehaviour != null)
            {
               pipeBehaviour.UnSelectAllItem();
            }
        }
    }
    public SlidePipeBehaviour GetSlidePipe(int uid)
    {
        SlidePipeBehaviour node = null;
        if (mPipeDict.TryGetValue(uid,out node))
        {
        }
        return node;
    }

    public bool IsOverMaxCount()
    {
        if (mPipeDict.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }

    public bool IsCanClone(GameObject curTarget)
    {
        var entity = curTarget.GetComponent<NodeBaseBehaviour>().entity;
        var comp = entity.Get<GameObjectComponent>();
        switch (comp.modelType)
        {
            case NodeModelType.SlidePipe:
            case NodeModelType.SlideItem:
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
                break;
        }

        return true;
    }


    public void OnChangeMode(GameMode mode)
    {
        if(mode == GameMode.Play || mode == GameMode.Guest)
        {
            foreach(var pipe in mPipeDict.Values)
            {
                if(pipe.IsVirtualMode())
                {
                    pipe.SetRenderVisible(false);
                }
            }

             //设置一下layer
            foreach (var item in mPipeDict)
            {
                SlidePipeBehaviour val = item.Value;
                val.UpdateLayer();
            }
        }
        if(mode == GameMode.Edit)
        {
            foreach(var pipe in mPipeDict.Values)
            {
                if(pipe.IsVirtualMode())
                {
                    pipe.SetRenderVisible(true);
                }
            }
            if (PlayerBaseControl.Inst!=null)
            {
                PlayerSlidePipeControl ctrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
                if (ctrl != null&&ctrl.IsOnSlide())
                {
                    ctrl.ForceAbortSlideAction();
                }
            } 
        }
    }
    public float GetSpeed(ESpeedType speedType)
    {
        float speed = 2;
        switch (speedType)
        {
            case ESpeedType.ExtraSlow:
                speed = 2;
                break;
            case ESpeedType.Slow:
                speed = 4;
                break;
            case ESpeedType.Medium:
                speed = 6f;
                break;
            case ESpeedType.Fast:
                speed = 8f;
                break;
            case ESpeedType.ExtraFast:
                speed = 10f;
                break;
            default:
                break;
        }
        return speed;
    }
    public float GetSpeed(int speedType)
    {
        return GetSpeed((ESpeedType)speedType);
    }
    public void Clear()
    {

    }

    public void OnReset()
    {
        if (PlayerBaseControl.Inst != null)
        {
            PlayerSlidePipeControl ctrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
            if (ctrl != null&&ctrl.IsOnSlide())
            {
                ctrl.ForceAbortSlideAction();
            }
        }
    }
}
