/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/13 17:23:54
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;

public class DCResPanel : InfoPanel<DCResPanel>
{
    public Text itemName;
    public Text itemDic;
    public Text creatorName;
    public Text soldNum;
    public Text totalNum;
    public Text priceNum;
    public GameObject coinGroup;
    public GameObject dcGroup;
    public Button closeArea;
    public RawImage itemPicture;
    public RawImage creatorIcon;
    public Text likeNum;
    private Coroutine GetPhotoCor;
    public Image defaultIcon;
    public Image defaultPicture;
    public Transform followParent;
    public Transform likeParent;
    public Transform mainGroup;
    private Tweener _tweener;
    public CinemachineVirtualCamera cam;
    public CinemachineTransposer transposer;
    public GameObject player;
    private CanvasScaler canvas;
    private float zoomSpeed = 0.6f;
    private const float bgWight = 957;
    private string creatorId;
    private string mapId;
    public string ugcJson;
    private DCResInfo ugcJsonInfo;
    private Vector3 Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(4, 10, -10);
    private Vector3 Zoom_IN_CAM_Rot_OFFSET = new Vector3(30, 0, 0);
    private Vector3 Zoom_OUT_CAM_FOLLOW_OFFSET = new Vector3(0, 0, 0);
    private Vector3 Zoom_OUT_CAM_Rot_OFFSET = new Vector3(0, 0, 0);
    private DataTypeEnum currentType;
    private int curLikeNum;
    private bool hideFollowRoot;

    [SerializeField] private Image banner;
    private SpriteAtlas gameAtlas;

    private readonly List<BundlePart> specialZoomPart = new List<BundlePart>()
    {
        BundlePart.Hats,
        BundlePart.Glasses,
        BundlePart.Hand,
        BundlePart.Bag,
    };

