using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:PGC特效 Undo/Redo 管理
/// Date: 2022/10/25 16:10:58
/// </summary>

public class PGCEffectUndoData
{
    public int effectID;
    public int colorIndex;
    public int colorType;
    public string colorStr;
    public SceneEntity targetEntity;
    public int undoType;
    public int tabId;
    public bool isExpand;
}
public class PGCEffectUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        PGCEffectUndoData helpData = record.BeginData as PGCEffectUndoData;
        ExcuteData(helpData);
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        PGCEffectUndoData helpData = record.EndData as PGCEffectUndoData;
        ExcuteData(helpData);
    }
    
    private void ExcuteData(PGCEffectUndoData helpData)
    {
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.undoType == (int)PGCSelectType.Color)
            {
                PGCEffectPanel.Instance.SetPGCEffectColorUndo(helpData.colorIndex, helpData.colorType, helpData.colorStr, helpData.tabId, helpData.isExpand);
            }
            if (helpData.undoType == (int)PGCSelectType.Type)
            {
                PGCEffectPanel.Instance.SetPGCEffectUndo(helpData.effectID, helpData.colorType, helpData.tabId, helpData.isExpand, helpData.colorIndex, helpData.colorStr);
            }
        }
    }
}
