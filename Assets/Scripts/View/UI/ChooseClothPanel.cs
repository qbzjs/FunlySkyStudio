/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/6/27 14:48:48
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class ChooseClothPanel : BasePanel<ChooseClothPanel>
{
    public GameObject downText;
    public GameObject loadingAnim;
    public GameObject loadingMask;
    public GameObject Tips;
    public Transform bg;
    public Button hidePanelButton;
    public Button reSetButton;
    public Button doneButton;
    //panel从右侧渐出距离
    private float defultePos;
    //panel从右侧渐入距离
    private float targetPos = 1570;
    private Tween showInOutTween;
    public RoleController controller;
    private RoleData roleData;
    public List<RoleSelectClothItem> matchList = new List<RoleSelectClothItem>();
    public Transform IconParent;
    public RoleSelectClothItem matchItem;
    public RoleSelectClothItem curItem;
    public const int MaxCount = 99;
    private const int pageSize = 12;
    private ReqQuerry httpReq = new ReqQuerry();
    private bool joyStickOrigState;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        defultePos = bg.localPosition.x;
        hidePanelButton.onClick.AddListener(OnHidePanel);
        reSetButton.onClick.AddListener(OnResetBtnClick);
        doneButton.onClick.AddListener(OnDoneBtnClick);
        GetAllSavedMatchList();
    }
    public void GetAllSavedMatchList()
    {
        httpReq.toUid = GameManager.Inst.ugcUserInfo.uid;
        httpReq.pageSize = pageSize;
        httpReq.cookie = "";
        GetMatchListRequest();
    }
    private void GetMatchListRequest()
    {
        HttpUtils.MakeHttpRequest("/image/getCollocation", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReq), GetSavedMatchListSuccess, GetSavedMatchListFail);
    }
    public void GetSavedMatchListSuccess(string msg)
    {
        LoggerUtils.Log("CollectionsView GetCollectListSuccess. msg is  " + msg);
        HttpResponDataStruct responseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        if (string.IsNullOrEmpty(responseData.data))
        {
            LoggerUtils.LogError("OnGetSavedMatchList : repData.data == null");
            return;
        }
        MatchDataList matchDataList = JsonConvert.DeserializeObject<MatchDataList>(responseData.data);
        httpReq.cookie = matchDataList.cookie;
        if (matchDataList.collocationInfo != null)
        {
            foreach (var matchData in matchDataList.collocationInfo)
            {
                var item = InitItem();
                item.imgName = matchData.name;
                item.coverUrl = matchData.coverUrl;
                item.roleData = JsonConvert.DeserializeObject<RoleData>(matchData.data);
                item.UpdateIconImg(item.coverUrl);
            }
        }
        if (matchDataList.isEnd != 1)
        {
            GetMatchListRequest();
        }
    }
    public RoleSelectClothItem InitItem()
    {
        var item = Instantiate(matchItem, IconParent);

        item.SetIconImgVisible(false);
        item.StyleBtn.onClick.AddListener(() =>
        {
            item.OnSelectMatchItem(controller);
            OnSelectClick(item);
        });
        item.SetSelectState(false);
        AddMatchItem(item);
        return item;
    }
    public virtual void OnSelectClick(RoleSelectClothItem item)
    {
        if (curItem == item)
        {
            return;
        }

        if (curItem != null)
        {
            curItem.SetSelectState(false);
        }
        curItem = item;
        curItem.SetSelectState(true);
    }

    public void AddMatchItem(RoleSelectClothItem item)
    {
        matchList.Add(item);
        Tips.SetActive(false);
       

    }
   
    public void GetSavedMatchListFail(string err)
    {
        LoggerUtils.LogError("Script:ChooseClothPanel GetSavedMatchListFail error = " + err);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        ShowPanel();
        DisabledJoyStick();
        SetRoleChangeCloth();
        if (matchList.Count <= 0)
        {
            Tips.SetActive(true);
        }
        else
        {
            Tips.SetActive(false);
        }
        SetModleRot();
    }
    private void ShowPanel()
    {
        bg.localPosition = new Vector3(defultePos + targetPos, bg.localPosition.y, bg.localPosition.z);
        SetShowOutInTween(defultePos);
    }

    private void DisabledJoyStick()
    {
        if (!PlayModePanel.Instance)
        {
            joyStickOrigState = false;
            return;
        }
        //记录原JoyStick状态，然后禁用
        joyStickOrigState = PlayModePanel.Instance.joyStick.enabled;
        PlayModePanel.Instance.joyStick.enabled = false;
    }

    public void OnHidePanel()
    {
        ClosetClientManager.Inst.ResetMove();
        HidePanel();
    }
    public void HidePanel()
    {
        if (PlayModePanel.Instance && gameObject.activeInHierarchy)
        {
            PlayModePanel.Instance.joyStick.enabled = joyStickOrigState;
        }
        if (SceneParser.Inst.GetBaggageSet() == 1)
        {
            CatchPanel.Show();
        }
        SetLoadingAnim(false);
        ReSetCurItem();
        UIControlManager.Inst.CallUIControl("choose_cloth_exit");
        SetShowOutInTween(targetPos);
        showInOutTween.onComplete += Hide;
    }
    private void SetShowOutInTween(float pos)
    {
        CleanTween();
        showInOutTween = bg.DOLocalMoveX(pos, 0.5f);
        
    }
    private void CleanTween()
    {
        if (showInOutTween != null)
        {
            showInOutTween.Kill();
            showInOutTween = null;
        }
    }
    //强制关闭界面
    public void ForseHidePanel()
    {
        if (PlayModePanel.Instance && gameObject.activeInHierarchy)
        {
            PlayModePanel.Instance.joyStick.enabled = joyStickOrigState;
        }
        UIControlManager.Inst.CallUIControl("choose_cloth_exit");
        SetLoadingAnim(false);
        ReSetCurItem();
        CleanTween();
        Hide();
    }
    public void SetRoleDate()
    {
        roleData =JsonConvert.DeserializeObject<RoleData>( GameManager.Inst.ugcUserInfo.imageJson);
        Debug.Log("roleData " + roleData);
    }
    public void SetRoleChangeCloth()
    {
       
        SetRoleDate();
        var rData = roleData.Clone() as RoleData;
        controller.InitRoleByData(rData);
        EyeStyleData eyeStyleData = RoleConfigDataManager.Inst.GetEyeStyleDataById(rData.eId);
        controller.StartEyeAnimation(eyeStyleData.id);
    }
    public void OnDoneBtnClick()
    {
        if (curItem!=null)
        {
            //todo:换装逻辑
            ClosetClientManager.Inst.UploadOutfitData(curItem.roleData, (str) =>
            {
                if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
                {
                    //如果驾驶状态，换装成功后需要下车
                    SteeringWheelManager.Inst.SendGetOffCar();
                }
            }, (str) =>
            {
                //换装失败也需要重置动画
                ClosetClientManager.Inst.ResetMove();
            });
            SwordManager.Inst.forceInterrupt();
            SetLoadingAnim(true);
        }
        
    }
    public void UpDateSuccess()
    {
        
        HidePanel();
    }
    public void OnResetBtnClick()
    {
        if (curItem==null)
        {
            return;
        }
        SecondComfirmPanel.Show();
        SecondComfirmPanel.Instance.SetTitle("Are you sure you want to restore to the original outfit?", "Cancel", "Restore");
        SecondComfirmPanel.Instance.LeftBthClickAct = null;
        SecondComfirmPanel.Instance.RightBtnClickAct = OnReset;

    }
    public void OnReset()
    {
        

        SetRoleChangeCloth();
        ReSetCurItem();
    }
    public void ReSetCurItem()
    {
        if (curItem != null)
        {
            curItem.SetSelectState(false);
        }
        curItem = null;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        CleanTween();
    }
    public void SetLoadingAnim(bool isLoading)
    {
        downText.SetActive(!isLoading);
        loadingAnim.SetActive(isLoading);
        loadingMask.SetActive(isLoading);
    }
    public void SetModleRot()
    {
        controller.gameObject.transform.eulerAngles = new Vector3(0, -180, 0);
    }
}
