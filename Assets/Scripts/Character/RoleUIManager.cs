using Newtonsoft.Json;
using RedDot;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ROLE_TYPE : int
{
    FIRST_ENTRY = 1,
    SET_IMAGE = 2,
    SET_PROFILE = 3,
    SET_WEAR = 5,
    PUBLIC_AVATAR = 6,
    SET_REWARDS = 7, //奖励/空投
}

public class DCItem
{
    public int resourceType;
    public int resourceId;
    public int nftType;
    public int publishStatus;
    public string itemId;
    public string budActId;
    public string name;
}

public enum Web3Status
{
    Open = 0,
    Close = 1
}

public class RoleUIManager : BMonoBehaviour<RoleUIManager>
{
    public GameObject BtnRoleReturn;
    public GameObject BtnProfileReturn;
    public GameObject RoleMenuPanel;
    public GameObject ProfilePanel;
    public GameObject LoadingMask;
    public Light spotLight; //头顶打光
    public Button BtnConfirmAvatar;
    public RoleController rController;
    private Button btnRoleReturn;
    public List<AvatarRedDots> avatarRedInfos = new List<AvatarRedDots>();
    public bool isStore = false;

    public GameObject UIMask;//在端上loading页关闭之前，放置一个Mask遮罩拦截点击事件
#if UNITY_EDITOR
    protected override void Awake()
    {
        base.Awake();
        UIMask.SetActive(false);
    }
#endif

    private void Start()
    {
        LoggerUtils.IsDebug = true;
        btnRoleReturn = BtnRoleReturn.GetComponent<Button>();
        btnRoleReturn.onClick.RemoveAllListeners();
        btnRoleReturn.onClick.AddListener(OnBtnRoleReturnClick);
        BtnConfirmAvatar.onClick.AddListener(OnBtnConfirmAvatarClick);
        UIManager.Inst.Init();
    }

    public void SetLoadingMaskVisible(bool isVisible)
    {
        LoadingMask.SetActive(isVisible);
    }

    private List<DCItem> dcItems = new List<DCItem>();
    private Web3Status web3Status = Web3Status.Open;
    private void OnSaveRoleDataFailed(string err)
    {
        SetLoadingMaskVisible(false);
        LoggerUtils.LogError("SaveRoleDataFail " + err);
        CharacterTipPanel.ShowToast("Operation failed, please try again.");
    }

    
    private void ShowNotOwnedDCList()
    {
        DCListPanel.Show();
        DCListPanel.Instance.UpdateList(dcItems, web3Status);
    }
    

    private void OnBtnConfirmAvatarClick()
    {
        ClearHomeAvatarRed();
        RoleData roleData = RoleMenuView.Ins.roleData;
        if (roleData == null)
        {
            LoggerUtils.Log("roleData == null");
            return;
        }
        SetLoadingMaskVisible(true);
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        switch (curEntryType)
        {
            case ROLE_TYPE.SET_WEAR:
            case ROLE_TYPE.SET_IMAGE:
            case ROLE_TYPE.SET_REWARDS:
                RoleLoadManager.Inst.SaveRoleData(roleData, (content)=>OnSaveRoleDataSucc(content, roleData, OnNormalAvatarBack), (err) =>
                {
                    OnSaveRoleDataFailed(err);
                });
                 break;

            case ROLE_TYPE.FIRST_ENTRY:
                // 新用户漏斗
                MobileInterface.Instance.LogCustomEventData(LogEventData.AVATAR_CLICK, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
                {
                });

                MobileInterface.Instance.LogCustomEventData(LogEventData.login_new_creation_done, (int)Log_Platform.Custom, new Dictionary<string, object>()
                {
                });
                SetLoadingMaskVisible(false);
                ProfilePanelActivity(true);
                var viewPanel = ProfilePanel.GetComponent<RoleProfileView>();
                viewPanel.imageJson = JsonConvert.SerializeObject(roleData);
                MobileInterface.Instance.LogCustomEventData(LogEventData.UNITY_SELFIE_CHOOSE_SCREEN, (int)Log_Platform.Firebase, new Dictionary<string, object>() {
                        {"new_user", 1 }
                    });
                break;
            case ROLE_TYPE.PUBLIC_AVATAR:
                RoleLoadManager.Inst.SaveRoleData(roleData, (content)=>OnSaveRoleDataSucc(content, roleData, OnPublicAvatarBack), 
                    (err) =>
                {
                    OnSaveRoleDataFailed(err);
                });
                break;
        }
    }

    private void OnPublicAvatarBack(RoleData roleData)
    {
        PublicAvatarPanel.Show();
        RoleMenuView.Ins.ResetPublicAvatarRoleDataToDefault(roleData);
    }
    
