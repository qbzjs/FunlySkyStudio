/// <summary>
/// Author:Mingo-LiZongMing
/// Description:射击道具 玩家射击、穿戴射击武器控制
/// Date: 2022-5-17 17:44:22
/// </summary>
using UnityEngine;
using BudEngine.NetEngine;

public class PlayerMeleeShoot : PlayerAttackBase
{
    private AttackData attackData;
    private string attackName = "pvp_attack";
    private string runAttackName = "pvp_runattack";
    private string underAttackName = "pvp_beattack";
    private string underAttackBackName = "pvp_beattackback";
    private string reloadName = "shoot_reload";
    public string PlayerId;
    public MeleeShootWeapon HoldWeapon;
    public AnimationController animCon;
    public Animator playerAnimator;
    public PlayerType playerType = PlayerType.SelfPlayer;
    private const int Offest = 500;
    private ParticleSystem effectPs;
    private ParticleSystem[] fireEffects;
    private GameObject fireEffectsGo;

    private const float SlowFireRate = 0.4f;
    private const float MediumFireRate = 0.2f;
    private const float FastFireRate = 0.1f;

    private float fireRate = 0.2f;
    public float fireRateTimer = -1;
    private float fireRate_CurTime = 0.5f;
    private float fireRate_LastTime = 0f;

    private float calibrateRate = 1.0f;
    private float calibrateRateTimer = -1;
    private float calibrateRate_CurTime = 0.5f;
    private float calibrateRate_LastTime = 0f;

    public PlayerMeleeShoot(GameObject player) : base(player)
    {
    }

    public void WearWeapon(MeleeShootWeapon weapon, int uid)
    {
        HoldWeapon = weapon;
        HoldWeapon.weaponUid = uid;
        InitFireRate();
    }

    public void DropWeapon()
    {
        HoldWeapon = null;
        fireEffects = null;
        if(fireEffectsGo != null)
        {
            GameObject.Destroy(fireEffectsGo);
        }
    }

    /// <summary>
    /// 初始化射速
    /// </summary>
    private void InitFireRate()
    {
        var entity = HoldWeapon.weaponBehaviour.entity;
        var shootComp = entity.Get<ShootWeaponComponent>();
        switch ((FireRate)shootComp.fireRate)
        {
            case FireRate.Slow:
                fireRate = SlowFireRate;
                break;
            case FireRate.Medium:
                fireRate = MediumFireRate;
                break;
            case FireRate.Fast:
                fireRate = FastFireRate;
                break;
        }
    }

