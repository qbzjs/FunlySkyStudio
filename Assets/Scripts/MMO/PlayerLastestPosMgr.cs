using System.Collections;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 进入地图后，拉取僵尸玩家之前的位置，防止玩家堆积在出生点
/// </summary>
public class PlayerLatestPosMgr : CInstance<PlayerLatestPosMgr>
{
    private static string PlayerLatestPosData;

    public void Init()
    {
        // MessageHelper.AddListener(MessageName.PlayerCreate, RefreshPlayerLatestPos);
    }

    public override void Release()
    {
        //base.Release();
        // MessageHelper.RemoveListener(MessageName.PlayerCreate, RefreshPlayerLatestPos);
    }

    private void RefreshPlayerLatestPos()
    {
        if (string.IsNullOrEmpty(PlayerLatestPosData)) return;
        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(PlayerLatestPosData);
        if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null) return;
        if (!getItemsRsp.mapId.Equals(GlobalFieldController.CurMapInfo.mapId)) return;
        if (getItemsRsp.playerLatestFrames == null) return;

        CoroutineManager.Inst.StartCoroutine(GoOnFrame(getItemsRsp));
    }

    private IEnumerator GoOnFrame(GetItemsRsp getItemsRsp)
    {
        var playerLatestFrames = getItemsRsp.playerLatestFrames;
        var curMapId = getItemsRsp.mapId;
        //等当前渲染帧执行完，保证OtherPlayerCtr的Start已被执行
        yield return new WaitForEndOfFrame();

        var otherPlayerDataDic = ClientManager.Inst.otherPlayerDataDic;
        foreach (var frame in playerLatestFrames)
        {
            if (frame == null)
            {
                continue;
            }
            UgcFrameData ugcFrameData = ClientManager.Inst.handleFrameData(frame.data);
            if (ugcFrameData == null) continue;
            if (Player.Id == frame.player_id) continue;

            //若此时GetBatchUserInfo未回包，人物形象未创建，则不处理
            if (!otherPlayerDataDic.ContainsKey(frame.player_id) || otherPlayerDataDic[frame.player_id] == null) continue;

            // 玩家进房后，若僵尸玩家在传送门的另一张图，隐藏僵尸玩家模型
            if (!ugcFrameData.mapId.Equals(curMapId))
            {
                LoggerUtils.Log($"PlayerLastestPosMgr--->{ugcFrameData.mapId} -- {curMapId}");
                otherPlayerDataDic[frame.player_id].gameObject.SetActive(false);
                continue;
            }

            //玩家模型数据有问题时，隐藏模型
            UserInfo syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(frame.player_id);
            if (syncPlayerInfo == null || string.IsNullOrEmpty(syncPlayerInfo.imageJson))
            {
                otherPlayerDataDic[frame.player_id].gameObject.SetActive(false);
                continue;
            }

            //玩家进房后，僵尸玩家在传送门的另一张图，新玩家使用传送门调用了GetItem, 此时应显示僵尸玩家模型
            if (otherPlayerDataDic.ContainsKey(frame.player_id))
            {
                otherPlayerDataDic[frame.player_id].OnFrame(ugcFrameData);
                otherPlayerDataDic[frame.player_id].gameObject.SetActive(true);

                LoggerUtils.Log($"PlayerLastestPosMgr--->{frame.player_id} -- {ugcFrameData.playerPos}");
            }
            else
            {
                LoggerUtils.Log($"PlayerLastestPosMgr not contains--->{frame.player_id}");
            }
        }
    }

    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========PlayerLastestPosMgr===>OnGetItems:" + dataJson);
        PlayerLatestPosData = dataJson;

        RefreshPlayerLatestPos();
    }
}