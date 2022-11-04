using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingFiveChooseItem : SettingItem
{
    private Text[] chooseTexts;
    private Button[] chooseBtns;
    private Image[] chooseImgs;
    public Action<int> OnChooseChange;
    private static Color COLOR_UNSELECT = new Color(0.6196f, 0.6196f, 0.6196f, 1);
    private static Color COLOR_SELECT = new Color(0.1960f, 0.1960f, 0.1960f, 1);
    private Sprite selectBack;
    private Sprite unselectBack;

    public override void FindViews()
    {
        base.FindViews();
        chooseTexts = new Text[5];
        chooseBtns = new Button[5];
        chooseImgs = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            chooseTexts[i] = transform.Find($"Choose{i+1}/Content").GetComponent<Text>();
            chooseImgs[i] = transform.Find($"Choose{i+1}/Back").GetComponent<Image>();
            chooseBtns[i] = transform.Find($"Choose{i+1}").GetComponent<Button>();
            int index = i;
            chooseBtns[i].onClick.AddListener(() => { OnButtonClick(index); });
        }
    }

    private void OnButtonClick(int i)
    {
        if (GetSelectedIndex() == i)
        {
            return;
        }
        SetSelected(i);
        OnChooseChange?.Invoke(i);
    }

    private int GetSelectedIndex()
    {
        for (int i = 0; i < chooseTexts.Length; i++)
        {
            if (chooseTexts[i].color == COLOR_SELECT)
            {
                return i;
            }
        }
        return -1;
    }

    public override void Init(SettingItemData settingItemData)
    {
        base.Init(settingItemData);
        selectBack = ResManager.Inst.GetGameAtlasSprite("btn_selected");
        unselectBack = ResManager.Inst.GetGameAtlasSprite("btn_unselected");
        if (settingItemData is SettingFiveChooseData data)
        {
            for (int i = 0; i < chooseTexts.Length; i++)
            {
                LocalizationConManager.Inst.SetLocalizedContent(chooseTexts[i], data.choose[i]);
            }
            SetSelected(data.defaultChoose);
            OnChooseChange = data.OnChooseChange;
        }
    }

    private void SetSelected(int selectIndex)
    {
        for (int i = 0; i < 5; i++)
        {
            chooseImgs[i].sprite = selectIndex == i ? selectBack : unselectBack;
            chooseTexts[i].color = selectIndex == i ? COLOR_SELECT : COLOR_UNSELECT;
        }
    }
}