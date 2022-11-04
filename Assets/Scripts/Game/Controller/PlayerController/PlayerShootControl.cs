/// <summary>
/// Author:Mingo-LiZongMing
/// Description:玩家射击控制器-玩家自己捡起武器丢弃武器
/// Date: 2022-5-17 17:44:22
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public static PlayerShootControl Inst;
    [HideInInspector]
    public PlayerMeleeShoot curShootPlayer;
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    private GameObject fpsPlayerModel;

    private const string fpsPlayerName = "fpsPlayer";
    private const string PICK_HAND_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/prop_r/pickNode";
    private Vector3 pickNodeRot = new Vector3(5, 269, 268);//局部坐标！
    private Transform selfPickNode;
    private Transform fpsPickNode;
    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Shoot, Inst);
        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
        var roleCom = playerBase.transform.GetComponentInChildren<RoleController>(true);
        selfPickNode = roleCom.GetBandNode((int)BodyNode.PickNode);

        curShootPlayer = new PlayerMeleeShoot(playerBase.gameObject);
        curShootPlayer.animCon = animCon;
        curShootPlayer.playerAnimator = playerAnim;
        curShootPlayer.playerType = PlayerAttackBase.PlayerType.SelfPlayer;
        curShootPlayer.PlayerId = GameManager.Inst.ugcUserInfo.uid;

        MessageHelper.AddListener<bool>(MessageName.OnEmoPlay, OnEmoPlay);
        MessageHelper.AddListener<bool>(MessageName.OnEmoEnd, OnEmoEnd);
    }

    public void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Shoot);
        }
        Inst = null;
        
        MessageHelper.RemoveListener<bool>(MessageName.OnEmoPlay, OnEmoPlay);
        MessageHelper.RemoveListener<bool>(MessageName.OnEmoEnd, OnEmoEnd);
    }

    public void OnWearWeapon(MeleeShootWeapon weapon, int weaponUid)
    {
        if (curShootPlayer != null)
        {
            curShootPlayer.WearWeapon(weapon, weaponUid);
            BindWeaponToFpsPlayerModel();
            PlayModeHandler.fpvRotMax = 45;
        }
    }

    public void OnDropWeapon()
    {
        if (curShootPlayer != null && curShootPlayer.HoldWeapon != null)
        {
            //还原Animator
            curShootPlayer.playerAnimator = playerAnim;
            ResetCamera();
            UnBindWeaponToFpsPlayerModel();
            curShootPlayer.DropWeapon();
            ShootWeaponFireManager.Inst.RemovePlayerInShootingList(curShootPlayer);
        }
    }

    /// <summary>
    /// 玩家按下开火键
    /// </summary>
    public void OnPointerDown()
    {
        if (curShootPlayer != null)
        {
            ShootWeaponFireManager.Inst.AddPlayerInShootingList(curShootPlayer);
            if (curShootPlayer.HoldWeapon != null && curShootPlayer.HoldWeapon.weaponBehaviour != null)
            {
                var baseBev = curShootPlayer.HoldWeapon.weaponBehaviour;
                ShootWeaponManager.Inst.SendOperateMsgToSever(baseBev, OPERATE_TYPE.OnBtnDown);
            }
        }
    }

    public void OnStartReload()
    {
        if (curShootPlayer != null)
        {
            curShootPlayer.OnStartReload();
        }
    }

    public void OnReloadComplete()
    {
        if (curShootPlayer != null)
        {
            curShootPlayer.onReloadComplete();
        }
    }

    /// <summary>
    /// 玩家松开开火键
    /// </summary>
    public void OnPointerUp()
    {
        if (curShootPlayer != null)
        {
            ShootWeaponFireManager.Inst.RemovePlayerInShootingList(curShootPlayer);
            curShootPlayer.OnPointerUp();
            if(curShootPlayer.HoldWeapon != null && curShootPlayer.HoldWeapon.weaponBehaviour != null)
            {
                var baseBev = curShootPlayer.HoldWeapon.weaponBehaviour;
                ShootWeaponManager.Inst.SendOperateMsgToSever(baseBev, OPERATE_TYPE.OnBtnUp);
            }
        }
    }

    /// <summary>
    /// 将fpsPlayerModel的手上的道具还原 并且隐藏 fpsPlayerModel
    /// </summary>
    private void UnBindWeaponToFpsPlayerModel()
    {
        var curWeaponBev = curShootPlayer.HoldWeapon.weaponBehaviour;
        var curWeaponPos = curWeaponBev.transform.localPosition;
        var curWeaponRot = curWeaponBev.transform.localEulerAngles;
        curWeaponBev.transform.localPosition = curWeaponPos;
        curWeaponBev.transform.localEulerAngles = curWeaponRot;
        fpsPlayerModel.SetActive(false);
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.SetFlyBtnActive(true);
            PlayModePanel.Instance.LookFpsMode(false);
        }
        GameManager.Inst.MainCamera.AddLayer(LayerMask.NameToLayer("Player"));
    }

    /// <summary>
    /// 将武器绑定到fpsPlayerModel的手上
    /// </summary>
    /// <returns></returns>
    private void BindWeaponToFpsPlayerModel()
    {
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.isTps = true;
            PlayModePanel.Instance.OnChangeViewBtnClick();
            PlayModePanel.Instance.LookFpsMode(true);
        }
        GameManager.Inst.MainCamera.RemoveLayer(LayerMask.NameToLayer("Player"));

        //开启背包时要把道具移到人物手上，不然道具会跟着fps模型被删除
        if (SceneParser.Inst.GetBaggageSet() == 1 && fpsPickNode != null)
        {
            for (int i = 0; i < fpsPickNode.transform.childCount; i++)
            {

                var item = fpsPickNode.transform.GetChild(i);
                var curItemPos = item.transform.localPosition;
                var curItemRot = item.transform.localEulerAngles;
                if (selfPickNode != null)
                {
                    item.transform.SetParent(selfPickNode);
                    item.transform.localPosition = curItemPos;
                    item.transform.localEulerAngles = curItemRot;
                    item.gameObject.SetActive(false);
                }
            }
        }

        //删除fpsPlayerModel - 因为现在可以场景内换装 - 不保证每次捡起时都是同样的穿着
        if (fpsPlayerModel != null)
        {
            GameObject.Destroy(fpsPlayerModel);
            fpsPlayerModel = null;
        }
        StopCoroutine("DelayGetFpsPlayerModel");
        StartCoroutine("DelayGetFpsPlayerModel");
    }

    private IEnumerator DelayGetFpsPlayerModel()
    {
        yield return null;
        fpsPlayerModel = GetFpsPlayerModel();
        var curWeaponBev = curShootPlayer.HoldWeapon.weaponBehaviour;
        var curWeaponPos = curWeaponBev.transform.localPosition;
        var curWeaponRot = curWeaponBev.transform.localEulerAngles;
        fpsPickNode = fpsPlayerModel.transform.Find(PICK_HAND_PATH);
        if (fpsPickNode != null)
        {
            for(int i = 0;i < fpsPickNode.transform.childCount; i++)
            {
                Destroy(fpsPickNode.transform.GetChild(i).gameObject);
            }
            fpsPickNode.transform.localEulerAngles = pickNodeRot;
            curWeaponBev.transform.SetParent(fpsPickNode);
            curWeaponBev.gameObject.SetActive(true);
            curWeaponBev.transform.localPosition = curWeaponPos;
            curWeaponBev.transform.localEulerAngles = curWeaponRot;
        }
        //开启射击动画
        var fpsPlayerAnim = fpsPlayerModel.GetComponent<Animator>();
        fpsPlayerAnim.runtimeAnimatorController = playerBase.playerShootAnimCon;
        curShootPlayer.playerAnimator = fpsPlayerAnim;
        //为Fps模型Add控制脚本
        var fpsShootComp = fpsPlayerModel.AddComponent<FpsShootPlayerControl>();
        fpsShootComp.InitData(fpsPlayerAnim, curShootPlayer);
        fpsPlayerModel.SetActive(true);
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.SetFlyBtnActive(false);
        }
        playerBase.SetFly(false);
    }

    private GameObject GetFpsPlayerModel()
    {
        RestPlayerModeState();
        var playerGo = playerAnim.gameObject;
        playerGo.SetActive(false);
        var lookCenter = playerBase.transform.Find("Play Mode Camera Center");
        var fpsPlayerGo = Instantiate(playerGo, lookCenter);
        fpsPlayerGo.name = fpsPlayerName;
        fpsPlayerGo.transform.localPosition = new Vector3(0, -1.06f, 0.082f);
        fpsPlayerGo.transform.localEulerAngles = Vector3.zero;
        //移除fpsModel身上的Component
        var scripts = fpsPlayerGo.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var script in scripts)
        {
            DestroyImmediate(script);
        }
        //fpsModel身上不接收阴影
        var meshRenders = fpsPlayerGo.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var render in meshRenders)
        {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        //设置fpsModel的层级
        var contents = fpsPlayerGo.GetComponentsInChildren<Transform>(true);
        foreach (var item in contents)
        {
            item.gameObject.layer = LayerMask.NameToLayer("Model");
        }
        return fpsPlayerGo;
    }

    private void OnEmoPlay(bool isSelfPlayer)
    {
        if (fpsPlayerModel != null && isSelfPlayer)
        {
            fpsPlayerModel.SetActive(false);
        }
    }

    private void OnEmoEnd(bool isSelfPlayer)
    {
        if (fpsPlayerModel != null && isSelfPlayer)
        {
            if (curShootPlayer.HoldWeapon != null)
            {
                fpsPlayerModel.SetActive(true);
            }
        }
    }
    public void ResetCamera()
    {
        //当低于默认第一人称视角扔道具时，重置相机位置
        PlayModeHandler.fpvRotMax = 20;
        var maxValue = ShootWeaponManager.cameraRotMax + 1;
        var minValue = PlayModeHandler.fpvRotMax;
        PlayerBaseControl.Inst.SetLookCenterMin(minValue, maxValue);
    }

    private void RestPlayerModeState()
    {
        var roleCom = playerBase.transform.GetComponentInChildren<RoleController>(true);
        var foodNode = roleCom.GetBandNode((int)BodyNode.FoodNode);
        for (int i = 0; i < foodNode.childCount; i++)
        {
            DestroyImmediate(foodNode.GetChild(i).gameObject);
        }
        selfPickNode.gameObject.SetActive(true);
    }
}
