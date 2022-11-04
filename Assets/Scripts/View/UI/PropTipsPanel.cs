using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropTipsPanel : BasePanel<PropTipsPanel>, IPanelOpposable
{
    public Button exitBtn;
    public Button tipsBtn;
    public Button infoBtn;
    public GameObject TipsPanel;
    public Text TextTip;
    public Text TextDes;
    BasePanel panel;

    private string tips = "";
    private string des = "";
    private bool isGlobalHide;
    private CoToggle tipsTog;
    private CoToggle infoTog;
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        tipsBtn.onClick.AddListener(() =>
        {
            SetPanelActive(true);
            UpdateBtnState(tipsBtn, false);
            UpdateBtnState(infoBtn, true);
            isGlobalHide = true;
        });
        exitBtn.onClick.AddListener(() =>
        {
            UpdateBtnState(tipsBtn, true);
            UpdateBtnState(infoBtn, true);
            TipsPanel.SetActive(false);
        });
        infoBtn.onClick.AddListener(() =>
        {
            bool ignoreTipsPanel = false;
            if (!isGlobalHide == true)
            {
                ignoreTipsPanel = true;
                TipsPanel.SetActive(false);
            }
            SetPanelActive(!isGlobalHide, ignoreTipsPanel);
            isGlobalHide = !isGlobalHide;
            UpdateBtnState(infoBtn, isGlobalHide);
            UpdateBtnState(tipsBtn, true);
        });
        UpdateBtnState(infoBtn, false);
    }

    public override void OnDialogBecameVisible()
    {
        TipsPanel.SetActive(false);
        UpdateBtnState(tipsBtn, true);
    }

    private void SetPanelActive(bool isActive, bool ignoreTipsPanel = false)
    {
        if (!ignoreTipsPanel)
        {
            TipsPanel.SetActive(isActive);
        }

        if (panel != null)
        {
            panel.gameObject.SetActive(!isActive);
        }
    }

    public void SetGlobalHide(bool isActive)
    {
       isGlobalHide = isActive;

        UpdateBtnState(infoBtn, true);
    }

    public void UpdateBtnState(Button btn, bool isActive)
    {
        var bg = btn.transform.Find("bg");
        var select = btn.transform.Find("select");
        bg.gameObject.SetActive(isActive);
        select.gameObject.SetActive(!isActive);
    }

    public void SetTipsInfo(BasePanel panel)
    {
        this.panel = panel;
        tipsBtn.gameObject.SetActive(false);
        infoBtn.gameObject.SetActive(true);
        if (isGlobalHide&& panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }

    public void SetTipsInfo(string tips, string des, BasePanel panel = null)
    {
        this.tips = tips;
        this.des = des;
        LocalizationConManager.Inst.SetLocalizedContent(TextTip, tips);
        LocalizationConManager.Inst.SetLocalizedContent(TextDes, des);
        this.panel = panel;
        TipsOrInfoPanel();
        if (isGlobalHide&& panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void TipsOrInfoPanel()
    {
        if (panel != null)
        {
            tipsBtn.gameObject.SetActive(true);
            infoBtn.gameObject.SetActive(true);
        }
        else
        {
            tipsBtn.gameObject.SetActive(true);
            infoBtn.gameObject.SetActive(false);
        }

    }
}

public interface IPanelOpposable
{
    void SetGlobalHide(bool isActive);
}
