using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: Shaocheng
/// Description: ugc衣服道具ui控制面板
/// Date: 2022-4-20 17:23:05
/// </summary>
public class UgcClothItemPanel : InfoPanel<UgcClothItemPanel>
{
    public GameObject noPanel;
    public GameObject hasPanel;
    public GameObject loadPanel;
    public Button noPanelButton;
    public Button changeButton;
    public RawImage clothCoverImg;

    private UgcClothItemBehaviour _sBehv;
    private Coroutine _loadCoverCor;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        noPanelButton.onClick.AddListener(OnButtonClick);
        changeButton.onClick.AddListener(OnChangeButtonClick);
        RefreshPanel();
    }

    public void SetEntity(SceneEntity entity)
    {
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        _sBehv = bindGo.GetComponent<UgcClothItemBehaviour>();
        RefreshPanel();
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        if (_loadCoverCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(_loadCoverCor);
            _loadCoverCor = null;
        }
    }

    private void OnDisable()
    {
        OnBackPressed();
    }

    private void RefreshPanel()
    {
        if (_sBehv == null) return;
        var isHasCloth = IsHasUgcCloth();

        noPanel.gameObject.SetActive(!isHasCloth);
        hasPanel.gameObject.SetActive(isHasCloth);

        RefreshClothCover();
    }

    private void RefreshClothCover()
    {
        if (!IsHasUgcCloth())
            return;
        if (_sBehv && _sBehv.ugcClothCoverTexture)
        {
            clothCoverImg.texture = _sBehv.ugcClothCoverTexture;
        }
        else
        {
            LoadClothCover();
        }
    }

    private bool IsHasUgcCloth()
    {
        if (!_sBehv) return false;
        if (!_sBehv.entity.HasComponent<UGCClothItemComponent>()) return false;
        var tempId = _sBehv.entity.Get<UGCClothItemComponent>().templateId;
        return tempId != 0;
    }

    private void OnButtonClick()
    {
#if UNITY_EDITOR
        Invoke("UnityLocalTest_TestLoadUgcCloth", 1f); 
        // UnityLocalTest_TestLoadUgcCloth();
#else
        MobileInterface.Instance.AddClientRespose(MobileInterface.openUgcClothPage, OnOpenUgcPageCallback);
        MobileInterface.Instance.OpenUgcClothPage();
#endif
    }

    private void OnChangeButtonClick()
    {
#if UNITY_EDITOR
        Invoke("UnityLocalTest_TestChangeUgcCloth", 1f); 
        // UnityLocalTest_TestChangeUgcCloth();
#else
        MobileInterface.Instance.AddClientRespose(MobileInterface.openUgcClothPage, OnOpenUgcPageCallback);
        MobileInterface.Instance.OpenUgcClothPage();
#endif
    }

    private void ControlLoadingShow(bool isShow)
    {
        loadPanel.SetActive(isShow);
        noPanel.gameObject.SetActive(!isShow);
        hasPanel.gameObject.SetActive(!isShow);
    }

    private void OnOpenUgcPageCallback(string content)
    {
        LoggerUtils.Log($"[openUgcClothPage] Ugc page callback:{content}");
        MobileInterface.Instance.DelClientResponse(MobileInterface.openUgcClothPage);

        if (string.IsNullOrEmpty(content))
            return;

        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        ClothStyleData ugcClothData = new ClothStyleData()
        {
            templateId = mapInfo.templateId,
            clothesUrl = mapInfo.clothesUrl,
            clothMapId = mapInfo.mapId,
            clothesJson = mapInfo.clothesJson,
            dataSubType = mapInfo.dataSubType,
        };
        if (mapInfo.dcPgcInfo != null)
        {
            ugcClothData.classifyType = mapInfo.dcPgcInfo.classifyType;
            ugcClothData.pgcId = mapInfo.dcPgcInfo.pgcId;
            ugcClothData.abUrl = "";
        }

#if UNITY_EDITOR
        LoggerUtils.Log($"Set Ugc cloth ==>{JsonConvert.SerializeObject(ugcClothData)}");
#endif
        if (_sBehv)
        {
            ControlLoadingShow(true);
            if (ugcClothData.pgcId >= 100000)
            {
                _sBehv.loadAB(ugcClothData, () =>
                {
                    if (_sBehv == null) return;
                    OnLoadSuccess(ugcClothData, mapInfo);
                },OnLoadFail);
            }
            else
            {
                _sBehv.LoadUgcCloth(ugcClothData, () =>
                {
                    if (_sBehv == null) return;
                    OnLoadSuccess(ugcClothData, mapInfo);
                }, OnLoadFail);
            }
        }
    }

    private void OnLoadSuccess(ClothStyleData ugcClothData, MapInfo mapInfo)
    {
        var walletAddress = mapInfo.dcInfo != null ? mapInfo.dcInfo.walletAddress : "";
        _sBehv.AddComponentData(ugcClothData, mapInfo.mapCover, mapInfo.isDC, mapInfo.dcInfo, walletAddress);
        _sBehv.entity.Get<UGCPropComponent>().isTradable = 1;
        _sBehv.SetCanBuyInMap();
        ControlLoadingShow(false);
        RefreshPanel();
        LoadClothCover();
    }

    private void OnLoadFail()
    {
        ControlLoadingShow(false);
        RefreshPanel();
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private void LoadClothCover()
    {
        if (_sBehv == null)
            return;
        
        clothCoverImg.gameObject.SetActive(false);
        clothCoverImg.texture = null;
        
        var coverUrl = _sBehv.entity.Get<UGCClothItemComponent>().clothCover;
        if (string.IsNullOrEmpty(coverUrl))
            return;

        if (_loadCoverCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(_loadCoverCor);
            _loadCoverCor = null;
        }

        _loadCoverCor = CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTexture(coverUrl,
            (tex) =>
            {
                clothCoverImg.gameObject.SetActive(true);
                clothCoverImg.texture = tex;

                if (_sBehv)
                    _sBehv.ugcClothCoverTexture = tex;
            },
            null));
    }

    #region Unity测试使用