    enum SubscribedEnum
    {
        //0: 未关注 1 关注 2：被对方关注 3：互关  4：自己
        None,
        Requesting,
        IsRequested,
        Mutual,
        Me
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
    public float temp = 0;
    public void SetZoom(Transform target)
    {
        Zoom_OUT_CAM_FOLLOW_OFFSET = cam.transform.position;
        Zoom_OUT_CAM_Rot_OFFSET = cam.transform.rotation.eulerAngles;

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
     
        if (specialZoomPart.Contains((BundlePart)ugcJsonInfo.classifyType))
        {
            cam.transform.DOMove(GetParentAnchor(target).position + forwardOffset + rightOffset, 0.6f);
            return;
        }

        cam.transform.DOMove(target.position + forwardOffset + rightOffset, 0.6f);
        
    }
    private void SetCombineSize(Transform target)
    {
        if (target != null)
        {
            var renders = target.GetComponentsInChildren<Collider>();
            if (renders != null && renders.Length > 0)
            {
                Collider maxSize = renders[0];
                int offsetValue = 2;
                foreach (var item in renders)
                {
                    if (Vector3.SqrMagnitude(item.bounds.size) > Vector3.SqrMagnitude(maxSize.bounds.size))
                    {
                        maxSize = item;
                    }
                }
                if (maxSize.bounds.size.x > maxSize.bounds.size.y)
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
    private void SetOrthCameraSize(float xmin, float xmax, float ymin, float ymax)
    {
        float xDis = xmax - xmin;
        float yDis = ymax - ymin;
        int offsetValue = 2;
        if (xDis > yDis)
        {
            Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(xDis / offsetValue, 0, -xDis * offsetValue);
        }
        else
        {
            Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(yDis / offsetValue, 0, -yDis * offsetValue);
        }
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
    private void AddBtnClick()
    {
        likeParent.GetChild(0).GetComponent<Button>().onClick.AddListener(() => OnLikeBtnClick((int)IsLikeEnum.None));
        likeParent.GetChild(1).GetComponent<Button>().onClick.AddListener(() => OnLikeBtnClick((int)IsLikeEnum.Liked));
        followParent.GetChild(0).GetComponent<Button>().onClick.AddListener(() => OnFollowBtnClick(SubscribedEnum.None));
        followParent.GetChild(2).GetComponent<Button>().onClick.AddListener(() => OnFollowBtnClick(SubscribedEnum.IsRequested));
    }
    private void OnLikeBtnClick( int type)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            LoggerUtils.Log("Current Id Is Null Or Empty");
            return;
        }
        DCInfo info = JsonConvert.DeserializeObject<DCInfo>(ugcJson);
        if (info==null)
        {
            return;
        }
        SetLikeMapInfo tempInfo = new SetLikeMapInfo
        {
            mapId = mapId,
            dataType = (int)currentType,
            dcInfo = info
        };
        SetLikeParm upLoadLikeBody = new SetLikeParm
        {
            mapInfo = tempInfo,
            operationType = type,
        };
        OfflineUpdateLikeBtn(type);
        HttpUtils.MakeHttpRequest("/ugcmap/setLike", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(upLoadLikeBody),
            (success) =>
            {
                LoggerUtils.Log("social set subscribe successed!");
            },
            (fail) =>
            {
                LoggerUtils.LogError("Script:DCResPanel OnLikeBtnClick error = " + fail);
                HttpResponseRaw responseDataRaw = GameUtils.GetHttpResponseRaw(fail);
                if (responseDataRaw.result <= 10000)
                {
                    TipPanel.ShowToast(responseDataRaw.rmsg);
                    return;
                }
                TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            });
    }
    private void OfflineUpdateLikeBtn(int likeEnum)
    {
        int tempNum = likeEnum == (int)IsLikeEnum.Liked ? 1 : -1;
        curLikeNum -= tempNum;
        likeNum.text = NumToString(curLikeNum);
        
        RefreshLikeBtn(likeEnum - tempNum);
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
                RefreshRelationBtn((int)type + 1);
                UpdatePlayerInfo(ugcJson);
                LoggerUtils.Log("social set subscribe successed!");
                DataLogUtils.NewUserFollowers();
            },
            (fail) =>
            {
                LoggerUtils.LogError("Script:DCResPanel OnFollowBtnClick error = " + fail);
                LoggerUtils.Log("social set subscribe failed!");
            });
    }
    private void OnClickClose()
    {
        _tweener = cam.transform.DOMove(Zoom_OUT_CAM_FOLLOW_OFFSET, zoomSpeed);
        cam.transform.DORotate(Zoom_OUT_CAM_Rot_OFFSET, zoomSpeed);
        mainGroup.gameObject.SetActive(false);
        HideOrShowPanel(true);
        StartCoroutine("waitDoTween");
    }
    private IEnumerator waitDoTween()
    {
        yield return _tweener.WaitForCompletion();
        cam.LookAt = player.transform;
        cam.Follow = player.transform;
        Hide();

    }
    public void InitPanel()
    {
        RefreshResInfo("", "", 0, "","",IsLikeEnum.None);
        RefershCreatorInfo("",  SubscribedEnum.None, "");
       
        if (GetPhotoCor != null)
        {
            StopCoroutine(GetPhotoCor);
        }
        defaultIcon.gameObject.SetActive(true);
        defaultPicture.gameObject.SetActive(true);
        creatorIcon.gameObject.SetActive(false);
        itemPicture.gameObject.SetActive(false);
        banner.gameObject.SetActive(false);
    }
    public void OnClickDC(string json, Transform resTrans, DataTypeEnum typeEnum = DataTypeEnum.Resource)
    {
        DCResInfo info = JsonConvert.DeserializeObject<DCResInfo>(json);
        this.hideFollowRoot = info.classifyType == (int)BundlePart.Respgc;
        if(hideFollowRoot)RefreshRelationBtnInternal(-1);
        currentType = typeEnum;
        mainGroup.gameObject.SetActive(true);
        InitPanel();
        ugcJson = json;
        ugcJsonInfo = info;
        SetZoom(resTrans);
        StopCoroutine("waitDoTween");
        SteeringWheelManager.Inst.OnPanelReset();
    }
    
    
    public void UpdatePlayerInfo(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            HttpUtils.MakeHttpRequest("/ugcmap/info", (int)HTTP_METHOD.GET, json, OnGetPlayerInfoSuccess, OnGetPlayerInfoFail);
        }
    }
    private void RefreshResInfo(string resName, string resDic, int likeNum,string price, string total, IsLikeEnum isLike)
    {
       
        LocalizationConManager.Inst.SetSystemTextFont(itemName);
        LocalizationConManager.Inst.SetSystemTextFont(itemDic);
        itemName.text = DataUtils.FilterNonStandardText(resName);
        itemDic.text = DataUtils.FilterNonStandardText(resDic);
        totalNum.text = StringToString(total);
        priceNum.text = StringToString(price);
        curLikeNum = likeNum;
        this.likeNum.text = NumToString(likeNum);
        RefreshLikeBtn((int)isLike);
    }
    private void RefreshPanel(StoreResData storeResInfo)
    {
        var mapInfo = storeResInfo.mapInfo;
        var dcInfo = storeResInfo.mapInfo.dcInfo;
        var creatorInfo = storeResInfo.mapInfo.dcInfo.creatorInfo;
        string uid = null;
        if (mapInfo != null)
        {
            RefreshResInfo(mapInfo.mapName, mapInfo.mapDesc, mapInfo.interactStatus.likes, dcInfo.maticPrice, dcInfo.supply, (IsLikeEnum)mapInfo.interactStatus.liked);
           
            if (creatorInfo!=null)
            {
                uid = creatorInfo.uid;
                RefershCreatorInfo(creatorInfo.userName, creatorInfo.relation==null? SubscribedEnum.None:(SubscribedEnum)creatorInfo.relation.subscribed, creatorInfo.uid);
            }
            RefershIdInfo(mapInfo.mapId, uid);


        }
    }

