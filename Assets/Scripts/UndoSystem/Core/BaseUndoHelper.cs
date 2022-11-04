/// <summary>
/// Author:JayWill
/// Description:UndoHelper基础类，所有UndoHelper需要继承本类，并至少实现Undo和Redo 方法
/// </summary>
public class BaseUndoHelper
{
    public virtual void Undo(UndoRecord record)
    {

    }

    public virtual void Redo(UndoRecord record)
    {
        
    }

    //超过undo最大限制被清除或redo时添加record导致redo栈被清除
    public virtual void OnRemoveFromPool(UndoRecord record,int fromType)
    {

    }
}
