using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CoToggle : MonoBehaviour
{
    [SerializeField]
    private CoToggleGroup group;
    [SerializeField]
    private bool isOn;
    private Button button;
    public Button TargetButton
    {
        get
        {
            if (!button) button = GetComponent<Button>();
            return button;
        }
    }

 
    public bool IsOn
    {
        get => isOn;
        set
        {
            isOn = value;
            TargetButton.interactable = !isOn;
            if (isOn && group)
            {
                group.TurnOn(this);
            }
        }
    }

   
}