#if UNITY_EDITOR
    private void UnityLocalTest_TestLoadUgcCloth()
    {
        string testJson =
            "{\"isDc\":1,\"templateId\":3,\"clothesJson\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothJson/1460531478844608512/940f75e9-1aa0-4beb-a1d6-04fbf60fdbb8clothJson.json\",\"clothesUrl\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothTex/1460531478844608512/b691fe53-bdd9-4f25-ace6-f57f23ad00dbclothTex.zip\",\"mapId\":\"18_100006_1460531478844608512_pgcdc5\",\"mapCover\":\"https://image-cdn.joinbudapp.com/ugcTemplate/cover/NFT/background/18_100006.png\",\"dcPgcInfo\":{\"classifyType\":18,\"pgcId\":100006,\"hasCount\":1},\"dcInfo\":{\"itemId\":\"0xa48c7aa142b6b601555e76f853e5f7df0d810c90_534\",\"tokenId\":\"534\",\"itemName\":\"nft3-8\",\"itemDesc\":\"jfjf\",\"itemCover\":\"https://image-cdn.joinbudapp.com/ugcTemplate/cover/NFT/list/18_100006.png\",\"budActId\":\"f3db5\",\"supply\":\"60\",\"itemUgcType\":2,\"price\":\"90\",\"maticPrice\":\"0.9\",\"royalties\":\"2.5\",\"itemStatus\":1,\"walletAddress\":\"0xdbce412f86a025edf8988c455fda062cc6425265\",\"contractAddress\":\"0xa48c7aa142b6b601555e76f853e5f7df0d810c90\",\"tokenStandard\":\"ERC-1155\",\"operationTime\":1661171190,\"creatorInfo\":{\"uid\":\"1460531478844608512\",\"userName\":\"wenwena\",\"userNick\":\"tww\",\"portraitUrl\":\"https://image-cdn.joinbudapp.com/filters:quality(30)/peopleHeadImg/14605314788446085121648449382065coverImg.png\",\"officialCert\":{\"accountClass\":1,\"certifications\":[{\"certName\":\"\",\"certIcon\":\"https://cdn.joinbudapp.com/static/ic_xunzhang@3x.png\"}]},\"relation\":{\"subscribed\":1,\"friendship\":0}},\"ownerInfo\":{\"uid\":\"1460531478844608512\",\"userName\":\"wenwena\",\"userNick\":\"tww\",\"portraitUrl\":\"https://image-cdn.joinbudapp.com/filters:quality(30)/peopleHeadImg/14605314788446085121648449382065coverImg.png\",\"officialCert\":{\"accountClass\":1,\"certifications\":[{\"certName\":\"\",\"certIcon\":\"https://cdn.joinbudapp.com/static/ic_xunzhang@3x.png\"}]},\"relation\":{\"subscribed\":1,\"friendship\":0}},\"publishStatus\":0,\"isOwned\":0,\"ownersNum\":1,\"dcType\":1,\"batchStatus\":1,\"buyStatus\":1,\"nftType\":0},}";
        OnOpenUgcPageCallback(testJson);
    }

    private void UnityLocalTest_TestChangeUgcCloth()
    {
        string testJson =
            // "{\"templateId\":1,\"clothesJson\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothJson/1499630531066150912/1499630531066150912_1646403725_json_clothes.json\",\"clothesUrl\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothTex/1499630531066150912/1499630531066150912_1646403724_tex_clothes.zip\",\"mapId\":\"1499630531066150912_1646374940_5\",\"mapCover\":\"https://image-cdn.joinbudapp.com/UgcImage/1499630531066150912/1499630531066150912_1646403726_CoverImg_clothes.png\"}";
            "{\"isDc\":1,\"templateId\":3,\"clothesJson\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothJson/1493445391172452352/628BAF03-5423-4AFD-998D-C6C1F116678B.json\",\"clothesUrl\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothTex/1493445391172452352/2C295A64-3F17-4C63-90CC-7312300C8327.zip\",\"mapId\":\"1493445391172452352_1657877881_dc5\",\"mapCover\":\"https://cdn.joinbudapp.com/UgcImage/1493445391172452352/44F29E71-20AC-4B8E-B7E8-BBC459C49795.png\",\"dcInfo\":{\"itemId\":\"0xa48c7aa142b6b601555e76f853e5f7df0d810c90_534\",\"tokenId\":\"534\",\"itemName\":\"nft3-8\",\"itemDesc\":\"jfjf\",\"itemCover\":\"https://cdn.joinbudapp.com/UgcImage/1493445391172452352/44F29E71-20AC-4B8E-B7E8-BBC459C49795.png\",\"budActId\":\"f3db5\",\"supply\":\"60\",\"itemUgcType\":2,\"price\":\"90\",\"maticPrice\":\"0.9\",\"royalties\":\"2.5\",\"itemStatus\":1,\"walletAddress\":\"0xdbce412f86a025edf8988c455fda062cc6425265\",\"contractAddress\":\"0xa48c7aa142b6b601555e76f853e5f7df0d810c90\",\"tokenStandard\":\"ERC-1155\",\"operationTime\":1661171190,\"creatorInfo\":{\"uid\":\"1460531478844608512\",\"userName\":\"wenwena\",\"userNick\":\"tww\",\"portraitUrl\":\"https://image-cdn.joinbudapp.com/filters:quality(30)/peopleHeadImg/14605314788446085121648449382065coverImg.png\",\"officialCert\":{\"accountClass\":1,\"certifications\":[{\"certName\":\"\",\"certIcon\":\"https://cdn.joinbudapp.com/static/ic_xunzhang@3x.png\"}]},\"relation\":{\"subscribed\":1,\"friendship\":0}},\"ownerInfo\":{\"uid\":\"1460531478844608512\",\"userName\":\"wenwena\",\"userNick\":\"tww\",\"portraitUrl\":\"https://image-cdn.joinbudapp.com/filters:quality(30)/peopleHeadImg/14605314788446085121648449382065coverImg.png\",\"officialCert\":{\"accountClass\":1,\"certifications\":[{\"certName\":\"\",\"certIcon\":\"https://cdn.joinbudapp.com/static/ic_xunzhang@3x.png\"}]},\"relation\":{\"subscribed\":1,\"friendship\":0}},\"publishStatus\":0,\"isOwned\":0,\"ownersNum\":1,\"dcType\":1,\"batchStatus\":1,\"buyStatus\":1,\"nftType\":0}}";
        OnOpenUgcPageCallback(testJson);
    }
#endif

    #endregion
}