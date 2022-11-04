using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BaseMaterialUndoData
{
    public int matId;
    public int colorId;
    public Vector2 tile;
    public SceneEntity targetEntity;
    public int baseMaterialType;
}
public class BaseMaterialUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        BaseMaterialUndoData helpData = record.BeginData as BaseMaterialUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        BaseMaterialUndoData helpData = record.EndData as BaseMaterialUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(BaseMaterialUndoData helpData)
    {
        LoggerUtils.Log("BaseMaterialUndoData colorId:" + helpData.colorId);
        LoggerUtils.Log("BaseMaterialUndoData matId:" + helpData.matId);
        LoggerUtils.Log("BaseMaterialUndoData tile:" + helpData.tile);


        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.baseMaterialType == (int)MaterialSelectType.Color)
            {
                BaseMaterialPanel.Instance.SetColorUndo(helpData.colorId);
            }
            else if (helpData.baseMaterialType == (int)MaterialSelectType.Material)
            {
                BaseMaterialPanel.Instance.SetMaterialUndo(helpData.matId);
            }
            else if (helpData.baseMaterialType == (int)MaterialSelectType.Tile)
            {
                BaseMaterialPanel.Instance.TileClickUndo(helpData.tile);
            }
        }
    }
}
