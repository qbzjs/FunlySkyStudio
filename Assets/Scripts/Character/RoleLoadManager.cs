/// <summary>
/// Author:Mingo-LiZongMing
/// Description:进入角色场景后
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Amazon;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;

public class RoleLoadManager : BMonoBehaviour<RoleLoadManager>
{
    public MobileInterface mobile
    {
        get { return MobileInterface.Instance; }
    }

    public UserInfo roleUserInfo;

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        TestNetParams.Inst.LoadConfig();
#endif
        Inst = this;
#if UNITY_EDITOR
        //GetRoleData By Http
        GetRoleData();
#else
        GetProfilePath();
#endif
    }

    private void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
#if UNITY_EDITOR
        GameManager.Inst.engineEntry = new EngineEntry()
        {
            sceneType = 5,
            subType = 2
        };
#endif
    }

    private void InitData()
    {
        //LoadRoleConfig
        RoleConfigDataManager.Inst.LoadRoleConfig();
        //LoadUGCClothConfig
        UGCClothesDataManager.Inst.Init();
        //GetRoleData By Http
        if((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            RoleMenuView.Ins.Init();
            //Set UI Display
            RoleUIManager.Inst.SetUIEnterMode();
        }
        else
        {
            GetRoleData();
        }
    }

    private void GetProfilePath()
    {
        MobileInterface.Instance.AddClientRespose(MobileInterface.getProfilePath, GetProfilePathSuccess);
        MobileInterface.Instance.GetProfilePath();
    }

    private void GetProfilePathSuccess(string content)
    {
        GameManager.Inst.nativeProfileParam = JsonConvert.DeserializeObject<NativeProfileParam>(content);
        InitData();
    }

    public void GetRoleData()
    {
        LoggerUtils.Log("GetRoleData");
        DataLogUtils.LogUnityUserInfoReq();
        HttpUtils.MakeHttpRequest("/image/getUserInfo", (int)HTTP_METHOD.GET, "", OnGetRoleDataSuccess, OnGetRoleDataFaill);
    }

    public void OnGetRoleDataSuccess(string msg)
    {
        DataLogUtils.LogUnityUserInfoRsp("0");
        LoggerUtils.Log("OnGetRoleDataSuccess msg = " + msg);
        RoleData loadRoleData = new RoleData();
        HttpResponDataStruct roleResponseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        RoleResponData roleResponData = JsonConvert.DeserializeObject<RoleResponData>(roleResponseData.data);
        GameManager.Inst.ugcUserInfo = roleResponData.userInfo;
        LoggerUtils.Log("userInfo.imageJson" + GameManager.Inst.ugcUserInfo.imageJson);
        loadRoleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        RoleConfigDataManager.Inst.SetRoleData(loadRoleData);
        RoleMenuView.Ins.Init();
        //Set UI Display
        RoleUIManager.Inst.SetUIEnterMode();
    }

    public void OnGetRoleDataFaill(string msg)
    {
        SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(msg);
        DataLogUtils.LogUnityUserInfoRsp(httpResponseRaw.result.ToString());
        LoggerUtils.LogError("Script:RoleLoadManager OnGetRoleDataFaill error = " + msg);
        mobile.QuitRole();
    }

    public void SaveRoleData(RoleData roleData, UnityAction<string> success, UnityAction<string> fail)
    {
        if (GameManager.Inst.ugcUserInfo == null || !RoleDataVerify.IsRoleDatalegal(roleData))
        {
            fail("GameManager.Inst.ugcUserInfo == null");
            return;
        }

        roleData.sceneType = 0;
        RoleUpLoadBody RoleUpLoadBody = GetRoleUpLoadBody(roleData);
        if (RoleUpLoadBody.userInfo == null)
        {
            fail("RoleUpLoadBody.userInfo == null");
            return;
        }
        
        HttpUtils.MakeHttpRequest("/image/setImage", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(RoleUpLoadBody), success, fail);
    }

   private RoleUpLoadBody GetRoleUpLoadBody(RoleData roleData)
    {
        RoleUpLoadBody RoleUpLoadBody = new RoleUpLoadBody();
        GameManager.Inst.ugcUserInfo.imageJson = JsonConvert.SerializeObject(roleData);
        //UGC信息
        GameManager.Inst.ugcUserInfo.clothesId = GetUgcMapIds(roleData);
        //Rewards信息
        GameManager.Inst.ugcUserInfo.rewards = GetRewardsItemList(roleData);
        //DC-UGC部件信息
        GameManager.Inst.ugcUserInfo.dcUgcInfos = GetDCUGCItemList(roleData);
        //DC-PGC部件信息
        GameManager.Inst.ugcUserInfo.dcPgcInfos = GetDCPGCItemList(roleData);
        RoleUpLoadBody.userInfo = GameManager.Inst.ugcUserInfo;
        RoleUpLoadBody.operationType = (int)RoleResGrading.DC;
        return RoleUpLoadBody;
    }

    public static string GetUgcMapIds(RoleData roleData)
    {
        var ugcids = new List<string>();
        //如果是UGC衣服，存入衣服mapId
        if (RoleConfigDataManager.Inst.CurClothesIsUgc(roleData.cloId))
        {
            ugcids.Add(roleData.clothMapId);
        }
        //如果是UGC面部彩绘，存入ugc面部彩绘mapId
        if (RoleConfigDataManager.Inst.CurPatternIsUgc(roleData.fpId))
        {
            ugcids.Add(roleData.ugcFPData.ugcMapId);
        }
        return string.Join(",", ugcids);
    }

    //获取身上奖励部件信息, 用于保存校验
    public static PGCInfo[] GetRewardsItemList(RoleData roleData)
    {
        var pgcList = RoleConfigDataManager.Inst.GetContainRewardsList(roleData);
        return pgcList.Count == 0 ? null : pgcList.ToArray();
    }

    public static DCUGCItemInfo[] GetDCUGCItemList(RoleData roleData)
    {
        var ugcList = new List<DCUGCItemInfo>();
        //ugc衣服
        var cloData = RoleConfigDataManager.Inst.GetClothesById(roleData.cloId);
        if (cloData != null && !cloData.IsPGC() && roleData.ugcClothType == (int)UGCClothesResType.DC)
        {
            ugcList.Add(new DCUGCItemInfo
            {
                classifyType = (int)ClassifyType.ugcCloth,
                ugcId = roleData.clothMapId
            });
        };
        //ugcDC面部彩绘
        var fpData = RoleConfigDataManager.Inst.GetPatternStylesDataById(roleData.fpId);
        if (fpData != null && !fpData.IsPGC() && !string.IsNullOrEmpty(roleData.ugcFPData.ugcMapId) && roleData.ugcFPData.ugcType == (int)UGCClothesResType.DC)
        {
            ugcList.Add(new DCUGCItemInfo
            {
                classifyType = (int)ClassifyType.ugcPatterns,
                ugcId = roleData.ugcFPData.ugcMapId
            });
        };
        return ugcList.Count == 0 ? null : ugcList.ToArray();
    }

    public static DCPGCItemInfo[] GetDCPGCItemList(RoleData roleData)
    {
        var pgcList = RoleConfigDataManager.Inst.GetContainDCList(roleData);
        return pgcList.Count == 0 ? null : pgcList.ToArray();
    }

    public void SaveUserInfo(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        if (GameManager.Inst.ugcUserInfo == null)
        {
            return;
        }
        RoleUpLoadBody RoleUpLoadBody = new RoleUpLoadBody();
        RoleUpLoadBody.userInfo = GameManager.Inst.ugcUserInfo;
        HttpUtils.MakeHttpRequest("/image/setImage", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(RoleUpLoadBody), onSuccess, onFail);
    }

    public void SaveRoleDataSuccess(string msg)
    {
        LoggerUtils.Log("SaveRoleDataSuccess");
    }

    public void SaveRoleDataFaill(string msg)
    {
        LoggerUtils.Log("SaveRoleDataFaill");
    }

    public void ExitScene()
    {
        StartCoroutine(WaitForFrameAndQuit());
    }

    public void Show()
    {
        StartCoroutine(WaitForFrameAndShow());
    }

    IEnumerator WaitForFrameAndShow()
    {
        yield return new WaitForSeconds(0.5f);
        //关闭端上Loading页面时 再关闭UI遮罩
        RoleUIManager.Inst.UIMask.SetActive(false);
        mobile.GetGameInfoRole();
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType != ROLE_TYPE.PUBLIC_AVATAR)
        {
            ShowBanToast();
        }
    }
    public void ShowBanToast()
    {
        //如果穿的是违规UGC的话弹Toast
        if (GameManager.Inst.ugcUserInfo.clothesIsBan == 1 || GameManager.Inst.ugcUserInfo.facePaintingIsBan == 1)
        {
            TipPanel.ShowToast("Your clothing was removed for violating our community guidelines.");
        }
    }

    IEnumerator WaitForFrameAndQuit()
    {
        yield return new WaitForSeconds(0.1f);
        mobile.QuitRole();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CInstanceManager.Release();
        GameUtils.Clear();
    }
}
