using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PortalPlayPanel : BasePanel<PortalPlayPanel>
{
    public Transform IconContent;
    public Transform FollowTrans;
    public Button PlayBtn;
    public Image btnIcon;
    public RectTransform oriPos;
    private Transform handTransform;
    private IconName curIcon = IconName.None;
    private SpriteAtlas priAtlas;

    private string curTargetId;
    private Sequence iconSequence;
    private GameObject playBtnEffect;
    private List<GameObject> _extraIconLst = new List<GameObject>();
    
    private readonly Dictionary<IconName, string> iconDic = new Dictionary<IconName, string>()
    {
        {IconName.Hand, "btn_interact"},
        {IconName.MagneticBoard, "btn_magnetic"},
        {IconName.SteeringWheel, "btn_steering"},
        {IconName.Collect, "btn_collect"},
        {IconName.Shopping,"btn_shopping"},
        {IconName.Like,"btn_like" },
        {IconName.Point,"btn_point" },
        {IconName.Portal,"btn_portal" },
        {IconName.Switch,"btn_switch" },
        {IconName.Attention,"btn_attention" },
        {IconName.DisplayBoard, "ic_playerprofile"},
        {IconName.Favorite, "btn_favorite"},
        {IconName.Sound, "btn_sound"},
        {IconName.HandShake, "btn_handshake"},
        {IconName.WaitHand, "btn_plzshake"},
        {IconName.PVP, "btn_pvp"},
        {IconName.AddFriend, "ic_addfriend"},
        {IconName.Dc, "ic_dc"},
        {IconName.Firework, "btn_firework"},
        {IconName.Eat, "btn_eat"},
        {IconName.Seesaw, "ic_seesaw_sit"},
        {IconName.Slide, "btn_slider"},
        {IconName.Swing, "btn_swing_sit"},

    };
    public enum IconName
    {
        None,
        Hand,
        MagneticBoard,
        SteeringWheel,
        Collect,
        Shopping,
        Like,
        Point,
        Portal,
        Switch,
        Attention,
        DisplayBoard,
        Favorite,
        Sound,
        HandShake,
        WaitHand, // 等待牵手
        PVP,
        AddFriend,
        Dc,
        Firework,
        Eat,
        Seesaw,
        Slide,//滑梯
        Swing,
    }

    private readonly HashSet<IconName> steeringDisable = new HashSet<IconName>()
    {
        IconName.Point,
        IconName.Portal,
        IconName.MagneticBoard,
        IconName.Slide,
    };
    private readonly HashSet<IconName> ladderDisable = new HashSet<IconName>()
    {
        IconName.SteeringWheel,
        IconName.MagneticBoard,
        IconName.Eat,
    };
    private readonly HashSet<IconName> slideDisable = new HashSet<IconName>()
    {
        IconName.SteeringWheel,
        IconName.MagneticBoard,
        IconName.Eat,
    };

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        SetIcon(IconName.Hand);
        transform.SetAsFirstSibling();
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        KillIconAnim();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
    }


    /// <summary>
    /// 设置显示的图标
    /// </summary>
    public void SetIcon(IconName iconName)
    {
        ClearExtraIcon();
        ReleasePlayBtnEffect();
        EnablePlayBtnImage();
        if (curIcon == iconName) return;

        if ((PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null && steeringDisable.Contains(iconName))
            || (StateManager.IsOnLadder&&ladderDisable.Contains(iconName)))
        {
            gameObject.SetActive(false);
            return;
        }
        if ((PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null && steeringDisable.Contains(iconName))
            || (StateManager.IsOnSlide && slideDisable.Contains(iconName)))
        {
            gameObject.SetActive(false);
            return;
        }

        //拍照模式不和部分道具交互:
        //沉浸购买-传送按钮-传送门
        //TODO:交互按钮配置化
        if (CameraModeManager.Inst.GetCurrentCameraMode() == CameraModeEnum.FreePhotoCamera)
        {
            if (iconName == IconName.Shopping || iconName == IconName.Portal || iconName == IconName.Point)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        
        //降落伞使用中不显示按钮
        if (StateManager.IsParachuteUsing)
        {
            gameObject.SetActive(false);
            return;
        }

        if (priAtlas == null)
        {
            priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        }
       
        btnIcon.sprite = priAtlas.GetSprite(iconDic[iconName]);
        curIcon = iconName;
        
        KillIconAnim();
    }

    public void AddExtraIcon(IconName iconName, Action callback)
    {
        var btn = GameObject.Instantiate(PlayBtn, IconContent);
        btn.gameObject.SetActive(true);
        btn.onClick.AddListener(() => {
            if (!CanClickButton())
            {
                return;
            }
            callback?.Invoke(); });
        var img = btn.GetComponent<Image>();
        img.sprite = priAtlas.GetSprite(iconDic[iconName]);
        _extraIconLst.Add(btn.gameObject);
    }

    public void ClearExtraIcon()
    {
        for (int i = _extraIconLst.Count - 1; i >= 0; i--)
            GameObject.Destroy(_extraIconLst[i]);

        _extraIconLst.Clear();
    }

    public void AddButtonClick(UnityAction act)
    {
        AddButtonClick(act, false);
       
    }
    public void AddButtonClick(UnityAction act,bool isChangePlayerPos)
    {
        curTargetId = null;
        PlayBtn.onClick.RemoveAllListeners();
        PlayBtn.onClick.AddListener(() => {
            if (!CanClickButton())
            {
                return;
            }
            if (isChangePlayerPos)
            {
                MessageHelper.Broadcast(MessageName.PosMove, false);
            }
            act();
        });
    }

    private bool CanClickButton()
    {
        if (StateManager.IsFishing)
        {
            return false;
        }

        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return false;
        }

        if (StateManager.IsParachuteUsing)
        {
            return false;
        }

        if (StateManager.IsSnowCubeSkating)
        {
            return false;
        }

        if (SwordManager.Inst.IsSelfInSword())
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        var animC = PlayerBaseControl.Inst.animCon;
        switch (curIcon)
        {
            case IconName.MagneticBoard:
                
                if (animC.isLooping && animC.loopingInfo.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
                {
                    //双人动作发起状态，不能上磁力板
                    animC.StopLoop();
                    return false;
                }

                if (StateManager.IsOnSeesaw)
                {
                    SeesawManager.Inst.ShowSeesawMutexToast();
                    return false;
                }
                if (StateManager.IsOnSwing)
                {
                    SwingManager.Inst.ShowSwingMutexToast();
                    return false;
                }
                return true;
            case IconName.SteeringWheel:
                if (animC.isLooping && animC.loopingInfo.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
                {
                    animC.StopLoop();
                    return false;
                }
                
                if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
                {
                    TipPanel.ShowToast("You could not interact with Steering wheel while driving");
                    return false;
                }
                return true;
            default:
                return true;
        }
    }

    public void SetCurTargetId(string targetId)
    {
        curTargetId = targetId;
    }

    public string GetCurTargetId()
    {
        return curTargetId;
    }


    public void SetTransform(Transform transform)
    {
        handTransform = transform;
        UpdatePos();
    }

    private void LateUpdate()
    {
        UpdatePos();
    }

    private void UpdatePos()
    {
        if (handTransform && Camera.main != null)
        {
            Vector3 CamPos = Camera.main.WorldToScreenPoint(handTransform.position);
            if (curIcon==IconName.DisplayBoard)
            {
                FollowTrans.localPosition = new Vector3(CamPos.x - Screen.width / 2, CamPos.y - Screen.height / 2+140, 0);
                return;
            }
            FollowTrans.localPosition = new Vector3(CamPos.x - Screen.width / 2, CamPos.y - Screen.height / 2, 0);
        }
    }
    
    public void SetPlayBtnVisible(bool isVisible)
    {
        PlayBtn.gameObject.SetActive(isVisible);
        foreach(var icon in _extraIconLst)
        {
            icon.gameObject.SetActive(isVisible);
        }
    }

    private void KillIconAnim()
    {
        if (iconSequence != null)
        {
            iconSequence.Kill();
            iconSequence = null;
        }
    }
    
    public void SetIconAnim()
    {
        KillIconAnim();

        iconSequence = DOTween.Sequence();
        iconSequence.Append(DOTween.ToAlpha(() => btnIcon.color, x => btnIcon.color = x, 0.5f, 0.5f).SetTarget(btnIcon));
        iconSequence.Append(DOTween.ToAlpha(() => btnIcon.color, x => btnIcon.color = x, 1, 0.5f).SetTarget(btnIcon));
        iconSequence.SetLoops(-1);
    }
    public void DisablePlayBtnImage()
    {
        btnIcon.enabled = false;
    }
    public void EnablePlayBtnImage()
    {
        btnIcon.enabled = true;
    }
    public void ReleasePlayBtnEffect()
    {
        if (playBtnEffect != null)
        {
            //先销毁再创建
            Destroy(playBtnEffect);
        }
    }
    public void AttachPlayBtnEffect(string effectPath)
    {
        ReleasePlayBtnEffect();
        GameObject effectPrefab=  ResManager.Inst.LoadRes<GameObject>(effectPath);
        playBtnEffect =  GameObject.Instantiate<GameObject>(effectPrefab);
        Transform transform = playBtnEffect.transform;
        transform.parent = PlayBtn.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one*2;
    }
}
