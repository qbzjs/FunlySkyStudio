using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SnowCubeUndoData
{
    public int colorId;
    public int colorType;
    public string colorStr;
    public Vector2 tile;
    public SceneEntity targetEntity;
    public int undoType;
    public int tabId;
    public bool isExpand;
}

public class SnowCubeUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        SnowCubeUndoData helpData = record.BeginData as SnowCubeUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        SnowCubeUndoData helpData = record.EndData as SnowCubeUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(SnowCubeUndoData helpData)
    {
        LoggerUtils.Log("SnowCubeUndoData colorId:" + helpData.colorId);
        LoggerUtils.Log("SnowCubeUndoData tile:" + helpData.tile);


        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.undoType == (int)SnowCubeType.Color)
            {
                SnowCubePanel.Instance.SetColorUndo(helpData.colorId, helpData.colorType, helpData.colorStr, helpData.tabId, helpData.isExpand);
            }
            else if (helpData.undoType == (int)SnowCubeType.Tile)
            {
                SnowCubePanel.Instance.TileClickUndo(helpData.tile);
            }
        }
    }
}
