using BudEngine.NetEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IceGemMatType
{
    Bright, //亮色
    Dark //灰色
}

public enum IceGemCollectState
{
    None, //未收集
    Complete, //已收集
    AllDone //全部集齐
}

public enum IceGemReqOpt 
{
    Collect, //收集
    Animation //合成动画
}

/// <summary>
/// Author: pzkunn
/// Description: 冰晶宝石管理器
/// Date: 2022/10/21 13:15:36
/// </summary>
public class CrystalStoneManager : ManagerInstance<CrystalStoneManager>, IManager
{
    private const int MaxCount = 1;
    private int uiCount; //显示已收集的宝石数量(处理UI动画-收集完成)
    private int collectCount; //已收集宝石数量(来自服务端数据, 用作UI展示)
    private int maxCount; //大地图宝石总数
    private List<IceGemCollectData> collectList;
    private List<IceGemCollectData> maxList;
    private Dictionary<int, CrystalStoneBehaviour> crystalsDic = new Dictionary<int, CrystalStoneBehaviour>(); //当前地图全部冰晶宝石
    public string GREATSNOW_FIRST_TIP_KEY = "GREATSNOW_FIRST_TIP_KEY";

    #region 宝石道具管理
    public void Init()
    {
        crystalsDic.Clear();
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Play || (mode == GameMode.Guest && !GlobalFieldController.IsDowntownEnter))
        {
            if (crystalsDic.Count > 0)
            {
                //显示宝石进度UI: 试玩0/1, 游玩广播更新
                PlayModePanel.Instance.ShowGreatSnowfieldPanel();
            }
        }
        //管理模式切换
        foreach (var behav in crystalsDic.Values)
        {
            if (behav)
            {
                behav.OnChangeMode(mode);
            }
        }
    }

    private void HandlePackPanelShow(bool show)
    {
        //打开组合页面隐藏冰晶宝石道具
        foreach (var key in crystalsDic.Keys)
        {
            crystalsDic[key].entity.Get<GameObjectComponent>().bindGo.SetActive(!show);
        }
    }

    //添加宝石列表
    public void AddCrystalNode(NodeBaseBehaviour behaviour)
    {
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        if (IsOverMaxCount() || crystalsDic.ContainsKey(uid))
        {
            LoggerUtils.LogError("AddCrystalNode --> Cannot Add");
            return;
        }
        crystalsDic.Add(uid, behaviour as CrystalStoneBehaviour);
        LoggerUtils.Log($"[Crystal]-->AddCrystalNode==>crystalBehavs:{crystalsDic.Count}");
    }

    //获取地图的第1个宝石
    public NodeBaseBehaviour GetCrystalNode()
    {
        foreach (var key in crystalsDic.Keys)
        {
            return crystalsDic[key];
        }
        return null;
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.CrystalStone)
        {
            int uid = goCmp.uid;
            if (crystalsDic.ContainsKey(uid))
            {
                crystalsDic.Remove(uid);
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.CrystalStone)
        {
            AddCrystalNode(behaviour);
        }
    }

    public bool IsOverMaxCount()
    {
        if (crystalsDic.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }

    public List<int> GetAllCrystalsList()
    {
        if (crystalsDic.Count == 0)
        {
            return null;
        }
        return new List<int>(crystalsDic.Keys);
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    public void Clear()
    {
        crystalsDic.Clear();
    }
    #endregion

    #region 人物收集逻辑
    public void OnPlayerCollectAnim()
    {
        if (!StateManager.CanPlayCharacterAnim())
        {
            return;
        }
        PlayerBaseControl player = PlayerBaseControl.Inst;
        player.PlayerResetIdle();
        player.animCon.PlayAnim((int)EmoName.EMO_CRYSTAL_GET);
        //禁用玩家行为, 播放结束/或被打断恢复
        player.SetJoystickReset(OnCollectAnimStart);
    }

    private void OnCollectAnimStart()
    {
        PlayerBaseControl player = PlayerBaseControl.Inst;
        player.waitPosChange = true;
        player.AddNoAbilityFlag(EObjAbilityType.Move);
        player.AddNoAbilityFlag(EObjAbilityType.Pickability);
        player.AddNoAbilityFlag(EObjAbilityType.SelfieMode);
        player.AddNoAbilityFlag(EObjAbilityType.Emo);
    }

    public void OnCollectAnimFinish(string playerId)
    {
        PlayerBaseControl player = PlayerBaseControl.Inst;
        player.waitPosChange = false;
        player.RemoveNoAbilityFlag(EObjAbilityType.Move);
        player.RemoveNoAbilityFlag(EObjAbilityType.Pickability);
        player.RemoveNoAbilityFlag(EObjAbilityType.SelfieMode);
        player.RemoveNoAbilityFlag(EObjAbilityType.Emo);
    }
    #endregion

    #region 联机广播处理
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========CrystalStoneManager===>OnGetItemsCallback:" + dataJson);
        if (string.IsNullOrEmpty(dataJson)) return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
        if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null || getItemsRsp.playerCustomDatas == null)
        {
            LoggerUtils.Log("[CrystalStoneManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
            return;
        }

        ActivityData[] activityDatas = null;
        foreach (var customData in getItemsRsp.playerCustomDatas)
        {
            if (customData.playerId.Equals(Player.Id))
            {
                activityDatas = customData.activitiesData;
            }
        }
        if (activityDatas != null && GlobalFieldController.IsDowntownEnter)
        {
            for (int i = 0; i < activityDatas.Length; i++)
            {
                ActivityData activityData = activityDatas[i];
                if (activityData != null && activityData.activityId == ActivityID.IceGem)
                {
                    SnowfieldCollectInfo info = JsonConvert.DeserializeObject<SnowfieldCollectInfo>(activityData.data);
                    if (info == null || info.collects == null)
                    {
                        LoggerUtils.Log("[CrystalStoneManager.OnGetItemsCallback]ActivityData.data is null");
                        return;
                    }

                    maxList = info.collects.FindAll(x => !string.IsNullOrEmpty(x.subMapId));
                    maxCount = maxList == null ? 0 : maxList.Count;
                    collectList = info.collects.FindAll(x => x.status == (int)IceGemCollectState.Complete && !string.IsNullOrEmpty(x.subMapId));
                    collectCount = collectList == null ? 0 : collectList.Count;
                    LoggerUtils.Log($"GetItems back --> CrystalStone collect {collectCount}/{maxCount}");
                    //更新宝石收集进度(全局)
                    GlobalFieldController.MaxIceGem = maxCount;
                    GlobalFieldController.CollectIceGem = collectCount;

                    if (info.isCompleted == (int)IceGemCollectState.Complete)
                    {
                        //修改: 不在返回大地图之后播放合成动画
                        //if (info.mapId == GlobalFieldController.CurMapInfo.mapId && !info.isPlayedAnimation)
                        //{
                            //当前地图是大地图, 初次收集齐 --> 播放合成动画
                            //PlayModePanel.Instance.ShowGreatSnowfieldPanel(maxCount, collectCount);
                            //PlayModePanel.Instance.ShowCollectedComplete();
                            //SendCompleteAnimRequest();
                        //}
                        //else
                        //{
                            //调起宝石收集齐的UI显示
                            PlayModePanel.Instance.ShowRewardBtn();
                        //}
                    }
                    else if (maxCount > 0)
                    {
                        //调起宝石收集进度UI
                        PlayModePanel.Instance.ShowGreatSnowfieldPanel(maxCount, collectCount);
                        uiCount = collectCount;
                    }

                    foreach (var item in info.collects)
                    {
                        if (item.subMapId == GlobalFieldController.CurMapInfo.mapId && item.status == (int)IceGemCollectState.Complete)
                        {
                            LoggerUtils.Log("GetItems CrystalStone Replace Material");
                            //替换已收集宝石的材质
                            var behav = GetCrystalNode() as CrystalStoneBehaviour;
                            if (behav)
                            {
                                behav.ChangeIceGemMaterial(IceGemMatType.Dark);
                            }
                        }
                    }
                }
            }
        }
    }

    private void SendCompleteAnimRequest()
    {
        IceGemSendData iceGemData = new IceGemSendData();
        iceGemData.playerId = Player.Id;
        iceGemData.mapId = GameManager.Inst.gameMapInfo.mapId;
        iceGemData.option = (int)IceGemReqOpt.Animation;

        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.IceGem,
            data = JsonConvert.SerializeObject(iceGemData),
        };
        LoggerUtils.Log("IceGem SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    public void OnReceiveServer(string msg)
    {
        LoggerUtils.Log("IceGem OnReceiveServer ==> " + msg);
        IceGemSendData rData = JsonConvert.DeserializeObject<IceGemSendData>(msg);
        if (GlobalFieldController.CurMapInfo != null && rData.subMapId == GlobalFieldController.CurMapInfo.mapId)
        {
            if (rData.status != (int)IceGemCollectState.None)
            {
                var curbehav = crystalsDic[rData.itemId];
                if (curbehav)
                {
                    curbehav.OnCollectSuccess();
                }
                //若不是重复收集, 更新收集数量(全局)
                if (collectList == null || collectList.FindIndex(x => x.subMapId.Equals(rData.subMapId) && x.itemId == rData.itemId) < 0)
                {
                    GlobalFieldController.CollectIceGem++;
                }
                if (rData.status == (int)IceGemCollectState.AllDone)
                {
                    GlobalFieldController.CollectIceGem = maxCount;
                }
            }
        }
    }
    #endregion

    #region UI表现
    public void OnRefreshCollectCount()
    {
        CoroutineManager.Inst.StartCoroutine(UpdateShowCount());
    }

    private IEnumerator UpdateShowCount()
    {
        yield return new WaitForSeconds(1);
        if (GlobalFieldController.CurGameMode == GameMode.Guest && GlobalFieldController.IsDowntownEnter && GlobalFieldController.CollectIceGem <= collectCount)
        {
            //游玩模式, 重复收集 -> 进度未更新, 直接返回主城
            //OnUIPanelUpdated(); //修改: 不做任何处理
        }
        else
        {
            //试玩模式 -> 更新进度 1/1 && 游玩模式 -> 进度 newCount/maxCount
            //PlayModePanel.Instance.CollectIceCrystal(OnUIPanelUpdated); //修改: 不返回主城, 若收集齐, 直接播放合成动画
            PlayModePanel.Instance.CollectIceCrystal(OnUICollectFinish);
            collectCount = GlobalFieldController.CollectIceGem;
        }
    }

    //修改: UI动效结束回调方法 --> 判断若收集齐, 直接播放合成动画
    private void OnUICollectFinish()
    {
        if (++uiCount == maxCount)
        {
            PlayModePanel.Instance.ShowCollectedComplete();
        }
    }

    //UI动效结束回调方法: 返回主城
    private void OnUIPanelUpdated()
    {
        CoroutineManager.Inst.StartCoroutine(ReturnBackToDowntown());
    }

    private IEnumerator ReturnBackToDowntown()
    {
        yield return new WaitForSeconds(1);
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            //回到编辑模式
            PlayModePanel.Instance.OnEditClick();
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest && GlobalFieldController.IsDowntownEnter)
        {
            //返回Downtown
            LoggerUtils.Log("CrystalStoneManager --> Return back to Downtown");
        }
    }

    public void InitFirstToastPanel()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && GlobalFieldController.IsDowntownEnter)
        {
            //0-显示 1-隐藏
            if (PlayerPrefs.GetInt(GREATSNOW_FIRST_TIP_KEY, 0) == 0)
            {
                UIControlManager.Inst.CallUIControl("snowfield_first_tip_enter");
            }
        }
    }
    #endregion
}
