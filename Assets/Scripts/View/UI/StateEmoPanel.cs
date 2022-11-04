using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;
using static PortalPlayPanel;

/// <summary>
/// Author:Shaocheng
/// Description:状态表情控制面板如加好友状态表情等
/// 2022-7-11:更新：自己看不到自己头顶的按钮
/// Date: 2022-7-5 15:23:43
/// </summary>
public class StateEmoPanel : BasePanel<StateEmoPanel>
{
    public EmoName CurStateEmo;

    //当前状态icon显示
    public Image StateIconImage;

    //取消状态按钮
    public Image CancelStateImage;
    public Button CancelStateBtn;
    private Transform handTransform;
    GameObject mEffectInstance;

    private SpriteAtlas priAtlas;

    private Dictionary<EmoName, StateEmoIcons> stateEmoDic = new Dictionary<EmoName, StateEmoIcons>()
    {
        {EmoName.EMO_ADD_FRIEND, new StateEmoIcons("ic_addfriend", "ic_addfriend_cancel")}
    };

    private struct StateEmoIcons
    {
        public string StateIcon;
        public string StateEmoCancelIcon;

        public StateEmoIcons(string stateIcon, string stateEmoCancelIcon)
        {
            StateIcon = stateIcon;
            StateEmoCancelIcon = stateEmoCancelIcon;
        }
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        Transform touchPos= PlayerBaseControl.Inst.playerAnim.transform.Find("touchPos");
        SetTransform(touchPos);
        AttachPlayBtnEffect("Effect/Please_add_friends/Please_add_friends");
    }
    public void AttachPlayBtnEffect(string effectPath)
    {
        GameObject effectPrefab = ResManager.Inst.LoadRes<GameObject>(effectPath);
        mEffectInstance = GameObject.Instantiate<GameObject>(effectPrefab);

        Transform transform = mEffectInstance.transform;
        transform.parent = StateIconImage.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = new Vector3(2,2,1);
    }
    public void SetIconHide()
    {
        StateIconImage.gameObject.SetActive(false);
    }
    public void SetIconShow()
    {
        StateIconImage.gameObject.SetActive(true);
    }
    public void SetIcon(EmoName emoEnum)
    {
        if (CurStateEmo == emoEnum) return;

        if (priAtlas == null)
        {
            priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        }

        StateIconImage.sprite = priAtlas.GetSprite(stateEmoDic[emoEnum].StateIcon);
        CancelStateImage.sprite = priAtlas.GetSprite(stateEmoDic[emoEnum].StateEmoCancelIcon);
        CurStateEmo = emoEnum;
    }

    public void SetCancelStateBtnClick(UnityAction action)
    {
        CancelStateBtn.onClick.RemoveAllListeners();
        CancelStateBtn.onClick.AddListener(action);
    }
    public void SetIsTps(bool isTps)
    {
        //如果是第三人称
        StateIconImage.gameObject.SetActive(isTps);
    }
    private void LateUpdate()
    {
        UpdatePos();
    }
    private void UpdatePos()
    {
        if (handTransform && Camera.main != null&& mEffectInstance!=null)
        {
            Vector3 CamPos = Camera.main.WorldToScreenPoint(handTransform.position);
            mEffectInstance.transform.localPosition = new Vector3(CamPos.x - Screen.width / 2, CamPos.y - Screen.height / 2, 0);
        }
    }
    public void SetTransform(Transform transform)
    {
        handTransform = transform;
    }
}