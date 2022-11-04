/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using UnityEngine;

public class OtherPlayerShootCtr : MonoBehaviour
{
    [HideInInspector]
    public PlayerMeleeShoot curShootPlayer;
    [HideInInspector]
    public AnimationController animCon;

    private Vector3 shootPickNodeEuler = new Vector3(5, -90, -92);
    private Vector3 nodePickNodeEuler = new Vector3(22.569f, -13.909f, -128.058f);

    private void Awake()
    {
        curShootPlayer = new PlayerMeleeShoot(this.gameObject);
        animCon = this.GetComponentInChildren<AnimationController>();
        curShootPlayer.animCon = animCon;
        curShootPlayer.playerAnimator = this.GetComponentInChildren<Animator>();
        curShootPlayer.PlayerId = GetComponent<PlayerData>().syncPlayerInfo.uid;
        curShootPlayer.playerType = PlayerMeleeAttack.PlayerType.OtherPlayer;
    }

    public void OnWearWeapon(MeleeShootWeapon weapon, int weaponUid)
    {
        if(curShootPlayer != null)
        {
            curShootPlayer.WearWeapon(weapon, weaponUid);
            SetPickNodeRot(true);
        }
    }

    public void OnDropWeapon()
    {
        if (curShootPlayer != null)
        {
            curShootPlayer.DropWeapon();
            ShootWeaponFireManager.Inst.RemovePlayerInShootingList(curShootPlayer);
            SetPickNodeRot(false);
        }
    }

    private void SetPickNodeRot(bool isPick)
    {
        var roleComp = animCon.GetComponentInChildren<RoleController>();
        if (roleComp)
        {
            var pickNode = roleComp.GetBandNode((int)BodyNode.PickNode);
            if (pickNode != null)
            {
                pickNode.localEulerAngles = isPick ? shootPickNodeEuler : nodePickNodeEuler;
            }
        }
    }
}
