using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using SavingData;

public class RoleProfileView : MonoBehaviour
{
    public MobileInterface mobile
    {
        get { return MobileInterface.Instance; }
    }

    public Button BtnSkip;
    public Button BtnReturn;
    public Button BtnShare;
    public Button BtnConfirm;
    public Text[] txts;
    public SpriteAtlas characterAtlas;
    public Transform content;
    public GameObject RoleEmoItem;
    public Animator animator;
    public Camera profileCamera;
    public GameObject ProfileMask;
    public GameObject RoleRawImage;
    public GameObject MaskPanel;
    public string imageJson;
    private RoleController rController;
    private int PoseId;

    private bool CanConfirm = true;

    private List<GameObject> emoBtns = new List<GameObject>();

    private string imageUrl;

    private string defProfileUrl = "https://cdn.joinbudapp.com/static/head_nor.png";

    private string[] RoleEmos =
    {
        "posture_01","posture_02","posture_03","posture_04","posture_05","posture_06",
    };

    private void Awake()
    {
        characterAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/posture");
        rController = GameObject.Find("CharacterPrefab").GetComponent<RoleController>();
        InitRoleEmoMenu();
    }

    private void OnEnable()
    {
        if (emoBtns[0])
        {
            emoBtns[0].GetComponent<RoleEmoItem>().OnSelectClick();
        }
    }

    private void Start()
    {
        BtnReturn.onClick.AddListener(OnReturnBtnClick);
        BtnConfirm.onClick.AddListener(OnConfirmBtnClick);
        BtnSkip.onClick.AddListener(OnSkipClick);
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (curEntryType == ROLE_TYPE.FIRST_ENTRY)
        {
            // 新用户漏斗
            MobileInterface.Instance.LogCustomEventData(LogEventData.SELFIE_PAGE_VIEW, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
            {
            });
        }

#if UNITY_IPHONE
        AdjustBtnForIos();
#endif
        Init();
    }

    public void Init()
    {
        //新用户用Continue，并启用skip按钮
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (curEntryType == ROLE_TYPE.FIRST_ENTRY)
        {
            txts[0].gameObject.SetActive(false);
            txts[1].gameObject.SetActive(true);
            BtnSkip.gameObject.SetActive(true);
        }
        else
        {
            txts[0].gameObject.SetActive(true);
            txts[1].gameObject.SetActive(false);
            BtnSkip.gameObject.SetActive(false);
        }
    }

    private void InitRoleEmoMenu()
    {
        for (int i = 0;i < RoleEmos.Length; i++)
        {
            GameObject emoItem = Instantiate(RoleEmoItem, content);
            RoleEmoItem roleEmoCom = emoItem.GetComponent<RoleEmoItem>();
            characterAtlas = characterAtlas == null ? ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/posture") : characterAtlas;
            Sprite sprite = characterAtlas.GetSprite(RoleEmos[i]);
            roleEmoCom.SetSprite(sprite);
            roleEmoCom.SetEmoName(RoleEmos[i], i, SetRoleEmoSelect);
            emoBtns.Add(emoItem);
        }
        rController.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
    }

    private void SetRoleEmoSelect(string emoName, int poseId)
    {
        PoseId = poseId + 1;
        animator.Play(emoName);
    }

    private void OnReturnBtnClick()
    {
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (curEntryType == ROLE_TYPE.SET_IMAGE || curEntryType == ROLE_TYPE.SET_REWARDS || curEntryType == ROLE_TYPE.FIRST_ENTRY)
        {
            RoleUIManager.Inst.ProfilePanelActivity(false);
        }
        else if (curEntryType == ROLE_TYPE.SET_PROFILE)
        {
            RoleLoadManager.Inst.ExitScene();
        }
    }

