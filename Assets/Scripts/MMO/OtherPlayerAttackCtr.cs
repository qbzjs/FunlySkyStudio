using UnityEngine;
/// <summary>
/// Author:Mingo-LiZongMing
/// Description:其他玩家攻击表现控制
/// Date: 2022-4-14 17:44:22
/// </summary>
public class OtherPlayerAttackCtr : MonoBehaviour
{
    [HideInInspector]
    public PlayerMeleeAttack curAttackPlayer;
    [HideInInspector]
    public AnimationController animCon;

    private void Awake()
    {
        curAttackPlayer = new PlayerMeleeAttack(this.gameObject);
        animCon = this.GetComponentInChildren<AnimationController>();
        curAttackPlayer.animCon = animCon;
        curAttackPlayer.playerAnimator = this.GetComponentInChildren<Animator>();
        curAttackPlayer.PlayerId = GetComponent<PlayerData>().syncPlayerInfo.uid;
        curAttackPlayer.playerType = PlayerMeleeAttack.PlayerType.OtherPlayer;
    }

    public void OnWearWeapon(MeleeAttackWeapon weapon, int weaponUid)
    {
        curAttackPlayer.WearWeapon(weapon, weaponUid);
    }

    public void OnDropWeapon()
    {
        if (curAttackPlayer != null)
        {
            curAttackPlayer.DropWeapon();
        }
    }
}
