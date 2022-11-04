using System.Collections.Generic;
using UnityEngine;
using System;

class ParsePropWithTipsManager : CInstance<ParsePropWithTipsManager>
{
    private bool isLoading = false;
    private GameObject tipsGameObj;
    public string curMapId;
    private BudTimer budTimer;
    private List<ParsePropWithTips> propList = new List<ParsePropWithTips>();
    public void AddPropParse(string rid, Action<UGCModelType> ugcTypeSetCall, NodeBaseBehaviour behav)
    {
        ParsePropWithTips parseWithTips = new ParsePropWithTips(rid,ugcTypeSetCall,behav);
        propList.Add(parseWithTips);
        parseWithTips.ParsePropAndBuildWithTips();
    }
    private void DestroyWithTip()
    {
        if(tipsGameObj != null)
        {
            TipPanel.ShowToast("Adding prop failed.");
            isLoading = false;
            EditModeController.restoreClick();
            UnityEngine.Object.Destroy(tipsGameObj);
        }
    }
    public void DestroyGameObj()
    {
        if(tipsGameObj != null)
        {
            isLoading = false;
            EditModeController.restoreClick();
            UnityEngine.Object.Destroy(tipsGameObj);
        }
    }
    public void InitTipGameObject()
    {
        StopTimer();
        if(tipsGameObj != null)
        {
            UnityEngine.Object.Destroy(tipsGameObj);
        }
        isLoading = true;
        curMapId = GlobalFieldController.CurMapInfo.mapId;
        EditModeController.HideGizmo?.Invoke();
        tipsGameObj = ResManager.Inst.LoadRes<GameObject>("Prefabs/Rp/rpPref");
        Camera camera = CameraUtils.Inst.GetMainCamera();
        Vector3 pos = EditModeController.curPos;
        if(pos == Vector3.zero)pos = CameraUtils.Inst.GetCreatePosition();
       
        tipsGameObj = UnityEngine.Object.Instantiate(tipsGameObj,pos,Quaternion.identity);
        tipsGameObj.transform.LookAt(camera.transform,new Vector3(0,1,0));
        tipsGameObj.transform.localScale = new Vector3(2.5f,2.5f,2.5f);
        budTimer = TimerManager.Inst.RunOnce("JsonCanot", 45, ()=>{
            DestroyWithTip();
        });
    }
    public bool GetIsLoading()
    {
        return isLoading;
    }
    public bool AllowIsLoading()
    {
        if (Inst.curMapId != GlobalFieldController.CurMapInfo.mapId ||
            GlobalFieldController.CurGameMode == GameMode.Play)
        {
            if(tipsGameObj != null)
            {
                UnityEngine.Object.Destroy(tipsGameObj);
            }
            isLoading = false;
            return false;
        }

        return true;
    }
    public void StopTimer()
    {
        if(budTimer != null)
        {
            TimerManager.Inst.Stop(budTimer);
            budTimer = null;
        }
    }
    public void Remove(ParsePropWithTips parseWithTips)
    {
        if(propList.Contains(parseWithTips))
            propList.Remove(parseWithTips);
    }
    public override void Release()
    {
        base.Release();
        foreach (var item in propList)
        {
            item.Clear();
        }
    }
}