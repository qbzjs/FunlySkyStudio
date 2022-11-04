using UnityEngine;
/// <summary>
/// Author:JayWill
/// Description:Undo/Redo实现创建于删除
/// </summary>
public class CreateDestroyUndoData
{
    public GameObject targetNode;
    public int createUndoMode;
}
public class CreateDestroyUndoHelper:BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);
        CreateDestroyUndoData beginData = record.BeginData as CreateDestroyUndoData;
        CreateDestroyUndoData endData  = record.EndData as CreateDestroyUndoData;
        //删除操作
        if (IsDestroyMode(beginData))
        {
            ExcuteCreate(beginData);
        }
        else if(IsCreateMode(endData))//创建或复制操作
        {
            ExcuteDestroy(endData);
        }  
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        CreateDestroyUndoData beginData = record.BeginData as CreateDestroyUndoData;
        CreateDestroyUndoData endData = record.EndData as CreateDestroyUndoData;
        //删除操作
        if (IsDestroyMode(beginData))
        {
            ExcuteDestroy(beginData);
        }
        else if (IsCreateMode(endData))
        {
            ExcuteCreate(endData);
        }
    }

    //在redo中的创建操作，在undo的删除操作 被清除时需要真正销毁节点
    public override void OnRemoveFromPool(UndoRecord record,int fromType)
    {
        base.OnRemoveFromPool(record,fromType);
        LoggerUtils.Log("CreateDestroyUndoHelper OnRemoveFromPool fromType:"+fromType);
        CreateDestroyUndoData beginData = record.BeginData as CreateDestroyUndoData;
        CreateDestroyUndoData endData  = record.EndData as CreateDestroyUndoData;
        //删除操作
        if (IsDestroyMode(beginData))
        {
            if (fromType == (int)UndoRedoType.Undo && SecondCachePool.Inst.IsContains(beginData.targetNode))
            {
                LoggerUtils.Log("undo真正销毁节点:" + beginData.targetNode.transform.name);
                SecondCachePool.Inst.RemoveItem(beginData.targetNode);
                SceneBuilder.Inst.DestroyEntity(beginData.targetNode);
            }
        }
        else if (IsCreateMode(endData))
        {
            if (fromType == (int)UndoRedoType.Redo && SecondCachePool.Inst.IsContains(endData.targetNode))
            {
                SecondCachePool.Inst.RemoveItem(endData.targetNode);
                LoggerUtils.Log("redo真正销毁节点:" + endData.targetNode.transform.name);
                SceneBuilder.Inst.DestroyEntity(endData.targetNode);
            }
        }
    }

    private void ExcuteCreate(CreateDestroyUndoData helpData)
    {
        SecondCachePool.Inst.RevertItem(helpData.targetNode);
        GameObject gameObject = helpData.targetNode;
        var nodeBehaviour = gameObject.GetComponent<NodeBaseBehaviour>();
        if (nodeBehaviour != null && nodeBehaviour.entity != null && nodeBehaviour.entity.Id != -1)
        {   
            SecondCachePool.Inst.RevertEntity(gameObject);
            EditModeController.SetSelect?.Invoke(nodeBehaviour.entity);
        }else{
            LoggerUtils.Log("该节点已被回收，操作无效");
        }
    }

    private void ExcuteDestroy(CreateDestroyUndoData helpData)
    {
        GameObject gameObject = helpData.targetNode;
        SecondCachePool.Inst.DestroyEntity(gameObject);
        EditModeController.UnSelectAll?.Invoke();
    }

    private bool IsCreateMode(CreateDestroyUndoData helpData)
    {
        return ((helpData.createUndoMode == (int)CreateUndoMode.Create || helpData.createUndoMode == (int)CreateUndoMode.Duplicate) && helpData.targetNode != null);
    }

    private bool IsDestroyMode(CreateDestroyUndoData helpData)
    {
        return (helpData.createUndoMode == (int)CreateUndoMode.Destroy && helpData.targetNode != null);
    }
}