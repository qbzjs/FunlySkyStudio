using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:PGC植物专有属性面板撤销管理
/// Date: 2022/8/6 23:10:58
/// </summary>

public class PGCPlantUndoData
{
    public int plantID;
    public int colorIndex;
    public int colorType;
    public string colorStr;
    public SceneEntity targetEntity;
    public int undoType;
    public int tabId;
    public bool isExpand;
}
public class PGCPlantUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        PGCPlantUndoData helpData = record.BeginData as PGCPlantUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        PGCPlantUndoData helpData = record.EndData as PGCPlantUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(PGCPlantUndoData helpData)
    {
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.undoType == (int)PGCSelectType.Color)
            {
                PGCPlantPanel.Instance.SetPGCPlantColorUndo(helpData.colorIndex,helpData.colorType,helpData.colorStr,helpData.tabId,helpData.isExpand);
            }
            if (helpData.undoType == (int)PGCSelectType.Type)
            {
                PGCPlantPanel.Instance.SetPGCPlantUndo(helpData.plantID,helpData.colorType,helpData.tabId,helpData.isExpand,helpData.colorIndex,helpData.colorStr);
            }
        }
    }
}
