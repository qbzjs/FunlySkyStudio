using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DetailsStyle
{
    None,
    Black,
    White,
}

/// <summary>
/// Author: pzkunn
/// Description: 官方NFT页面
/// Date: 2022/7/21 20:8:37
/// </summary>
public class DigitalCollectView : BaseView
{
    public Button detailsBtn;
    public Image detailsIconImg;
    private Image detailsBGImg;
    private Text detailsText;
    private int curSeriesId = -1;
    private const int defSeriesId = 1;

    public GameObject seriesIconPref;
    public Transform seriesParent;
    public List<Toggle> seriesToggles; //初始有All (index = 0)
    private ReqQuerry reqSeriesQuerry = new ReqQuerry();
    private List<NFTSeriesInfo> seriesInfos = new List<NFTSeriesInfo>();

    public RoleNFTView viewPref;
    public Transform viewParent;
    private Dictionary<string, RoleNFTView> viewList = new Dictionary<string, RoleNFTView>();

    private void Start()
    {
        RoleMenuView.Ins.SetAction(InitDigitalCollectView);
        detailsBtn.onClick.AddListener(OnDetailsBtnClick);
        detailsBGImg = detailsBtn.GetComponent<Image>();
        detailsText = detailsBtn.GetComponentInChildren<Text>(true);
    }

    private void InitDigitalCollectView()
    {
        this.classifyType = ClassifyType.digitalCollect;
        //初始化all系列
        seriesToggles[0].onValueChanged.AddListener((isOn) => { if (isOn) { SetSeriesSelect(0); } });
        seriesToggles[0].isOn = true;
        seriesInfos.Add(new NFTSeriesInfo() { seriesId = 0, seriesName = "all" });
    }

