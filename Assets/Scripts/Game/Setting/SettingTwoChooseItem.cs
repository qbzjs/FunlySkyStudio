using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingTwoChooseItem : SettingItem
{
    private Text choose1Text;
    private Text choose2Text;
    private Button choose1Btn;
    private Button choose2Btn;
    private Image choose1Img;
    private Image choose2Img;
    public Action<int> OnChooseChange;
    private static Color COLOR_UNSELECT = new Color(0.6196f, 0.6196f, 0.6196f, 1);
    private static Color COLOR_SELECT = new Color(0.1960f, 0.1960f, 0.1960f, 1);
    private SettingTwoChooseItemData data;

    public override void FindViews()
    {
        base.FindViews();
        choose1Text = transform.Find("Choose1/Content").GetComponent<Text>();
        choose1Img = transform.Find("Choose1/Back").GetComponent<Image>();
        choose1Btn = transform.Find("Choose1").GetComponent<Button>();
        choose2Text = transform.Find("Choose2/Content").GetComponent<Text>();
        choose2Img = transform.Find("Choose2/Back").GetComponent<Image>();
        choose2Btn = transform.Find("Choose2").GetComponent<Button>();
        choose1Btn.onClick.AddListener(() => { OnButtonClick(0); });
        choose2Btn.onClick.AddListener(() => { OnButtonClick(1); });
    }

    private void OnButtonClick(int i)
    {
        if (GetSelectedIndex() == i)
        {
            return;
        }
        //拦截当前想要选择的选项，有可能现在的逻辑不允许选择当前选项
        if (data.intercept != null)
        {
            SettingTwoItemInterceptData interceptData = new SettingTwoItemInterceptData()
            {
                src = GetSelectedIndex(),
                toSet = i
            };
            if (data.intercept(interceptData))
            {
                return;
            }
        }
        SetSelected(i);
        OnChooseChange?.Invoke(i);
    }

    private int GetSelectedIndex()
    {
        if (choose1Text.color == COLOR_SELECT)
        {
            return 0;
        }

        return 1;
    }

    public override void Init(SettingItemData settingItemData)
    {
        base.Init(settingItemData);
        if (settingItemData is SettingTwoChooseItemData data)
        {
            LocalizationConManager.Inst.SetLocalizedContent(choose1Text, data.firstChoose);
            LocalizationConManager.Inst.SetLocalizedContent(choose2Text, data.secondChoose);
            SetSelected(data.defaultChoose);
            OnChooseChange = data.OnChooseChange;
            this.data = data;
        }
    }

    public void SetSelected(int selectIndex)
    {
        if (selectIndex == 0)
        {
            choose1Img.sprite = ResManager.Inst.GetGameAtlasSprite("btn_selected");
            choose2Img.sprite = ResManager.Inst.GetGameAtlasSprite("btn_unselected");
            choose1Text.color = COLOR_SELECT;
            choose2Text.color = COLOR_UNSELECT;
        }
        else if (selectIndex == 1)
        {
            choose1Img.sprite = ResManager.Inst.GetGameAtlasSprite("btn_unselected");
            choose2Img.sprite = ResManager.Inst.GetGameAtlasSprite("btn_selected");
            choose1Text.color = COLOR_UNSELECT;
            choose2Text.color = COLOR_SELECT;
        }
    }

    public class SettingTwoItemInterceptData
    {
        public int src;
        public int toSet;
    }
}