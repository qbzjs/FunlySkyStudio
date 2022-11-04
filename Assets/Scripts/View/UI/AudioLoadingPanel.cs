using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoadingPanel : BasePanel<AudioLoadingPanel>
{
    public static bool isBgmDownloading;
    protected RectTransform titleTrans;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        titleTrans = transform.Find("Panel/Title") as RectTransform;
    }

    public void SetLoadingPos()
    {
        LoggerUtils.Log(" PlayModePanel.Instance.IsShowCollectTip() = " + PlayModePanel.Instance.IsShowCollectTip());
        titleTrans.anchoredPosition = PlayModePanel.Instance.IsShowCollectTip() ? new Vector3(960, -120) : new Vector3(630, -120, 0);
    }
}
