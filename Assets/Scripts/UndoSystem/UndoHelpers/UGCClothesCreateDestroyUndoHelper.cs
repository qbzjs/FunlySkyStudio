using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UGCClothesCreateDestroyUndoData
{
    public GameObject targetNode;
    public int createUndoMode;
    public int type;
    public GameObject selectPart;
}
public class UGCClothesCreateDestroyUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);
        UGCClothesCreateDestroyUndoData beginData = record.BeginData as UGCClothesCreateDestroyUndoData;
        UGCClothesCreateDestroyUndoData endData = record.EndData as UGCClothesCreateDestroyUndoData;
        //删除操作
        if (IsDestroyMode(beginData))
        {
            ExcuteCreate(beginData);
        }
        else if (IsCreateMode(endData))//创建或复制操作
        {
            ExcuteDestroy(endData);
        }
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        UGCClothesCreateDestroyUndoData beginData = record.BeginData as UGCClothesCreateDestroyUndoData;
        UGCClothesCreateDestroyUndoData endData = record.EndData as UGCClothesCreateDestroyUndoData;
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
    public override void OnRemoveFromPool(UndoRecord record, int fromType)
    {
        base.OnRemoveFromPool(record, fromType);
        LoggerUtils.Log("CreateDestroyUndoHelper OnRemoveFromPool fromType:" + fromType);
        UGCClothesCreateDestroyUndoData beginData = record.BeginData as UGCClothesCreateDestroyUndoData;
        UGCClothesCreateDestroyUndoData endData = record.EndData as UGCClothesCreateDestroyUndoData;
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

    private void ExcuteCreate(UGCClothesCreateDestroyUndoData helpData)
    {
        if (helpData.selectPart != MainUGCResPanel.Inst.curSelectPart)
        {
            MainUGCResPanel.Inst.ChangeParts(helpData.selectPart);
        }
        SecondCachePool.Inst.RevertItem(helpData.targetNode);
        GameObject gameObject = helpData.targetNode;
        if (gameObject)
        {
            SecondCachePool.Inst.RevertEntity(gameObject);
            if(TransformInteractorController.Inst.interActor)
            {
                var behav = gameObject.GetComponent<ElementBaseBehaviour>();
                if (behav)
                {
                    behav.RedoInfo();
                    TransformInteractorController.Inst.interActor.Settup(behav.rectTrans, behav.OnTransformChange, behav.Init);
                }
            }
            ExcuteMirObj(gameObject, true);
        }
        else
        {
            LoggerUtils.Log("该节点已被回收，操作无效");
        }
    }

    private void ExcuteDestroy(UGCClothesCreateDestroyUndoData helpData)
    {
        if (helpData.selectPart != MainUGCResPanel.Inst.curSelectPart)
        {
            MainUGCResPanel.Inst.ChangeParts(helpData.selectPart);
        }
        GameObject gameObject = helpData.targetNode;
        ExcuteMirObj(gameObject, false);
        SecondCachePool.Inst.DestroyEntity(gameObject);
        if (TransformInteractorController.Inst.interActor)
        {
            var behav = gameObject.GetComponent<ElementBaseBehaviour>();
            if (behav)
            {
                behav.UndoInfo();
                TransformInteractorController.Inst.interActor.ResetInfo();
            }
        }
    }

    private void ExcuteMirObj(GameObject obj,bool isRevert)
    {
        var tbehav = obj.GetComponent<ElementBaseBehaviour>();
        GameObject mirObj = tbehav.ExcuteMirObj();
        if (mirObj)
        {
            if (isRevert)
            {
                SecondCachePool.Inst.RevertEntity(mirObj);
                tbehav.OnTransformChange();
            }
            else
            {
                SecondCachePool.Inst.DestroyEntity(mirObj);
            }
        }
    }

    private bool IsCreateMode(UGCClothesCreateDestroyUndoData helpData)
    {
        return (helpData.createUndoMode == (int)CreateUndoMode.Create && helpData.targetNode != null);
    }

    private bool IsDestroyMode(UGCClothesCreateDestroyUndoData helpData)
    {
        return (helpData.createUndoMode == (int)CreateUndoMode.Destroy && helpData.targetNode != null);
    }
}
