using UnityEngine;

public interface IPlayerAttackInterface
{
    void Attack();
    void UnderAttack(int sendPlayer, int dir, int attackPart = (int)HitPart.HEAD);
}

/// <summary>
/// Author:Shaocheng
/// Description:玩家攻击、穿戴武器控制
/// Date: 2022-4-14 17:44:22
/// </summary>
public abstract class PlayerAttackBase : IPlayerAttackInterface
{
    public enum PlayerType
    {
        SelfPlayer = 1,
        OtherPlayer = 2,
    }

    public enum AttrackDirection
    {
        Forward = 1,
        Back = 2
    }

    public struct AttackData
    {
        public Vec3 AttackDir;
        public AttrackDirection AnimDir;
        public int AttackPart;
    }

    public GameObject CurPlayer;

    //public WeaponBase HoldWeapon;
    public PlayerAttackBase(GameObject player)
    {
        CurPlayer = player;
        //HoldWeapon = weapon;
    }

    public abstract void Attack();

    public abstract void UnderAttack(int sendPlayer, int dir, int attackPart = (int)HitPart.HEAD);
}
