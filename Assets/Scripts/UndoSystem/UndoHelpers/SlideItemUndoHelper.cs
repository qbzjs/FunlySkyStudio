using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SlideItemUndoData
{
    public int matId;
    public int colorType;
    public Color color;
    public Vector2 tile;
    public SceneEntity targetEntity;
    public int baseMaterialType;
    public int tabId;
    public bool isExpand;
}
public class SlideItemUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        SlideItemUndoData helpData = record.BeginData as SlideItemUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        SlideItemUndoData helpData = record.EndData as SlideItemUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(SlideItemUndoData helpData)
    {
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.baseMaterialType == (int)MaterialSelectType.Color)
            {
                SlidePipePanel.Instance.SetColorUndo(helpData.colorType,helpData.color, helpData.tabId,helpData.isExpand);
            }
            else if (helpData.baseMaterialType == (int)MaterialSelectType.Material)
            {
                SlidePipePanel.Instance.SetMaterialUndo(helpData.matId,helpData.tabId,helpData.isExpand);
            }
            else if (helpData.baseMaterialType == (int)MaterialSelectType.Tile)
            {
                SlidePipePanel.Instance.TileClickUndo(helpData.tile);
            }
        }
    }
}
