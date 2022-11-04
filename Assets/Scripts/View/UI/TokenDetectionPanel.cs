using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using GRTools.Localization;
using System.Text;

/// <summary>
/// Author:WenJia
/// Description: VIP 区域 DC token 检测弹窗界面
/// Date: 2022/10/8 14:08:44
/// </summary>

public class TokenDetectionPanel : BasePanel<TokenDetectionPanel>
{
    public Button closeArea;
    public RawImage dcIcon;
    public Text dcName;
    public Text detectTips;
    public Animator undersideAnim, identification_zoneAnim;
    public GameObject correctNode, mistakeNode;
    private Coroutine GetPhotoCor;
    private string normalTips = "You could only enter the VIP Zone if you own this DC token.";
    private string noWalletTips = "You could not own a DC Token without a wallet. Please create or import it if you have one.";
    private string successTips = "You have the DC token, the door of the VIP zone has been opened for you, have a good journey!";
    private string failTips = "You don't have the DC token, go buy one and try again.";
    private bool isCanClose = false;
    public bool IsCanShowResult  = false;
    private int widthLimit = 520;
    private int textLimit = 20;
    private BudTimer timeoutTimer, delayCloseTimer, hideTimer, showResultTimer;
    

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        closeArea.onClick.AddListener(OnClickClose);
    }

    public override void OnDialogBecameVisible() {
        base.OnDialogBecameVisible();
        isCanClose = false;
        IsCanShowResult  = false;

        //此前若打开过表情UIpanel，关闭表情界面
        if (EmoMenuPanel.Instance && EmoMenuPanel.Instance.gameObject.activeSelf)
        {
            EmoMenuPanel.Hide();
            PlayModePanel.Instance.EmoMenuPanelBecameVisible(false);
        }

        VIPZoneManager.Inst.PlayVIPAreaSound("Play_VipArea_Scanning_ScreenAppears", gameObject);
        InitPanel();
        ClearTimers();
        timeoutTimer = TimerManager.Inst.RunOnce("timeout", 5f, () =>
        {
            isCanClose = true;
            OnClickClose();
        });

        showResultTimer = TimerManager.Inst.RunOnce("showResult", 3, () =>
        {
            ShowDetectResult(VIPZoneManager.Inst.resultState);
        });
    }

    public void InitPanel()
    {
        SetDetectTips(normalTips);
        correctNode.SetActive(false);
        mistakeNode.SetActive(false);
        dcIcon.gameObject.SetActive(false);
        dcName.text = "";
    }

    public void SetDCInfo(string coverUrl, string name)
    {
        var icon = VIPZoneManager.Inst.GetDCIcon(coverUrl);
        if(icon == null)
        {
            GetPhotoCor = StartCoroutine(LoadSprite(coverUrl, dcIcon));
        }
        else
        {
            dcIcon.texture = icon;
            dcIcon.gameObject.SetActive(true);
        }
        
        LocalizationConManager.Inst.SetLocalizedContent(dcName, name);
        var nameText = dcName.preferredWidth > widthLimit ? name.Substring(0, textLimit) + "..." : name;
        dcName.text = nameText;
    }

    private void SetDetectTips(string tips)
    {
        LocalizationConManager.Inst.SetLocalizedContent(detectTips, tips);
    }

    IEnumerator LoadSprite(string url, RawImage image)
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (wr.result == UnityWebRequest.Result.Success)
        {
            image.texture = texDl.texture;
            dcIcon.gameObject.SetActive(true);
            VIPZoneManager.Inst.AddDCIcon(url, texDl.texture);
        }
        else
        {
            LoggerUtils.LogError("OnLoadSpriteFail !");
        }
        texDl.Dispose();
        wr.Dispose();
    }

    public void OnClickClose()
    {
        if (!isCanClose)
        { 
            return; 
        }
        undersideAnim.Play("token_gated_end");
        identification_zoneAnim.Play("identification_zone_end");
        VIPZoneManager.Inst.PlayVIPAreaSound("Play_VipArea_Scanning_ScreenExit", gameObject);
        VIPZoneManager.Inst.ExitDetectMode();
        VIPZoneManager.Inst.SetDetectEffectVisible(VIPZoneManager.Inst.selfId, false);
        ClearTimers();
        hideTimer = TimerManager.Inst.RunOnce("hideTimer", 0.5f, () =>
        {
            Hide();
        });
        
    }

    public void ShowDetectResult(int state)
    {
        if(state == (int) DETECTION_RESULT.NONE)
        {
            IsCanShowResult  = true;
            return;
        }
        bool isSuccess = state == (int)DETECTION_RESULT.SUCCESS;
        mistakeNode.SetActive(!isSuccess);
        correctNode.SetActive(isSuccess);
        if(isSuccess)
        {
            SetDetectTips(successTips);
        }else if(state == (int)DETECTION_RESULT.FAIL)
        {
            SetDetectTips(failTips);
        }
        else
        {
            SetDetectTips(noWalletTips);
        }
        var name = isSuccess ? "Play_VipArea_Scanning_Success" : "Play_VipArea_Scanning_Fail";
        VIPZoneManager.Inst.PlayVIPAreaSound(name, gameObject);
        VIPZoneManager.Inst.SetDetectEffectVisible(VIPZoneManager.Inst.selfId, false);
        isCanClose = true;
        ClearTimers();
        delayCloseTimer = TimerManager.Inst.RunOnce("delayClose", 3f, () =>
        {
            OnClickClose();
        });
    }

    public void ClearTimers()
    {
        TimerManager.Inst.Stop(timeoutTimer);
        TimerManager.Inst.Stop(delayCloseTimer);
        TimerManager.Inst.Stop(hideTimer);
        TimerManager.Inst.Stop(showResultTimer);
    }
}
