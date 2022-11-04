
using BudEngine.NetEngine;
using UnityEngine;
/// <summary>
/// Author:Meimei-LiMei
/// Description:烟花特效行为:播放烟花
/// Date: 2022/7/31 14:45:36
/// </summary>
public class FireworkEffectBehaviour
{
    public bool isPlay = false;
    public NodeBaseBehaviour behv { get; set; }
    public FireworkComponent comp { get; set; }
    public Vector3 targetPos;
    public Transform parentPos;
    public Transform fireworkEffectTs;
    public FireworkEffectBehaviour(NodeBaseBehaviour nodeBaseBehaviour)
    {
        behv = nodeBaseBehaviour;
        comp = behv.entity.Get<FireworkComponent>();
        parentPos = behv.transform;
    }
    /// <summary>
    /// 播放烟花
    /// </summary>
    /// <param name="isSendReq">是否发送请求</param>
    public void PlayFirework()
    {
        if (isPlay == true)//上一个烟花还在播放
        {
            return;
        }
        isPlay = true;
        fireworkEffectTs = FireworkPool.Inst.GetFireWorkEffect().transform;
        var fireworkEffect = fireworkEffectTs.GetComponent<FireworkEffect>();
        fireworkEffect.InitFireworkEffect(comp, StopPlay, parentPos);
    }
    public void StopPlay()
    {
        isPlay = false;
    }
    public void OnTriggerFirework()
    {
        if (behv.entity.HasComponent<PickablityComponent>())
        {
            var comp = behv.entity.Get<PickablityComponent>();
            if (comp.isPicked)//被拾取不处理
            {
                return;
            }
        }
        if (isPlay == true)
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Play) // 试玩模式的表现（只展示特效）
        {
            PlayFirework();
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            //发送使用烟花道具请求
            var uid = behv.entity.Get<GameObjectComponent>().uid;
            FireworkManager.Inst.SendFireworkPlayReq(uid);
        }
    }
}
