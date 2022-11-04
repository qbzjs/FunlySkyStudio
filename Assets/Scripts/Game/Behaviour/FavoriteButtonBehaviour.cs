using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class FavoriteButtonBehaviour : NodeBaseBehaviour
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

    private bool isEnterPortalGate = false;

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
        InitCollectState();
    }
    public void OnCollectSuccess(string msg)
    {
        //LoggerUtils.Log("OnLikeSuccess=>" + msg);
        TipPanel.ShowToast("Added to favorites");
        SceneBuilder.Inst.UpdateAllFavoriteButton(1);
    }

    public void OnCollectFailed(string msg)
    {
        //LoggerUtils.Log("OnLikeFailed=>" + msg);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        isCanClick = true;
    }


    public override void OnReset()
    {
        base.OnReset();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }
    private void OnDestroy()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }


    public override void OnRayEnter()
    {
        if (!isCanClick) return;
     //   HighLight(true);
        isEnter = true;
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Favorite);
        PortalPlayPanel.Instance.AddButtonClick(OnClickCollect);
        PortalPlayPanel.Instance.SetTransform(transform);
    }

    public override void OnRayExit()
    {
        if (isCanClick == false) return;
     //   HighLight(false);
        isEnter = false;
        PortalPlayPanel.Hide();
    }

    void OnClickCollect()
    {
        if (!isCanClick) return;
        if (curGameMode == GameMode.Guest)
        {
            DoRequestCollect();
        }
        else
        {
            TipPanel.ShowToast("Collected(playtest)");
            SceneBuilder.Inst.UpdateAllFavoriteButton(1);
        }
    }

    private void DoRequestCollect()
    {
        UpLoadLikeBody upLoadLikeBody = new UpLoadLikeBody
        {
            mapInfo = GlobalFieldController.CurMapInfo.Clone(),
            operationType = 2,
        };
        //LoggerUtils.Log("JsonConvert.SerializeObject(UpLoadLikeBody) = " + JsonConvert.SerializeObject(upLoadLikeBody));
        HttpUtils.MakeHttpRequest("/ugcmap/setLike", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(upLoadLikeBody), OnCollectSuccess, OnCollectFailed);
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
            if (oldColor != null && oldColor.Length == renderers.Length)
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
    public void InitCollectState()
    {
        if (GlobalFieldController.CurMapInfo != null && GlobalFieldController.CurMapInfo.interactStatus.isCollect == 1)
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
    private void OnChangeMode(GameMode mode)
    {
        curGameMode = mode;
        if (mode == GameMode.Edit)
            SceneBuilder.Inst.UpdateAllFavoriteButton(0);
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
}
