using System;
using GameData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using SavingData;
using Newtonsoft.Json;
using DG.Tweening;
using System.Collections.Generic;

public class RoleStyleItem : MonoBehaviour
{
    public Button StyleBtn;
    public Image StyleImg;
    public Image newImg;
    public Image labelImg;
    public GameObject collectTag;
    public GameObject colorSelectGo;
    public GameObject iconLoader;
    public GameObject wearLoader;
    public GameObject iconLost;
    public RoleIconData rcData;
    private Action<RoleStyleItem> OnSelect;
    public bool isCollected = false;
    public ClassifyType type;
    [HideInInspector]
    public string redDotId; //红点识别ID(DC)
    [HideInInspector]
    public bool canClearRed = false;

    protected virtual void Start()
    {
        StyleBtn.onClick.AddListener(OnSelectClick);
        StyleBtn.onClick.AddListener(LogWearEvent);

        var longPressBtn = StyleBtn.GetComponent<RoleItemLongPress>();
        longPressBtn.OnLongPress = OnLongPressItem;
        longPressBtn.scrollRect = transform.GetComponentInParent<ScrollRect>();
        if (string.IsNullOrEmpty(rcData.texName))
        {
            longPressBtn.isCanLongPress = false;
        }
    }

    /// <summary>
    /// 点击ITEM时上报DC试穿
    /// </summary>
    public void LogWearEvent()
    {
        DataLogUtils.AvatarPGCWear(type, rcData.id, rcData.grading);
    }

    private void OnEnable()
    {
        collectTag.transform.localScale = Vector3.one;
    }
    
    public void PlayLoadingAnim(bool isPlay)
    {
        wearLoader.SetActive(isPlay);
    }

    public virtual void Init(RoleIconData data, Action<RoleStyleItem> select, SpriteAtlas spriteAtlas)
    {
        rcData = data;
        OnSelect = select;
        SetSelectState(false);
        UpdateItemIsNew(false);
        // 收藏态显示
        collectTag.SetActive(isCollected);
        // label显示
        SetLableState(data);
        // Icon显示
        SetIconImg(data.spriteName, spriteAtlas);
    }

    private void SetIconImg(string spriteName, SpriteAtlas spriteAtlas = null)
    {
        RoleConfigDataManager.Inst.SetAvatarIcon(StyleImg, spriteName, spriteAtlas, SetIconState);
    }

    private void SetIconState(ImgLoadState state)
    {
        iconLoader.SetActive(state == ImgLoadState.Loading);
        iconLost.SetActive(state == ImgLoadState.Failed);
        StyleImg.gameObject.SetActive(state == ImgLoadState.Complete);
    }

    public void SetLableState(RoleIconData data)
    {
        if (data.IsNormal())
        {
            labelImg.gameObject.SetActive(false);
        }
        else
        {
            string spriteType;
            if (data.origin != (int)RoleOriginType.Normal)
            {
                spriteType = ((RoleOriginType)data.origin).ToString();
            }
            else if (data.grading != (int)RoleResGrading.Normal)
            {
                spriteType = ((RoleResGrading)data.grading).ToString();
            }
            else
            {
                spriteType = ((RoleSubType)data.subType).ToString();
            }
            var spriteName = string.Format("{0}_label", spriteType);
            labelImg.sprite = SpriteAtlasManager.Inst.GetAvatarCommonSprite(spriteName);
            labelImg.gameObject.SetActive(true);
        }
    }

    public virtual void OnSelectClick()
    {
        OnSelect?.Invoke(this);
        if (iconLost.activeSelf)
        {
            //重新加载icon图片
            SetIconImg(rcData.spriteName);
        }
        var savesView = RoleMenuView.Ins.GetView<SavesView>();
        savesView.ClearSelectState();
        var zoomType = RoleClassifiyView.Ins.GetZoomType(type);
        RoleMenuController.Ins.SetCameraZoom(zoomType);
    }

    public virtual void SetSelectState(bool isVisible)
    {
        colorSelectGo.SetActive(isVisible);
        if (isVisible)
        {
            ClearRed();
        }
    }

    public virtual void OnLongPressItem()
    {
        LoggerUtils.Log("LongPress item");
        if (rcData == null)
        {
            return;
        }
        isCollected = !isCollected;
        if (isCollected)
        {
            SendCollectClothing();
        }
        else
        {
            SendCanceClothingCollection();
        }
    }

    public void SendCollectClothing()
    {
        ClothingData data = new ClothingData();
        data.id = rcData.id.ToString();
        data.type = (int)type;
        data.data = JsonConvert.SerializeObject(rcData);
        HttpUtils.MakeHttpRequest("/image/setFavorites", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(data), OnCollectSuccess, OnFail);
    }

    public void SendCanceClothingCollection()
    {
        ClothingData data = new ClothingData();
        data.id = rcData.id.ToString();
        data.type = (int)type;
        HttpUtils.MakeHttpRequest("/image/delFavorites", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(data), OnCancelCollectSuccess, OnFail);
    }

    public void OnCollectSuccess(string msg)
    {
        CharacterTipPanel.ShowToast("Added to favorites");
        var sequence = DOTween.Sequence();
        sequence.Append(collectTag.transform.DOScale(1.2f, 0.8f))
            .Append(collectTag.transform.DOScale(1, 1f));
        //清除红点(DC)
        ClearRed();
        var collectionsView = RoleMenuView.Ins.GetView<CollectionsView>();
        collectionsView.CollectClothingItem(type, rcData, true);
    }

    public void OnCancelCollectSuccess(string msg)
    {
        var collectionsView = RoleMenuView.Ins.GetView<CollectionsView>();
        collectionsView.CancelCollectClothingItem(type, rcData.id);
    }
    public void OnFail(string err)
    {
        LoggerUtils.Log(err);
    }

    public void UpdateItemCollect(bool isCollect)
    {
        isCollected = isCollect;
        SetCollectTagVisible();
    }

    public virtual void SetCollectTagVisible()
    {
        collectTag.SetActive(isCollected);
    }

    public void UpdateItemIsNew(bool isNew)
    {
        //新用户不显示New标签
        newImg.gameObject.SetActive(isNew && (ROLE_TYPE)GameManager.Inst.engineEntry.subType != ROLE_TYPE.FIRST_ENTRY);
    }

    public void ClearRed()
    {
        if (!rcData.IsOrigin() && !string.IsNullOrEmpty(redDotId) && newImg.gameObject.activeInHierarchy && canClearRed)
        {
            var resourceKinds = new List<string>();
            resourceKinds.Add(redDotId);
            ClearRedDots clearRedDots = new ClearRedDots
            {
                cleanKind = 1,
                resourceKinds = resourceKinds.ToArray()
            };
            HttpUtils.MakeHttpRequest("/other/cleanResourceRedDot", (int)HTTP_METHOD.POST, JsonUtility.ToJson(clearRedDots),
                (success) =>
                {
                    LoggerUtils.Log("AvatarPGCRedClearSuccess.Msg");
                },
                (fail) =>
                {
                    LoggerUtils.LogError("AvatarPGCRedClearFail.Msg");
                });
            UpdateItemIsNew(false);
            //关联处理Airdrop红点
            if (rcData.origin == (int)RoleOriginType.Airdrop)
            {
                var aItem = RoleMenuView.Ins.GetView<AirdropView>().GetPgcItem((int)type, rcData.id);
                if (aItem) aItem.SetIsNewState(false);
                var oItem = BaseView.Ins.GetItem(ItemDictType.AllOwned, type, rcData.id);
                if (oItem) oItem.UpdateItemIsNew(false);
            }
        }
    }
}