using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ColorListItem : MonoBehaviour
{
    public Button mainBtn;
    public GameObject lastBtn;
    public GameObject nextBtn;
    public Button delBtn;
    public GameObject disactiveIcon;
    public Color CurColor { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsSelected { get; private set; } = false;

    public void Select()
    {
        //SelectWithoutNotify(showLast, showNext);
        if (IsActive && !IsSelected)
        {
            mainBtn.onClick.Invoke();
        }      
    }

    public void OnSelect(bool showLast = true, bool showNext = true)
    {
        IsSelected = true;
        mainBtn.interactable = false;
        delBtn?.gameObject.SetActive(true);
        lastBtn?.SetActive(showLast);
        nextBtn?.SetActive(showNext);
    }

    public void Diselect()
    {
        IsSelected = false;
        mainBtn.interactable = true;
        delBtn?.gameObject.SetActive(false);
        lastBtn?.SetActive(false);
        nextBtn?.SetActive(false);        
    }

    public void SetActive(bool isOn)
    {
        IsActive = isOn;
        if (!isOn)
        {
            Diselect();
        }
        mainBtn.gameObject.SetActive(isOn);
        mainBtn.interactable = isOn;
        disactiveIcon.SetActive(!isOn);
    }

    public void SetColor(Color color)
    {
        CurColor = color;
        (mainBtn.targetGraphic as Image).color = color;
    }


    public void AddSelectListener(UnityAction action)
    {
        mainBtn.onClick.AddListener(action);
    }

    public void AddNextListener(UnityAction action)
    {
        nextBtn?.GetComponentInChildren<Button>()?.onClick.AddListener(action);
    }

    public void AddLastListener(UnityAction action)
    {
        lastBtn?.GetComponentInChildren<Button>()?.onClick.AddListener(action);
    }

    public void AddDelListener(UnityAction action)
    {
        delBtn?.onClick.AddListener(action);
    }

}
