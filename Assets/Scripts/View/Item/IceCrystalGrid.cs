using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IceCrystalGrid : MonoBehaviour
{
    [SerializeField] private Image gridImg;
    [SerializeField] private GameObject icyCrystal;
    [SerializeField] private GameObject showIceCrystalEffect;
    [SerializeField] private GameObject collectCompleteEffect;
    [SerializeField] private ParticleSystemListener particleListener;

    private Tweener rotationTween;
    private Tweener moveTween;
    private Tweener scaleTween;
    private Action playAniComplete;

    #region public
    /// <summary>
    /// 显示冰晶
    /// </summary>
    /// <param name="isShowCrystal"></param>
    public void ShowIceCrystal(bool isShowCrystal, bool isShowEffect = true, Action callback = null)
    {
        showIceCrystalEffect.SetActive(isShowEffect);
        icyCrystal.SetActive(isShowCrystal);
        gridImg.enabled = !isShowCrystal;
        particleListener.CompleteAction = callback;
    }

    /// <summary>
    /// 播放收集完成动画
    /// </summary>
    /// <param name="desPos"></param>
    /// <param name="desScale"></param>
    /// <param name="duration"></param>
    /// <param name="aniComplete"></param>
    public void PlayCollectedCompleteAni(Vector3 desPos, float desScale, float duration, Action aniComplete)
    {
        this.playAniComplete = aniComplete;
        collectCompleteEffect.SetActive(true);

        int randomNum = UnityEngine.Random.Range(0, 2);
        if (randomNum == 0)
            randomNum--;
        rotationTween = transform.DORotate(new Vector3(0, 0, 20f * randomNum), 0.1f).SetLoops(2, LoopType.Yoyo).Play().OnComplete(() =>
        {
            rotationTween = transform.DORotate(new Vector3(0, 0, -20f * randomNum), 0.1f).SetLoops(2, LoopType.Yoyo).Play().OnComplete(() =>
            {
                scaleTween = transform.DOScale(Vector3.one * desScale, duration).Play();
                moveTween = transform.DOMove(desPos, duration).Play().OnComplete(() => playAniComplete?.Invoke());
            });
        });

    }
    #endregion

    private void OnDisable()
    {
        showIceCrystalEffect.SetActive(false);
        collectCompleteEffect.SetActive(false);

        if (rotationTween != null)
        {
            rotationTween.Kill();
            rotationTween = null;
            if (moveTween != null)
            {
                moveTween.Kill();
                moveTween = null;
                scaleTween.Kill();
                scaleTween = null;
            }

            // playAniComplete?.Invoke();
            playAniComplete = null;
        }
    }
}
