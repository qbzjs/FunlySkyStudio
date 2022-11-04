using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PortalGateAnimPanel:BasePanel<PortalGateAnimPanel>
{
    public RawImage mapImage;
    public GameObject loadGo;
    public Image blackImage;
    private Texture2D screenShot;
    private UnityAction shotClick;
    private UnityAction blackClick;
    public void StartShow(UnityAction callback)
    {
        shotClick = callback;
        StartCoroutine("GetSceneShot");
    }

    private IEnumerator GetSceneShot()
    {
        loadGo.SetActive(true);
        mapImage.enabled = false;
        blackImage.color = new Color(0, 0, 0, 0);
        yield return new WaitForEndOfFrame();
        try
        {
            screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenShot.Apply();
            loadGo.SetActive(false);
            mapImage.texture = screenShot;
            mapImage.enabled = true;
            shotClick?.Invoke();
        }
        catch
        {
            loadGo.SetActive(false);
            TipPanel.ShowToast("Try again:(");
            Hide();
        }
    }


    public void StartBlackAnim(bool mapVisible, UnityAction callback)
    {
        loadGo.SetActive(false);
        mapImage.enabled = mapVisible;
        blackImage.color = new Color(0, 0, 0, 0);
        blackClick = callback;
        DOTween.ToAlpha(() => blackImage.color, x => blackImage.color = x, 1, 0.5f).SetTarget(blackImage).OnComplete(() =>
        {
            mapImage.enabled = false;
            mapImage.texture = null;
            blackClick?.Invoke();
            if (mapVisible)
            {
                SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(true);
                FPSController.Inst.StartCollectFPS();
            }
            DOTween.ToAlpha(() => blackImage.color, x => blackImage.color = x, 0, 0.5f).SetTarget(blackImage).OnComplete(() =>
            {
                if (mapVisible)
                {
                    var fps = FPSController.Inst.GetAverageFPS();
                    SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(fps > GameConsts.averageFPS);
                }
                Destroy(screenShot);
                blackClick = null;
                Hide();
            });
        });
    }
}