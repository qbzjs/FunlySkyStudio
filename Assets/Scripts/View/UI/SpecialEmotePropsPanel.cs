using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.U2D;

public enum SpecialEmotePropsType
{
    sword = 1,
}

public class SpecialEmotePropsPanel: BasePanel<SpecialEmotePropsPanel>
{
    public Text titel;
    public Transform content;
    public Button doneBtn;
    public Button closeBtn;
    public Button closeArea;
    public SpecialEmotePropsType specialEmoteType;
    public GameObject itemPrefab;


    public virtual void Init(string titelName,Action doneAct, SpecialEmotePropsType specialEmoteType)
    {
        LocalizationConManager.Inst.SetLocalizedContent(titel, titelName);
        doneBtn.onClick.RemoveAllListeners();
        doneBtn.onClick.AddListener(()=> {
            doneAct?.Invoke();
            UIControlManager.Inst.CallUIControl("special_emote_exit");
        });
        closeBtn.onClick.AddListener(OnCloseBtnClick);
        closeArea.onClick.AddListener(OnCloseBtnClick);
        this.specialEmoteType = specialEmoteType;
    }

    public SpecialEmotePropItem LoadProp(RoleIconData data, BundlePart part)
    {
        if(data == null)
        {
            return null;
        }
        GameObject item = Instantiate(itemPrefab, content);
        SpecialEmotePropItem itemScr = item.GetComponent<SpecialEmotePropItem>();
        if (itemScr != null)
        {
            LoadImage(itemScr.icon, data.spriteName, part);
            itemScr.selectImg.gameObject.SetActive(false);
        }
        return itemScr;
    }

    private void LoadImage(Image icon, string imageName, BundlePart part)
    {
        SpriteAtlas Atlas = null;
        switch (part)
        {
            case BundlePart.Bag:
                Atlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/Bag");
                break;
            case BundlePart.Hand:
                Atlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/Hand");
                break;
        }
        RoleConfigDataManager.Inst.SetAvatarIcon(icon, imageName, Atlas, (state) => icon.gameObject.SetActive(state == ImgLoadState.Complete));
    }

    private void OnCloseBtnClick()
    {
        Hide();
        UIControlManager.Inst.CallUIControl("special_emote_exit");
    }
}
