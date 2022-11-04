using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class HalfConfirmBoxPanel : BasePanel<HalfConfirmBoxPanel>
{
    public Text title;
    public Text content;
    
    public void Init(string tit, string con)
    {
        LocalizationConManager.Inst.SetSystemTextFont(title);
        title.SetTextWithEllipsis(LocalizationConManager.Inst.GetLocalizedText(tit));
        LocalizationConManager.Inst.SetSystemTextFont(content);
        content.SetTextWithEllipsis(LocalizationConManager.Inst.GetLocalizedText(con));
    }
    
    public void OnBtnOK()
    {
        Hide();
    }
}
