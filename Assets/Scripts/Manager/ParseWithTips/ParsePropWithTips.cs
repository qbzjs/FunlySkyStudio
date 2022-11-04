using UnityEngine;
using System;

class ParsePropWithTips
{
    private string rid;
    // private GameObject go
    private BudTimer budTimer;
    private bool isTimeOut;
    private Action<UGCModelType> ugcTypeSetCall;
    private NodeBaseBehaviour nBehav;
    private UGCModelType lodType;
    public bool isLoading;

    public ParsePropWithTips(string rid,Action<UGCModelType> ugcTypeSetCall,NodeBaseBehaviour nobe,UGCModelType lodType = UGCModelType.High)
    {
        this.rid = rid;
        this.ugcTypeSetCall = ugcTypeSetCall;
        this.lodType = lodType;
        this.nBehav = nobe;
    }
    public void ParsePropAndBuildWithTips()
    {
        // InitTipGameObject();
        isTimeOut = false;
        
        isLoading = true;

        //
        OfflineRenderData renderData = null;
        if(rid != null)GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out renderData);
        if(renderData != null)
        {
            OfflineResManager.Inst.LoadFileAsync(renderData, lodType, TMPLoadCallBack);
            if(!renderData.IsCache())
                budTimer = TimerManager.Inst.RunOnce("TmpTimeOut", 10, ()=>{
                    TMPLoadCallBack(false);
                });

        }
        else
        {
            TMPLoadCallBack(false);
        }
    }
    void TMPLoadCallBack(bool isSuccess)
    {
        if (isTimeOut)
        {
            if(budTimer != null)
            {
                TimerManager.Inst.Stop(budTimer);
                budTimer = null;
            }
            return;
        }
        isLoading = false;
        ClearSelf();
        isTimeOut = true;
        ugcTypeSetCall?.Invoke(lodType);
        
        if(nBehav != null)
        {
            var tmpGo = nBehav.transform.Find("tmpGo");
            if(tmpGo != null)
                UnityEngine.Object.DestroyImmediate(tmpGo.gameObject);
        }
    }
    public void ClearSelf()
    {
        Clear();
        ParsePropWithTipsManager.Inst.Remove(this);
    }
    public void Clear()
    {
        ParsePropWithTipsManager.Inst.StopTimer();
        if(GlobalFieldController.CurGameMode == GameMode.Edit) //在编辑模式下销毁占位预制
            ParsePropWithTipsManager.Inst.DestroyGameObj();
        if(budTimer != null)
        {
            TimerManager.Inst.Stop(budTimer);
            budTimer = null;
        }
    }
}