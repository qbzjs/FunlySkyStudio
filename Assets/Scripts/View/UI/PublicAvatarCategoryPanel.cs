/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public enum PubCategory
{
    Outfits,
    Shoes,
    Accessories,
    Bag,
    Crossbody,
    Special,
    Headwear,
    Hair,
    Glasses,
    Patterns,
    Hand,
    eyes,
    effect
}

public class PlayerHodingInfo
{
    public int classifyType; //角色部件分类，同ClassifyType枚举值
    public int id; //配置表重的配饰Id
    public MapInfo mapInfo; //当前Item的信息
    public int hasCount; //拥有数量：0表示未拥有
}

public class GetPlayerHodingInfo
{
    public string toUid = "";
}

public class PlayerHodingInfoRsp
{
    public PlayerHodingInfo[] avatarInfos;
}

public class PublicAvatarCategoryPanel : MonoBehaviour
{
    public PublicAvatarItemInfoPanel _itemInfoPanel;
    private Dictionary<PubCategory, PlayerHodingInfo> AvatarItemDic = new Dictionary<PubCategory, PlayerHodingInfo>();
    private Dictionary<PubCategory, PublicAvatarCategoryItem> _buttonPool = new Dictionary<PubCategory, PublicAvatarCategoryItem>();
    public Transform _btnParent;
    public GameObject _btnPrefab;
    private UserInfo _curUserInfo;
    private RoleController _publicRoleCtr;

    private const string Default_Tittle = "BUD Original";
    private const string Default_Desc = "Try on in Edit Avatar";

    private const string Reward_Tittle = "Special Reward";
    private const string Reward_Desc = "Go complete the quests to get it!";
    private Dictionary<PubCategory, ClassifyType> TypeDics = new Dictionary<PubCategory, ClassifyType>()
    {
        {PubCategory.Outfits,ClassifyType.outfits},
        {PubCategory.Shoes,ClassifyType.shoes},
        {PubCategory.Accessories,ClassifyType.accessories},
        {PubCategory.Bag,ClassifyType.bag},
        {PubCategory.Crossbody,ClassifyType.bag},
        {PubCategory.Special,ClassifyType.special},
        {PubCategory.Headwear,ClassifyType.headwear},
        {PubCategory.Hair,ClassifyType.hair},
        {PubCategory.Glasses,ClassifyType.glasses},
        {PubCategory.Patterns,ClassifyType.patterns},
        {PubCategory.Hand,ClassifyType.hand},
    };

    public void SetData(UserInfo userInfo, RoleController roleCtr)
    {
        _publicRoleCtr = roleCtr;
        _curUserInfo = userInfo;
        InitOtherPlayerAvatarOutfit();
        InitAvatarItemDicInfo();
        GetPlayerHoldingState();
    }

    public void InitOtherPlayerAvatarOutfit()
    {
        if(_curUserInfo != null)
        {
            var imageJson = _curUserInfo.imageJson;
            var roleData = JsonConvert.DeserializeObject<RoleData>(imageJson);
            //替换未拥有的DC 可能已经被买空
            RoleConfigDataManager.Inst.ReplaceNotOwnedDC(_curUserInfo, roleData);
            _publicRoleCtr.InitRoleByData(roleData);

            //特殊处理一下眼睛
            HandleEyeTexure(roleData, _publicRoleCtr);
        }
    }

    #region 获取当前主页的玩家身上穿戴的\DC\UGC\DCPGC信息，并且获取自己的持有状态
    public void GetPlayerHoldingState()
    {
        var otherPlayerId = _curUserInfo.uid;
        GetPlayerHodingInfo getPlayerHodingInfo = new GetPlayerHodingInfo()
        {
            toUid = otherPlayerId,
        };

        HttpUtils.MakeHttpRequest("/ugcmap/getAvatars", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(getPlayerHodingInfo), OnGetPlayerHoldingStateSuccess,
            (error) =>
            {
                LoggerUtils.LogError("GetPlayerHoldingState Error = " + error);
                InitView();
            });
    }
    #endregion

    private void InitAvatarItemDicInfo()
    {
        var imageJson = _curUserInfo.imageJson;
        var curRoleData = JsonConvert.DeserializeObject<RoleData>(imageJson);
        AvatarItemDic = new Dictionary<PubCategory, PlayerHodingInfo>()
        {
            { PubCategory.Outfits, new PlayerHodingInfo(){ id = curRoleData.cloId } },
            { PubCategory.Shoes, new PlayerHodingInfo(){ id = curRoleData.shoeId } },
            { PubCategory.Accessories, new PlayerHodingInfo(){ id = curRoleData.acId } },
            { PubCategory.Bag, new PlayerHodingInfo(){ id = curRoleData.bagId } },
            { PubCategory.Crossbody, new PlayerHodingInfo(){ id = curRoleData.cbId } },
            { PubCategory.Special, new PlayerHodingInfo(){ id = curRoleData.saId } },
            { PubCategory.Headwear, new PlayerHodingInfo(){ id = curRoleData.hatId } },
            { PubCategory.Hair, new PlayerHodingInfo() { id = curRoleData.hId } },
            { PubCategory.Glasses, new PlayerHodingInfo(){ id = curRoleData.glId } },
            { PubCategory.Patterns, new PlayerHodingInfo(){ id = curRoleData.fpId } },
            { PubCategory.Hand, new PlayerHodingInfo(){ id = curRoleData.hdId } },
        };
    }

