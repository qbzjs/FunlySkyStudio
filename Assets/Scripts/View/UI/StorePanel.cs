using Cinemachine;
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using BudEngine.NetEngine;
using UnityEngine.Events;
using System.Globalization;
using System.Runtime.CompilerServices;

public enum DataTypeEnum
{
    //0 地图 1 素材 2 衣服 3 space
    None,
    Resource,
    Clothing,
    Space
}
/// <summary>
/// Author:LiShuZhan
/// Description:沉浸购买，在购买时跳到Unity制作的物品详情页
/// Date: 2022.03.28
/// </summary>
public class StorePanel : InfoPanel<StorePanel>
{
    public Button closeArea;

    public RawImage creatorIcon;
    public RawImage itemPicture;
    public Image defaultIcon;
    public Image defaultPicture;

    public Text creatorName;
    public Text updetedTime;
    public Text itemName;
    public Text itemDic;
    public Text itemPurchasesNum;
    public Text likeNum;
    public Text limitText;
    public Text ownerNum;
    public Text totalNum;

    public Transform followParent;
    public Transform ownedParent;
    public Transform limitParent;
    public Transform likeParent;
    public Transform limitGroup;
    public Transform mainGroup;
    public CinemachineVirtualCamera cam;
    public CinemachineTransposer transposer;
    public GameObject player;
    public GameObject customBtn;
    public GameObject customShowBtn;
    public static Action<SceneEntity> onGet;
    public string ugcJson;

    private Coroutine GetPhotoCor;
    private string mapId;
    private string creatorId;
    private Vector3 camPos;
    private float zoomSpeed = 0.6f;
    private Tweener _tweener;
    private CanvasScaler canvas;
    private const float bgWight = 957;
    private DataTypeEnum currentType;
    private Action<Transform, int, GameObject, GameObject> refreshCustomBtn;
    private int cacheLike;
    private SceneEntity entity;
    
    enum SubscribedEnum
    {
        //0: 未关注 1 关注 2：被对方关注 3：互关  4：自己
        None,
        Requesting,
        IsRequested,
        Mutual,
        Me
    }

    enum IsOwnedEnum
    {
        //0：未拥有 1：拥有
        None,
        Purchased
    }

    enum BtnType
    {
        Follow,
        Owned,
        Like
    }

    enum IsLimitEnum
    {
        None,
        Restricted
    }

    enum LimitList
    {
        None,
        Restricted
    }

    enum PropsBuyLevel
    {
        None,
        Follow,
        Friend,
        Personal
    }

    enum IsLikeEnum
    {
        None,
        Liked
    }

    

    enum OperationType
    {
        //0(follow),1(unfollow),2(addFriend),3(cancelFriend)
        Follow,
        Unfollow,
        AddFriend,
        CancelFriend
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        closeArea.onClick.AddListener(OnClickClose);
        cam = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();
        transposer = cam.GetCinemachineComponent<CinemachineTransposer>();
        player = GameObject.Find("Play Mode Camera Center");
        InitPanel();
        AddBtnClick();
        canvas = GameObject.Find("Canvas").GetComponent<CanvasScaler>();
    }
    
    public void InitPanel()
    {
        RefreshResInfo("", "", 0, 0,IsLikeEnum.None);
        RefershCreatorInfo("", 0, SubscribedEnum.None, IsOwnedEnum.None,"");
        CheckRestricted(IsLimitEnum.None);
        CheckLimitList(LimitList.Restricted, LimitList.Restricted, IsOwnedEnum.None);
        RefershLimitGroupInfo(0, 0, LimitList.Restricted);
        CheckPropsBuyLevel(PropsBuyLevel.None, false);

        if (GetPhotoCor != null)
        {
            StopCoroutine(GetPhotoCor);
        }
        defaultIcon.gameObject.SetActive(true);
        defaultPicture.gameObject.SetActive(true);
        creatorIcon.gameObject.SetActive(false);
        itemPicture.gameObject.SetActive(false);
    }
    
    public void BindEntity(SceneEntity entity)
    {
        this.entity = entity;
    }
    
    
    void OnClickClose()
    {
        _tweener = cam.transform.DOMove(Zoom_OUT_CAM_FOLLOW_OFFSET, zoomSpeed);
        cam.transform.DORotate(Zoom_OUT_CAM_Rot_OFFSET, zoomSpeed);
        mainGroup.gameObject.SetActive(false);
        HideOrShowPanel(true);
        StartCoroutine("waitDoTween");
    }

