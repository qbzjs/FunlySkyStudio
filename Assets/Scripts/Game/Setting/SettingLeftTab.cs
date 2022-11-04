using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class SettingLeftTab : MonoBehaviour
{
    private Text content;
    private Transform selected;
    public Action<bool> StatusChange;
    public Action OnClick;
    private static Color colorSelected = new Color(1,1,1);
    private static Color colorUnselected = new Color(0.6196f,0.6196f,0.6196f);
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            OnClick?.Invoke();
        });
        content = transform.Find("Text").GetComponent<Text>();
        content.color = colorUnselected;
        selected = transform.Find("Selected");
        selected.gameObject.SetActive(false);
    }

    public void Init(string text)
    {
        LocalizationConManager.Inst.SetLocalizedContent(content, text);
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected() == selected)
        {
            return;
        }
        this.selected.gameObject.SetActive(selected);
        content.color = selected ? colorSelected : colorUnselected;
        StatusChange(selected);
    }

    public bool IsSelected()
    {
        return selected.gameObject.activeSelf;
    }
}