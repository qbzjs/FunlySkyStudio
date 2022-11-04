using UnityEngine.UI;

public class SettingTwoSliderItem : SettingItem
{
    private Slider firstSlider;
    private Text firstValueText;
    private Text firstTitle;
    private Slider secondSlider;
    private Text secondValueText;
    private Text secondTitle;
    public override void FindViews()
    {
        base.FindViews();
        firstSlider = transform.Find("First/Slider").GetComponent<Slider>();
        firstValueText = transform.Find("First/Value").GetComponent<Text>();
        firstTitle = transform.Find("First/Title").GetComponent<Text>();
        secondSlider = transform.Find("Second/Slider").GetComponent<Slider>();
        secondValueText = transform.Find("Second/Value").GetComponent<Text>();
        secondTitle = transform.Find("Second/Title").GetComponent<Text>();
    }

    public override void Init(SettingItemData settingItemData)
    {
        base.Init(settingItemData);
        if (settingItemData is SettingTwoSliderItemData data)
        {
            LocalizationConManager.Inst.SetLocalizedContent(firstTitle, data.firstTitle);
            firstSlider.minValue = data.firstMinValue;
            firstSlider.maxValue = data.firstMaxValue;
            firstSlider.value = data.firstDefaultValue;
            firstValueText.text = ((int)data.firstDefaultValue).ToString();
            firstSlider.onValueChanged.AddListener((v) =>
            {
                var value = (int)v;
                firstValueText.text = value.ToString();
                data.firstValueChange(value);
            });
            LocalizationConManager.Inst.SetLocalizedContent(secondTitle, data.secondTitle);
            secondSlider.minValue = data.secondMinValue;
            secondSlider.maxValue = data.secondMaxValue;
            secondSlider.value = data.secondDefaultValue;
            secondValueText.text = ((int)data.secondDefaultValue).ToString();
            secondSlider.onValueChanged.AddListener((v) =>
            {
                var value = (int)v;
                secondValueText.text = value.ToString();
                data.secondValueChange(value);
            });
        }
    }
}