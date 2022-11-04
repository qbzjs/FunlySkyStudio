using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:提供给各实体对象 Manager 继承使用的单例类
/// 继承了 ManagerInstance 的子类，必须要实现 IManager 接口
/// Date: Date: 2021/12/30 19:00:54
/// </summary>

public class ManagerInstance<T> : BaseInstance where T : BaseInstance, IManager, new()
{
    protected static T _instance;

    public static T Inst
    {
        get
        {
            if (_instance == null)
            {
                _instance = NodeBehaviourManager.Inst.CreateManagerInstance<T>();
            }
            return _instance;
        }
    }

    // 目前 IOS 关闭后无法释放静态内存，调用 Release 会删除单例对象，
    public override void Release()
    {
        base.Release();
        _instance = null;

    }
}

/// <summary>
/// Author:WenJia
/// Description:各实体对象 Manager 集中管理接口
/// Date: 2021/12/30 19:00:54
/// </summary>

public interface IManager
{
    // 移除指定的 NodeBaseBehaviour 关联的实体 (对应原来 Manager 的 OnRemoveNode 方法)
    void RemoveNode(NodeBaseBehaviour behaviour);
    //撤销删除，恢复Manager与NodeBaseBehaviour的关联
    void RevertNode(NodeBaseBehaviour behaviour);
    // 移除所有相关的 NodeBaseBehaviour 关联的实体 (对应原来 Manager 的 ClearBevs 方法)
    void Clear();
}