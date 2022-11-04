using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BudEngine.NetEngine;

public class PortalGateManager : ManagerInstance<PortalGateManager>, IManager
{

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("PortalGateManager OnReceiveServer ==>" + " " + msg);
        PortalDataRsp protalRsp = JsonConvert.DeserializeObject<PortalDataRsp>(msg);
        if (int.Parse(protalRsp.code) != 1)
        {
            return false;
        }
        var playerid = protalRsp.playerId;

        if (protalRsp.targetMapId == GlobalFieldController.CurMapInfo.mapId && playerid != null)
        {
            var sbehav = SpawnPointManager.Inst.GetSpawnPointBehavByGameMode(protalRsp.spawnId);
            PVPManager.Inst.UpdatePlayerHpShow(playerid, sbehav.hpValue);
        }

        if (playerid != null && playerid != GameManager.Inst.ugcUserInfo.uid)
        {
            PickabilityManager.Inst.ClearPlayerBag(playerid);
        }
        GameManager.Inst.PlayerSpawnId = protalRsp.spawnId;
        return true;
    }

    public void Clear()
    {
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }

}