    #region 数据请求
    public void GetAllSeriesInfo()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            //新用户过滤
            return;
        }
        reqSeriesQuerry.pageSize = 50;
        reqSeriesQuerry.cookie = "";
        reqSeriesQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
        RefreshSeriesList();
    }

    private void RefreshSeriesList()
    {
        HttpUtils.MakeHttpRequest("/other/getOfficialSeries", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(reqSeriesQuerry), OnGetSeriesInfoSuccess, OnGetSeriesInfoFail);
    }

    private void OnGetSeriesInfoSuccess(string content)
    {
        LoggerUtils.Log("GetAllDigitalCollectSeries Success --> " + content);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("GetAllDigitalCollectSeries Failed --> repData.data == null");
            return;
        }
        NFTSeriesRepInfo resourceInfo = JsonConvert.DeserializeObject<NFTSeriesRepInfo>(repData.data);
        reqSeriesQuerry.cookie = resourceInfo.cookie;
        if (resourceInfo.seriesList != null)
        {
            resourceInfo.seriesList.ForEach(item => CreateSeriesItem(item));
        }

        if (resourceInfo.isEnd != 1)
        {
            RefreshSeriesList();
        }
    }

    private void OnGetSeriesInfoFail(string error)
    {
        LoggerUtils.LogError("Script:DigitalCollectView GetAllDigitalCollectSeries Failed error = " + error);
    }

    private void OnDetailsBtnClick()
    {
        var seriesInfo = seriesInfos.Find(x => x.seriesId == curSeriesId);
        var itemInfo = viewList[seriesInfo.seriesName].GetCurrentItemInfo();
        if (itemInfo != null)
        {
            string itemId = itemInfo.itemId;
            string budActId = itemInfo.budActId;
            if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(budActId))
            {
                LoggerUtils.LogError("OnDetailsBtnClick --> info string is empty");
                CharacterTipPanel.ShowToast("Oops! Something went wrong. Please try again!");
                return;
            }

            NativeDetailParam detailParam = new NativeDetailParam()
            {
                optType = (int)DetailPageType.Nft,
                dataType = (int)DCItemType.Clothes,
                itemId = itemId,
                budActId = budActId
            };
            //LoggerUtils.Log("OnDetailsBtnClick --> " + JsonConvert.SerializeObject(detailParam));
            MobileInterface.Instance.OpenNativeDetailPage(JsonConvert.SerializeObject(detailParam), true);
        }
    }
    #endregion

    #region 创建流程
    private void CreateSeriesItem(NFTSeriesInfo item)
    {
        //创建seriesIcon
        seriesInfos.Add(item);
        var iconGO = Instantiate(seriesIconPref, seriesParent);
        var iconTog = iconGO.GetComponent<Toggle>();
        var iconImg = iconGO.GetComponent<Image>();
        seriesToggles.Add(iconTog);
        iconTog.group = seriesParent.GetComponent<ToggleGroup>();
        iconTog.onValueChanged.AddListener((isOn) => { if (isOn) { SetSeriesSelect(item.seriesId); } });
        RoleConfigDataManager.Inst.SetAvatarIcon(iconImg, item.seriesIcon, sprite);
    }

    public RoleStyleItem CreateNFTItem(ClassifyType type, int pgcId, Transform parentTF, Action<RoleStyleItem> select)
    {
        var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(type, pgcId);
        return CreateItemByData(type, parentTF, rcData, select);
    }
    #endregion

    #region UI表现
    public override void UpdateSelectState()
    {
        //重置每个子页面的选中状态
        foreach (var key in viewList.Keys)
        {
            viewList[key].ClearSelectState();
        }
    }

    public override void OnSelect()
    {
        base.OnSelect();
        if (seriesInfos.Count <= 1)
        {
            //除了All都没有加载出来
            return;
        }
        if (curSeriesId == -1)
        {
            SetSeriesSelect(seriesInfos[defSeriesId].seriesId);
            seriesToggles[defSeriesId].isOn = true;
        }
        else
        {
            SetSeriesSelect(curSeriesId);
        }
    }

    private void SetSeriesSelect(int seriesId)
    {
        var info = seriesInfos.Find(x => x.seriesId == seriesId);
        curSeriesId = seriesId;
        //首次点击创建
        if (!viewList.ContainsKey(info.seriesName))
        {
            //创建seriesIcon
            var view = Instantiate(viewPref, viewParent);
            view.parentView = this;
            view.GetItemListInfo(info.seriesId, info.seriesName);
            viewList.Add(info.seriesName, view);
        }
        //显示选中列表
        foreach (var key in viewList.Keys)
        {
            viewList[key].gameObject.SetActive(key == info.seriesName);
        }
        //刷新背景
        RefreshBackground(info);
        //拉镜头(暂定全拉远)
        var zoomType = RoleClassifiyView.Ins.GetZoomType(classifyType);
        RoleMenuController.Ins.SetCameraZoom(zoomType);
    }

    private void RefreshBackground(NFTSeriesInfo info)
    {
        if (info.seriesId > 0)
        {
            viewList[info.seriesName].SetBannerVisiable(true);
            RoleClassifiyView.Ins.ChangeBGIconColorToWhite();
            RefreshBannerBySelectSeries(info);
        }
        else
        {
            //选择All的情况
            viewList[info.seriesName].SetBannerVisiable(false);
            RoleClassifiyView.Ins.ChangeBGToDefault();
        }
    }

    private void RefreshBannerBySelectSeries(NFTSeriesInfo info)
    {
        var curView = viewList[info.seriesName];
        RoleConfigDataManager.Inst.SetAvatarIconDynamic(curView.bannerImg, info.bannerUrl, sprite, curView.SetBannerState);
        RoleConfigDataManager.Inst.SetAvatarIconDynamic(RoleClassifiyView.Ins.backgroundImage, info.backgroundUrl, sprite);
        RoleConfigDataManager.Inst.SetAvatarIconDynamic(RoleClassifiyView.Ins.shadowImg, info.shadowUrl, sprite);
        curView.RefreshBannerBySelectItem();
    }

    public void ChangeDetailsBtnStyle(int detailsStyle)
    {
        switch ((DetailsStyle)detailsStyle)
        {
            case DetailsStyle.Black:
                detailsBGImg.sprite = sprite.GetSprite("details_b_btn");
                detailsIconImg.sprite = sprite.GetSprite("details_b_ic");
                detailsText.color= DataUtils.DeSerializeColorByHex("#FFFFFF");
                break;
            case DetailsStyle.White:
                detailsBGImg.sprite = sprite.GetSprite("details_w_btn");
                detailsIconImg.sprite = sprite.GetSprite("details_w_ic");
                detailsText.color = DataUtils.DeSerializeColorByHex("#000000");
                break;
        }
    }
    #endregion
}