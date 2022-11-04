using UnityEngine;

public class SeesawUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);
        if (record.BeginData is SeesawSetMatUndoData matData)
        {
            SeesawPanel.Instance.SetMatUndo(matData);
        }else if (record.BeginData is SeesawSetColorUndoData colorData)
        {
            SeesawPanel.Instance.SetColorUndo(colorData);
        }else if (record.BeginData is SeesawChangeTilingUndoData tilingData)
        {
            SeesawPanel.Instance.SetTileUndo(tilingData);
        }
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        if (record.EndData is SeesawSetMatUndoData matData)
        {
            SeesawPanel.Instance.SetMatUndo(matData);
        }else if (record.EndData is SeesawSetColorUndoData colorData)
        {
            SeesawPanel.Instance.SetColorUndo(colorData);
        }else if (record.EndData is SeesawChangeTilingUndoData tilingData)
        {
            SeesawPanel.Instance.SetTileUndo(tilingData);
        }
    }
}

public class SeesawSetMatUndoData
{
    public int mat;
    public int tabId;
    public bool isExpand;
}

public class SeesawSetColorUndoData
{
    public string color;
    public int tabId;
    public bool isExpand;
    public int colorType;
}

public class SeesawChangeTilingUndoData
{
    public Vector2 tiling;
}