    private void OnGetPlayerHoldingStateSuccess(string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            HttpResponDataStruct rspDataStruct = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
            PlayerHodingInfoRsp playerHodingInfoRsp = JsonConvert.DeserializeObject<PlayerHodingInfoRsp>(rspDataStruct.data);
            PlayerHodingInfo[] hodingInfos = playerHodingInfoRsp.avatarInfos;
            LoggerUtils.Log("OnGetPlayerHoldingStateSuccess hodingInfos = " + JsonConvert.SerializeObject(hodingInfos));
            if (hodingInfos != null)
            {
                for (int i = 0; i < hodingInfos.Length; i++)
                {
                    var classifyType = (ClassifyType)hodingInfos[i].classifyType;
                    var pubCategory = TypeDics.FirstOrDefault(x => x.Value == classifyType).Key;
                    if (classifyType == ClassifyType.outfits || classifyType == ClassifyType.DCUGCCloth || classifyType == ClassifyType.ugcCloth)
                    {
                        pubCategory = PubCategory.Outfits;
                    }
                    if (classifyType == ClassifyType.ugcPatterns)
                    {
                        pubCategory = PubCategory.Patterns;
                    }
                    if (classifyType == ClassifyType.bag)//临时兼容背包三级分类
                    {
                        pubCategory = GetBagComponentType(hodingInfos[i].id);
                    }
                    if (AvatarItemDic.ContainsKey(pubCategory))
                    {
                        var hodingInfo = AvatarItemDic[pubCategory];
                        hodingInfo.mapInfo = hodingInfos[i].mapInfo;
                        hodingInfo.hasCount = hodingInfos[i].hasCount;
                        AvatarItemDic[pubCategory] = hodingInfo;
                    }
                }
            }
        }
        InitView();
    }
    //临时兼容背包三级分类
    private PubCategory GetBagComponentType(int id)
    {
        var data = RoleConfigDataManager.Inst.GetBagStylesDataById(id);
        switch ((BagCompType)data.bagCompType)
        {
            case BagCompType.Backpack:
                return PubCategory.Bag;
            case BagCompType.Crossbody:
                return PubCategory.Crossbody;
        }
        return PubCategory.Bag;
    }

    private void InitView()
    {
        foreach (var item in AvatarItemDic)
        {
            var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(TypeDics[item.Key], item.Value.id);
            if (rcData != null && !_buttonPool.ContainsKey(item.Key))
            {
                if (string.IsNullOrEmpty(rcData.texName) && (item.Value.mapInfo == null)) {
                    continue;
                }

                var itemBtn = Instantiate(_btnPrefab, _btnParent);
                var itemScript = itemBtn.GetComponent<PublicAvatarCategoryItem>();
                itemScript.Init();
                _buttonPool.Add(item.Key, itemScript);
                itemScript.gameObject.SetActive(true);
                InitButtonImg(item.Key, item.Value.id);
                itemScript.AddClick(() => { OnButtonClick(item.Key); });
            }
        }
        foreach (var btn in _buttonPool)
        {
            if (btn.Value.gameObject.activeSelf)
            {
                OnButtonClick(btn.Key);
                break;
            }
        }
    }

    private void InitButtonImg(PubCategory cardType, int id)
    {
        RoleIconData data = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(TypeDics[cardType], id);
        BaseView view = RoleClassifiyView.Ins.GetViewByType(TypeDics[cardType]);
        var sprite = view.sprite;
        //PGC流程
        if (data != null && !string.IsNullOrEmpty(data.texName) && !string.IsNullOrEmpty(data.spriteName))
        {
            var curSprite = sprite.GetSprite(data.spriteName);
            if (curSprite == null)
            {
                var imageComp = _buttonPool[cardType].GetIconImage();
                string url = RoleConfigDataManager.Inst.GetAvatarIconPath(TypeDics[cardType], id);
                if (url != null)
                {
                    UGCResourcePool.Inst.DownloadAndGet(url, tex =>
                    {
                        if (tex != null)
                        {
                            curSprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                            imageComp.sprite = curSprite;
                            if (_buttonPool[cardType].GetSelectState())
                            {
                                _itemInfoPanel.SetIconImage(curSprite);
                            }
                        }
                    });
                }
            }
            _buttonPool[cardType].SetIconImage(curSprite);
        }

        //如果是DCUGC、UGC 
        var hodingInfo = AvatarItemDic[cardType];
        if (hodingInfo.mapInfo != null && (hodingInfo.mapInfo.isPGC == 0))
        {
            var coverUrl = hodingInfo.mapInfo.mapCover;
            var imageComp = _buttonPool[cardType].GetIconImage();
            CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTexture(coverUrl, (dlTexture) => {
                Sprite sprite = Sprite.Create((Texture2D)dlTexture, new Rect(0, 0, dlTexture.width, dlTexture.height), new Vector2(0.5f, 0.5f));
                imageComp.sprite = sprite;
                if (_buttonPool[cardType].GetSelectState())
                {
                    _itemInfoPanel.SetIconImage(sprite);
                }
            }, (error) => {
                LoggerUtils.LogError("PublicAvatarCategoryPanel InitButtonImg error " + error);
            }));

        }
    }

    private void OnButtonClick(PubCategory cardType)
    {
        foreach (var btn in _buttonPool.Values)
        {
            btn.SetSelectState(false);
        }
        if (_buttonPool.ContainsKey(cardType))
        {
            var curBtn = _buttonPool[cardType];
            var imageComp = curBtn.GetIconImage();
            curBtn.SetSelectState(true);
            _itemInfoPanel.SetIconImage(imageComp.sprite);
            _itemInfoPanel.SetOnClickAction(() => {
                SetItemInfoAction(cardType);
            });
            SetItemInfoText(cardType);
        }
    }

    private void SetItemInfoText(PubCategory cardType)
    {
        var hodingInfo = AvatarItemDic[cardType];
        //如果是DCUGC、DCPGC、UGC
        if (hodingInfo.mapInfo != null)
        {
            //DC包含Airdrop
            if (hodingInfo.mapInfo.isDC > 0 && hodingInfo.mapInfo.dcInfo != null)
            {
                _itemInfoPanel.SetTittle(hodingInfo.mapInfo.dcInfo.itemName);
                _itemInfoPanel.SetDesc(hodingInfo.mapInfo.dcInfo.itemDesc);
            }
            else
            {
                _itemInfoPanel.SetTittle(hodingInfo.mapInfo.mapName);
                _itemInfoPanel.SetDesc(hodingInfo.mapInfo.mapDesc);
            }

            var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(TypeDics[cardType], hodingInfo.id);//奖励DC
            if (rcData != null && rcData.origin == (int)RoleOriginType.Rewards)
            {
                _itemInfoPanel.SetLabelIcon(LabelType.RW);
            }
            else if (hodingInfo.mapInfo.dcInfo != null && hodingInfo.mapInfo.dcInfo.nftType == (int)NftType.Airdrop)//Airdrop
            {
                LabelType type = hodingInfo.mapInfo.isPGC > 0 ? LabelType.AIR : LabelType.UGCAIR;
                _itemInfoPanel.SetLabelIcon(type);
            }
            else if (hodingInfo.mapInfo.isPGC > 0)//官方DC
            {
                _itemInfoPanel.SetLabelIcon(LabelType.PGC);
            }
            else if (hodingInfo.mapInfo.isDC > 0)//ugcDC
            {
                _itemInfoPanel.SetLabelIcon(LabelType.DC);
            }
            else
            {
                _itemInfoPanel.SetLabelIcon(LabelType.NONE);
            }
        }
        else
        {
            var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(TypeDics[cardType], hodingInfo.id);
            //签到奖励PGC
            if (rcData != null && rcData.origin == (int)RoleOriginType.Rewards)
            {
                _itemInfoPanel.SetTittle(Reward_Tittle);
                _itemInfoPanel.SetDesc(Reward_Desc);
                _itemInfoPanel.SetLabelIcon(LabelType.RW);
            }
            //其他PGC
            else
            {
                _itemInfoPanel.SetTittle(Default_Tittle);
                _itemInfoPanel.SetDesc(Default_Desc);
                _itemInfoPanel.SetLabelIcon(LabelType.NONE);
            }
        }
    }

    private void SetItemInfoAction(PubCategory cardType)
    {
        var hodingInfo = AvatarItemDic[cardType];
        var imageJson = GameManager.Inst.ugcUserInfo.imageJson;
        var roleData = JsonConvert.DeserializeObject<RoleData>(imageJson);
        var curMapInfo = hodingInfo.mapInfo;
        //如果是DCUGC、DCPGC、UGC
        if (curMapInfo != null)
        {
            if (hodingInfo.hasCount <= 0)
            {
                if (curMapInfo.dcInfo != null && curMapInfo.dcInfo.nftType == (int)NftType.Airdrop && curMapInfo.isPGC > 0 && curMapInfo.dcPgcInfo != null && curMapInfo.dcPgcInfo.openStatus != (int)ActivityStatus.Active)
                {
                    //官方Airdrop-S级空投需要校验活动状态
                    CharacterTipPanel.ShowToast("Sorry, this quest has already expired.");
                }
                else
                {
                    OpenAvatarItemDetail(cardType);
                }
            }
            else
            {
                SetClassifyItemSelect(cardType, hodingInfo.id, roleData);
            }
        }
        else
        {
            var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(TypeDics[cardType], hodingInfo.id);
            //签到奖励PGC
            if (rcData != null && rcData.origin == (int)RoleOriginType.Rewards && hodingInfo.hasCount <= 0)
            {
                OpenCheckInRewardsDetail();
            }
            //其他PGC
            else
            {
                SetClassifyItemSelect(cardType, hodingInfo.id, roleData);
            }
        }
    }

    private void SetClassifyItemSelect(PubCategory cardType, int id, RoleData roleData)
    {
        var selfRoleCtr = RoleUIManager.Inst.rController;
        //将人物形象数据恢复成为当前玩家自己的数据 并且设置成当前所选的配饰
        PublicAvatarPanel.Instance.SetCharacterModeActive(false);
        selfRoleCtr.InitRoleByData(roleData);
        RoleClassifiyView.Ins.SetClassifyItemSelect(TypeDics[cardType], id);
        if (TypeDics[cardType] == ClassifyType.outfits || TypeDics[cardType] == ClassifyType.patterns)
        {
            SetClothItemSelect(cardType, id);
        }
        HandleEyeTexure(roleData, selfRoleCtr);
        PublicAvatarPanel.Hide();
    }

    private void SetClothItemSelect(PubCategory type, int id)
    {
        var hodingInfo = AvatarItemDic[type];
        var curMapInfo = hodingInfo.mapInfo;
        if (curMapInfo != null && curMapInfo.isPGC == 0)
        {
            UGCClothInfo ugcClothInfo = new UGCClothInfo()
            {
                mapId = curMapInfo.mapId,
                clothesJson = curMapInfo.clothesJson,
                clothesUrl = curMapInfo.clothesUrl,
                templateId = curMapInfo.templateId,
                dataSubType=curMapInfo.dataSubType,
            };
            UGCClothesResType ugcClothType = (curMapInfo.isDC > 0) ? UGCClothesResType.DC : UGCClothesResType.UGC;
            RoleUgcManager.Inst.WearUgc(ugcClothInfo, ugcClothType);
        }
    }

    //打开签到奖励详情
    public void OpenCheckInRewardsDetail()
    {
        RoleUIManager.Inst.SetLoadingMaskVisible(true);
        RewardsRepQuerry httpReqQuerry = new RewardsRepQuerry()
        {
            type = 1
        };

        HttpUtils.MakeHttpRequest("/facade/checkins", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReqQuerry), (content) =>
        {
            RoleUIManager.Inst.SetLoadingMaskVisible(false);
            if (string.IsNullOrEmpty(content))
            {
                return;
            }
            LoggerUtils.Log("RewardsRepQuerry --> success : " + content);
            var response = JsonConvert.DeserializeObject<HttpResponse>(content);
            var activityInfo = JsonConvert.DeserializeObject<RewardsActivityInfo>(response.data);
            if (activityInfo.activity == null)
            {
                return;
            }

            if (activityInfo.activity.status == (int)ActivityStatus.Finished)
            {
                CharacterTipPanel.ShowToast("Sorry, this quest has already expired.");
            }
            else
            {
                //活动未结束跳转活动详情页
                MobileInterface.Instance.OpenCheckInRewardsPage();
            }
        }, (error) =>
        {
            RoleUIManager.Inst.SetLoadingMaskVisible(false);
            LoggerUtils.Log("RewardsRepQuerry --> failed : " + error);
        });
    }

    //Call IOS 
    public void OpenAvatarItemDetail(PubCategory cardType)
    {
        var hodingInfo = AvatarItemDic[cardType];
        var mapInfo = hodingInfo.mapInfo;
        mapInfo.dataType = 2;
        MobileInterface.Instance.OpenAvatarItemDetail(JsonConvert.SerializeObject(mapInfo));
    }

    //特殊处理一下眼睛
    public void HandleEyeTexure(RoleData roleData, RoleController roleController)
    {
        string eyeName = roleController.GetAnimName(roleData.eId);
        roleController.SetEyesStyle(eyeName);
        roleController.SetSpecialEyesStyle(eyeName);
        roleController.SetEyePupilColor(eyeName, roleData.eCr);
        roleController.StartEyeAnimation(roleData.eId);
    }
}
