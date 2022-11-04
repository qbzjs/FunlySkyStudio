/// <summary>
/// Author:Mingo-LiZongMing
/// Description:FPS模式下，模型身上的控制器，主要控制动画
/// </summary>
using UnityEngine;

public class FpsShootPlayerControl : MonoBehaviour
{
    private Animator fpsPlayerAnim;
    private PlayerMeleeShoot curShootPlayer;
    private PlayerBaseControl playerBase;

    public void InitData(Animator fpsPlayerAnim, PlayerMeleeShoot curShootPlayer)
    {
        this.fpsPlayerAnim = fpsPlayerAnim;
        this.curShootPlayer = curShootPlayer;
        playerBase = PlayerControlManager.Inst.playerBase;
    }

    private  void PlayAnimation(AnimId animId, bool state)
    {
        if (PlayerControlManager.Inst.AnimNameDict.ContainsKey((int)animId))
        {
            string animName = PlayerControlManager.Inst.AnimNameDict[(int)animId];
            if(fpsPlayerAnim != null)
            {
                fpsPlayerAnim.SetBool(animName, state);
            }
        }
    }

    /// <summary>
    /// 实时检测，更新复制出来的模型的动画
    /// </summary>
    private void Update()
    {
        if (curShootPlayer == null || curShootPlayer.HoldWeapon == null)
        {
            return;
        }
        if (fpsPlayerAnim == null)
        {
            return;
        }
        PlayAnimation(AnimId.IsGround, playerBase.isGround);
        PlayAnimation(AnimId.IsMoving, playerBase.isMoving);
        PlayAnimation(AnimId.IsFastRun, playerBase.isFastRun);
        PlayAnimation(AnimId.IsJump, !playerBase.isGround);
    }
}
