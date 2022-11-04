/// <summary>
/// Author:LiShuZhan
/// Description:背包item界面，负责处理item的表现逻辑
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum BaggageItemType
{
    none = -1,
}
public class BaggageItem : MonoBehaviour
{
    public Image selectImg;
    public Button itemBtn;
    public RawImage icon;
    public Image baseIcon;
    public GameObject loading;
    public GameObject lostTips;
    public int uid;
    public Action<BaggageItem> btnAct;
    public GameItemLongPress longPressBtn;
    public Image mask;
    private Animation maskAnim;
    public void Init()
    {
        ResetInfo();
        itemBtn.onClick.AddListener(OnitemBtnClick);
        btnAct = BaggageManager.Inst.OnChangeItemClick;
        longPressBtn.OnLongPress = OnLongPressItem;
        longPressBtn.playMaskAnim = MaskAnim;
        longPressBtn.closeMaskAnim = CloseMaskAnim;
        longPressBtn.btnType = ItemBtnType.BaggageItem;
        maskAnim = mask.GetComponent<Animation>();
    }

    public void ResetInfo()
    {
        selectImg.gameObject.SetActive(false);
        icon.gameObject.SetActive(false);
        icon.texture = null;
        baseIcon.gameObject.SetActive(false);
        lostTips.gameObject.SetActive(false);
        baseIcon.sprite = null;
        uid = -1;
        loading.gameObject.SetActive(false);
        CloseMaskAnim();
    }

    public void OnitemBtnClick()
    {
        //空中滑行和使用降落伞不许切换背包||梯子不允许
        if (StateManager.IsParachuteUsing||StateManager.IsOnLadder||StateManager.IsOnSlide|| StateManager.IsOnSeesaw || StateManager.IsOnSwing)
        {
            return;
        }
        if (!StateManager.Inst.CanDropCurProp())
        {
            return;
        }
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.animCon.isEating)
        {
            return;
        }
        if (StateManager.IsFishing)
        {
            return;
        }
        if (uid != (int)BaggageItemType.none && BaggageManager.Inst.handleItemId != uid)
        {
            btnAct?.Invoke(this);
            if (CatchPanel.Instance)
            {
                CatchPanel.Instance.catchBtnLock.BtnLock();
                CatchPanel.Instance.dropBtnLock.BtnLock();
            }
        }
    }

    public void OnLongPressItem()
    {
        if (uid == (int)BaggageItemType.none || StateManager.IsParachuteUsing || StateManager.IsOnLadder||StateManager.IsOnSlide|| StateManager.IsOnSeesaw || StateManager.IsOnSwing)
        {
            return;
        }
        if (!StateManager.Inst.CanDropCurProp())
        {
            return;
        }
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.animCon.isEating)
        {
            return;
        }
        if (StateManager.IsFishing)
        {
            return;
        }
        var selfUid = GameManager.Inst.ugcUserInfo.uid;
        if(selfUid != null)
        {
            PickabilityManager.Inst.RecordPlayerDrop(selfUid);
            PickabilityManager.Inst.RecordPlayerPick(selfUid, uid);
            PickabilityManager.Inst.LongPressHandleDropProp();
        }
    }

    public void MaskAnim()
    {
        if(uid == (int)BaggageItemType.none || StateManager.IsParachuteUsing || StateManager.IsOnLadder||StateManager.IsOnSlide|| StateManager.IsOnSeesaw || StateManager.IsOnSwing)
        {
            return;
        }
        mask.gameObject.SetActive(true);
        maskAnim.Play();
    }

    private void CloseMaskAnim()
    {
        mask.gameObject.SetActive(false);
    }
}
