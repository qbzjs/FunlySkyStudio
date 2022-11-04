using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleColorAdjustView : RoleAdjustView
{
    public GameObject ColorView;

    public override void OnResetClick()
    {
        base.OnResetClick();
        ColorView.GetComponentInChildren<RoleColorView>().SetSelect(color);
    }

    public void ShowColorView(bool isActive)
    {
            GetComponent<RectTransform>().offsetMax = isActive == true ? new Vector2(0, -150f) : new Vector2(0, 0);
            ColorView.transform.GetComponent<RectTransform>().offsetMax=new Vector2(0,155f);
            ColorView.SetActive(isActive);
            if (isActive)
            {
                SetAdjustScale(520f);
            }
            else
            {
                SetAdjustScale(700f);
            }
    }
}
