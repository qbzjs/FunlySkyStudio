using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropLittleTipsPanel : BasePanel<PropLittleTipsPanel>
{
    public Button exitBtn;
    public Button tipsBtn;

    public GameObject TipsPanel;

    public Text TextDes;

    private string des = "";

    private void Start()
    {
        tipsBtn.onClick.AddListener(() => {
            SetTipsPanelActive(true);
        });
        exitBtn.onClick.AddListener(() => {
            SetTipsPanelActive(false);
        });
    }

    public override void OnDialogBecameVisible()
    {
        TipsPanel.SetActive(false);
    }

    private void SetTipsPanelActive(bool isActive)
    {
        TipsPanel.SetActive(isActive);
    }


    public void SetTipsInfo( string des)
    {
        this.des = des;
        TextDes.text = des;
    }
}
