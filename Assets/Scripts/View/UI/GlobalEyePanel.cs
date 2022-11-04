using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalEyePanel : BasePanel<GlobalEyePanel>
{
    public Button ShowEditBtn;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        ShowEditBtn.onClick.AddListener(OnShowEdit);
    }

    private void OnShowEdit()
    {
        Hide();
        UIManager.Inst.uiCanvas.gameObject.SetActive(true);
        if (ReferManager.Inst.isRefer)
        {
            ReferPanel.Instance.OnReferMode();
        }
    }
}
