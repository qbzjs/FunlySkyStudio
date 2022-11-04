/// <summary>
/// Author:Mingo-LiZongMing
/// Description:公共主页Panel
/// </summary>
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class PublicAvatarPanel : BasePanel<PublicAvatarPanel>
{
    public GameObject characterPrefab;
    public Button _btnExit;
    public PublicAvatarCategoryPanel _categoryPanel;
    public RenderTexture roleImage;
    public RenderTexture tryOnImage;
    private RoleController _publicRoleCtr;
    private UserInfo publicUserInfo;
    private UGCClothesPreviewHandle _handle;
    private Camera roleCamera;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
#if !UNITY_EDITOR
        InitUIData();
        InitPublicRoleController();
        InitHandleData();
        //获取当前玩家的形象
        MobileInterface.Instance.GetPublicAvatarUserInfo();
        MobileInterface.Instance.AddClientRespose(MobileInterface.getPublicAvatarUserInfo, OnGetPublicAvatarUserInfo);

        //端上返回Unity3D时 - 需要刷新页面
        MobileInterface.Instance.AddClientRespose(MobileInterface.viewWillAppear, OnViewWillAppear);
        //端上打开tryOn
        MobileInterface.Instance.AddClientRespose(MobileInterface.openUnityTryOnPage, OnOpenUnityTryOnPage);
        //端上打开Wear
        MobileInterface.Instance.AddClientRespose(MobileInterface.openUnityWearPage, OnOpenUnityWearPage);
#else
        InitUIData();
        InitPublicRoleController();
        InitHandleData();
        OnGetPublicAvatarUserInfo(JsonConvert.SerializeObject(GameManager.Inst.ugcUserInfo));
#endif
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        RoleMenuController.Ins.SetCameraZoomImImmediately(ViewType.ZoomUpperBody);
        SetCharacterModeActive(true);
    }

    private void GetRoleCamera()
    {
        var roleCameraObj = GameObject.Find("RoleCamera");
        roleCamera = roleCameraObj.GetComponent<Camera>();
    }

    private void SetCameraRawImage(bool isInPublicView)
    {
        if(roleCamera == null)
        {
            GetRoleCamera();
        }
        roleCamera.targetTexture = isInPublicView ? tryOnImage : roleImage;
        SetPreviewCameraSize(isInPublicView);
    }

    private void SetPreviewCameraSize(bool isInPublicView)
    {
        var orthographicSize = isInPublicView ? 11f : 7.1f;
        Tweener tw = DOTween.To(() => roleCamera.orthographicSize, x => roleCamera.orthographicSize = x, orthographicSize, 0.5f);
        RoleMenuController.Ins.SetCameraZoomImImmediately(ViewType.ZoomUpperBody);
    }

    private void InitUIData() {
        _btnExit.onClick.AddListener(OnExitBtnClick);
    }

    private void InitPublicRoleController()
    {
        var CharacterPrefab = Instantiate(characterPrefab);
        _publicRoleCtr = CharacterPrefab.GetComponent<RoleController>();
        SetCharacterModeActive(true);
    }

    private void InitHandleData()
    {
        _handle = this.GetComponentInChildren<UGCClothesPreviewHandle>();
        _handle.rController = _publicRoleCtr;
    }

    private void OnGetPublicAvatarUserInfo(string content)
    {
        LoggerUtils.Log("OnGetPublicAvatarUserInfo content = " + content);
        publicUserInfo = JsonConvert.DeserializeObject<UserInfo>(content);
        _categoryPanel.SetData(publicUserInfo, _publicRoleCtr);
    }

    private void OnExitBtnClick()
    {
        RoleLoadManager.Inst.ExitScene();
    }

    private void OnViewWillAppear(string content)
    {
        LoggerUtils.Log("OnViewWillAppear content = " + content);
        _categoryPanel.GetPlayerHoldingState();
    }

    public void SetCharacterModeActive(bool isInPublicView)
    {
        RoleUIManager.Inst.rController.gameObject.SetActive(!isInPublicView);
        _publicRoleCtr.gameObject.SetActive(isInPublicView);
        _publicRoleCtr.gameObject.transform.eulerAngles = new Vector3(0, -180, 0);
        SetCameraRawImage(isInPublicView);
        if (isInPublicView)
        {
            _categoryPanel.InitOtherPlayerAvatarOutfit();
            RoleMenuView.Ins.ResetRoleDataToDefault();
        }
    }


    public void OnOpenUnityTryOnPage(string content)
    {
        LoggerUtils.Log("OnOpenUnityTryOnPage content = " + content);
        var mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        var ugcClothInfo = JsonConvert.DeserializeObject<PublicUGCClothInfo>(content);
        SetCharacterModeActive(false);
        SetCameraRawImage(true);
        TryOnPanel.Show();
        TryOnPanel.Instance.SetTryOnData(RoleMenuView.Ins.rController, mapInfo, ugcClothInfo);
        TryOnPanel.Instance.SetTryOnReturnAction(() => {
            TryOnPanel.Hide();
            PublicAvatarPanel.Show();
        });
        Hide();
    }

    public void OnOpenUnityWearPage(string content)
    {
        LoggerUtils.Log("OnOpenUnityWearPage content = " + content);
        SetCharacterModeActive(false);
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        UGCClothInfo ugcClothInfo = new UGCClothInfo()
        {
            mapId = mapInfo.mapId,
            clothesJson = mapInfo.clothesJson,
            clothesUrl = mapInfo.clothesUrl,
            templateId = mapInfo.templateId,
            dataSubType = mapInfo.dataSubType,
        };

        UGCClothesResType ugcClothType = (mapInfo.isDC > 0) ? UGCClothesResType.DC : UGCClothesResType.UGC;
        ugcClothType = (mapInfo.isPGC > 0) ? UGCClothesResType.PGC : ugcClothType;
        RoleUgcManager.Inst.WearUgc(ugcClothInfo, ugcClothType);
        //特殊处理眼睛
        var selfRoleCtr = RoleUIManager.Inst.rController;
        var roleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        _categoryPanel.HandleEyeTexure(roleData, selfRoleCtr);
        Hide();
        _categoryPanel.GetPlayerHoldingState();
    }
}
