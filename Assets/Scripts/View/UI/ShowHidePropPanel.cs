using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHidePropPanel : BasePanel<ShowHidePropPanel>,IUndoRecord
{
    public Button btnShow;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnShow.onClick.AddListener(OnShowBtnClick);
    }

    private void OnShowBtnClick()
    {
        AddRecord();
        LockHideManager.Inst.ClearHideList();
    }
    public void AddRecord(UndoRecord record)
    {
       
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord()
    {
        LockHideUndoData beginData = new LockHideUndoData();
        beginData.hideList = new List<SceneEntity>();
        for (int i = 0; i < LockHideManager.Inst.hideList.Count; i++)
        {
            beginData.hideList.Add(LockHideManager.Inst.hideList[i]);
        }
        beginData.activeSelf = false;
        beginData.LockHideType = (int)LockHideType.Show;
        LockHideUndoData endData = new LockHideUndoData();
        endData.activeSelf = true;
        endData.LockHideType = (int)LockHideType.Show;
        UndoRecord record = new UndoRecord(UndoHelperName.LockHideUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }
}