    private void OnConfirmBtnClick()
    {
        if (!CanConfirm)
        {
            return;
        }
        if (CanConfirm == true)
        {
            CanConfirm = false;
            Invoke("WaitForFrameAndConfirm", 1.0f);
        }
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (curEntryType == ROLE_TYPE.FIRST_ENTRY)
        {
            MobileInterface.Instance.LogCustomEventData(LogEventData.login_new_selfie, (int)Log_Platform.Firebase, new Dictionary<string, object>()
            {
                {"pose_id", PoseId}
            });

            // 新用户漏斗
            MobileInterface.Instance.LogCustomEventData(LogEventData.SELFIE_CLICK, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
            {
                {"source","confirm"}
            });
        }

        LoggerUtils.Log("OnConfirmBtnClick");
        Rect rect = GetScreenShotRect();
        try
        {
            PlayLoadingAnim(true);
            LoggerUtils.Log("GetScreenShotRect");
            byte[] imgBytes = ScreenShotUtils.TakeProfileShot(profileCamera, rect);
            switch ((ROLE_TYPE)GameManager.Inst.engineEntry.subType)
            {
                case ROLE_TYPE.FIRST_ENTRY:
                    //SaveImage To local
                    try
                    {
                        //ImangeJsonUpload(imgBytes);
                        NewUserContinue(imgBytes);
                    }
                    catch {
                        LoggerUtils.LogError("SaveProfile Fail");
                        //FinishUnityHeadUrlSetup(defProfileUrl);                      
                    }
                    break;

                case ROLE_TYPE.SET_PROFILE:
                    //UpLoadToAws
                    UpLoadProfile(imgBytes);
                    break;
            }
        }
        catch {
            LoggerUtils.Log("GetScreenShotRect Fail");
        }
    }

    //点击跳过按钮 不保存 直接上传ImageJson
    private void OnSkipClick()
    {
        if (!CanConfirm)
        {
            return;
        }
        if (CanConfirm == true)
        {
            CanConfirm = false;
            Invoke("WaitForFrameAndConfirm", 1.0f);
        }
        MobileInterface.Instance.LogCustomEventData(LogEventData.SELFIE_CLICK, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
        {
            {"source","skip"}
        });
        LoggerUtils.Log("Selfie Skipped");
        ImangeJsonUpload();
    }

    //新用户设置头像新流程：获取截图保存路径->保存截图->上传ImageJson
    private void NewUserContinue(byte[] imgBytes)
    {
        MobileInterface.Instance.AddClientRespose(MobileInterface.getProfilePath, (string content) => {
            GameManager.Inst.nativeProfileParam = JsonConvert.DeserializeObject<NativeProfileParam>(content);
            SaveScreenShot(imgBytes);
            ImangeJsonUpload();
        });
        MobileInterface.Instance.AddClientFail(MobileInterface.getProfilePath, GetClientFail);
        MobileInterface.Instance.GetProfilePath();
    }

    private void SaveScreenShot(byte[] imgBytes)
    {
        if (imgBytes.Length == 0)
        {
            LoggerUtils.Log("GetScreenShot Fail imgBytes.Length == 0");
            return;
        }
        string profileName = GameManager.Inst.nativeProfileParam.imgName;
        string profilePath = GameManager.Inst.nativeProfileParam.imgPath;
        if (!Directory.Exists(profilePath))
        {
            Directory.CreateDirectory(profilePath);
        }
        if (File.Exists(profilePath + profileName))
        {
            File.Delete(profilePath + profileName);
        }
        FileStream stream = new FileStream(profilePath + profileName, FileMode.Create);
        stream.Write(imgBytes, 0, imgBytes.Length);
        stream.Flush();
        stream.Close();
    }

    private void ImangeJsonUpload()
    {
        if (imageJson == null)
        {
            return;
        }
        //1.41版本不需要回调
        /*       
        MobileInterface.Instance.AddClientRespose(MobileInterface.newUserRegistrationCompleted, (string content) =>
        {
            
        });
        MobileInterface.Instance.AddClientFail(MobileInterface.newUserRegistrationCompleted, GetClientFail);*/
        mobile.NewUserRegistrationCompleted(imageJson);
    }


    private void UpLoadProfile(byte[] imgBytes)
    {
        if (imgBytes.Length == 0)
        {
            LoggerUtils.Log("GetScreenShot Fail imgBytes.Length == 0");
            return;
        }
        string fileName = DataUtils.SaveProfile(imgBytes);
        AWSUtill.UpLoadImage(fileName, OnSaveProfileSuccess, OnSaveProfileFail, true);
    }



