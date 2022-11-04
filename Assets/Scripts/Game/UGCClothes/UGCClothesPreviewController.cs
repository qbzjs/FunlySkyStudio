using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UGCClothesPreviewController : MonoBehaviour
{
    public RoleController rcont;
    private RoleData loadRoleData;

    
    void Start()
    {
#if UNITY_EDITOR
        TestNetParams.Inst.LoadConfig();
#endif
        LoggerUtils.Log("Unity-UGCClothesPreviewController-StartGame");
        UIManager.Inst.Init();
        RoleConfigDataManager.Inst.LoadRoleConfig();
        UGCClothesDataManager.Inst.Init();
        GetRoleData();
    }


    public void GetRoleData()
    {
        LoggerUtils.Log("Unity-UGCClothesPreviewController-GetRoleData");
        if (GameManager.Inst.ugcUserInfo == null || GameManager.Inst.ugcClothInfo == null)
        {
            LoggerUtils.LogError("ugcUserInfo UGCClothesPreviewController is NULL");
            HttpGetRoleData();
            return;
        }

        rcont.ShowFrame = CallShowFrame;

        var clotContent = JsonConvert.SerializeObject(GameManager.Inst.ugcClothInfo);
        var mapInfo = GameManager.Inst.gameMapInfo;
        var ugcClothInfo = JsonConvert.DeserializeObject<PublicUGCClothInfo>(clotContent);
        TryOnPanel.Show();
        TryOnPanel.Instance.SetTryOnData(rcont, mapInfo, ugcClothInfo);
        TryOnPanel.Instance.SetTryOnReturnAction(OnBackBtnClick);

        OwnePgcClothShowFrame();
    }

    private void OwnePgcClothShowFrame()
    {
        var ugcInfo = GameManager.Inst.ugcClothInfo;
        if (ugcInfo == null)
        {
            return;
        }

        if (ugcInfo.dcPgcInfos!= null && ugcInfo.dcPgcInfos.Length >= 0)
        {
            CallShowFrame();
        }
        else if (ugcInfo.dcPgcInfo.Equals(default(PGCInfo)))
        {
            CallShowFrame();
        }
    }

    private void CallShowFrame()
    {
        rcont.ShowFrame = null;
        if (MobileInterface.Instance != null)
        {
            MobileInterface.Instance.GetGameInfoRole();
        }
    }

    private void OnBackBtnClick()
    {
        if (MobileInterface.Instance != null)
        {
            MobileInterface.Instance.QuitRole();
        }
    }

    private void OnDestroy()
    {
        CInstanceManager.Release();
    }

    public void HttpGetRoleData()
    {
        LoggerUtils.Log("GetRoleData");
        HttpUtils.MakeHttpRequest("/image/getUserInfo", (int)HTTP_METHOD.GET, "", OnGetRoleDataSuccess, OnGetRoleDataFail);
    }

    public void OnGetRoleDataSuccess(string msg)
    {
        LoggerUtils.Log("OnGetRoleDataSuccess msg = " + msg);
        loadRoleData = new RoleData();
        HttpResponDataStruct roleResponseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        RoleResponData roleResponData = JsonConvert.DeserializeObject<RoleResponData>(roleResponseData.data);
        GameManager.Inst.ugcUserInfo = roleResponData.userInfo;
        LoggerUtils.Log("userInfo.imageJson" + GameManager.Inst.ugcUserInfo.imageJson);
        loadRoleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        RoleConfigDataManager.Inst.SetRoleData(loadRoleData);
        rcont.ShowFrame = CallShowFrame;
        if(GameManager.Inst.gameMapInfo != null)
        {
            var clotContent = JsonConvert.SerializeObject(GameManager.Inst.gameMapInfo);
            var mapInfo = GameManager.Inst.gameMapInfo;
            var ugcClothInfo = JsonConvert.DeserializeObject<PublicUGCClothInfo>(clotContent);
            TryOnPanel.Show();
            TryOnPanel.Instance.SetTryOnData(rcont, mapInfo, ugcClothInfo);
            TryOnPanel.Instance.SetTryOnReturnAction(OnBackBtnClick);
            bool isPgcData = ugcClothInfo.dcPgcInfo != null;
            if (isPgcData)
            {
                CallShowFrame();
            }
        }
        else
        {
            OnFailPreview("GameManager.Inst.gameMapInfo is Null");
        }
    }

    public void OnGetRoleDataFail(string msg)
    {
        LoggerUtils.LogError("Script:UGCClothesPreviewController OnGetRoleDataFail error = " + msg);
    }

    private void OnFailPreview(string err)
    {
        LoggerUtils.LogError(err);
        MobileInterface.Instance.QuitRole();
    }
}
