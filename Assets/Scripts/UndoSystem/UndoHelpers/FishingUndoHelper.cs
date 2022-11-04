public class FishUndoData{
    public int uid;
    public bool isRod;
}

public class FishingUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        if (FishingRodPanel.Instance != null)
            FishingRodPanel.Instance.RedoUndo(record, true);
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);

        if (FishingRodPanel.Instance != null)
            FishingRodPanel.Instance.RedoUndo(record, false);
    }
}



