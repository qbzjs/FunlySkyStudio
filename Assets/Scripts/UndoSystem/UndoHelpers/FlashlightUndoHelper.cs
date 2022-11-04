using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightUndoData
{
    public SceneEntity targetEntity;
    public List<Color> colors;
    public int select;
}

public class FlashlightUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);
        FlashlightUndoData data = record.BeginData as FlashlightUndoData;
        ExecuteData(data);
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        FlashlightUndoData data = record.EndData as FlashlightUndoData;
        ExecuteData(data);
    }

    private void ExecuteData(FlashlightUndoData helpData)
    {
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            FlashLightPanel.Instance.SetColorUndo(helpData.colors, helpData.select);
            FlashLightPanel.Instance.SelectUndoTab();
        }
    }
}
