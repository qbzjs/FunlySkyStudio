using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using SavingData;
using DG.Tweening;
using UnityEngine.U2D;

public class RoleStyleUgcItem : MonoBehaviour
{
    public Button StyleBtn;
    public Image spriteImg;
    public RoleItemLongPress longPressBtn;
    public GameObject newImage;
    public GameObject iconLoader;
    public GameObject colorSelectGo;
    public GameObject wearLoader;
    public GameObject collectTag;
    public Image labelImage;
    public RoleUGCIconData rcData;
    private Action<RoleStyleUgcItem> OnSelect;
    public bool isCollected = false;
    public ClassifyType type;
    public bool IsPGC { private set; get; }
    //添加可调节功能(目前仅适配官方Airdrop)
    public bool CanAdjust;
    public Button CustomBtn;
    private Action customClick;
    public Action CancelAct;

    private void OnEnable()
    {
        collectTag.transform.localScale = Vector3.one;
    }

    protected virtual void Start()
    {
        longPressBtn.OnLongPress = OnLongPressItem;
        longPressBtn.scrollRect = transform.GetComponentInParent<ScrollRect>();
        StyleBtn.onClick.AddListener(OnSelectClick);
        StyleBtn.onClick.AddListener(LogWearEvent);
        CustomBtn.onClick.AddListener(OnCustomClick);
    }

    public void SetCustomView(Action act)
    {
        customClick = act;
    }

    private void OnCustomClick()
    {
        customClick?.Invoke();
    }

    private void LogWearEvent()
    {
        if (rcData.pgcId > 0)
        {
            DataLogUtils.AvatarPGCWear(type, rcData.pgcId, rcData.grading);
        }
        else if (!string.IsNullOrEmpty(rcData.mapId))
        {
            DataLogUtils.AVatarUGCWear(rcData.mapId, rcData.grading, type);
        }
    }

    public void Init(RoleUGCIconData data, Action<RoleStyleUgcItem> select)
    {
        rcData = data;
        type = (ClassifyType)data.classifyType;
        IsPGC = rcData.pgcId != 0;
        newImage.SetActive(data.isNew == 1);
        SetLableState(data);
        spriteImg.sprite = null;
        SetLoaderActive(true);
        isCollected = rcData.isFavorites == 1;
        SetCollectTagVisible();
        OnSelect = select;
        colorSelectGo.SetActive(false);
        wearLoader.SetActive(false);
        longPressBtn.isCanLongPress = true;
        CanAdjust = false;
        CustomBtn.gameObject.SetActive(false);
        StyleBtn.onClick.RemoveListener(OnResStoreBtnClick);
        StyleBtn.onClick.RemoveListener(OnCancelClick);
    }

    /// <summary>
    /// 处理特殊按钮
    /// </summary>
    /// <param name="tex"></param>
    private void SetSpecialBtnState(Texture tex)
    {
        SetItemTexture(tex);
        wearLoader.SetActive(false);
        colorSelectGo.SetActive(false);
        newImage.SetActive(false);
        collectTag.SetActive(false);
        labelImage.gameObject.SetActive(false);
        isCollected = false;
        CanAdjust = false;
        OnSelect = null;
        longPressBtn.isCanLongPress = false;
        StyleBtn.onClick.RemoveListener(OnResStoreBtnClick);
        StyleBtn.onClick.RemoveListener(OnCancelClick);
    }

    #region 商城按钮交互逻辑
    public void SetStoreTexture(Texture tex)
    {
        SetSpecialBtnState(tex);
        StyleBtn.onClick.AddListener(OnResStoreBtnClick);
    }

    private void OnResStoreBtnClick()
    {
        MobileInterface.Instance.OpenClothStore();
    }
    #endregion

    #region 取消按钮交互逻辑
    public void OnCancelClick()
    {
        CancelAct?.Invoke();
        SetSelectState(true);
    }

    public void SetCancelBtn(Texture tex, Action cancelAct)
    {
        SetSpecialBtnState(tex);
        if (CancelAct == null)
        {
            CancelAct = cancelAct;
        }
        StyleBtn.onClick.AddListener(OnCancelClick);
    }
    #endregion

    public void SetItemTexture(Texture tex)
    {
        if (tex != null)
        {
            Sprite sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            spriteImg.sprite = sprite;
            SetLoaderActive(false);
        }
    }

    public void SetItemTexture(string texName, SpriteAtlas spriteAtlas)
    {
        //处理pgc部件
        RoleConfigDataManager.Inst.SetAvatarIcon(spriteImg, texName, spriteAtlas, SetIconState);
    }

    private void SetIconState(ImgLoadState state)
    {
        //ugcItem的loading状态初始化已经处理
        if (state == ImgLoadState.Complete)
        {
            SetLoaderActive(false);
        }
    }

    private void SetLoaderActive(bool state)
    {
        iconLoader.SetActive(state);
        spriteImg.gameObject.SetActive(!state);
    }

    private void SetLableState(RoleUGCIconData data)
    {
        if (data.grading == (int)RoleResGrading.DC)
        {
            string type = data.pgcId == 0 ? "Ugc" : "";
            string typeName = data.origin == (int)RoleOriginType.Airdrop ? "Airdrop" : "DC";
            string spriteName = string.Format("{0}{1}_label", type, typeName);
            labelImage.sprite = SpriteAtlasManager.Inst.GetAvatarCommonSprite(spriteName);
            labelImage.gameObject.SetActive(true);
        }
        else
        {
            labelImage.gameObject.SetActive(false);
        }
    }

