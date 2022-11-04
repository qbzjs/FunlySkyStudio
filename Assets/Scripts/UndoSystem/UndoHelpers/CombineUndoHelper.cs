/// <summary>
/// Author:JayWill
/// Description:Undo/Redo实现组合与取消组合
/// </summary>
public class CombineUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);
        EditModeController.UnSelectAll?.Invoke();
        CombineUndoData beginData = record.BeginData as CombineUndoData;
        LoggerUtils.Log("CombineUndoHelper Undo");

        if (beginData.combineUndoMode == (int)CombineUndoMode.Combine)//原来是组合则开始解组
        {
            LoggerUtils.Log("CombineUndoHelper undo group");
            ExcuteUnCombine(record);
            TipPanel.ShowToast("undo group");
        }
        else
        {
            LoggerUtils.Log("CombineUndoHelper undo ungroup");
            ExcuteCombine(record);
            TipPanel.ShowToast("undo ungroup");
        }
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);

        EditModeController.UnSelectAll?.Invoke();

        CombineUndoData beginData = record.BeginData as CombineUndoData;
        if (beginData.combineUndoMode == (int)CombineUndoMode.Combine)//原来是组合则还原组合
        {
            ExcuteCombine(record);
            TipPanel.ShowToast("redo group");
        }
        else
        {
            ExcuteUnCombine(record);
            TipPanel.ShowToast("redo ungroup");
        }

    }

    private void ExcuteUnCombine(UndoRecord record)
    {
        CombineUndoData beginData = record.BeginData as CombineUndoData;
        CombineUndoData endData = record.EndData as CombineUndoData;

        CombineUndoData multiData = beginData;//组合前数据
        CombineUndoData combinedData = endData;//组合后数据

        //如果是拆解则数据相反
        if (beginData.combineUndoMode == (int)CombineUndoMode.UnCombine)
        {
            multiData = endData;
            combinedData = beginData;
        }

        SceneEntity combinedEntity = combinedData.combinedItem?.combineNode;
        var combineItemList = multiData.combineItemList;
        foreach (CombineUndoItem item in combineItemList)
        {
            var gComp = item.combineNode.Get<GameObjectComponent>();
            if ((ResType)gComp.type == ResType.CommonCombine)
            {
                var gameObject = SecondCachePool.Inst.GetGameObjectByUid(gComp.uid);
                if (gameObject != null)
                {
                    SecondCachePool.Inst.RevertEntity(gameObject);
                    var combineChildren = item.combineChildren;
                    if (combineChildren != null && combineChildren.Count > 0)
                    {
                        item.combineChildren.ForEach(child =>
                        {
                            var childGo = child.Get<GameObjectComponent>().bindGo;
                            childGo.transform.SetParent(gameObject.transform);
                            childGo.transform.localScale = DataUtils.LimitVector3(childGo.transform.localScale);
                        });
                    }
                }
                else
                {
                    //找不到原节点
                    LoggerUtils.LogError("CombineUndoHelper ExcuteUnCombine can not find the node in SecondCachePool!");
                }
            }
            else
            {
                if (combinedEntity != null)
                {
                    var combinedComp = combinedEntity.Get<GameObjectComponent>();
                    var combinedNode = combinedComp.bindGo;
                    var childNode = gComp.bindGo;
                    var parent = combinedNode.transform.parent;
                    if(parent == null)
                    {
                        parent = SceneBuilder.Inst.StageParent;
                    }
                    childNode.transform.SetParent(parent);
                    childNode.transform.localScale = DataUtils.LimitVector3(childNode.transform.localScale);
                }
            }
        }
        if (combinedEntity != null)
        {
            var combinedComp = combinedEntity.Get<GameObjectComponent>();
            SecondCachePool.Inst.DestroyEntity(combinedComp.bindGo);
        }
    }

    private void ExcuteCombine(UndoRecord record)
    {
        LoggerUtils.Log("CombineUndoHelper ExcuteCombine");
        CombineUndoData beginData = record.BeginData as CombineUndoData;
        CombineUndoData endData = record.EndData as CombineUndoData;

        CombineUndoData multiData = beginData;//组合前数据
        CombineUndoData combinedData = endData;//组合后数据

        //如果是拆解则数据相反
        if (beginData.combineUndoMode == (int)CombineUndoMode.UnCombine)
        {
            multiData = endData;
            combinedData = beginData;
        }

        SceneEntity combinedEntity = combinedData.combinedItem?.combineNode;
        if (combinedEntity == null)
        {
            //原组合SceneEntity为空
            LoggerUtils.Log("CombineUndoHelper Redo ExcuteCombine combinedEntity is null");
            return;
        }

        var combinedComp = combinedEntity.Get<GameObjectComponent>();
        var combinedGo = SecondCachePool.Inst.GetGameObjectByUid(combinedComp.uid);
        if (combinedGo == null)
        {
            //原组合combinedGo为空
            LoggerUtils.Log("CombineUndoHelper Redo ExcuteCombine Comnine combinedGo is null,combinedComp.uid:" + combinedComp.uid);
            return;
        }

        SecondCachePool.Inst.RevertEntity(combinedGo);
        var combineItemList = multiData.combineItemList;

        foreach (CombineUndoItem item in combineItemList)
        {
            var entity = item.combineNode;
            var gComp = entity.Get<GameObjectComponent>();

            if ((ResType)gComp.type == ResType.CommonCombine)
            {
                var gameObject = gComp.bindGo;
                var combineChildren = item.combineChildren;
                if (combineChildren != null && combineChildren.Count > 0)
                {
                    item.combineChildren.ForEach(child =>
                    {
                        var childGo = child.Get<GameObjectComponent>().bindGo;
                        childGo.transform.SetParent(combinedGo.transform);
                        childGo.transform.localScale = DataUtils.LimitVector3(childGo.transform.localScale);
                    });
                }
                SecondCachePool.Inst.DestroyEntity(gComp.bindGo);
            }
            else
            {
                gComp.bindGo.transform.SetParent(combinedGo.transform);
            }
        }
    }

    //在redo中的创建操作，在undo的删除操作 被清除时需要真正销毁节点
    public override void OnRemoveFromPool(UndoRecord record, int fromType)
    {
        base.OnRemoveFromPool(record, fromType);


        CombineUndoData beginData = record.BeginData as CombineUndoData;
        CombineUndoData endData = record.EndData as CombineUndoData;

        CombineUndoData multiData = beginData;//组合前数据
        CombineUndoData combinedData = endData;//组合后数据

        LoggerUtils.Log("CombineUndoHelper OnRemoveFromPool fromType:" + fromType + "  combineUndoMode:" + beginData.combineUndoMode);

        //如果是拆解则数据相反
        if (beginData.combineUndoMode == (int)CombineUndoMode.UnCombine)
        {
            multiData = endData;
            combinedData = beginData;
        }

        //是undo且是组合操作
        if ((fromType == (int)UndoRedoType.Undo && beginData.combineUndoMode == (int)CombineUndoMode.Combine) ||
             (fromType == (int)UndoRedoType.Redo && beginData.combineUndoMode == (int)CombineUndoMode.UnCombine))
        {
            var combineItemList = multiData.combineItemList;
            foreach (CombineUndoItem item in combineItemList)
            {
                TryDestroyEntity(item.combineNode);
            }
        }

        if ((fromType == (int)UndoRedoType.Undo && beginData.combineUndoMode == (int)CombineUndoMode.UnCombine) ||
            (fromType == (int)UndoRedoType.Redo && beginData.combineUndoMode == (int)CombineUndoMode.Combine))
        {
            SceneEntity combinedEntity = combinedData.combinedItem?.combineNode;
            if (combinedEntity != null)
            {
                TryDestroyEntity(combinedEntity);
            }
        }
    }

    private void TryDestroyEntity(SceneEntity entity)
    {
        if (entity == null) return;
        var gComp = entity.Get<GameObjectComponent>();
        if ((ResType)gComp.type == ResType.CommonCombine)
        {
            //找到要删除的组合节点
            LoggerUtils.Log("TryDestroyEntity:Destroy the CommonCombine Node");
            var gameObject = SecondCachePool.Inst.GetGameObjectByUid(gComp.uid);
            if (gameObject != null)
            {
                SecondCachePool.Inst.RemoveItem(gameObject);
                SceneBuilder.Inst.DestroyEntity(gameObject);
            }
        }
        else
        {
            //非组合点，不是要删除的组合节点
            LoggerUtils.Log("TryDestroyEntity:Do not Destroy this node is not CommonCombine!");
        }
    }
}