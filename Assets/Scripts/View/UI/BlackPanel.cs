using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BlackPanel : BasePanel<BlackPanel>
{
    public Image BlackImage;
    public RawImage rawImage;
    public GameObject Black;

    private bool canPlayAnim = true;

    private Sequence panelSequence;

    private Action callBack;
    private Action endCallBack;
    public void PlayTransitionAnim()
    {

        if (panelSequence != null)
        {
            return;
        }
        InitColor();
        SetSibling();
        panelSequence = DOTween.Sequence();
        panelSequence.Append(DOTween.ToAlpha(() => BlackImage.color, x => BlackImage.color = x, 1, 0.5f).SetTarget(BlackImage));
        panelSequence.Append(DOTween.ToAlpha(() => BlackImage.color, x => BlackImage.color = x, 0, 0.8f).SetTarget(BlackImage));
        panelSequence.AppendCallback(() => CallBack());
    }
    
    public void PlayTransitionAnimAct(Action action)
    {
        if (panelSequence != null)
        {
            callBack?.Invoke();
            action?.Invoke();
            callBack = null;
            return;
        }
        InitColor();
        SetSibling();

        callBack = action;
        panelSequence = DOTween.Sequence();
        panelSequence.Append(DOTween.ToAlpha(() => BlackImage.color, x => BlackImage.color = x, 1, 0.5f).SetTarget(BlackImage));
        panelSequence.AppendCallback(() =>
        {
            callBack?.Invoke();
            callBack = null;
        });
        panelSequence.Append(DOTween.ToAlpha(() => BlackImage.color, x => BlackImage.color = x, 0, 0.8f).SetTarget(BlackImage));
        panelSequence.AppendCallback(() => CallBack());
    }

    public void ForceKillTransformAnim()
    {
        callBack = null;
        CallBack();
    }

    public void PlayTransitionAnimAct(Action action,Action endAction)
    {
        if (panelSequence != null)
        {
            callBack?.Invoke();
            action?.Invoke();
            callBack = null;
            return;
        }
        InitColor();
        SetSibling();

        callBack = action;
        endCallBack = endAction;
        panelSequence = DOTween.Sequence();
        panelSequence.Append(DOTween.ToAlpha(() => BlackImage.color, x => BlackImage.color = x, 1, 0.5f).SetTarget(BlackImage));
        panelSequence.AppendCallback(() =>
        {
            callBack?.Invoke();
            callBack = null;
        });
        panelSequence.Append(DOTween.ToAlpha(() => BlackImage.color, x => BlackImage.color = x, 0, 0.8f).SetTarget(BlackImage));
        panelSequence.AppendCallback(() =>
        {
            endCallBack?.Invoke();
            CallBack();
        });
    }

    public void PlayBlackBg()
    {
        SetSibling();
        Black.SetActive(true);
    }

    public void SetSibling()
    {
        int count = Instance.gameObject.transform.parent.childCount;
        Instance.gameObject.transform.SetSiblingIndex(count - 1);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (panelSequence != null)
        {
            panelSequence.Kill();
        }
       
        panelSequence = null;
        callBack = null;
    }
    private void CallBack()
    {
        if (panelSequence != null)
        {
            panelSequence.Kill();
        }
        panelSequence = null;
        Hide();
    }
    private void InitColor()
    {
        BlackImage.color = new Color(0, 0, 0, 0);
       
        rawImage.color = new Color(1, 1, 1, 0);
    }
}