    private void OnNormalAvatarBack(RoleData roleData)
    {
        RoleLoadManager.Inst.ExitScene();
    }
    
    private void OnSaveRoleDataSucc(string content, RoleData roleData, Action<RoleData> succ = null)
    {
        SetLoadingMaskVisible(false);
        var jobj = JsonConvert.DeserializeObject<JObject>(content);
        var avatarData = JsonConvert.DeserializeObject<JObject>(jobj["data"]?.ToString() ?? string.Empty);
        int result = 0;
        if (avatarData == null)
        {
            LoggerUtils.LogError($"RoleUIManager::OnSaveRoleDataSucc avatarData==null content:{content}");
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            return;
        }
        else
        {
            result = avatarData.Value<int>("succCode");
        }
        
        if (result == (int)HttpOptErrorCode.None)
        {
            LoggerUtils.Log("SaveRoleDataSuccess");
            MobileInterface.Instance.NotifyRefreshPlayerInfoRoleCall();
            succ?.Invoke(roleData);
        }
        else if (result == (int)HttpOptErrorCode.DC_NOT_OWNED)
        {
            web3Status = (Web3Status)avatarData.Value<int>("web3Status");
            var dcList = avatarData.Value<JToken>("notOwnedList");
            if (dcList != null)
            {
                dcItems = JsonConvert.DeserializeObject<List<DCItem>>(dcList["avatarDcInfos"]?.ToString() ?? string.Empty);
                if (dcItems != null && dcItems.Count > 0)
                {
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
    
    
    public void SetUIEnterMode() {
        var type = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        switch (type) {
            case ROLE_TYPE.SET_WEAR:
            case ROLE_TYPE.SET_IMAGE:
            case ROLE_TYPE.SET_REWARDS:
                LoggerUtils.Log("SET_IMAGE");
                MobileInterface.Instance.LogCustomEventData(LogEventData.UNITY_AVATAR_SET_SCREEN, (int)Log_Platform.Firebase, new Dictionary<string, object>() {
                        {"new_user", 0 }
                    });
                RoleMenuPanel.SetActive(true);
                ProfilePanelActivity(false);
                BtnRoleReturn.SetActive(true);
                BtnProfileReturn.SetActive(true);
                break;

            case ROLE_TYPE.FIRST_ENTRY:
                LoggerUtils.Log("FIRST_ENTRY");
                MobileInterface.Instance.LogCustomEventData(LogEventData.UNITY_AVATAR_SET_SCREEN, (int)Log_Platform.Firebase, new Dictionary<string, object>() {
                        {"new_user", 1 }
                    });
                RoleMenuPanel.SetActive(true);
                ProfilePanelActivity(false);
                BtnRoleReturn.SetActive(false);
                BtnProfileReturn.SetActive(false);
                break;

            case ROLE_TYPE.SET_PROFILE:
                MobileInterface.Instance.LogCustomEventData(LogEventData.UNITY_SELFIE_CHOOSE_SCREEN, (int)Log_Platform.Firebase, new Dictionary<string, object>() {
                        {"new_user", 0 }
                    });
                LoggerUtils.Log("SET_PROFILE");
                RoleMenuPanel.SetActive(true);
                ProfilePanelActivity(true);
                BtnRoleReturn.SetActive(false);
                BtnProfileReturn.SetActive(true);
                break;

            case ROLE_TYPE.PUBLIC_AVATAR:
                LoggerUtils.Log("PUBLIC_AVATAR");
                RoleMenuPanel.SetActive(true);
                ProfilePanelActivity(false);
                BtnRoleReturn.SetActive(true);
                BtnProfileReturn.SetActive(false);
                PublicAvatarPanel.Show();
                break;
        }
        RoleLoadManager.Inst.Show();
    }

    public void ProfilePanelActivity(bool isShow)
    {
        rController.ChangeAnimCtr(isShow);
        ProfilePanel.SetActive(isShow);
        spotLight.shadows = isShow ? LightShadows.None : LightShadows.Soft;
    }
    private void ClearHomeAvatarRed()
    {
        RedDotTree tree = RoleMenuView.Ins.mAvatarRedDotManager.Tree;
        Node root = tree.GetNode((int)AvatarRedDotSystem.ENodeType.Root);
        if (root != null&& root.Count<=0)
        {
            LoggerUtils.Log("-----ClearHomeAvatarRed");
            MobileInterface.Instance.RefreshAvatarRedDot();
        }
    }
    public void OnBtnRoleReturnClick()
    {
        ClearHomeAvatarRed();
        var type = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (type == ROLE_TYPE.PUBLIC_AVATAR)
        {
            PublicAvatarPanel.Show();
        }
        else
        {
            RoleLoadManager.Inst.ExitScene();
        }
    }
}
