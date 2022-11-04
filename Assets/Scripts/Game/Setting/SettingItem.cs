using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text;
using Object = UnityEngine.Object;

public abstract class SettingItem : MonoBehaviour
{
    private Text title;
    private Button tips;

    void Awake()
    {
        FindViews();
    }

    public virtual void FindViews()
    {
        title = transform.Find("Title").GetComponent<Text>();
        tips = transform.Find("Title/Tips").GetComponent<Button>();
    }
    
    public virtual void Init(SettingItemData settingItemData)
    {
        LocalizationConManager.Inst.SetLocalizedContent(title, settingItemData.title);
        if (settingItemData.textLength > 0 && settingItemData.widthLimit > 0 && title.preferredWidth > settingItemData.widthLimit)
        {
            var localText = GameUtils.SubStringByBytes(title.text, settingItemData.textLength, Encoding.Unicode);
            title.text = localText;
        }
        if (string.IsNullOrEmpty(settingItemData.tips))
        {
            tips.gameObject.SetActive(false);
            return;
        }
        tips.gameObject.SetActive(true);
        tips.onClick.AddListener(() =>
        {
            SettingTipsPanel.Show();
            SettingTipsPanel.Instance.AdjustTipsPosition(settingItemData.tips,title.transform);
        });
    }
}


public abstract class SettingItemData
{
    public string title;
    public string tips;
    public int textLength = 0;
    public float widthLimit = 0;
    public Func<System.Object,bool> intercept;
}

public class SettingTwoChooseItemData : SettingItemData
{
    public string firstChoose;
    public string secondChoose;
    public int defaultChoose;
    public Action<int> OnChooseChange;
}

public class SettingFiveChooseData : SettingItemData
{
    public string[] choose;
    public int defaultChoose;
    public Action<int> OnChooseChange;
}

public class SettingSliderItemData : SettingItemData
{
    public float minValue;
    public float maxValue;
    public float defaultValue;
    public UnityAction<float> OnValueChange;
}

public class SettingTwoSliderItemData : SettingItemData
{
    public string firstTitle;
    public float firstMinValue;
    public float firstMaxValue;
    public float firstDefaultValue;
    public UnityAction<float> firstValueChange;
    
    public string secondTitle;
    public float secondMinValue;
    public float secondMaxValue;
    public float secondDefaultValue;
    public UnityAction<float> secondValueChange;
}