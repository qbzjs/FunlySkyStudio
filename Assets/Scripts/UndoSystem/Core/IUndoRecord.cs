/// <summary>
/// Author:JayWill
/// Description:Undo/Redo 系统接口，IUndoRecord
/// </summary>
public interface IUndoRecord
{
    public void AddRecord(UndoRecord record);

}
