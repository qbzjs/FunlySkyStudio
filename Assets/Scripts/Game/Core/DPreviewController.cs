using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
/// <summary>
/// Author:Shaocheng
/// Description:3d素材预览控制
/// Date: 2022-3-30 19:43:08
/// </summary>
public class DPreviewController : MonoBehaviour
{
    public Button backBtn;
    public Button buyBtn;
    public ResPreviewCtr previewCtr;

    public Toggle[] switchBgToggles;
    public GameObject[] bgGameObjects;
    public List<GameObject> switchImgs = new List<GameObject>();
    public List<GameObject> switchSelectImgs = new List<GameObject>();
    public Image backImgBlack;
    public Image backImgWhite;
    public Image toggleBgWhite;
    public Image toggleBgBlack;
    public SCENE_TYPE sceneType = SCENE_TYPE.DPreview;
    void Start()
    {
        LoggerUtils.Log("Unity-DPreviewController-StartGame");
        GameManager.Inst.Init();
        UIManager.Inst.Init();
        SceneBuilder.Inst.Init();
        SceneBuilder.Inst.InitPreviewSceneParent();

        backBtn.onClick.AddListener(OnBackBtnClick);
        buyBtn.onClick.AddListener(OnBuyBtnClick);
        previewCtr = gameObject.GetComponent<ResPreviewCtr>();
        previewCtr.SetPreviewHandler();

        for (int i = 0; i < switchBgToggles.Length; i++)
        {
            Toggle t = switchBgToggles[i];
            int index = i;
            switchImgs.Add(t.gameObject.transform.Find("img").gameObject);
            switchSelectImgs.Add(t.gameObject.transform.Find("img_select").gameObject);
            
            t.onValueChanged.AddListener((isOn) =>
            {
                int position = index;

                if (isOn)
                {
                    foreach (var bg in bgGameObjects) bg.SetActive(false);
                    bgGameObjects[position].SetActive(true);

                    for (int j = 0; j < switchImgs.Count; j++)
                    {
                        switchImgs[j].SetActive(j != position);
                        switchSelectImgs[j].SetActive(j == position);
                    }
                    
                    backImgBlack.gameObject.SetActive(position != 0);
                    backImgWhite.gameObject.SetActive(position == 0);
                    toggleBgBlack.gameObject.SetActive(position != 0);
                    toggleBgWhite.gameObject.SetActive(position == 0);
                }
            });
        }

#if UNITY_EDITOR
        Test();
#else
        StartPreview3DProp();
#endif
    }

    #region Test
    private void Test()
    {
        //string jsonUrl = "https://buddy-app-bucket.s3.us-west-1.amazonaws.com/PropsJson/1460975788223664128_1637449748m.json";
        //string jsonUrl = "https://buddy-app-bucket.s3.us-west-1.amazonaws.com/PropsJson/1450777565634633728_1635936079m.json";
        string jsonUrl = "https://buddy-app-bucket.s3.us-west-1.amazonaws.com/PropsJson/1450306028838199296_1636577197m.json";
        BuildFromJson(jsonUrl);
        
    }
    #endregion

    private void StartPreview3DProp()
    {
        if (GameManager.Inst.ugcUntiyMapDataInfo == null)
        {
            LoggerUtils.LogError("GameManager.Inst.ugcUntiyMapDataInfo is null");
            return;
        }
#if UNITY_EDITOR
        MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo, getMapInfo =>
        {
            var mInfo = getMapInfo.mapInfo;
            if (mInfo != null && !string.IsNullOrEmpty(mInfo.propsJson))
            {
          
                SetBuyBtnShow(mInfo.isDC == 0);
                LoggerUtils.Log("mInfo.propsJson==>" + mInfo.propsJson);
                BuildFromJson(mInfo.propsJson);
            }
            else
            {
                TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
                //LoggerUtils.LogError("Enter 3d preview Fail ---> GetMapInfo Fail");
                MobileInterface.Instance.QuitRole();
            }
        }, OnFailPreview);
    
#else
        
       sceneType = GameManager.Inst.sceneType;
        var mInfo = GameManager.Inst.gameMapInfo;
        if (mInfo != null)
        {
            SetBuyBtnShow(mInfo.isDC == 0);
            if (sceneType == SCENE_TYPE.MPreview)
            {
                LoggerUtils.Log($"PreviewMaterial scene png url: {mInfo.dataUrl}");
                CreatUGCMaterial();
                DataLogUtils.View3DProps();
                CallShowFrame();
                return;
            }
            if (mInfo.IsScenePgc())
            {
                LoggerUtils.Log($"Preview3DProp scene pgc id: {mInfo.dcPgcInfo.pgcId}");
                SceneBuilder.Inst.CreateDCPGC(mInfo);
                DataLogUtils.View3DProps();
                CallShowFrame();
                return;
            }
            if (!string.IsNullOrEmpty(mInfo.propsJson))
            {
                LoggerUtils.Log("mInfo.propsJson==>" + mInfo.propsJson);
                BuildFromJson(mInfo.propsJson);
                DataLogUtils.View3DProps();
                return;
            }
        }

        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        MobileInterface.Instance.QuitRole();
        
#endif
    }

    private void OnFailPreview(string err)
    {
        LoggerUtils.LogError("DPreviewController OnFailPreview" + err);
        MobileInterface.Instance.QuitRole();
    }

    private void OnDestroy()
    {
        CInstanceManager.Release();
    }
    
    private void BuildFromJson(string jsonUrl)
    {
        if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
        {
            StartCoroutine(GetByte(jsonUrl, (content) =>
            {
                string jsonStr = ZipUtils.SaveZipFromByte(content);
                if (string.IsNullOrEmpty(jsonStr))
                {
                    OnGetFail("UnZip Failed");
                    return;
                }
                OnGetSuccess(jsonStr);
            }, (error) =>
            {
                OnGetFail(error);
            }));
        }
        else
        {
            StartCoroutine(GetText(jsonUrl, (content) =>
            {
                OnGetSuccess(content);
            }, (error) =>
            {
                OnGetFail(error);
            }));
        }
    }

    private void OnGetSuccess(string content)
    {
        LoggerUtils.Log("BuildFromJson success ->" + content);
        SceneBuilder.Inst.ParsePropAndBuild(content, Vector3.zero);
        CallShowFrame();
    }

    private void OnGetFail(string error)
    {
        LoggerUtils.LogError("Get MapJson Fail => " + error);
        MobileInterface.Instance.QuitRole();
    }
    private void CreatUGCMaterial()
    {
        var modle = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Preview/PreviewCube");
        var cube = Instantiate(modle);
        UGCTexManager.Inst.GetUGCTex(GameManager.Inst.gameMapInfo.dataUrl, (tex) =>
        {
            var render = cube.GetComponentInChildren<Renderer>();
            render.material.SetTexture("_MainTex", tex);
        });
    }
    IEnumerator GetText(string url, UnityAction<string> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.text);
        }
    }

    IEnumerator GetByte(string url, UnityAction<byte[]> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.data);
        }
    }

    private void CallShowFrame()
    {
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

    private void OnBuyBtnClick()
    {
        if (MobileInterface.Instance != null)
        {
            MobileInterface.Instance.QuitRole();
        }
    }
    private void SetBuyBtnShow(bool isShow)
    {
        buyBtn.gameObject.SetActive(isShow);
    }
}
