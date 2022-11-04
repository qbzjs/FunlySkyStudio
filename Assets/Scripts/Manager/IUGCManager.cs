/// <summary>
/// Author:YangJie
/// Description: UGC 接口类, 若需要监听UGC状态变化, 则需要实现该接口， 并且在UGCBehaviorManager的 Init中添加 实现类
/// Date: 2022/8/3 20:00:00
/// </summary>
public interface IUGCManager
{
    void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour);
}
