using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:Undo/Redo 组合与取消组合数据类
/// </summary>

public class CombineUndoData
{
    public int combineUndoMode;//0:combine 1:uncombine
    public List<CombineUndoItem> combineItemList;//组合前的点
    public CombineUndoItem combinedItem;//组合后的点

    public CombineUndoData InitMultiData(List<SceneEntity> entitys)
    {
        combineItemList = new List<CombineUndoItem>();
        for (var i = 0; i < entitys.Count; i++)
        {
            var combineEntity = entitys[i];
            var gComp = combineEntity.Get<GameObjectComponent>();
            CombineUndoItem combineItem = new CombineUndoItem();
            combineItem.combineNode = combineEntity;
            if((ResType)gComp.type == ResType.CommonCombine)
            {
                Transform combineTrans = combineEntity.Get<GameObjectComponent>().bindGo.transform;
                combineItem.combineChildren = new List<SceneEntity>();
                for (int j = 0; j < combineTrans.childCount; j++)
                {
                    var nodeBehav = combineTrans.GetChild(j).GetComponent<NodeBaseBehaviour>();
                    if (nodeBehav != null)
                    {
                        combineItem.combineChildren.Add(nodeBehav.entity);
                    }
                }
            }
            combineItemList.Add(combineItem);
        }
        return this;
    }

    public CombineUndoData InitCombinedData(SceneEntity entity)
    {
        if(entity == null)
        {
            LoggerUtils.LogError("CombineUndoData InitCombinedData entity is null");
            return this;
        }
        combinedItem = new CombineUndoItem();
        combinedItem.combineNode = entity;
        combinedItem.combineChildren = new List<SceneEntity>();

        Transform newTrans = entity.Get<GameObjectComponent>().bindGo.transform;
        for (int j = 0; j < newTrans.childCount; j++)
        {
            var nodeBehav = newTrans.GetChild(j).GetComponent<NodeBaseBehaviour>();
            if (nodeBehav != null)
            {
                combinedItem.combineChildren.Add(nodeBehav.entity);
            }
        }
        return this;
    }
}