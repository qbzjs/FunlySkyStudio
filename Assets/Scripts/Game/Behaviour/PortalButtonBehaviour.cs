using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

[SerializeField]
public class PortalPointData
{
    public int id;
}

public class PortalButtonBehaviour : NodeBaseBehaviour
{
    public TextMeshPro textMesh;
    public int pid;
    private bool isCanClick = true;
    private Animator mAnimator;


    public static MaterialPropertyBlock mpb;
    [HideInInspector]
    public Renderer[] renderers;
    private Color[] oldColor;

    private PlayerBaseControl playerCom;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        renderers = GetComponentsInChildren<Renderer>();
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        if (textMesh == null)
        {
            textMesh = this.GetComponentInChildren<TextMeshPro>(true);
        }
        if(mAnimator == null)
        {
            mAnimator = this.GetComponentInChildren<Animator>();
            mAnimator.Play("Inacbtn", 0, 0);
        }

        playerCom = GameObject.Find("GameStart").GetComponent<GameController>().playerCom;
    }

    public override void OnReset()
    {
        base.OnReset();
        pid = 0;
        textMesh.text = "";
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    private void Start()
    {
        textMesh.text = pid.ToString();
    }

    private void OnDestroy()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public void InitDefaultArg(object data)
    {
        pid = (int)data;
        textMesh.text = pid.ToString();
        PortalPointManager.Inst.AddPortalButton(pid, this.entity);
    }

    private void OnChangeMode(GameMode mode)
    {
        textMesh.gameObject.SetActive(mode == GameMode.Edit);
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Point);
        PortalPlayPanel.Instance.AddButtonClick(OnChangePlayerPosClick,true);
        PortalPlayPanel.Instance.SetTransform(transform);
     }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    public void OnChangePlayerPosClick()
    {
        //在游戏等待区域，不能和传送按钮交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart ||PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("You could not interact with teleport spawn point in Waiting Zone");
            return;
        }

        // 自己为牵手状态中的跟随者，不能和传送按钮交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isFollowPlayer)
        {
            TipPanel.ShowToast("You could not interact with teleport spawn point while Hand-in-hand");
            return;
        }
        //冻结状态不允许点击按钮
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return;
        }

        if (isCanClick)
        {
            isCanClick = false;
            AKSoundManager.Inst.PostEvent("play_button", gameObject);
            mAnimator.Play("Inacbtn", 0, 0);
            Invoke("GotoNewPos", 0.3f);
        }
    }

    private void GotoNewPos()
    {
        isCanClick = true;
        var point = PortalPointManager.Inst.GetPointGo(pid);
        if(point == null)
        {
            return;
        }
        if (point.Get<GameObjectComponent>().bindGo != null)
        {
            BlackPanel.Show();
            BlackPanel.Instance.PlayTransitionAnim();
            Transform pointTransform = point.Get<GameObjectComponent>().bindGo.GetComponent<Transform>();

            playerCom.SetPlayerPos(pointTransform.position, pointTransform.rotation);
        }
    }

    public void RefreshButtonId()
    {
        textMesh.text = pid.ToString();
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref oldColor);
    }
}
