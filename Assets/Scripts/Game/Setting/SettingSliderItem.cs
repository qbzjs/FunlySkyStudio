using UnityEngine.UI;

public class SettingSliderItem : SettingItem
{
    private Slider slider;
    private Text valueText;
    public override void FindViews()
    {
        base.FindViews();
        slider = transform.Find("Slider").GetComponent<Slider>();
        valueText = transform.Find("Value").GetComponent<Text>();
    }

    public override void Init(SettingItemData settingItemData)
    {
        base.Init(settingItemData);
        if (settingItemData is SettingSliderItemData data)
        {
            slider.minValue = data.minValue;
            slider.maxValue = data.maxValue;
            slider.value = data.defaultValue;
            SetValue(data.defaultValue);
            slider.onValueChanged.AddListener((v) =>
            {
                SetValue(v);
                data.OnValueChange(v);
            });
        }
    }

    public void SetValue(float v)
    {
        var num = (int)(((v - slider.minValue) / (slider.maxValue - slider.minValue)) * 100);
        valueText.text = num.ToString();
    }
}