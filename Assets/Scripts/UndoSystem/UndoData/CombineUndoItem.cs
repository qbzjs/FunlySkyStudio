using System.Collections.Generic;
/// <summary>
/// Author:JayWill
/// Description:Undo/Redo 组合与取消组合数据类
/// </summary>
public class CombineUndoItem
{
    public SceneEntity combineNode;//组合的节点
    public List<SceneEntity> combineChildren;//组合节点下的各个子节点
}