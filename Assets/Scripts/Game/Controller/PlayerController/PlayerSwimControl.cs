using UnityEngine;
using System.Collections;

public enum SwimEffect
{
    Swim,
    idle,
    walk
}

public enum SwimSound
{
    Swim,
    idle,
    walk
}

/// <summary>
/// Author:WenJia
/// Description:Player 游泳状态控制 
/// 主要包含 Player 位于水方块中的逻辑
/// Date: 2022/3/31 11:14:20
/// </summary>

public class PlayerSwimControl : MonoBehaviour, IPlayerCtrlMgr
{
    public enum SwimStatus
    {
        Up = 2,
        down = -2,
        stop = 0
    }
    public SwimLimite swimLimite = SwimLimite.nonLimit; //当玩家在游泳时到达上下边界时触发不可上或下移动逻辑 1:上边界 0:无 -1:下边界
    public enum SwimLimite
    {
        upLimit = 1,
        downLimit = -1,
        nonLimit = 0
    }

    [HideInInspector]
    public SwimSound curSwimSound = SwimSound.idle;
    [HideInInspector]
    public SwimEffect curSwimEffect = SwimEffect.idle;
    public static PlayerSwimControl Inst;
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    public CharacterController character;
    [HideInInspector]
    public bool isInWater;
    private GameObject curSwimEffectObj;
    [HideInInspector]
    public bool isSwimming;
    private const float WATER_GRAVITY = 0.1f; //水下重力系数
    public float jumpForceInWater = 2;
    private Vector3 swimmingBoxSize = new Vector3(0.4f, 0.3f, 0.4f);
    private Vector3 swimmingBoxCenter = new Vector3(0, -0.16f, 0);

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Swim, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
        character = playerBase.Character;