    /// <summary>
    /// 玩家进行射击1.控制子弹射出 2.播放枪口特效 3.播放攻击动画
    /// </summary>
    public override void Attack()
    {
        fireRate_CurTime = Time.time;
        if (fireRate_CurTime - fireRate_LastTime >= fireRate)
        {
            fireRate_LastTime = fireRate_CurTime;
            fireRateTimer = 0;

            var shootPoint = HoldWeapon.ShootPoint;
            Vector3 targetPos = new Vector3();
            if (playerType == PlayerType.SelfPlayer)
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                var normalVec = ray.direction.normalized;
                targetPos = normalVec * Offest;
                WeaponBulletManager.Inst.InitShootBehaviour(shootPoint.position, targetPos);
                RaycastHit hit;
                if (Physics.Raycast(shootPoint.position, targetPos, out hit, 100, ~(1 << LayerMask.NameToLayer("PVPArea") | 1 << LayerMask.NameToLayer("SpecialModel") | 1 << LayerMask.NameToLayer("TriggerModel") | 1 << LayerMask.NameToLayer("Airwall"))))
                {
                    OnTriggerPlayer(hit.transform.gameObject, CurPlayer.transform);
                }
            }
            else if (playerType == PlayerType.OtherPlayer)
            {
                var normalVec = CurPlayer.transform.forward.normalized;
                targetPos = normalVec * Offest;
                WeaponBulletManager.Inst.InitShootBehaviour(shootPoint.position, targetPos);
            }
            PlayFireEffect(shootPoint, targetPos);
            PlayAttackAnim();
            UpdateBulletCount();
        }
    }

    public override void UnderAttack(int sendPlayer, int dir, int attackPart = (int)HitPart.HEAD)
    {
        if (HoldWeapon != null && playerType == PlayerType.SelfPlayer)
        {
            //第一人称下不播放受击动画
            AKSoundManager.Inst.PlayAttackSound("gun", "play_pvp_hit_1p", "pvp_hit", CurPlayer);
            return;
        }
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        PlayUnderAttackEffect(sendPlayer,dir);
    }

    public void OnStartReload()
    {
        PlayReloadAnim();
        if(playerType == PlayerType.SelfPlayer)
        {
            ShootWeaponManager.Inst.SendOperateMsgToSever(HoldWeapon.weaponBehaviour, OPERATE_TYPE.StartReload);
        }
    }

    private void PlayReloadAnim()
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        PlayAnim(playerAnimator, reloadName);
    }

    public void onReloadComplete()
    {
        var entity = HoldWeapon.weaponBehaviour.entity;
        var shootComp = entity.Get<ShootWeaponComponent>();
        shootComp.curBullet = shootComp.capacity;
        if (playerType == PlayerType.SelfPlayer)
        {
            ShootWeaponManager.Inst.SendOperateMsgToSever(HoldWeapon.weaponBehaviour, OPERATE_TYPE.EndReload);
        }
    }

    public void BulletCalibration(int curBullet)
    {
        var entity = HoldWeapon.weaponBehaviour.entity;
        var shootComp = entity.Get<ShootWeaponComponent>();
        shootComp.curBullet = curBullet;
    }

    private void PlayAnim(Animator animator, string stateName)
    {
        if (playerType == PlayerType.SelfPlayer)
        {
            animator.Play(stateName, -1);
        }
        else
        {
            animCon.PlayAnim(null, stateName, -1);
        }
    }

    /// <summary>
    /// 播放开火动作
    /// </summary>
    private void PlayAttackAnim()
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        switch (playerType)
        {
            case PlayerType.SelfPlayer:
                PlayAnim(playerAnimator, attackName);
                //SendAttackMsg();
                AKSoundManager.Inst.PlayAttackSound("gun", "play_pvp_attack_1p", "pvp_attack", CurPlayer);
                break;
            case PlayerType.OtherPlayer:
                var isMoving = playerAnimator.GetBool("IsMoving");
                var curAttackName = isMoving ? runAttackName : attackName;
                PlayAnim(playerAnimator, curAttackName);
                AKSoundManager.Inst.PlayAttackSound("gun", "play_pvp_attack_3p", "pvp_attack", CurPlayer);
                break;
        }
    }

    /// <summary>
    /// 播放枪口特效
    /// </summary>
    private void PlayFireEffect(Transform shootPoint,Vector3 targetPos)
    {
        if (fireEffects == null)
        {
            GameObject shootEffect = ResManager.Inst.LoadRes<GameObject>("Effect/gun_light/gun_light");
            fireEffectsGo = GameObject.Instantiate(shootEffect, shootPoint);
            fireEffects = fireEffectsGo.GetComponentsInChildren<ParticleSystem>();
        }
        fireEffectsGo.transform.LookAt(targetPos);
        for (int i = 0; i < fireEffects.Length; i++)
        {
            fireEffects[i].Play();
        }
    }

    /// <summary>
    /// 更新子弹数量 - 只有开启了弹匣 - 才会更新子弹数量
    /// </summary>
    private void UpdateBulletCount()
    {
        var entity = HoldWeapon.weaponBehaviour.entity;
        var shootComp = entity.Get<ShootWeaponComponent>();
        var hasCap = shootComp.hasCap;
        if (hasCap == (int)CapState.HasCap && shootComp.curBullet > 0)
        {
            shootComp.curBullet--;
        }
    }

    /// <summary>
    /// 播放受击动作和音效
    /// </summary>
    /// <param name="dir">受击方向</param>
    private void PlayUnderAttackEffect(int sendPlayer, int dir)
    {
        var animName = string.Empty;
        switch (dir)
        {
            case (int)AttrackDirection.Forward:
                animName = underAttackName;
                break;
            case (int)AttrackDirection.Back:
                animName = underAttackBackName;
                break;
        }
        switch (playerType)
        {
            case PlayerType.SelfPlayer:
                AKSoundManager.Inst.PlayAttackSound("gun", "play_pvp_hit_1p", "pvp_hit", CurPlayer);
                break;
            case PlayerType.OtherPlayer:
                AKSoundManager.Inst.PlayAttackSound("gun", "play_pvp_hit_3p", "pvp_hit", CurPlayer);
                break;
        }
        PlayAnim(playerAnimator, animName);
    }

    private void SendAttackMsg()
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest) return;

        AttackWeaponItemData weaponItemData = new AttackWeaponItemData();
        weaponItemData.affectPlayers = new AttackWeaponAffectPlayerData[] { };
        var weaponUid = HoldWeapon.weaponUid;
        WeaponSystemController.Inst.SendWeaponAttackReq(ItemType.SHOOT_WEAPON, weaponUid, weaponItemData,
            (errCode, errMsg) => {
                LoggerUtils.Log($"SendWeaponShootReq callback ,errorCode:{errCode}, {errMsg}");
            });
    }

    private void OnTriggerPlayer(GameObject other, Transform hitTransform)
    {
        if (playerType == PlayerType.OtherPlayer)
            return;
        //TODO:SendMsgToCilent
        var isCanAttack = CanBeAttacked(other);
        if ((other.layer == LayerMask.NameToLayer("OtherPlayer") || other.layer == LayerMask.NameToLayer("Touch")) && isCanAttack)
        {
            attackData.AttackDir = other.transform.position - hitTransform.position;
            attackData.AttackDir.y = 0;
            Vector3 playerDic = other.transform.forward;
            playerDic.y = 0;
            bool isForward = Vector3.Dot(attackData.AttackDir, playerDic) >= 0;
            attackData.AnimDir = isForward ? AttrackDirection.Back : AttrackDirection.Forward;
            OnHitOtherPlayer(other);
        }
    }

    private void OnHitOtherPlayer(GameObject other)
    {
        AttackWeaponAffectPlayerData affectData = new AttackWeaponAffectPlayerData();
        affectData.PlayerId = PlayerInfoManager.GetPlayerIdByObj(other);
        affectData.AttackPlayerId = Player.Id;
        affectData.Damage = HoldWeapon.Damage;
        affectData.AnimDir = attackData.AnimDir;
        affectData.AttackDir = attackData.AttackDir;
        AttackWeaponItemData weaponItemData = new AttackWeaponItemData();
        weaponItemData.affectPlayers = new[]
        {
            affectData,
        };
        var weaponUid = HoldWeapon.weaponUid;
        WeaponSystemController.Inst.SendWeaponAttackReq(ItemType.SHOOT_WEAPON, weaponUid, weaponItemData, null);
    }

    public void OnDeath()
    {
        //玩家已死亡不处理
        if (PlayerManager.Inst.GetPlayerDeathState(PlayerId))
        {
            return;
        }
        PlayDeathPs();
        PlayerManager.Inst.OnPlayerDeath(PlayerId);
        AKSoundManager.Inst.PlayDeathSound(CurPlayer);
    }

    private void PlayDeathPs()
    {
        if (effectPs == null)
        {
            GameObject deathEffect = ResManager.Inst.LoadRes<GameObject>("Effect/death_smoke/death_smoke");
            var effect = GameObject.Instantiate(deathEffect, CurPlayer.transform);
            effectPs = effect.GetComponentInChildren<ParticleSystem>();
        }
        effectPs.Play();
    }

    private bool CanBeAttacked(GameObject other)
    {
        //TODO:待添加全场景玩家管理器
        var otherPlayerId = PlayerInfoManager.GetPlayerIdByObj(other);
        if (string.IsNullOrEmpty(otherPlayerId))
        {
            return false;
        }
        var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(otherPlayerId);
        if (otherComp != null)
        {
            if (otherComp.steeringWheel != null)
            {
                return false;
            }

            if (MagneticBoardManager.Inst.IsOtherPlayerOnBoard(otherComp))
            {
                return false;
            }
            if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(otherComp))
            {
                return false;
            }
        }
        return true;
    }

    public void OnPointerDown()
    {
        if(HoldWeapon != null)
        {
            KeepFire();
            KeepBulletCalibrate();
        }
    }

    public void OnPointerUp()
    {
        fireRateTimer = -1;
        calibrateRateTimer = -1;
    }

    private void KeepFire()
    {
        if (fireRateTimer >= 0)
        {
            fireRateTimer += Time.deltaTime;
        }
        if (fireRateTimer >= fireRate)
        {
            Attack();
        }
    }

    private void KeepBulletCalibrate()
    {
        var entity = HoldWeapon.weaponBehaviour.entity;
        var shootComp = entity.Get<ShootWeaponComponent>();
        var hasCap = shootComp.hasCap;
        if (playerType == PlayerType.SelfPlayer && hasCap == (int)CapState.HasCap)
        {
            if (calibrateRateTimer >= 0)
            {
                calibrateRateTimer += Time.deltaTime;
            }
            if (calibrateRateTimer >= calibrateRate)
            {
                SendBulletCalibrate();
            }
        }
    }

    public void SendBulletCalibrate()
    {
        calibrateRate_CurTime = Time.time;
        if (calibrateRate_CurTime - calibrateRate_LastTime >= calibrateRate)
        {
            calibrateRate_LastTime = calibrateRate_CurTime;
            calibrateRateTimer = 0;
            ShootWeaponManager.Inst.SendOperateMsgToSever(HoldWeapon.weaponBehaviour, OPERATE_TYPE.BulletCalibration);
        }
    }
}