    private void WaitForFrameAndConfirm()
    {
        CanConfirm = true;
    }

    private Rect GetScreenShotRect()
    {
        Rect rect = new Rect(0, 0, profileCamera.pixelWidth, profileCamera.pixelHeight);
        return rect;
    }

    private void OnSaveProfileSuccess(string url)
    {
        LoggerUtils.Log("OnSaveProfileSuccess" + url);
        RoleUpLoadBody httpUpLoadObj = new RoleUpLoadBody();
        imageUrl = url;
        httpUpLoadObj.portraitUrl = url;
        HttpUtils.MakeHttpRequest("/image/setPortrait", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(httpUpLoadObj), OnUploadProfileSuccess, OnUploadProfileFaill);
    }

    private void OnSaveProfileFail(string err)
    {
        LoggerUtils.Log("OnSaveProfileFail " + err);
    }

    private void OnUploadProfileSuccess(string msg)
    {
        LoggerUtils.Log("Save RoleProfile Success");
        if(imageUrl != null)
        {
            FinishUnityHeadUrlSetup(imageUrl);
            PlayLoadingAnim(false);
        }
    }

    private void OnUploadProfileFaill(string msg)
    {
        LoggerUtils.Log("OnUploadProfileFaill");
        imageUrl = null;
    }

    private void FinishUnityHeadUrlSetup(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            LoggerUtils.Log("FinishUnityHeadUrlSetup + url is null or Empty");
            return;
        }
        UnityImageBean unityImageBean = new UnityImageBean();
        unityImageBean.imageUrl = url;
        unityImageBean.action = 0;
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        if (curEntryType == ROLE_TYPE.FIRST_ENTRY)
        {
            unityImageBean.action = 0;
        }
        else
        {
            unityImageBean.action = 1;
        }
        BlackPanel.Show();
        BlackPanel.Instance.PlayBlackBg();
        mobile.FinishUnityHeadUrlSetup(JsonConvert.SerializeObject(unityImageBean));
    }

    
    

    private void GetClientFail(string content)
    {
        PlayLoadingAnim(false);
        BlackPanel.Hide();
    }
    
    private void PlayLoadingAnim(bool isPlay)
    {
        MaskPanel.SetActive(isPlay);
        //GameObject text = BtnConfirm.transform?.Find("Text").gameObject;
        var curEntryType = (ROLE_TYPE)GameManager.Inst.engineEntry.subType;
        int textIndex = curEntryType == ROLE_TYPE.FIRST_ENTRY ? 1 : 0;
        GameObject text = txts[textIndex].gameObject;
        GameObject loader = BtnConfirm.transform?.Find("loader").gameObject;
        if(text != null && loader != null)
        {
            text.SetActive(!isPlay);
            loader.SetActive(isPlay);
        }
    }

    private void SaveProfile(byte[] imgBytes)
    {
        if (imgBytes.Length == 0)
        {
            LoggerUtils.Log("GetScreenShot Fail imgBytes.Length == 0");
            return;
        }
        string profileName = GameManager.Inst.nativeProfileParam.imgName;
        string profilePath = GameManager.Inst.nativeProfileParam.imgPath;
        if (!Directory.Exists(profilePath))
        {
            Directory.CreateDirectory(profilePath);
        }
        if (File.Exists(profilePath + profileName))
        {
            File.Delete(profilePath + profileName);
        }
        FileStream stream = new FileStream(profilePath + profileName, FileMode.Create);
        stream.Write(imgBytes, 0, imgBytes.Length);
        stream.Flush();
        stream.Close();
        FinishUnityHeadUrlSetup(defProfileUrl);
    }

    private void AdjustBtnForIos()
    {
        RectTransform trans = BtnConfirm.transform as RectTransform;
        trans.anchoredPosition += new Vector2(0, Screen.safeArea.yMin);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        animator.Play("interface_idle");
    }

    private void OnDestroy()
    {
        CancelInvoke("WaitForFrameAndConfirm");
    }
}
