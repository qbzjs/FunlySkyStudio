using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:关注按钮行为
/// Date: 2022-3-30 19:43:08
/// </summary>
public class AttentionButtonBehaviour : NodeBaseBehaviour
{
    public static MaterialPropertyBlock mpb;
    [HideInInspector]
    public Renderer[] renderers;
    private Color[] oldColor;
    private bool isEnter = false;
    private Animator mAnimator;

    private GameMode curGameMode;
    private Color selectColor = new Color(1f, 1f, 1f);
    private Color unSelectColor = new Color(0, 0, 0);
    private GameObject mCube;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        renderers = GetComponentsInChildren<Renderer>();

        if (mAnimator == null)
        {
            mAnimator = this.GetComponentInChildren<Animator>();
        }
        mCube = this.gameObject.transform.Find("button").Find("thumbsuptap").gameObject;

        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        SetLocalColor(mCube, unSelectColor);
        InitAttentionButton();
    }

    public override void OnReset()
    {
        base.OnReset();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    private void OnChangeMode(GameMode mode)
    {
        curGameMode = mode;
        if (mode == GameMode.Edit)
            SceneBuilder.Inst.UpdateAllAttentionButton(0);
    }

    private void OnDestroy()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }


    public override void OnRayEnter()
    {
        if (!isCanClick) return;
        HighLight(true);
        isEnter = true;
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Attention);
        PortalPlayPanel.Instance.AddButtonClick(OnClickAttention);
        PortalPlayPanel.Instance.SetTransform(transform);
    }

    public override void OnRayExit()
    {
        if (isCanClick == false) return;
        HighLight(false);
        isEnter = false;
        PortalPlayPanel.Hide();
    }

    void OnClickAttention()
    {
        if (!isCanClick) return;
        if (curGameMode == GameMode.Guest)
        {
            if (GlobalFieldController.CurMapInfo.mapCreator.uid == GameInfo.Inst.myUid)
            {
                TipPanel.ShowToast("You can't follow yourself:)");
            }
            else
            {
                DoRequestAttention();
            }
        }
        else
        {
            TipPanel.ShowToast("Followed(playtest)");
            SceneBuilder.Inst.UpdateAllAttentionButton(1);
        }
    }

    private void DoRequestAttention()
    {
        UpLoadAttentionBody upLoadAttentionBody = new UpLoadAttentionBody
        {
            toUid = GlobalFieldController.CurMapInfo.mapCreator.uid,
            operationType = 0,
            clickPage = 1
        };
        //LoggerUtils.Log("JsonConvert.SerializeObject(UpLoadAttentionBody) = " + JsonConvert.SerializeObject(upLoadAttentionBody));
        HttpUtils.MakeHttpRequest("/social/setSubscribe", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(upLoadAttentionBody), OnAttentionSuccess, OnAttentionFailed);
    }

    public void OnAttentionSuccess(string msg)
    {
        //LoggerUtils.Log("OnAttentionSuccess=>" + msg);
        TipPanel.ShowToast("Followed");
        SceneBuilder.Inst.UpdateAllAttentionButton(1);
        DataLogUtils.NewUserFollowers();
    }

    public void OnAttentionFailed(string msg)
    {
        LoggerUtils.LogError("Script:AttentionButtonBehaviour error = " + msg);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        isCanClick = true;
    }

    #region switch color/animation/sounds.....

    public void InitAttentionButton()
    {
        //1-关注  3-互关
        if (GlobalFieldController.CurMapInfo != null && GlobalFieldController.CurMapInfo.relation != null && (GlobalFieldController.CurMapInfo.relation.subscribed == 1 || GlobalFieldController.CurMapInfo.relation.subscribed == 3))
        {
            isCanClick = false;
            PortalPlayPanel.Hide();
            mAnimator.Play("push");
            SetLocalColor(mCube, selectColor);
        }
        else
        {
            isCanClick = true;
        }
    }

    public void SetSelectState(int selectState)
    {
        if (selectState == 1)
        {
            isCanClick = false;
            PortalPlayPanel.Hide();
            AKSoundManager.Inst.PostEvent("play_button", gameObject);
            mAnimator.Play("push");
            
            CoroutineManager.Inst.StartCoroutine(DelayAni(0.8f, () =>
            {
                SetLocalColor(mCube, selectColor);
            }, 0));
        }
        else
        {
            isCanClick = true;
            SetLocalColor(mCube, unSelectColor);
            mAnimator.Play("pull");
        }
    }

    IEnumerator DelayAni(float animTime, Action aniCallBack, float DelayTime)
    {
        yield return new WaitForSeconds(animTime + DelayTime);
        aniCallBack();
    }

    public override void HighLight(bool isHigh)
    {
        if (renderers == null)
        {
            return;
        }
        if (isHigh)
        {
            oldColor = new Color[renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {
                renderers[i].GetPropertyBlock(mpb);
                oldColor[i] = mpb.GetColor("_Color");
                Color hColor = new Color(1.3f, 1.3f, 1.3f, 1);
                mpb.SetColor("_Color", hColor);
                renderers[i].SetPropertyBlock(mpb);
            }
        }
        else
        {
            if(oldColor != null && oldColor.Length == renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    renderers[i].GetPropertyBlock(mpb);
                    Color oColor = new Color(1f, 1f, 1f, 1);
                    mpb.SetColor("_Color", oColor);
                    renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
    }

    private void SetLocalColor(GameObject obj, Color color)
    {
        if (obj == null)
        {
            LoggerUtils.Log("obj is null");
            return;
        }
        MeshRenderer render = obj.GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        render.GetPropertyBlock(mpb);
        mpb.SetColor("_EmissionColor", color);
        render.SetPropertyBlock(mpb);
    }

    private void SetLocalTexture(GameObject obj, Texture texture)
    {
        MeshRenderer render = obj.GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        render.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", texture);
        render.SetPropertyBlock(mpb);
    }


    #endregion

}
