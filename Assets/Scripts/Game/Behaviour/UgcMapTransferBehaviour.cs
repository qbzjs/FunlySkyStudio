using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UgcMapTransferBehaviour : BaseTransferBehaviour
{
    public override void StartTransfer()
    {
        base.StartTransfer();
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            DowntownLoadingPanel.Show();
            TransferToDowntown();
        }
        else
        {
            TipPanel.ShowToast("Only In Guest Mode!");
        }
    }

    private void TransferToDowntown()
    {
        //将地图ID还原为Downtown地图的MapId
        GlobalFieldController.curMapMode = MapMode.Downtown;
        GameManager.Inst.curDiyMapId = GameManager.Inst.gameMapInfo.mapId;
        PlayerManager.Inst.ExitShowPlayerState();
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.BgBehaviour.StopEnr();
        WeatherManager.Inst.PauseShowWeather();
        //背包模式传送时不执行拾取的逻辑
        if (SceneParser.Inst.GetBaggageSet() != 1)
        {
            PickabilityManager.Inst.OnDealPortal();
        }
        else
        {
            PickabilityManager.Inst.OnBaggageDealPortal();
        }
        SceneSystem.Inst.StopSystem();
        SceneBuilder.Inst.DestroyScene();
        BloodPropManager.Inst.ClearBloodPropDict();
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        PortalPlayPanel.Hide();
        SceneBuilder.Inst.DowntownParseAndBuild(GameManager.Inst.downtownJson);
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.BgBehaviour.Play(true);
        SceneBuilder.Inst.BgBehaviour.PlayEnr();
        WeatherManager.Inst.ShowCurrentWeather();
        FollowModeManager.Inst.OnChangeMode(GameMode.Guest);
        BaggageManager.Inst.InitBaggageVisiable();
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SceneSystem.Inst.StartSystem();
        PlayerManager.Inst.StartShowPlayerState();
        MessageHelper.Broadcast(MessageName.ChangeMode, GlobalFieldController.CurGameMode);
        ClientManager.Inst.SendGetItems();
        PlayModePanel.Instance.EntryPortalMode(true);
        PlayModePanel.Instance.SetFlyButtonVisibleByPortal();
        SetPlayerPosAndRot();
        DowntownLoadingPanel.Hide();
        PlayTransferEffect(TransAnimType.End);
        TimerManager.Inst.RunOnce("TransferAnim", 2f, () => {
            PlayerBaseControl.Inst.PlayerResetIdle();
            StopTransferEffect();
        });
        MessageHelper.Broadcast(MessageName.OnEnterSnowfield);
    }
}