    private void AddBtnClick()
    {
        likeParent.GetChild(0).GetComponent<ContinuousClickDetection>().AddListener(() => OnLikeOrOwnedBtnClick(BtnType.Like, (int)IsLikeEnum.None, "/ugcmap/setLike")) ;
        likeParent.GetChild(1).GetComponent<ContinuousClickDetection>().AddListener(() => OnLikeOrOwnedBtnClick(BtnType.Like, (int)IsLikeEnum.Liked, "/ugcmap/setLike")) ;
        ownedParent.GetChild(0).GetComponent<ContinuousClickDetection>().AddListener(() => OnLikeOrOwnedBtnClick(BtnType.Owned, (int)IsOwnedEnum.None, "/ugcmap/experience"));
        followParent.GetChild(0).GetComponent<ContinuousClickDetection>().AddListener(() => OnFollowBtnClick(SubscribedEnum.None));
        followParent.GetChild(2).GetComponent<ContinuousClickDetection>().AddListener(() => OnFollowBtnClick(SubscribedEnum.IsRequested));
    }

    private void OnLikeOrOwnedBtnClick(BtnType btnType,int type,string request)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            LoggerUtils.Log("Current Id Is Null Or Empty");
            return;
        }
        SetLikeMapInfo tempInfo = new SetLikeMapInfo
        {
            mapId = mapId,
            dataType = (int)currentType
        };
        SetLikeParm upLoadLikeBody = new SetLikeParm
        {
            mapInfo = tempInfo,
            operationType = type,
        };
        if (btnType == BtnType.Like)
        {
            OfflineUpdateLikeBtn(type);
        }
        HttpUtils.MakeHttpRequest(request, (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(upLoadLikeBody),
            (success) =>
            {
                if (btnType == BtnType.Owned)
                {
                    onGet?.Invoke(entity);
                    RefreshRelationBtn(btnType, type + 1);
                }
                LoggerUtils.Log("social set subscribe successed!");
            },
            (fail) =>
            {
                HttpResponseRaw responseDataRaw = GameUtils.GetHttpResponseRaw(fail);
                if (responseDataRaw.result <= 10000)
                {
                    TipPanel.ShowToast(responseDataRaw.rmsg);
                    return;
                }
                TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            });
    }
    
    private void OnFollowBtnClick(SubscribedEnum type)
    {
        if (string.IsNullOrEmpty(creatorId))
        {
            LoggerUtils.Log("Current Id Is Null Or Empty");
            return;
        }
        SetSubscribeParam req = new SetSubscribeParam()
        {
            toUid = creatorId,
            operationType = (int)OperationType.Follow,
        };
        LoggerUtils.Log("social -- setSubscribe : " + JsonUtility.ToJson(req));
        HttpUtils.MakeHttpRequest("/social/setSubscribe", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(req),
            (success) =>
            {
                RefreshRelationBtn(BtnType.Follow, (int)type + 1);
                UpdatePlayerInfo(ugcJson);
                LoggerUtils.Log("social set subscribe successed!");
                DataLogUtils.NewUserFollowers();
            },
            (fail) =>
            {
                LoggerUtils.Log("social set subscribe failed!");
            });
    }

    /// <summary>
    /// 外部自定义按钮行为
    /// </summary>
    /// <param name="text">要设置的按钮文本</param>
    /// <param name="onCustomAction">按钮按下的动作</param>
    public void SetOnCustomBtnClick(string text, Action<GameObject, GameObject> onCustomAction)
    {
        var cusText = customBtn.GetComponentInChildren<Text>();
        LocalizationConManager.Inst.SetLocalizedContent(cusText, text);

        var cusButton = customBtn.GetComponent<Button>();
        cusButton.onClick.RemoveAllListeners();
        cusButton.GetComponent<ContinuousClickDetection>().AddListener(() =>
        {
            if (onCustomAction != null)
            {
                onCustomAction(customBtn, customShowBtn);
            }
        });
    }
    
    
    public void SetOnCustomBtnIsShow(Action<Transform, int, GameObject, GameObject> action)
    {
        refreshCustomBtn = null;
        refreshCustomBtn = action;
    }

    public void UpdatePlayerInfo(string ugcJson)
    {
        if (!string.IsNullOrEmpty(ugcJson))
        {
            HttpUtils.MakeHttpRequest("/ugcmap/info", (int)HTTP_METHOD.GET, ugcJson, OnGetPlayerInfoSuccess, OnGetPlayerInfoFail);
        }
    }

    private void OnGetPlayerInfoSuccess(string content)
    {
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        StoreResData socialResInfo = JsonConvert.DeserializeObject<StoreResData>(hResponse.data);
        LoggerUtils.Log("content = " + hResponse.data);
        if (socialResInfo.mapInfo == null || socialResInfo.mapInfo.mapCreator == null)
        {
            LoggerUtils.LogError($"mapInfo data error:{socialResInfo}");
            return;
        }
        RefreshPanel(socialResInfo);
        GetPhoto(socialResInfo.mapInfo.mapCover, itemPicture);
        GetPhoto(socialResInfo.mapInfo.mapCreator.portraitUrl, creatorIcon);
    }

    private void RefreshPanel(StoreResData storeResInfo)
    {
        var mapInfo = storeResInfo.mapInfo;
        if (mapInfo != null)
        {
            RefershIdInfo(mapInfo.mapId, mapInfo.mapCreator.uid);
            RefreshResInfo(mapInfo.mapName, mapInfo.mapDesc, mapInfo.interactStatus.purchasesNum, mapInfo.interactStatus.likes,(IsLikeEnum)mapInfo.interactStatus.liked);
            RefershCreatorInfo(mapInfo.mapCreator.userName, mapInfo.lastModifiedTime, (SubscribedEnum)mapInfo.relation.subscribed, (IsOwnedEnum)mapInfo.interactStatus.isOwned,mapInfo.mapCreator.uid);
            RefershLimitGroupInfo(mapInfo.interactStatus.purchasesNum, mapInfo.buyNumLimit, (LimitList)mapInfo.limitList.numLimit);
            bool tempActive = CheckLimitList((LimitList)mapInfo.limitList.numLimit, (LimitList)mapInfo.limitList.shareWithLimit, (IsOwnedEnum)mapInfo.interactStatus.isOwned);
            CheckPropsBuyLevel((PropsBuyLevel)mapInfo.propsBuyLevel.propsType, tempActive);
        }
    }

    private void OnGetPlayerInfoFail(string content)
    {
        HttpResponseRaw responseDataRaw = GameUtils.GetHttpResponseRaw(content);
        if (responseDataRaw.result <= 10000)
        {
            TipPanel.ShowToast(responseDataRaw.rmsg);
            return;
        }
    }


    
    private void RefreshResInfo(string resName, string resDic, int resPurchasesNum, int resLikes, IsLikeEnum isLike)
    {
        LocalizationConManager.Inst.SetSystemTextFont(itemName);
        LocalizationConManager.Inst.SetSystemTextFont(itemDic);
        itemName.text = DataUtils.FilterNonStandardText(resName);
        itemDic.text = DataUtils.FilterNonStandardText(resDic);
        itemPurchasesNum.text = NumToString(resPurchasesNum);
        cacheLike = resLikes;
        likeNum.text = NumToString(cacheLike);
        RefreshRelationBtn(BtnType.Like, (int)isLike);
    }

    private void RefershCreatorInfo(string creatorName, int lastModifiedTime, SubscribedEnum subscribed,IsOwnedEnum isOwned,string playerUid)
    {
        this.creatorName.text = creatorName;
        TimestampConversion(lastModifiedTime);
        int tempFollowId = GameManager.Inst.ugcUserInfo.uid == playerUid ? (int)SubscribedEnum.Me : (int)subscribed;
        RefreshRelationBtn(BtnType.Follow, tempFollowId);
        RefreshRelationBtn(BtnType.Owned, (int)isOwned);
    }

    private void RefershIdInfo(string mapid,string creatoruid)
    {
        mapId = mapid;
        creatorId = creatoruid;
    }

    private void RefershLimitGroupInfo(int owners, int total, LimitList numLimit)
    {
        bool active = total > 0 ? true : false;
        limitGroup.gameObject.SetActive(active);
        LocalizationConManager.Inst.SetLocalizedContent(ownerNum, "{0} owners", owners);
        LocalizationConManager.Inst.SetLocalizedContent(totalNum, "{0} total", total);
    }

    private void RefreshRelationBtn(BtnType type, int state)
    {
        Transform btnTF = null;
        switch (type)
        {
            case BtnType.Follow:
                btnTF = followParent;
                break;
            case BtnType.Owned:
                btnTF = ownedParent;
                break;
            case BtnType.Like:
                btnTF = likeParent;
                break;
        }
        for (int i = 0; i < btnTF.childCount; i++)
        {
            btnTF.GetChild(i).gameObject.SetActive(state == i);
        }
        
        //显示外部custom按钮
        if (type == BtnType.Owned)
        {
            if (refreshCustomBtn != null)
            {
                refreshCustomBtn(btnTF, state, customBtn, customShowBtn);
            }
        }
    }

    IEnumerator LoadSprite(string url, RawImage image)
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (wr.result == UnityWebRequest.Result.Success)
        {
            image.texture = texDl.texture;
            var defaultImage = image == creatorIcon ? defaultIcon : defaultPicture;
            image.gameObject.SetActive(true);
            defaultImage.gameObject.SetActive(false);
        }
        else
        {
            LoggerUtils.LogError("OnLoadSpriteFail !");
        }
        texDl.Dispose();
        wr.Dispose();
    }

    private void GetPhoto(string photoUrl, RawImage image)
    {
        if (!string.IsNullOrEmpty(photoUrl))
        {
            GetPhotoCor = StartCoroutine(LoadSprite(photoUrl, image));
        }
    }

    private void TimestampConversion(int timestamp)
    {
        if (timestamp == 0)
        {
            LocalizationConManager.Inst.SetLocalizedContent(updetedTime, "Updated on {0}", 0);
            return;
        }
        DateTime dt = new DateTime(1970, 1, 1);
        dt = dt.AddSeconds(timestamp);
        dt = dt.ToLocalTime();
        string month = dt.ToString("MMMM", new CultureInfo("en-us")).Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
        string day = dt.ToString("dd", CultureInfo.InvariantCulture);
        LocalizationConManager.Inst.SetLocalizedContent(updetedTime, "Updated on {0},{1}", month, day);
    }

    private string NumToString(int num)
    {
        int b = num / 1000000000;
        int m = (num % 1000000000) / 1000000;
        int k = (num % 1000000) / 1000;
        int p = num % 1000;

        if (b > 0)
        {
            return string.Format("{0}.{1}B", b, m / 100);
        }
        else if (m > 0)
        {
            return string.Format("{0}.{1}M", m, k / 100);
        }
        else if (k > 0)
        {
            return string.Format("{0}.{1}K", k, p / 100);
        }
        else
        {
            return p.ToString();
        }
    }

    private void CheckRestricted(IsLimitEnum isLimit)
    {
        var tempLimit = isLimit == IsLimitEnum.None ? IsLimitEnum.None : IsLimitEnum.Restricted;
        if (isLimit == IsLimitEnum.None)
        {
            limitParent.gameObject.SetActive(false);
        }
        else if(isLimit == IsLimitEnum.Restricted)
        {
            limitParent.gameObject.SetActive(true);
        }
    }

    private bool CheckLimitList(LimitList numLimit, LimitList shareLimit, IsOwnedEnum isOwnedEnum)
    {
        var tempNumLimit = numLimit == LimitList.None ? LimitList.None : LimitList.Restricted;
        var tempShareLimit = shareLimit == LimitList.None ? LimitList.None : LimitList.Restricted;

        if (tempNumLimit == LimitList.None && tempShareLimit == LimitList.None)
        {
            if (isOwnedEnum == IsOwnedEnum.Purchased) { return true; }
            RefreshRelationBtn(BtnType.Owned, 0);
            return true;
        }
        else
        {
            if (isOwnedEnum == IsOwnedEnum.Purchased) { return false; }
            RefreshRelationBtn(BtnType.Owned, 2);
            return false;
        }
    }

    private void CheckPropsBuyLevel(PropsBuyLevel propsBuyLevel, bool isHaveLevel)
    {
        switch (propsBuyLevel)
        {
            case PropsBuyLevel.None:
                break;
            case PropsBuyLevel.Follow:
                SetLimitText("Followers Only : only creator's followers can purchase this prop.");
                break;
            case PropsBuyLevel.Friend:
                SetLimitText("Selected Only : only users selected can purchase this prop.");
                break;
            case PropsBuyLevel.Personal:
                if (isHaveLevel)
                {
                    SetLimitText("Private : only you can purchase this prop.");
                }
                else
                {
                    SetLimitText("Private : only creator can use this prop.");
                }
                break;
        }
    }

    private void SetLimitText(string text)
    {
        limitParent.gameObject.SetActive(true);
        LocalizationConManager.Inst.SetLocalizedContent(limitText, text);
    }

    private void OfflineUpdateLikeBtn(int likeEnum)
    {
        int tempNum = likeEnum == (int)IsLikeEnum.Liked ? 1 : -1;
        cacheLike -= tempNum;
        likeNum.text = NumToString(cacheLike);
        RefreshRelationBtn(BtnType.Like, likeEnum - tempNum);
    }

    private Vector3 Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(4,10,-10);
    private Vector3 Zoom_IN_CAM_Rot_OFFSET = new Vector3(30, 0, 0);
    private Vector3 Zoom_OUT_CAM_FOLLOW_OFFSET = new Vector3(0,0,0);
    private Vector3 Zoom_OUT_CAM_Rot_OFFSET = new Vector3(0, 0, 0);
    public float temp = 0;
    public void SetZoom(Transform target)
    {
        Zoom_OUT_CAM_FOLLOW_OFFSET = cam.transform.position;
        Zoom_OUT_CAM_Rot_OFFSET = cam.transform.rotation.eulerAngles;
        camPos = cam.transform.position;
        cam.LookAt = null;
        cam.Follow = null;
        var bounding = DataUtils.CalculateBoundingBox(target);
        if (target.GetComponentInChildren<NodeBehaviour>())
        {
            SetOrthCameraSize(bounding.center.x - bounding.extents.x, bounding.center.x + bounding.extents.x,
                bounding.center.y - bounding.extents.y, bounding.center.y + bounding.extents.y);
        }
        else
        {
            SetCombineSize(target);
        }
        float canvspri = (canvas.referenceResolution.x - bgWight) / bgWight;
        Vector3 forwardOffset = cam.transform.forward.normalized * Zoom_IN_CAM_FOLLOW_OFFSET.z;
        Vector3 rightOffset = cam.transform.right.normalized * canvspri * Zoom_IN_CAM_FOLLOW_OFFSET.x;
        cam.transform.DOMove(target.position + forwardOffset + rightOffset, 0.6f);
    }

    //聚焦到Player正面
    public void SetZoomInPlayer(Transform target)
    {
        if (Zoom_OUT_CAM_FOLLOW_OFFSET == Vector3.zero)
        {
            Zoom_OUT_CAM_FOLLOW_OFFSET = cam.transform.position;
            Zoom_OUT_CAM_Rot_OFFSET = cam.transform.rotation.eulerAngles;
        }

        camPos = cam.transform.position;
        cam.LookAt = null;
        cam.Follow = null;

        //尝试从人物身上获取
        var playerRender = target.GetComponentsInChildren<CapsuleCollider>();
        if (playerRender == null || playerRender.Length <= 0) return;
        CapsuleCollider maxSize = playerRender[0];
        int offsetValue = 2;

        if(maxSize.bounds.size.x > maxSize.bounds.size.y)
        {
            Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(maxSize.bounds.size.x / offsetValue, 0, -maxSize.bounds.size.x * offsetValue);
        }
        else
        {
            Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(maxSize.bounds.size.y / offsetValue, 0, -maxSize.bounds.size.y * offsetValue);
        }
        
        float canvspri = (canvas.referenceResolution.x - bgWight) / bgWight;
        Vector3 forwardOffset = cam.transform.forward.normalized * Zoom_IN_CAM_FOLLOW_OFFSET.z;
        Vector3 rightOffset = cam.transform.right.normalized * canvspri * Zoom_IN_CAM_FOLLOW_OFFSET.x;

        //人物转向camera
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.playerAnim && !StateManager.IsOnSeesaw && !StateManager.IsOnSwing)
        {
            PlayerBaseControl.Inst.playerAnim.transform.localRotation = Quaternion.Euler(new Vector3(0,-180,0));
        }

        cam.transform.DOMove(target.position + forwardOffset + rightOffset, 0.6f);
    }

    private void SetOrthCameraSize(float xmin, float xmax, float ymin, float ymax)
    {
        float xDis = xmax - xmin;
        float yDis = ymax - ymin;
        int offsetValue = 2;
        if (xDis > yDis)
        {
            Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(xDis/ offsetValue, 0, -xDis* offsetValue);
        }
        else
        {
            Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(yDis / offsetValue, 0, -yDis* offsetValue);
        }
    }

    private void SetCombineSize(Transform target)
    {
        if (target != null)
        {
            var renders = target.GetComponentsInChildren<MeshCollider>();
            if (renders != null && renders.Length > 0)
            {
                MeshCollider maxSize = renders[0];
                int offsetValue = 2;
                foreach (var item in renders)
                {
                    if(Vector3.SqrMagnitude(item.bounds.size) > Vector3.SqrMagnitude(maxSize.bounds.size))
                    {
                        maxSize = item;
                    }
                }
                if(maxSize.bounds.size.x > maxSize.bounds.size.y)
                {
                    Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(maxSize.bounds.size.x / offsetValue, 0, -maxSize.bounds.size.x * offsetValue);
                }
                else
                {
                    Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(maxSize.bounds.size.y / offsetValue, 0, -maxSize.bounds.size.y * offsetValue);
                }
            }
        }
    }

    private IEnumerator waitDoTween()
    {
        yield return _tweener.WaitForCompletion();
        cam.LookAt = player.transform;
        cam.Follow = player.transform;
        Hide();
        
        refreshCustomBtn = null;
        entity = null;
    }

    public void HideOrShowPanel(bool Active)
    {
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(Active);
        }
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.gameObject.SetActive(Active);
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.gameObject.SetActive(Active);
        }
        if (AttackWeaponCtrlPanel.Instance && PlayerAttackControl.Inst)
        {
            if (PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon == null)
            {
                return;
            }

            if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
                ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater)||
                StateManager.IsOnLadder || StateManager.IsOnSeesaw || StateManager.IsOnSwing
                || StateManager.IsOnSlide)
            {
                return;
            }

            AttackWeaponCtrlPanel.Instance.gameObject.SetActive(Active);
        }
        if (ShootWeaponCtrlPanel.Instance && PlayerShootControl.Inst)
        {
            if (PlayerShootControl.Inst.curShootPlayer.HoldWeapon == null)
            {
                return;
            }

            if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
                ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater)||
                (StateManager.IsOnLadder)|| StateManager.IsOnSeesaw || StateManager.IsOnSwing
                ||StateManager.IsOnSlide)
            {
                return;
            }

            ShootWeaponCtrlPanel.Instance.gameObject.SetActive(Active);
        }
        if (Active)
        {
            SetShowPanel();
        }
        else
        {
            SetHidePanel();
        }
        if (BaggagePanel.Instance)
        {
            BaggagePanel.Instance.gameObject.SetActive(Active);
        }

        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetHpPanelVisible(Active);
        }

        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(Active);
        }

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(Active);
        }

        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(Active);
        }
    }

    private void SetHidePanel()
    {
        if (RoomChatPanel.Instance)
        {
            RoomChatPanel.Hide();
        }
    }

    private void SetShowPanel()
    {
        if (RoomChatPanel.Instance)
        {
            RoomChatPanel.Show();
        }
    }

    public void ClickShoping(string ugcJson,Transform resTrans,DataTypeEnum typeEnum = DataTypeEnum.Resource)
    {
        currentType = typeEnum;
        HideOrShowPanel(false);
        mainGroup.gameObject.SetActive(true);
        InitPanel();
        this.ugcJson = ugcJson;
        UpdatePlayerInfo(ugcJson);
        SetZoom(resTrans);
        StopCoroutine("waitDoTween");
        SteeringWheelManager.Inst.OnPanelReset();
    }

    public void RePlayerCam()
    {
        cam.LookAt = player.transform;
        cam.Follow = player.transform;
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.gameObject.SetActive(true);
        }
        if (AttackWeaponCtrlPanel.Instance && PlayerAttackControl.Inst)
        {
            if (PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
            {
                AttackWeaponCtrlPanel.Instance.gameObject.SetActive(true);
            }
        }
        if (ShootWeaponCtrlPanel.Instance && PlayerShootControl.Inst)
        {
            if (PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
            {
                ShootWeaponCtrlPanel.Instance.gameObject.SetActive(true);
            }
        }
        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(true);
        }
    }
}