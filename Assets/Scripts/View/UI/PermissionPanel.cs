/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/5/6 15:4:31
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PermissionPanel : BasePanel<PermissionPanel>
{
    public Button sitBtn;
    public Button cancelBtn;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        sitBtn.onClick.AddListener(()=> {
            MobileInterface.Instance.OpenNativeSettingsPage();
            Hide();
        });
        cancelBtn.onClick.AddListener(Hide);
    }
}
