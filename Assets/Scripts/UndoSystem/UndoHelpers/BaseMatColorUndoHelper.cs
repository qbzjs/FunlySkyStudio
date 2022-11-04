using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BaseMatColorUndoData
{
    public int matId;
    public int colorType;
    public Color color;
    public Vector2 tile;
    public SceneEntity targetEntity;
    public int baseMaterialType;
    public int tabId;
    public bool isExpand;
    public string ugcMatUrl;
    public string ugcMatMapId;
}
public class BaseMatColorUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        BaseMatColorUndoData helpData = record.BeginData as BaseMatColorUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        BaseMatColorUndoData helpData = record.EndData as BaseMatColorUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(BaseMatColorUndoData helpData)
    {
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.baseMaterialType == (int)MaterialSelectType.Color)
            {
                BaseMatColorPanel.Instance.SetColorUndo(helpData.colorType,helpData.color, helpData.tabId,helpData.isExpand);
            }
            else if (helpData.baseMaterialType == (int)MaterialSelectType.Material)
            {
                BaseMatColorPanel.Instance.SetMaterialUndo(helpData.matId,helpData.ugcMatMapId,helpData.ugcMatUrl,helpData.tabId,helpData.isExpand);
            }
            else if (helpData.baseMaterialType == (int)MaterialSelectType.Tile)
            {
                BaseMatColorPanel.Instance.TileClickUndo(helpData.tile);
            }
        }
    }
}
