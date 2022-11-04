using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MToggle : MonoBehaviour
{
    public bool isOn;
    private Button button;
    public Button ToggleBtn
    {
        get
        {
            if (!button) button = GetComponent<Button>();
            return button;
        }
    }

    public void SetIsOn(bool isSelected)
    {
        Transform checkImg = this.gameObject.transform.Find("Background/Checkmark");
        if(checkImg)
        {
            checkImg.gameObject.SetActive(isSelected);
        }
        
        isOn = isSelected;
    }
}