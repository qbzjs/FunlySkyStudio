using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Author:Shaocheng
/// Description:显示在UI上方的提示 在此需求中新增：https://pointone.feishu.cn/docs/doccnJmCJNVMOEBedaadxcPdn4R
/// Date: 2022-7-5 11:03:59
/// </summary>
public class TipNotifyPanel : BasePanel<TipNotifyPanel>
{
    private Animator anim;
    private Text title;
    private RawImage profile;
    private bool isOnShow = false;

    private Coroutine loadProfileCor;
    private Dictionary<string, Texture> profileCache = new Dictionary<string, Texture>();
    private UnityAction hideCallback;

    private void OnHide()
    {
        isOnShow = false;
        anim.enabled = false;
        if (loadProfileCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(loadProfileCor);
            loadProfileCor = null;
        }

        if (hideCallback != null)
        {
            hideCallback.Invoke();
        }
        hideCallback = null;

        Hide();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        profileCache.Clear();
        hideCallback = null;
    }

    private void ControlProfileShow(bool isShow, string playerId)
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            profile.gameObject.SetActive(false);
            return;
        }

        if (isShow && !string.IsNullOrEmpty(playerId))
        {
            if (loadProfileCor != null)
            {
                CoroutineManager.Inst.StopCoroutine(loadProfileCor);
                loadProfileCor = null;
            }

            //若未来有在unity场景更换头像的功能，此Cache将不再适用。
            if (profileCache != null && profileCache.ContainsKey(playerId) && profileCache[playerId] != null)
            {
                profile.texture = profileCache[playerId];
                profile.gameObject.SetActive(true);
            }
            else
            {
                UserInfo syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playerId);
                if (syncPlayerInfo == null)
                {
                    profile.gameObject.SetActive(false);
                    return;
                }

                if (!string.IsNullOrEmpty(syncPlayerInfo.portraitUrl))
                {
                    profile.gameObject.SetActive(false);
                    profile.texture = null;
                    loadProfileCor = CoroutineManager.Inst.StartCoroutine(LoadSprite(playerId, syncPlayerInfo.portraitUrl, profile));
                }
            }
        }
        else
        {
            profile.gameObject.SetActive(false);
        }
    }

    IEnumerator LoadSprite(string playerId, string url, RawImage image)
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (!wr.isNetworkError)
        {
            image.gameObject.SetActive(true);
            image.texture = texDl.texture;

            if (profileCache != null && !profileCache.ContainsKey(playerId))
            {
                profileCache.Add(playerId, image.texture);
            }
        }

        texDl.Dispose();
        wr.Dispose();
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        title = this.GetComponentInChildren<Text>();
        anim = this.transform.GetComponentInChildren<Animator>();
        profile = this.transform.GetComponentInChildren<RawImage>();
        anim.enabled = false;
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        if (isOnShow)
            return;
        isOnShow = true;
        anim.enabled = true;
        anim.Play("ToastNotifyAnim", 0, 0);
        CancelInvoke("OnHide");
        Invoke("OnHide", 1.4f);
    }

    public void SetTitle(string content, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(title, content, formatArgs);
    }

    public static void SetHideCallback(UnityAction cb)
    {
        if (cb != null && TipNotifyPanel.Instance)
        {
            TipNotifyPanel.Instance.hideCallback = cb;
        }
    }

    //显示带玩家头像的Tip
    public static void ShowToastWithPlayer(string playerId, string content, params object[] formatArgs)
    {
        TipNotifyPanel.Show(true);
        TipNotifyPanel.Instance.SetTitle(content, formatArgs);
        TipNotifyPanel.Instance.transform.SetAsLastSibling();
        TipNotifyPanel.Instance.ControlProfileShow(true, playerId);
    }

    //不带玩家头像的Tip
    public static void ShowToast(string content, params object[] formatArgs)
    {
        TipNotifyPanel.Show(true);
        TipNotifyPanel.Instance.SetTitle(content, formatArgs);
        TipNotifyPanel.Instance.transform.SetAsLastSibling();
        TipNotifyPanel.Instance.ControlProfileShow(false, string.Empty);
    }

}