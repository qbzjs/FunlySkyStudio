using System.Collections.Generic;
/// <summary>
/// Author:JayWill
/// Description:Undo/Redo数据池，管理undo数据队列 以及 redo数据栈
/// </summary>
public class UndoRecordPool: CInstance<UndoRecordPool>
{
    // private Deque
    private List<UndoRecord> _undoDeque = new List<UndoRecord>();
    private Stack<UndoRecord> _redoStack = new Stack<UndoRecord>();

    private int _maxCount = 15;

    private void ClearDeque()
    {
        _undoDeque.Clear();
    }

    public void ClearPool()
    {
        _undoDeque?.Clear();
        _redoStack?.Clear();
    }

    private void AddTail(UndoRecord record)
    {
        _undoDeque.Add(record);
    }

    private UndoRecord RemoveTail()
    {
        if(_undoDeque.Count == 0){
            return null;
        }
        UndoRecord record = _undoDeque[_undoDeque.Count-1];
        _undoDeque.RemoveAt(_undoDeque.Count-1);
        return record;
    }

    private UndoRecord GetTail()
    {
        if(_undoDeque.Count == 0){
            return null;
        }
        UndoRecord record = _undoDeque[_undoDeque.Count-1];
        return record;
    }

    private void AddHead(UndoRecord record)
    {
        _undoDeque.Insert(0,record);
    }

    private UndoRecord RemoveHead()
    {
        if(_undoDeque.Count == 0){
            return null;
        }
        UndoRecord record = _undoDeque[0];
        _undoDeque.RemoveAt(0);
        return record;
    }

    private UndoRecord GetHead()
    {
        if(_undoDeque.Count == 0){
            return null;
        }
        UndoRecord record = _undoDeque[0];
        return record;
    }

    private void OnRemoveRecord(List<UndoRecord> records,int opType)
    {
        if(records == null || records.Count == 0) return;
        UndoRedoManager.Inst.OnRecordsRemoveFromPool(records,opType);
    }

    private void OnRemoveRecord(UndoRecord record,int opType)
    {
        if(record == null) return;
        List<UndoRecord> list = new List<UndoRecord>();
        list.Add(record);
        OnRemoveRecord(list,opType);
    }

    public void PushRecord(UndoRecord record)
    {
        if(_redoStack.Count > 0){
            List<UndoRecord> list = new List<UndoRecord>();
            foreach(var redoRecord in _redoStack){
                list.Add(redoRecord);
            }
            OnRemoveRecord(list,(int)UndoRedoType.Redo);
        }
        _redoStack.Clear();
        PushUndo(record);
    }

    public void PushUndo(UndoRecord record)
    {
        if(_undoDeque.Count == _maxCount){
            var removeRecord = RemoveHead();
            OnRemoveRecord(removeRecord,(int)UndoRedoType.Undo);
        }
        AddTail(record);
        MessageHelper.Broadcast(MessageName.UpdateUndoView);
    }

    public UndoRecord PopUndo()
    {
        UndoRecord record = RemoveTail();
        return record;
    }

    public void PushRedo(UndoRecord record)
    {
        _redoStack.Push(record);
        MessageHelper.Broadcast(MessageName.UpdateUndoView);
    }

    public UndoRecord PopRedo()
    {
        return _redoStack.Pop();
    }

    public int GetUndoCount()
    {
        return _undoDeque.Count;
    }

    public int GetRedoCount()
    {
        return _redoStack.Count;
    }

    public override void Release()
    {
        base.Release();
        ClearPool();
    }

}