        PlayerControlManager.Inst.AddAnimName((int)AnimId.IsInWater, "IsInWater");
        PlayerControlManager.Inst.AddAnimName((int)AnimId.IsSwimming, "IsSwimming");
    }

    private void OnDisable()
    {
        ForceOutWater();
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Swim);
        }
        Inst = null;
    }

    private void Update()
    {
        if (!PlayerOnBoardControl.Inst || !PlayerOnBoardControl.Inst.isOnBoard||
            !PlayerLadderControl.Inst || !PlayerLadderControl.Inst.isOnLadder
            && !StateManager.IsOnSeesaw && !StateManager.IsOnSwing)
        {
            SetInWaterStatus();
            SetSwimEffect();
        }
        SetSwimSound();

        if (character && character.isGrounded)
        {
            if (isInWater && isSwimming)
            {
                PlayModePanel.Instance.waterPanel.StopSwimBtnClick();
            }
        }
    }

    public void JudgeSwimLimit()
    {
        // 判断游泳时是否到达上下边界
        if (isInWater)
        {
            if (isSwimming)
            {
                if ((swimLimite == SwimLimite.upLimit && playerBase.moveVec.y > 0)
                || (swimLimite == SwimLimite.downLimit && playerBase.moveVec.y < 0))
                {
                    playerBase.moveVec.y = 0;
                    playerBase.moveY = 0;
                }
                playerBase.moveVec.x *= 0.8f;
                playerBase.moveVec.z *= 0.8f;
            }
            else
            {
                playerBase.moveVec.x *= 0.5f;
                playerBase.moveVec.z *= 0.5f;
            }
        }
    }

    IEnumerator StartSwim()
    {
        playerBase.upwardVec.y = 3;
        yield return new WaitForFixedUpdate();
        playerBase.upwardVec.y = 0;
    }

    public void SwimSpeed(SwimStatus speed)
    {
        if (!animCon.isPlaying)
        {
            int spe = (int)speed;
            playerBase.moveVec.y = spe;
            playerBase.moveY = spe;
        }
        else
        {
            animCon.StopLoop();
        }
    }

    public void SetSwim()
    {
        StartCoroutine(StartSwim());
        isSwimming = true;
        // playerAnim.SetBool("IsSwimming", true);
        playerBase.PlayAnimation(AnimId.IsSwimming, true);
        playerBase.gravity = 0;
        PlayerControlManager.Inst.waterTriggerSize = swimmingBoxSize;
        PlayerControlManager.Inst.waterTriggerCenter = swimmingBoxCenter;
    }

    public void StopSwim()
    {
        // playerAnim.SetBool("IsSwimming", false);
        playerBase.PlayAnimation(AnimId.IsSwimming, false);
        isSwimming = false;
        swimLimite = SwimLimite.nonLimit;
        playerBase.gravity = isInWater ? Physics.gravity.y * WATER_GRAVITY : Physics.gravity.y;
        playerBase.moveVec.y = 0;
        playerBase.moveY = 0;
        PlayerControlManager.Inst.waterTriggerSize = PlayerControlManager.Inst.normalBoxSize;
        PlayerControlManager.Inst.waterTriggerCenter = PlayerControlManager.Inst.normalBoxCenter;
    }

    public void SetInWaterStatus()
    {
        if (playerAnim.GetBool("IsInWater") == isInWater)
        {
            return;
        }

        playerBase.PlayAnimation(AnimId.IsInWater, isInWater);
        playerBase.PlayAnimation(AnimId.IsSwimming, isSwimming);
        if (isInWater)
        {
            playerBase.PlayAnimation(AnimId.IsFlying, false);
        }
    }

    public void IsInWater(bool isIn)
    {
        if (isIn)
        {
            if (!PlayerBaseControl.Inst.isTps)
            {
                StartCoroutine(PlayerBaseControl.Inst.SetPlayerRoleActive(0.1f, false));
            }
        }
        if (isInWater == isIn)
        {
            return;
        }
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard
        || StateManager.IsOnLadder || StateManager.IsOnSeesaw || StateManager.IsOnSwing
        || StateManager.IsOnSlide
        || PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual
        || PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel
        || StateManager.IsInSelfieMode)
        {
            isInWater = false;
            return;
        }
        isInWater = isIn;
        if (isInWater)
        {
			AttackWeaponManager.Inst.SetAttackCtrPanelActive(false);
            ShootWeaponManager.Inst.SetAttackCtrPanelActive(false);
            AudioController.Inst.PlayWaterAmbientMusic();
            //AKSoundManager.Inst.WaterSound(gameObject, isInWater);
            playerBase.SetFly(false);
            playerBase.gravity = Physics.gravity.y * WATER_GRAVITY;
            PlayModePanel.Instance.SetInWaterBtn(true);
            if (playerBase.upwardVec.y < 0)
            {
                playerBase.upwardVec.y = 0;
            }
            playerBase.moveVec.y = 0;
            playerBase.moveY = 0;
        }
        else
        {
            OutWater();
        }
    }

    public void OutWater()
    {
        CleanSwimEffect();
        if (AudioController.Inst)
        {
            AudioController.Inst.StopAmbientMusic();
            AudioController.Inst.PlayAmbientMusic();
        }

        playerBase.gravity = Physics.gravity.y;
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.waterPanel.StopSwimBtnClick();
            PlayModePanel.Instance.SetInWaterBtn(false);
        }

        SetInWaterStatus();
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        if (!PlayerBaseControl.Inst.isTps && PlayerControlManager.Inst.isPickedProp)
        {
            CoroutineManager.Inst.StartCoroutine(PlayerBaseControl.Inst.SetPlayerRoleActive(0, true));
        }

        // 移除游泳控制脚本
        Destroy(this);
    }

    public SwimSound GetSwimSoundState()
    {
        if (playerBase.isMoving)
        {
            return isSwimming ? SwimSound.Swim : SwimSound.walk;
        }
        else
        {
            return SwimSound.idle;
        }
    }

    public void SetSwimSound()
    {
        if (isInWater)
        {
            SwimSound soundName = GetSwimSoundState();
            if (soundName != curSwimSound)
            {
                curSwimSound = soundName;
                if (soundName == SwimSound.Swim)
                {
                    PlaySwimSound();
                }
                else if (soundName == SwimSound.idle)
                {
                    PlayWaterIdleSound();
                }
            }
        }
    }

    public SwimEffect GetSwimEffectState()
    {
        if (playerBase.isMoving)
        {
            return isSwimming ? SwimEffect.Swim : SwimEffect.walk;
        }
        else
        {
            return SwimEffect.idle;
        }
    }

    public void SetSwimEffect()
    {
        if (isInWater)
        {
            SwimEffect effectName = GetSwimEffectState();

            if (effectName != curSwimEffect || curSwimEffectObj == null)
            {
                curSwimEffect = effectName;
                ShowSwimEffect();
            }
            if (curSwimEffect == SwimEffect.Swim)
            {
                if (swimLimite == SwimLimite.upLimit)
                {
                    if (curSwimEffectObj.activeSelf)
                    {
                        curSwimEffectObj.SetActive(false);
                    }
                }
                else
                {
                    if (!curSwimEffectObj.activeSelf)
                    {
                        curSwimEffectObj.SetActive(true);
                    }
                }
            }
        }
    }

    public void PlaySwimSound()
    {
        if (!AKSoundManager.Inst.isOpenFootSound)
            return;
        if (isInWater && isSwimming && curSwimSound == SwimSound.Swim)
        {
            CancelInvoke("PlaySwimSound");
            AKSoundManager.Inst.PlaySwimSound("play_swim_1p", swimLimite == SwimLimite.upLimit, playerBase.gameObject);
            var deltaTime = 0.5f;
            Invoke("PlaySwimSound", deltaTime);
        }
    }

    public void PlayWaterIdleSound()
    {
        if (!AKSoundManager.Inst.isOpenFootSound)
            return;
        if (isInWater && curSwimSound == SwimSound.idle)
        {
            CancelInvoke("PlayWaterIdleSound");
            AKSoundManager.Inst.PostEvent("play_underwater_loading_1p", playerBase.gameObject);
            var deltaTime = 5f;
            Invoke("PlayWaterIdleSound", deltaTime);
        }
    }

    public void ShowSwimEffect()
    {
        Transform parent = playerBase.transform;
        string path = "";
        Vector3 pos = Vector3.zero;
        switch (curSwimEffect)
        {
            case SwimEffect.Swim:
                path = "Prefabs/Emotion/Express/swimming";
                parent = playerBase.transform.Find("Player/shoe");
                break;
            case SwimEffect.idle:
                path = "Prefabs/Emotion/Express/swimming_idle";
                pos = new Vector3(0, -GameConsts.PlayerNodeHigh, 0);
                break;
            case SwimEffect.walk:
                path = "Prefabs/Emotion/Express/swimming_walk";
                pos = new Vector3(0, -GameConsts.PlayerNodeHigh, 0);
                break;
        }
        GameObject movePrefab = ResManager.Inst.LoadCharacterRes<GameObject>(path);

        CleanSwimEffect();
        if (movePrefab != null)
        {
            curSwimEffectObj = Instantiate(movePrefab);
            curSwimEffectObj.transform.parent = parent;
            curSwimEffectObj.transform.localRotation = Quaternion.identity;
            curSwimEffectObj.transform.localPosition = pos;
            curSwimEffectObj.transform.localScale = Vector3.one;
        }
    }

    public void CleanSwimEffect()
    {
        if (curSwimEffectObj != null)
        {
            Destroy(curSwimEffectObj);
            curSwimEffectObj = null;
        }
    }

    public void ForceOutWater()
    {
        if (isInWater)
        {
            isInWater = false;
            OutWater();
        }
    }

    public void JumpInWater()
    {
        if (!animCon.isPlaying)
        {
            if (character && character.isGrounded)
            {
                playerBase.upwardVec.y = jumpForceInWater;
                playerBase.isGround = false;
                AudioController.Inst.audioState = MoveAudioState.Jump;
                AudioController.Inst.StopStepAudio();
                AudioController.Inst.PlayJumpAudio();
            }
        }
        else
        {
            animCon.StopLoop();

            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.StopFollowerLoop();
            }
        }
    }

    /*
    *切换视角时，恢复游泳状态
    */
    public void ChangeViewResetSwimState()
    {
        playerBase.PlayAnimation(AnimId.IsInWater, isInWater);
        playerBase.PlayAnimation(AnimId.IsSwimming, isSwimming);
        SetSwimEffect();
    }
}