    private void RefershIdInfo(string mapid, string creatoruid)
    {
        mapId = mapid;
        creatorId = creatoruid;
    }

    private void RefershCreatorInfo(string creatorName,  SubscribedEnum subscribed, string playerUid)
    {
        this.creatorName.text = creatorName;
        int tempFollowId = GameManager.Inst.ugcUserInfo.uid == playerUid ? (int)SubscribedEnum.Me : (int)subscribed;
        RefreshRelationBtn(tempFollowId);
        
    }
    private void RefreshLikeBtn( int state)
    {
        
        for (int i = 0; i < likeParent.childCount; i++)
        {
            likeParent.GetChild(i).gameObject.SetActive(state == i);
        }

    }
    private void RefreshRelationBtn(int state)
    {
        if (hideFollowRoot)
        {
            return;
        }
        RefreshRelationBtnInternal(state);
    }

    private void RefreshRelationBtnInternal(int state)
    {
        for (int i = 0; i < followParent.childCount; i++)
        {
            followParent.GetChild(i).gameObject.SetActive(state == i);
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
        InfoSuccess(socialResInfo);
    }
    public void InfoSuccess(StoreResData socialResInfo)
    {
        GetSoldNum(socialResInfo.mapInfo.dcInfo);
        RefreshPanel(socialResInfo);
        GetPhoto(socialResInfo.mapInfo.mapCover, itemPicture);
        GetPhoto(socialResInfo.mapInfo.dcInfo.creatorInfo.portraitUrl, creatorIcon);

        if (!String.IsNullOrEmpty(socialResInfo.mapInfo.banner))
        {
            UGCResourcePool.Inst.DownloadAndGet(socialResInfo.mapInfo.banner, tex =>
            {
                if (tex != null)
                {
                    Sprite sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    banner.sprite = sprite;
                    banner.gameObject.SetActive(true);
                    banner.SetNativeSize();
                }
            });
        }
    }
    private void GetSoldNum(DCInfo info)
    {
        DCGetSoldNumInfo dc = new DCGetSoldNumInfo
        {
            tokenId = info.tokenId,
            supply = info.supply,
            contractAddress = info.contractAddress,
            walletAddress = info.walletAddress
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.getDCResSoldNum, OnGetDCResSoldNum);
        MobileInterface.Instance.GetDCResSoldNum(JsonConvert.SerializeObject(dc));
    }
    public void OnGetDCResSoldNum(string content)
    {
        LoggerUtils.Log("OnGetDCResSoldNum:  "+content);
        SoldNum sold = JsonConvert.DeserializeObject<SoldNum>(content);
        if (sold!=null)
        {
            soldNum.text = StringToString(sold.sold.ToString());
        }
       
    }
    private void OnGetPlayerInfoFail(string content)
    {
        LoggerUtils.LogError("Script:DCResPanel OnGetPlayerInfoFail error = " + content);
        HttpResponseRaw responseDataRaw = GameUtils.GetHttpResponseRaw(content);
        if (responseDataRaw.result <= 10000)
        {
            TipPanel.ShowToast(responseDataRaw.rmsg);
            return;
        }
    }
    private void GetPhoto(string photoUrl, RawImage image)
    {
        if (!string.IsNullOrEmpty(photoUrl))
        {
            GetPhotoCor = StartCoroutine(LoadSprite(photoUrl, image));
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
    private string NumToString(long num)
    {
        if (num==0)
        {
            return "0";
        }
        long b = num / 1000000000;
        long m = (num % 1000000000) / 1000000;
        long k = (num % 1000000) / 1000;
        long p = num % 1000;
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
    private string StringToString(string num)
    {
        if (string.IsNullOrEmpty(num))
        {
            return "0";
        }
        if (num.Contains("."))
        {
            return num;
        }
        int length = num.Length;
        if (length >= 4)
        {
            num = num.Insert(length-3,","); 
        }
        if (length >= 7)
        {
            num = num.Insert(length - 6, ",");
        }
        if (length >= 10)
        {
            num = num.Insert(length - 9, ",");
        }
        return num;
    }
    public void HideOrShowPanel(bool Active)
    {
        UIControlManager.Inst.CallUIControl(Active?"dc_res_exit": "dc_res_enter");
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(Active);
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.gameObject.SetActive(Active);
        }
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(Active);
        }
    }
    public void SetOwnedPanel(bool isOwn)
    {
        coinGroup.SetActive(!isOwn);
        dcGroup.SetActive(!isOwn);
    }

    private Transform GetParentAnchor(Transform target)
    {
        Renderer render = target.GetComponentInChildren<Renderer>();
        if (render && render.transform.parent)
        {
            return render.transform.parent;
        }
        return target;
    }
}