    public void LoadClothCover()
    {
        if (string.IsNullOrEmpty(rcData.coverUrl))
        {
            return;
        }
        var coverUrl = rcData.coverUrl;

        UGCResourcePool.Inst.DownloadAndGet(coverUrl, tex =>
        {
            if (tex != null)
            {
                Sprite sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                spriteImg.sprite = sprite;
                SetLoaderActive(false);
            }
        });
    }

    public void OnSelectClick()
    {
        OnSelect?.Invoke(this);
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
        //适配可调节功能
        CustomBtn.gameObject.SetActive(isVisible && CanAdjust);
        SetCollectTagVisible();
    }

    public void SetIsNewState(bool isNew)
    {
        rcData.isNew = isNew ? 1 : 0;
        newImage.SetActive(isNew);
    }

    public void PlayLoadTexAnim(bool isPlay)
    {
        wearLoader.SetActive(isPlay);
    }

    public void OnLongPressItem()
    {
        LoggerUtils.Log("LongPress item");
        if (rcData == null)
        {
            return;
        }
        isCollected = !isCollected;
        SendCollectClothing(isCollected);
    }

    public void SendCollectClothing(bool isCollected)
    {
        ClothingData data = new ClothingData();
        data.id = IsPGC ? rcData.pgcId.ToString() : rcData.mapId;
        data.type = IsPGC ? rcData.classifyType : (int)ClassifyType.ugcCloth; //以后ugc其他类型也用ugcCloth, 仅用作区分是否ugc
        if (isCollected)
        {
            data.data = JsonConvert.SerializeObject(rcData);
            HttpUtils.MakeHttpRequest("/image/setFavorites", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(data), OnCollectSuccess, OnFail);
        }
        else
        {
            HttpUtils.MakeHttpRequest("/image/delFavorites", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(data), OnCancelCollectSuccess, OnFail);
        }
    }

    public void OnCollectSuccess(string msg)
    {
        CharacterTipPanel.ShowToast("Added to favorites");
        var sequence = DOTween.Sequence();
        sequence.Append(collectTag.transform.DOScale(1.2f, 0.8f))
            .Append(collectTag.transform.DOScale(1, 1f));
        ClearRed();
        rcData.isFavorites = 1;
        isCollected = true;
        SetCollectTagVisible();
        var collectionsView = RoleMenuView.Ins.GetView<CollectionsView>();
        if (IsPGC)
        {
            var iconData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId((ClassifyType)rcData.classifyType, rcData.pgcId);
            collectionsView.CollectClothingItem((ClassifyType)rcData.classifyType, iconData, true);
        }
        else
        {
            collectionsView.CollectUgcClothingItem(rcData, true);
        }
    }

    public void OnCancelCollectSuccess(string msg)
    {
        rcData.isFavorites = 0;
        isCollected = false;
        collectTag.SetActive(isCollected);
        var collectionsView = RoleMenuView.Ins.GetView<CollectionsView>();
        if (IsPGC)
        {
            collectionsView.CancelCollectClothingItem((ClassifyType)rcData.classifyType, rcData.pgcId);
        }
        else
        {
            collectionsView.CancelUgcClothingItem(rcData);
        }
    }

    public void OnFail(string err)
    {
        LoggerUtils.Log(err);
        CharacterTipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    public void UpdateItemCollect(bool isCollect)
    {
        isCollected = isCollect;
        rcData.isFavorites = isCollect ? 1 : 0;
        SetCollectTagVisible();
    }

    public void SetCollectTagVisible()
    {
        //适配可调节功能
        if (CustomBtn.gameObject.activeSelf)
        {
            collectTag.SetActive(false);
        }
        else
        {
            collectTag.SetActive(rcData.isFavorites == 1);
        }
    }

    public void ClearRed()
    {
        if (rcData.isNew == 1)
        {
            var resourceKinds = new List<string>();
            if (!resourceKinds.Contains(rcData.mapId))
            {
                resourceKinds.Add(rcData.mapId);
            }
            ClearRedDots clearRedDots = new ClearRedDots
            {
                cleanKind = 1,
                resourceKinds = resourceKinds.ToArray()
            };
            HttpUtils.MakeHttpRequest("/other/cleanResourceRedDot", (int)HTTP_METHOD.POST, JsonUtility.ToJson(clearRedDots),
                (success) =>
                {
                    LoggerUtils.Log("AvatarUGCRedClearSuccess.Msg");
                },
                (fail) =>
                {
                    LoggerUtils.LogError("AvatarUGCRedClearFail.Msg");
                });
            rcData.isNew = 0;
            newImage.SetActive(false);
            //关联处理Airdrop红点: TODO统一管理UGCItem, 管理isNew更新
            if (rcData.origin == (int)RoleOriginType.Airdrop)
            {
                //官方Airdrop
                var oItem = BaseView.Ins.GetItem(ItemDictType.AllOwned, type, rcData.pgcId);
                if (oItem) oItem.UpdateItemIsNew(false);
                //创作者Airdrop
                if (type == ClassifyType.ugcCloth || type == ClassifyType.ugcPatterns)
                {
                    var aItem = RoleMenuView.Ins.GetView<AirdropView>().GetUgcItem(rcData.mapId);
                    if (aItem) aItem.SetIsNewState(false);

                    var ugcIconView = RoleClassifiyView.Ins.GetViewByType((ClassifyType)rcData.classifyType);
                    if (ugcIconView)
                    {
                        var iconView = ugcIconView.GetComponentInChildren<RoleDCBaseView>(true);
                        var uItem = iconView.GetUgcItem(rcData.mapId);
                        if (uItem) uItem.SetIsNewState(false);
                    }
                }
            }
        }
    }
}
