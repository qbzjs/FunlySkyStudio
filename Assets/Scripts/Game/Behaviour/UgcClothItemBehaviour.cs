using System;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using RTG;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public enum DCItemStatus
{
    Created,
    Listed,
    Sold
}
public enum DetailPageDCType
{
    owned = 0,
    listing = 1,
    resell = 2,
}
/// <summary>
/// Author: Shaocheng
/// Description: UGC衣服道具行为
/// Date: 2022年4月20日16:13:21
/// </summary>
public class UgcClothItemBehaviour : NodeBaseBehaviour
{
    private static MaterialPropertyBlock _mpb;
    private Color[] _oldColor;

    private GameObject _defaultNode;
    private GameObject _touchNode;
    public GameObject soldOut;
    public bool isSoldOut;
    public Texture ugcClothCoverTexture;
    private BoxCollider dcPartObjBc;
    private BoxCollider originjBc;

    private Action<NodeBaseBehaviour> onSetAsset;
    private GameObject dcPartObj;

    public void AddOnSetAssetAction(Action<NodeBaseBehaviour> ac)
    {
        onSetAsset += ac;
        if(dcPartObj != null)ac?.Invoke(this);
    }
    
    
    private const string dcPartObjName = "dcPartObj";
    private Vector3 originSize = new Vector3(1.1f, 1.1f, 0.2f);
    
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        onSetAsset = null;
        if (_mpb == null)
        {
            _mpb = new MaterialPropertyBlock();
        }

        _defaultNode = gameObject.transform.Find("default").gameObject;
        _touchNode = gameObject.transform.Find("clickTouch").gameObject;
        soldOut = gameObject.transform.Find("soldout").gameObject;
        
        var abTrs = gameObject.transform.Find(dcPartObjName);
        bool isPGC = false;
        if (abTrs != null)
        {
            dcPartObj = abTrs.gameObject;
            if (entity != null && entity.HasComponent<UGCClothItemComponent>())
            {
                var data = entity.Get<UGCClothItemComponent>();
                isPGC = data.pgcId >= 100000;
            }
        }

        originjBc = _touchNode.GetComponent<BoxCollider>();
        if (originSize == Vector3.zero && !isPGC)
        {
            //使用对象池时需要做还原
            originSize = originjBc.size;
        }

        originjBc.size = originSize;
        _touchNode.transform.localEulerAngles = Vector3.zero;
        
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    private void OnBecameVisible()
    {
        SetCanBuyInMap();
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        //双人牵手不让进行操作
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            return;
        }

