using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceExitPanel : BasePanel<ForceExitPanel>
{
    public Button leftBtn;
    public Text contentText;


    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
    }


    void Start()
    {
        leftBtn.onClick.AddListener(() => {
            OnLeftBtnClick();
        });
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        PlayerNetworkManager.Inst.isForceExitPanelShow = false;
    }

    private void OnLeftBtnClick()
    {
        DataLogUtils.LogUnityPingTimeSend();
        DataLogUtils.LogTotalPlayTime(2);
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_timeout_kickout);
        MobileInterface.Instance.Quit();
    }
}
