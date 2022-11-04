using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using BudEngine.NetEngine;

/// <summary>
/// Author:WenJia
/// Description:Player 角色数据
/// 主要包含 Player 角色数据的加载等逻辑
/// Date: 2022/3/31 11:14:20
/// </summary>
public class PlayerRoleData : MonoBehaviour
{
    public Animator playerAnim;
    public AnimationController animCon;
    public static PlayerRoleData Inst;

    private void Awake()
    {
        Inst = this;
        GlobalSettingManager.Inst.OnShowUserNameChange += ShowUserNameChange;
    }

    private void ShowUserNameChange(bool open)
    {
        GameObject playerNode = playerAnim.gameObject;
        SuperTextMesh nick = playerNode.transform.Find("playerInfo/nick").GetComponent<SuperTextMesh>();
        bool isVerify = GameManager.Inst.ugcUserInfo.officialCert != null 
                        && GameManager.Inst.ugcUserInfo.officialCert.accountClass == 1;
        playerNode.transform.Find("playerInfo/nick/verify").gameObject.SetActive(open && isVerify);
        nick.gameObject.SetActive(open);
    }

    public void GetUserInfo()
    {
        DataLogUtils.LogUnityUserInfoReq();
        HttpUtils.MakeHttpRequest("/image/getUserInfo", (int) HTTP_METHOD.GET, "", OnGetRoleDataSuccess,
            OnGetRoleDataFaill);
    }

    public void OnGetRoleDataFaill(string error)
    {
        SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
        DataLogUtils.LogUnityUserInfoRsp(httpResponseRaw.result.ToString());
        LoggerUtils.LogError("Script:PlayerRoleData OnGetRoleDataFaill error = " + error);
        if (ReferPanel.Instance)
        {
            ReferPanel.Instance.isFarst = false;
        }

        //StartCoroutine(CallShowFrame());
    }

    private IEnumerator CallShowFrame()
    {
        LoggerUtils.LogError("Enter Map Fail");
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        if (MobileInterface.Instance != null)
        {
            yield return new WaitForSeconds(0.5f);
            MobileInterface.Instance.GetGameInfo();
        }
    }

    public void OnGetRoleDataSuccess(string msg)
    {
        DataLogUtils.LogUnityUserInfoRsp("0");
        LoggerUtils.Log("OnGetRoleDataSuccess");
        RoleData roleData = new RoleData();
        HttpResponDataStruct roleResponseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        RoleResponData roleResponData = JsonConvert.DeserializeObject<RoleResponData>(roleResponseData.data);
        GameManager.Inst.ugcUserInfo = roleResponData.userInfo;
        LoggerUtils.Log("GetUserInfo OnGetRoleDataSuccess：" + roleResponseData.data);
        InitUserInfo();
    }

    public void InitUserInfo()
    {
        RoleData roleData = new RoleData();
        roleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        //替换被ban调的ugc部件
        if (RoleConfigDataManager.Inst.ReplaceUGC(roleData, GameManager.Inst.ugcUserInfo))
        {
            TipPanel.ShowToast("Your clothing was removed for violating our community guidelines.");
        }

        //替换未拥有的DC部件
        if (RoleConfigDataManager.Inst.ReplaceNotOwnedDC(GameManager.Inst.ugcUserInfo, roleData))
        {
            TipPanel.ShowToast("The digital collectibles contained in your outfit have been sold.");
        }

        GameObject playerNode = playerAnim.gameObject;
        //Init Role Data
        var roleCom = playerNode.GetComponent<RoleController>();
        roleCom.InitRoleByData(roleData);
        roleCom.InitPlayerLayer();
        //Init Player Nick
        SuperTextMesh nick = playerNode.transform.Find("playerInfo/nick").GetComponent<SuperTextMesh>();
        bool showUserName = GlobalSettingManager.Inst.IsShowUserName();
        nick.text = GameManager.Inst.ugcUserInfo.userName;
        nick.gameObject.SetActive(showUserName);
        Transform verify = playerNode.transform.Find("playerInfo/nick/verify");
        bool isVerify = GameManager.Inst.ugcUserInfo.officialCert != null 
                        && GameManager.Inst.ugcUserInfo.officialCert.accountClass == 1;
        verify.gameObject.SetActive(showUserName && isVerify);
        AdjustVerifyPos(verify);

        if (PlayerControlManager.Inst.playerBase)
        {
            PlayerControlManager.Inst.playerBase.isChanged = true;
        }

        if (PlayerControlManager.Inst.isPickedProp)
        {
            PlayerControlManager.Inst.ChangeAnimClips();
        }

        if (ReferPanel.Instance && ReferPanel.Instance.isFarst)
        {
            ReferPanel.Instance.SetReferPlayer();
        }

        if (!PlayerBaseControl.Inst.isTps)
        {
            PlayerBaseControl.Inst.SetPlayerRoleActive();
        }
        else
        {
            playerAnim.gameObject.SetActive(true);
        }

        animCon.PlayEyeAnim(roleData.eId);
        if (PVPWaitAreaManager.Inst.PVPBehaviour == null)
        {
            PlayerManager.Inst.ShowPlayerState(Player.Id, false);
        }
    }
    
    private void AdjustVerifyPos(Transform verify)
    {
        VerifyItemPos verifyItemPos = verify.GetComponent<VerifyItemPos>();
        verifyItemPos.RefreshPos();
    }

    private void OnDestroy()
    {
        Inst = null;
    }
}