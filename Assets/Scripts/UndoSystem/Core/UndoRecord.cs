using System;
/// <summary>
/// Author:JayWill
/// Description:Undo/Redo记录类
/// </summary>
public class UndoRecord
{
    /// <summary> 对应UndoHelper 的类名</summary>
    public string Key;
    /// <summary> 操作前的数据</summary>
    public object BeginData;
    /// <summary> 操作后的数据</summary>
    public object EndData;

    public UndoRecord(string helperKey)
    {
        Key = helperKey;
    }

}