        if (entity.HasComponent<UGCPropComponent>() && entity.Get<UGCPropComponent>().isTradable == 1)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(entity.HasComponent<UGCClothItemComponent>()&& entity.Get<UGCClothItemComponent>().isDc != 0 ? PortalPlayPanel.IconName.Dc : PortalPlayPanel.IconName.Shopping);
            PortalPlayPanel.Instance.AddButtonClick(OnClickShop);
            PortalPlayPanel.Instance.SetTransform(transform);
        }
    }
    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    public void SetCanBuyInMap()
    {
        if (entity.HasComponent<UGCPropComponent>())
        {
            var uComp = entity.Get<UGCPropComponent>();
            _touchNode.layer = uComp.isTradable == 1 ? LayerMask.NameToLayer("Touch") : LayerMask.NameToLayer("Model");
            if (dcPartObjBc != null) dcPartObjBc.gameObject.layer = _touchNode.layer;
        }
    }
    private string ugcJson;
    private bool isDCRequesting;
    private void OnClickShop()
    {
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.animCon.IsChanging)
        {
            return;
        }
        if (isDCRequesting)
        {
            return;
        }
        UGCClothItemComponent com = entity.Get<UGCClothItemComponent>();
        if (string.IsNullOrEmpty(com.clothMapId))
            return;
        //DCResInfo info = new DCResInfo()
        //{
        //    itemId = "0x7ad8fb5c2cb47c5327a874aa47f1918c2917f4a0_3754",
        //    budActId = "f58ea"
        //};
        //var ugcJson = JsonConvert.SerializeObject(info);
        //if (!string.IsNullOrEmpty(ugcJson))
        //{
        //    HttpUtils.MakeHttpRequest("/ugcmap/itemInfo", (int)HTTP_METHOD.GET, ugcJson, OnDCPlayerInfoSuccess, OnGetPlayerInfoFail);
        //    return;
        //}

        if (com.isDc == 1)
        {
            DCResInfo info = new DCResInfo()
            {
                itemId = com.dcId,
                budActId = com.budActId,
                classifyType = com.classifyType,
                pgcId = com.pgcId,
            };
            ugcJson = JsonConvert.SerializeObject(info);
            if (!string.IsNullOrEmpty(ugcJson))
            {
                HttpUtils.MakeHttpRequest("/ugcmap/itemInfo", (int)HTTP_METHOD.GET, ugcJson, OnDCPlayerInfoSuccess, OnGetPlayerInfoFail);
                isDCRequesting = true;
            }

        }
        else
        {
            NativeResInfo ugcMapInfo = new NativeResInfo
            {
                type = 2,
                mapId = com.clothMapId
            };
            ugcJson = JsonConvert.SerializeObject(ugcMapInfo);
            LoggerUtils.Log("open ugc cloth shop => " + ugcJson);
            if (!string.IsNullOrEmpty(ugcJson))
            {
                StorePanel.Show();
                StorePanel.Instance.BindEntity(entity);
                StorePanel.Instance.SetOnCustomBtnIsShow(OnControlStoreShow);
                StorePanel.Instance.SetOnCustomBtnClick(UgcClothItemManager.WEAR_BTN_TEXT, OnStorePanelWearClick);
                StorePanel.Instance.ClickShoping(ugcJson, transform, DataTypeEnum.Clothing);
                DataLogUtils.DetailPageView(ugcMapInfo.mapId, null, "clothing");//上报衣服详情页预览
            }

        }

    }
    private void OnDCPlayerInfoSuccess(string content)
    {
        isDCRequesting = false;
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        StoreResData socialResInfo = JsonConvert.DeserializeObject<StoreResData>(hResponse.data);
        LoggerUtils.Log("content:"+hResponse.data);
        if (socialResInfo.mapInfo == null || socialResInfo.mapInfo.mapCreator == null || socialResInfo.mapInfo.dcInfo == null)
        {
            LoggerUtils.LogError($"mapInfo data error:{socialResInfo}");
            return;
        }
        if (socialResInfo.mapInfo.dcInfo.itemStatus == (int)DCItemStatus.Sold)
        {
            SetSoldOut();
            OnSoldOut(socialResInfo.mapInfo.dcInfo.itemId, socialResInfo.mapInfo.dcInfo.walletAddress);
            return;
        }
        DCResPanel.Show();
        DCResInfo info = JsonConvert.DeserializeObject<DCResInfo>(ugcJson);
        DCResPanel.Instance.SetOwnedPanel(string.IsNullOrEmpty(info.budActId));
        DCResPanel.Instance.OnClickDC(ugcJson, transform, DataTypeEnum.Clothing);
        DCResPanel.Instance.InfoSuccess(socialResInfo);
        DCResPanel.Instance.HideOrShowPanel(false);
        DataLogUtils.DetailPageView(socialResInfo.mapInfo.mapId, socialResInfo.mapInfo.dcInfo, "clothing");//上报衣服详情页预览
    }
    private void OnSoldOut(string id,string address)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            DcSaveInfo info = new DcSaveInfo()
            {
                dcId = id,
                address = address
            };
            CustomData data = new CustomData();
            data.type = (int)ChatCustomType.DCClothSoldOut;
            data.data = JsonConvert.SerializeObject(info);
            RoomChatData roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Custom,
                data = JsonConvert.SerializeObject(data),
            };
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }
    }
    private void OnGetPlayerInfoFail(string content)
    {
        LoggerUtils.LogError($"OnGetPlayerInfoFail:{content}");
        isDCRequesting = false;

    }
    private void OnControlStoreShow(Transform t, int state, GameObject wearObj, GameObject woreObj)
    {
        //只处理Owned后的情况
        if (state != 1)
        {
            return;
        }

        var isWearing = false;

        RoleData roleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        var comp = entity.Get<UGCClothItemComponent>();
        if (roleData != null && !string.IsNullOrEmpty(comp.clothMapId))
        {
            var curClothMapId = comp.clothMapId;
            switch (comp.dataSubType)
            {
                case (int)DataSubType.Clothes:
                    isWearing = curClothMapId.Equals(roleData.clothMapId);
                    break;
                case (int)DataSubType.Patterns:
                    if (roleData.ugcFPData.ugcMapId != null)
                    {
                        isWearing = curClothMapId.Equals(roleData.ugcFPData.ugcMapId);
                    }
                    break;
            }
        }
        //已穿上，显示wore
        t.GetChild(state).gameObject.SetActive(false);
        wearObj.SetActive(!isWearing);
        woreObj.SetActive(isWearing);
    }
    private void SetClothesData(UGCClothItemComponent ugcClothItemCmp, RoleData roleData)
    {
        ClothStyleData clothesData = RoleConfigDataManager.Inst.GetClothesByTemplateId(ugcClothItemCmp.templateId);
        if (clothesData == null)
        {
            return;
        }
        roleData.cloId = clothesData.id;
        roleData.clothesJson = ugcClothItemCmp.clothesJson;
        roleData.clothesUrl = ugcClothItemCmp.clothesUrl;
        roleData.clothMapId = ugcClothItemCmp.clothMapId;
        roleData.sceneType = 1;
        string newImageJson = JsonConvert.SerializeObject(roleData);
    }
    private void SetPatternData(UGCClothItemComponent ugcClothItemCmp, RoleData roleData)
    {
        PatternStyleData patternData = RoleConfigDataManager.Inst.GetPatternByTemplateId(ugcClothItemCmp.templateId);
        if (patternData == null)
        {
            return;
        }
        roleData.fpId = patternData.id;
        roleData.ugcFPData = new UgcResData
        {
            ugcJson = ugcClothItemCmp.clothesJson,
            ugcMapId = ugcClothItemCmp.clothMapId,
            ugcUrl = ugcClothItemCmp.clothesUrl,
            ugcType = (int)UGCClothesResType.UGC
        };
        roleData.sceneType = 1;
        string newImageJson = JsonConvert.SerializeObject(roleData);
    }

    private void OnStorePanelWearClick(GameObject wearObj, GameObject woreObj)
    {
        //在方向盘上，先下车
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            SteeringWheelManager.Inst.SendGetOffCar();
        }

        var ugcClothItemCmp = entity.Get<UGCClothItemComponent>();
        if (ugcClothItemCmp == null || ugcClothItemCmp.templateId == 0)
        {
            return;
        }

        var playerNode = UgcClothItemManager.Inst.FindPlayerNode();

        #region 穿衣服参数封装

        RoleData roleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        switch (ugcClothItemCmp.dataSubType)
        {
            case (int)DataSubType.Clothes:
                SetClothesData(ugcClothItemCmp, roleData);
                break;
            case (int)DataSubType.Patterns:
                SetPatternData(ugcClothItemCmp, roleData);
                break;
        }
        string newImageJson = JsonConvert.SerializeObject(roleData);

        RoleUpLoadBody roleUpLoadBody = new RoleUpLoadBody();
        UserInfo paraUserinfo = new UserInfo()
        {
            imageJson = newImageJson,
            clothesId = RoleLoadManager.GetUgcMapIds(roleData)
            //TODO : 获取换装后DC信息(目前传null,后端重新获取)
        };
        roleUpLoadBody.userInfo = paraUserinfo;

        LoggerUtils.Log("ChangeCloth:" + JsonConvert.SerializeObject(roleUpLoadBody));

        #endregion

        //试玩模式 && 游玩模式
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            LoggerUtils.Log($"Get on ugc cloth success in play mode!");
            UgcClothItemManager.Inst.SetSourceUserInfo(GameManager.Inst.ugcUserInfo);
            UgcClothItemManager.Inst.ChangeClothInStorePanel(playerNode, ugcClothItemCmp, wearObj, woreObj);
            UgcClothItemManager.Inst.UpdateUserinfoCloth(paraUserinfo.imageJson, paraUserinfo.clothesId);
        }
        else
        {
            
            HttpUtils.MakeHttpRequest("/image/setImage", (int) HTTP_METHOD.POST, JsonConvert.SerializeObject(roleUpLoadBody), (s) =>
                {
                    LoggerUtils.Log($"Get on ugc cloth success!:{s}");
                    ugcClothItemCmp.imageJson = paraUserinfo.imageJson;
                    ugcClothItemCmp.clothesId = paraUserinfo.clothesId;
                    UgcClothItemManager.Inst.SendChangeClothMsg(ugcClothItemCmp);
                    UgcClothItemManager.Inst.ChangeClothInStorePanel(playerNode, ugcClothItemCmp, wearObj, woreObj);
                    UgcClothItemManager.Inst.UpdateUserinfoCloth(paraUserinfo.imageJson, paraUserinfo.clothesId);
                    UgcClothItemManager.Inst.isNotifyRefreshPlayer = true;
                }
                , (s) =>
                {
                    LoggerUtils.Log($"Get on ugc cloth failed!:{s}");
                    TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
                });
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref _oldColor);
    }

    public void LoadUgcCloth(ClothStyleData clothData, Action onSuccess = null, Action onFail = null)
    {
        if (dcPartObj != null)
        {
            dcPartObj.SetActive(false);
        }

        originjBc.size = originSize;

        if (clothData == null || clothData.templateId == 0)
        {
            this.gameObject.SetActive(true);
            _defaultNode.gameObject.SetActive(true);
            onFail?.Invoke();
            return;
        }

        this.gameObject.SetActive(false);
        switch (clothData.dataSubType)
        {
            case (int)DataSubType.Clothes:
                ClothLoadManager.Inst.LoadUGCClothRes(clothData, CreateUgcClothGo(clothData), () =>
                { OnLoadUGCSucceed(onSuccess); }, () => { OnLoadUGCFail(onFail); });
                break;
            case (int)DataSubType.Patterns:
                LoadUgcResData loadUgcResData = new LoadUgcResData()
                {
                    clothesUrl = clothData.clothesUrl,
                    templateId = clothData.templateId,
                };
                ClothLoadManager.Inst.LoadUGCPatternsRes(loadUgcResData, CreateUgcClothGo(clothData), () =>
                { OnLoadUGCSucceed(onSuccess); }, () => { OnLoadUGCFail(onFail); });
                break;
        }

    }
    private void OnLoadUGCSucceed(Action onSuccess)
    {
        this.gameObject.SetActive(true);
        HideDefaultNode();
        SetCanBuyInMap();
        onSuccess?.Invoke();
    }
    private void OnLoadUGCFail(Action onFail)
    {
        this.gameObject.SetActive(true);
        _defaultNode.gameObject.SetActive(true);
        onFail?.Invoke();
    }

    public void AddComponentData(ClothStyleData clothData, string coverUrl,int isdc,DCInfo info,string walletAddress)
    {
        if (entity == null || clothData == null)
        {
            return;
        }

        entity.Get<UGCClothItemComponent>().templateId = clothData.templateId;
        entity.Get<UGCClothItemComponent>().clothMapId = clothData.clothMapId;
        entity.Get<UGCClothItemComponent>().clothesUrl = clothData.clothesUrl;
        entity.Get<UGCClothItemComponent>().clothCover = coverUrl;
        entity.Get<UGCClothItemComponent>().isDc = isdc;
        entity.Get<UGCClothItemComponent>().dcId = info==null ? null:info.itemId;
        entity.Get<UGCClothItemComponent>().walletAddress = walletAddress;
        entity.Get<UGCClothItemComponent>().budActId = info == null ? null : info.budActId;
        entity.Get<UGCClothItemComponent>().classifyType = clothData.classifyType;
        entity.Get<UGCClothItemComponent>().pgcId = clothData.pgcId;
        entity.Get<UGCClothItemComponent>().dataSubType = clothData.dataSubType;
    }
    private GameObject CreateUgcClothGo(ClothStyleData clothData)
    {
        if (transform.Find("UGCClothes") != null)
        {
            Destroy(transform.Find("UGCClothes").gameObject);
        }
        var model = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/UGCClothes/UGCClothes_" + clothData.templateId);
        var _ugcClothNode = Instantiate(model, this.gameObject.transform);
        _ugcClothNode.name = "UGCClothes";
        _ugcClothNode.transform.localPosition = new Vector3(0, -0.5f, 0);
        _ugcClothNode.transform.localScale = new Vector3(2, 2, 2);
        _ugcClothNode.transform.localEulerAngles = new Vector3(0, -180, 0);

        foreach (var go in _ugcClothNode.GetAllChildrenAndSelf())
        {
            if (go)
            {
                go.layer = LayerMask.NameToLayer("Model");
            }
        }

        HideDefaultNode();
        if (isSoldOut)
        {
            _ugcClothNode.SetActive(false);
        }
        return _ugcClothNode;
    }

    private void HideDefaultNode()
    {
        if (_defaultNode == null)
        {
            _defaultNode = gameObject.transform.Find("default").gameObject;
        }

        _defaultNode.SetActive(false);
    }
    public void SetSoldOut()
    {
        if (isSoldOut==false)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            soldOut.SetActive(true);
            if (GlobalFieldController.CurGameMode == GameMode.Edit)
            {
                _touchNode.SetActive(true);
                if (dcPartObjBc != null) dcPartObjBc.enabled = true;
            }
            isSoldOut = true;
        }
       
    }
    public void OnModeChange(GameMode mode)
    {
        if (isSoldOut)
        {
            _touchNode.SetActive(mode == GameMode.Edit);
            if (dcPartObjBc != null) dcPartObjBc.enabled = mode == GameMode.Edit;
        }
    }

    public void loadAB(ClothStyleData clothData, Action onSuccess = null, Action onFail = null)
    {
        var e = entity.Get<UGCClothItemComponent>();
        if (dcPartObj != null)
        {
            if (e.classifyType == clothData.classifyType && e.pgcId == clothData.pgcId)
            {
                dcPartObj.SetActive(true);
                onSuccess?.Invoke();
                return;
            }
            Destroy(dcPartObj);
        }
        
        var roleIconData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId((ClassifyType)clothData.classifyType ,clothData.pgcId);
        if (roleIconData != null)
        {
            BundleMgr.Inst.LoadBundle((BundlePart)clothData.classifyType, roleIconData.texName, ab =>
            {
                var go = ab.LoadAsset<GameObject>(roleIconData.modelName);
                if (go != null)
                {
                    dcPartObj = Instantiate(go);
                    dcPartObj.name = dcPartObjName;
                    var trs = dcPartObj.transform;
                    trs.SetParent(transform);
                    trs.localPosition = Vector3.zero;
                    trs.localScale = Vector3.one;
                    trs.localEulerAngles = Vector3.zero;
                    HideDefaultNode();
                    var ugc = transform.Find("UGCClothes");
                    if (ugc != null)
                    {
                        ugc.gameObject.SetActive(false);
                    }
                    var mr = dcPartObj.GetComponentInChildren<MeshRenderer>();
                    GameObject obj;
                    SkinnedMeshRenderer smr;
                    if (mr == null)
                    {
                        smr = dcPartObj.GetComponentInChildren<SkinnedMeshRenderer>();
                        obj = smr.gameObject;
                    }
                    else
                    {
                        obj = mr.gameObject;
                    }
                    dcPartObjBc = obj.AddComponent<BoxCollider>();
                    dcPartObjBc.gameObject.layer = _touchNode.layer;
                    originjBc.size = Vector3.zero;;
                    onSuccess?.Invoke();
                    onSetAsset?.Invoke(this);
                }
                else
                {
                    onFail?.Invoke();
                }
            }, () =>
            {
                onFail?.Invoke();
            }, clothData.abUrl);
        }
    }
}
