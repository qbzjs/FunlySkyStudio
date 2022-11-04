/// <summary>
/// Author:Mingo-LiZongMing
/// Description:人物捏脸界面控制
/// </summary>
using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using SavingData;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using RedDot;

public enum VecAxis
{
    None,
    X,
    XY,
    YZ,
    Z,
    Y
}

public class RoleMenuView : MonoBehaviour
{
    public RoleController rController;
    public static RoleMenuView Ins;
    public RoleConfigData roleConfigData;
    public RoleColorConfig roleColorConfig;
    public RoleData roleData;
    public RoleData roleDefaultData;
    public Action InitView;
    public GameObject CollectImg;
    public Button saveBtn;
    public Button resetBtn;
    public Button randomBtn;
    public Camera roleCamera;
    public RoleController tempRoleController;
    public Text saveText;
    //save 按钮的初始宽度
    private const float saveTextDefaultWidth = 97;
    // save 按钮背景的相对文本宽度的偏移
    private const float textWidthOffset = 40;
    public RedDotSystemManager mRedDotSystemManager;
    public AvatarRedDotManager mAvatarRedDotManager;
    void Awake()
    {
        Ins = this;
        //构建红点系统
        mRedDotSystemManager = new RedDotSystemManager();
        mAvatarRedDotManager = new AvatarRedDotManager(this);
        mAvatarRedDotManager.ConstructRedDotSystem();
    }
#if UNITY_EDITOR
    void Start()
    {

        RoleConfigDataManager.Inst.LoadRoleConfig();
        UGCClothesDataManager.Inst.Init();
    }
#else
#endif
    public void Init()
    {
        roleConfigData = RoleConfigDataManager.Inst.CurConfigRoleData;
        roleColorConfig=RoleConfigDataManager.Inst.roleColorConfig;
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            roleData = RoleDataRandomMgr.Inst.GetRandomRoleData().Clone() as RoleData;
        }
        else
        {
            roleData = RoleConfigDataManager.Inst.CurRoleData.Clone() as RoleData;
        }
        var isLegal = RoleDataVerify.CheckRoleDataIsLegal(roleData);
        if (!isLegal)
        {
            roleData = RoleDataVerify.DefRoleData;
            LoggerUtils.LogError("RoleData is Error ==> Uid = " + GameManager.Inst.ugcUserInfo.uid + " |RoleData = " + GameManager.Inst.ugcUserInfo.imageJson + " |Time = " + GameUtils.GetTimeStamp());
        }
        //替换未拥有的DC部件
        if (RoleConfigDataManager.Inst.ReplaceNotOwnedDC(GameManager.Inst.ugcUserInfo, roleData))
        {
            CharacterTipPanel.ShowToast("The digital collectibles contained in your outfit have been sold.");
        }
        BaseView.InitData(roleConfigData, roleData,roleColorConfig,rController);
        //页面相关初始化
        InitRoleClassify();
        //初始化人物形象
        rController.InitRoleByData(roleData);
        roleDefaultData = roleData.Clone() as RoleData;
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (curEntryType == ROLE_TYPE.FIRST_ENTRY)
        {
            // 新用户漏斗
            MobileInterface.Instance.LogCustomEventData(LogEventData.AVATAR_PAGE_VIEW, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
            {
            });
        }
        
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            resetBtn.gameObject.SetActive(false);
            saveBtn.gameObject.SetActive(false);
        }
        else
        {
            resetBtn.gameObject.SetActive(true);
            saveBtn.gameObject.SetActive(true);
            saveBtn.onClick.AddListener(OnSaveClick);
        }
        resetBtn.onClick.AddListener(OnResetClick);
        randomBtn.onClick.AddListener(OnRandomClick);
        HandleSpecialText();
    }

    public void UpdateRoleData()
    {
        roleData = RoleConfigDataManager.Inst.CurRoleData;
        BaseView.UpdateRoleData(roleData);
    }

    public void SetAction(Action initView)
    {
        InitView +=initView;
    }

    /**
    * 获取指定类型的界面
    */
    public T GetView<T>() where T : BaseView
    {
        return transform.GetComponentInChildren<T>(true);
    }



    private void InitRoleClassify()
    {
        InitView();
        RoleClassifiyView.Ins.InitClassifyView();
        var collectionsView = GetView<CollectionsView>();
        collectionsView.GetAllCollectClothingList();

        var savesView = GetView<SavesView>();
        savesView.GetAllSavedMatchList();

        var digitalView = GetView<DigitalCollectView>();
        digitalView.GetAllSeriesInfo();

        mAvatarRedDotManager.AddListener(RequestRedDotCallBack);
        mAvatarRedDotManager.GetAvatarRedInfo();
    }
    private void OnDestroy()
    {
        Ins = null;
    }
    public void RequestRedDotCallBack(bool isInited)
    {
        if (isInited)
        {
            List<AvatarRedDots> datas = mAvatarRedDotManager.mAvatarRedInfos;
            RoleClassifiyView.Ins.RedDotInited(datas);
        }
    }

    public void OnResetClick()
    {
        CharacterTipDialogPanel.Show();
        CharacterTipDialogPanel.Instance.SetTitle("Are you sure you want to restore to the original outfit?", "Restore");
        CharacterTipDialogPanel.Instance.RightBtnClickAct = ResetRoleDataToDefault;
    }
    public void ChangeRoleData(RoleData rData)
    {
        RoleConfigDataManager.Inst.SetRoleData(rData);
        RoleMenuView.Ins.UpdateRoleData();
        rController.InitRoleByData(rData);
        rController.StartEyeAnimation(rData.eId);
        RoleClassifiyView.Ins.ResetCurViewItemSelect();
    }
    public void ResetRoleDataToDefault()
    {
        ChangeRoleData(roleDefaultData);
    }
    public void OnRandomClick()
    {
        var rData = RoleDataRandomMgr.Inst.GetRandomRoleData();
        ChangeRoleData(rData);
    }
    public void ResetPublicAvatarRoleDataToDefault(RoleData roleData)
    {
        var rData = roleData.Clone() as RoleData;
        ChangeRoleData(rData);
    }


    public void OnSaveClick()
    {
        var savesView = GetView<SavesView>();
        bool isOverLimit = savesView.IsOverMaxCount();
        if (isOverLimit)
        {
            return;
        }

        if (tempRoleController)
        {
            Destroy(tempRoleController.gameObject);
            tempRoleController = null;
        }

        tempRoleController = Instantiate(rController);
        roleCamera = tempRoleController.matchCamera;
        tempRoleController.animCom.Play("interface_idle", 0, 0);
        tempRoleController.ChangeAnimCtr(false);

        tempRoleController.transform.localPosition = new Vector3(1000, 1000, 1000);
        tempRoleController.transform.rotation = Quaternion.Euler(0, -180, 0);
        CoroutineManager.Inst.StartCoroutine(TakeMatchPhoto());
    }

    public IEnumerator TakeMatchPhoto()
    {
        yield return new WaitForEndOfFrame();
        RoleUIManager.Inst.SetLoadingMaskVisible(true);
        Rect rect = GetScreenShotRect();
        byte[] imgBytes = ScreenShotUtils.TakeProfileShot(roleCamera, rect);
        if (tempRoleController)
        {
            Destroy(tempRoleController.gameObject);
            tempRoleController = null;
        }
        SaveRoleImg(imgBytes);
    }

    private Rect GetScreenShotRect()
    {
        Rect rect = new Rect(0, 0, roleCamera.pixelWidth, roleCamera.pixelHeight);
        return rect;
    }

    private void SaveRoleImg(byte[] imgBytes)
    {
        int timeout = 30;
        string fileName = DataUtils.SaveResImg(imgBytes);

        LoggerUtils.Log("SaveRoleImg Success!!! fileName is " + fileName);

        AWSUtill.UpLoadImage(fileName, (imageUrl) =>
        {
            // 保存成功
            LoggerUtils.Log("SaveRoleImg Success!!! imageUrl is " + imageUrl);

            RoleUIManager.Inst.SetLoadingMaskVisible(false);
            var savesView = GetView<SavesView>();
            bool isOverLimit = savesView.IsOverMaxCount();
            if (isOverLimit)
            {
                return;
            }

            var imgName = fileName.Replace(".png", "");
            var rData = roleData.Clone() as RoleData;

            SetMatchData matchData = new SetMatchData();
            matchData.name = imgName;
            matchData.coverUrl = imageUrl;
            matchData.data = JsonConvert.SerializeObject(rData);

            bool patternIsUgc = RoleConfigDataManager.Inst.CurPatternIsUgc(roleData.fpId);
            bool clothIsUgc = RoleConfigDataManager.Inst.CurClothesIsUgc(roleData.cloId);
            if (clothIsUgc || patternIsUgc)
            {
                //ugc衣服
                matchData.isUgcClothes = 1;
                matchData.clothesId = RoleLoadManager.GetUgcMapIds(rData);
            }

            //rewards
            matchData.rewards = RoleLoadManager.GetRewardsItemList(rData);
            //dc
            matchData.dcUgcInfos = RoleLoadManager.GetDCUGCItemList(rData);
            matchData.dcPgcInfos = RoleLoadManager.GetDCPGCItemList(rData);

            HttpUtils.MakeHttpRequest("/image/setCollocation", (int)HTTP_METHOD.POST,
            JsonConvert.SerializeObject(matchData),
            (msg) =>
            {
                OnSaveRoleDataSucc(msg, () =>
                {
                    CharacterTipPanel.ShowToast("Outfit saved");
                    var savesView = GetView<SavesView>();
                    var item = savesView.InitItem(true);
                    item.imgName = fileName.Replace(".png", "");
                    item.roleData = rData;
                    item.SetIconImgVisible(false);
                    item.UpdateIconImg(imageUrl);
                    LoggerUtils.Log("SaveRoleImg Success!!! item.imgName is " + item.imgName);
                });
               
            }, (err) =>
            {
                LoggerUtils.Log("SaveRoleImg fail --- /image/setCollocation ---- errInfo: " + err);
            });
        }, (err) =>
        {
            OnFail();
        }, true, timeout);
    }

    private List<DCItem> dcItems = new List<DCItem>();
    private Web3Status web3Status = Web3Status.Open;
    private void OnSaveRoleDataSucc(string content, Action succ = null)
    {
        var jobj = JsonConvert.DeserializeObject<JObject>(content);
        var avatarData = JsonConvert.DeserializeObject<JObject>(jobj["data"]?.ToString() ?? string.Empty);
        int result = 0;
        if (avatarData == null)
        {
            LoggerUtils.LogError($"RoleMenuView::OnSaveRoleDataSucc avatarData==null content:{content}");
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            return;
        }

        result = avatarData.Value<int>("succCode");

        if (result == (int)HttpOptErrorCode.None)
        {
            succ?.Invoke();
        }
        else if (result == (int)HttpOptErrorCode.DC_NOT_OWNED)
        {
            
            var dcList = avatarData.Value<JToken>("notOwnedList");
            if (dcList != null)
            {
                dcItems = JsonConvert.DeserializeObject<List<DCItem>>(dcList["avatarDcInfos"]?.ToString() ?? string.Empty);
                if (dcItems != null && dcItems.Count > 0)
                {
                    web3Status = (Web3Status)avatarData.Value<int>("web3Status");
                    MobileInterface.Instance.AddClientRespose(MobileInterface.checkWallet, (c)=>BindWalletUtils.CheckWalletResponse(c, ShowNotOwnedDCList));
                    MobileInterface.Instance.MobileSendMsgBridge(MobileInterface.checkWallet, "");
                    return;
                }
            }
            CharacterTipPanel.ShowToast("Oops! Your outfit contains digital collectibles you do not own.");
            LoggerUtils.Log("OnSaveRoleDataSucc dcItems==null");
        }
        else if (result == (int)HttpOptErrorCode.ITEM_NOT_OWNED)
        {
            CharacterTipPanel.ShowToast("Oops! Your outfit contains items you do not own.");
        }
    }
    
    private void ShowNotOwnedDCList()
    {
        DCListPanel.Show();
        DCListPanel.Instance.UpdateList(dcItems, web3Status);
    }
    
    
    private void OnFail(string content = "")
    {
        RoleUIManager.Inst.SetLoadingMaskVisible(false);
        CharacterTipPanel.ShowToast("Failed to save, please try again.");
    }

    public void HandleSpecialText()
    {
        var tempStr = GameUtils.SubStringByBytes(saveText.text, 18, Encoding.Unicode);
        saveText.text = tempStr;
        if (saveText.preferredWidth > saveTextDefaultWidth)
        {
            var saveRectTrans = saveBtn.GetComponent<RectTransform>();
            var saveBtnSize = saveRectTrans.sizeDelta;
            var sizeWidth = saveText.preferredWidth + textWidthOffset;
            saveRectTrans.sizeDelta = new Vector2(sizeWidth, saveBtnSize.y);
        }
    }
}