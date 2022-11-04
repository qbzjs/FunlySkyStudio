using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Author:Shaocheng
/// Description:降落伞开伞UI
/// Date: 2022-8-8 18:31:57
/// </summary>
public class ParachuteCtrlPanel : BasePanel<ParachuteCtrlPanel>
{
    public Button btnOpenParachute;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnOpenParachute.onClick.AddListener(OnOpenParachuteBtnClicked);
    }

    public override void OnDialogBecameVisible()
    {
        
    }

    private void OnOpenParachuteBtnClicked()
    {
        LoggerUtils.Log("OnOpenParachuteBtnClicked");

        if (PlayerParachuteControl.Inst)
        {
            PlayerParachuteControl.Inst.OpenParachute();
        }
    }
}