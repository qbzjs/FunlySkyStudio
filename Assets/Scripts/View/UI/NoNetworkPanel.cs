using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoNetworkPanel : BasePanel<NoNetworkPanel>
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

    private void OnLeftBtnClick()
    {
        // DataLogUtils.LogTotalPlayTime(2);
        // MobileInterface.Instance.LogEventByEventName(LogEventData.unity_timeout_kickout);
        // todo:埋点数据待定
        MobileInterface.Instance.Quit();
    }
}
