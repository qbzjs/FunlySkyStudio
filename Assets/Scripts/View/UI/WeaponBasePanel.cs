using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

/// <summary>
/// Author:Shaocheng
/// Description:武器通用UI面板,外部使用UGC作为武器模型的可使用(例如攻击道具，射击道具等),可参考AttackWeaponPanel
/// Date: 2022-4-14 17:44:22 
/// </summary>
public class UgcWeaponItem
{
    public GameObject itemObj;
    public RawImage resCover; //Ugc素材封面图
    public GameObject selectBg;
    public MapInfo mapInfo;
    public string mapJsonContent;
}
//TODO:待优化，去除manager绑定关系
public abstract class WeaponBasePanel<T> : InfoPanel<T> where T : BasePanel<T>
{
    #region UI

    public GameObject goNoPanel;
    public GameObject goHasPanel;
    public Button closePanelBtn;

    public Button btnChoosePropInNoPanel;
    public Button btnChoosePropInHasPanel;

    public RectTransform content;

    #endregion

    //已经选过的UGC的rid,显示在panel下方供选择
    private Dictionary<string, UgcWeaponItem> ugcWeaponItems = new Dictionary<string, UgcWeaponItem>();

    protected string WEAPON_LOG = "WeaponBasePanel:";
    protected NodeBaseBehaviour curBehav;
    private string currentChooseItem = string.Empty;
    
    #region 各武器按需实现数据交互

    protected virtual List<string> GetAllUgcWeaponRidList()
    {
        return null;
    }

    protected virtual void AddDataToWeaponManager(NodeBaseBehaviour nBehav,string rId)
    {

    }

    protected virtual void SetLastChooseWeapon(UgcWeaponItem weaponItem)
    {

    }

    protected virtual void RefreshUI()
    {
    }

    #endregion

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        closePanelBtn.onClick.AddListener(OnCloseBtnClick);
        btnChoosePropInNoPanel.onClick.AddListener(OpenUgcBagpackAndChoose);
        btnChoosePropInHasPanel.onClick.AddListener(OpenUgcBagpackAndChoose);
        RefreshUI();
    }

    public void OnCloseBtnClick()
    {
        var opanel = UIManager.Inst.uiCanvas.GetComponentsInChildren<IPanelOpposable>(true);
        for (int i = 0; i < opanel.Length; i++)
        {
            opanel[i].SetGlobalHide(true);
        }
        this.gameObject.SetActive(false);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        RefreshUI();
        InitWeaponPanel();
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        StopAllCoroutines();
        currentChooseItem = string.Empty;
    }

    #region 再次进入编辑模式，初始化武器UIpanel

    protected void InitWeaponPanel()
    {
        var allUsedUgcs = GetAllUgcWeaponRidList();

        if (allUsedUgcs != null && allUsedUgcs.Count > 0)
        {
            var ugcToReqArray = new List<string>();
            foreach (var usedUgc in allUsedUgcs)
            {
                if (!ugcWeaponItems.ContainsKey(usedUgc))
                {
                    ugcToReqArray.Add(usedUgc);
                }
            }

            if (ugcToReqArray.Count > 0)
            {
                LoggerUtils.Log("WeaponBasePanel ugcToReqArray==>" + ugcToReqArray);
                
                var httpMapDataInfo = new HttpBatchMapDataInfo
                {
                    mapIds = ugcToReqArray.ToArray(),
                    dataType = 1
                };
                
                MapLoadManager.Inst.GetBatchMapInfo(httpMapDataInfo, getMapInfo =>
                {
                    LoggerUtils.Log($"WeaponBasePanel GetBatchMapInfo Success:{JsonConvert.SerializeObject(getMapInfo)}");

                    var mapInfos = getMapInfo.mapInfos;
                    if (mapInfos != null && mapInfos.Length > 0)
                    {
                        foreach (var mapInfo in mapInfos)
                        {
                            CreateWeaponItemUI(mapInfo);
                        }
                    }
                }, error =>
                {
                    //TODO:初始化数据失败的情况处理
                    LoggerUtils.LogError($"WeaponBasePanel GetBatchMapInfo faild:{error}");
                });
            }
        }
    }

    private IEnumerator LoadResCoverTexture(MapInfo mapInfo)
    {
        yield return ResBagManager.Inst.LoadTexture(mapInfo.mapCover, (texture) =>
        {
            if (texture)
            {
                if (ugcWeaponItems.ContainsKey(mapInfo.mapId))
                {
                    return;
                }
                if (!gameObject.activeSelf)
                {
                    return;
                }
                string jsonUrl = mapInfo.propsJson;
                if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
                {
                    StartCoroutine(GetByte(jsonUrl, (content) =>
                    {
                        LoggerUtils.Log("WeaponBasePanel-->LoadTexture and get json success-->" + content);
                        string jsonStr = ZipUtils.SaveZipFromByte(content);
                        CreateWeaponUiItem(mapInfo, (newItem) =>
                        {
                            newItem.resCover.texture = texture;
                            newItem.resCover.gameObject.SetActive(true);
                            newItem.mapJsonContent = jsonStr;
                            ugcWeaponItems.Add(mapInfo.mapId, newItem);
                        });
                        ChooseItem(currentChooseItem);
                    }, (error) => { LoggerUtils.LogError("Get ResMapJson Fail"); }));
                }
                else
                {
                    StartCoroutine(GetText(jsonUrl, (content) =>
                    {
                        LoggerUtils.Log("WeaponBasePanel-->LoadTexture and get GetText json success-->" + content);
                        CreateWeaponUiItem(mapInfo, (newItem) =>
                        {
                            newItem.resCover.texture = texture;
                            newItem.resCover.gameObject.SetActive(true);
                            newItem.mapJsonContent = content;
                            ugcWeaponItems.Add(mapInfo.mapId, newItem);
                        });
                        ChooseItem(currentChooseItem);
                    }, (error) => { LoggerUtils.LogError("Get ResMapJson Fail"); }));
                }
            }
        }, (error) => { LoggerUtils.LogError($"WeaponBasePanel LoadResCoverTexture Fail: {mapInfo.mapCover},error:{error}"); });
    }

    public IEnumerator GetByte(string url, Action<byte[]> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
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

    public IEnumerator GetText(string url, Action<string> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
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

    #endregion

    protected void ChooseItem(string rid)
    {
        currentChooseItem = rid;
        
        foreach (var item in ugcWeaponItems.Values)
        {
            if (item != null)
            {
                item.selectBg.SetActive(false);
            }
        }

        if (!string.IsNullOrEmpty(rid) && ugcWeaponItems.ContainsKey(rid))
        {
            var selectBg = ugcWeaponItems[rid].selectBg;
            if (selectBg) selectBg.gameObject.SetActive(true);
            SetLastChooseWeapon(GetWeaponItemByRd(rid));
        }
    }

    /// <summary>
    /// 创建UGC UI item
    /// </summary>
    /// <param name="mapInfo"></param>
    protected void CreateWeaponItemUI(MapInfo mapInfo)
    {
        if (!ugcWeaponItems.ContainsKey(mapInfo.mapId))
        {
            if (ResBagManager.Inst.resItemDataDic.ContainsKey(mapInfo.mapId))
            {
                //直接从素材背包数据获取封面
                var resItemInfo = ResBagManager.Inst.resItemDataDic[mapInfo.mapId];
                CreateWeaponUiItem(mapInfo, (newItem) =>
                {
                    newItem.resCover.texture = resItemInfo.mapCover;
                    newItem.resCover.gameObject.SetActive(true);
                    newItem.mapJsonContent = ResBagManager.Inst.resItemDataDic[mapInfo.mapId].mapJsonContent;
                    ugcWeaponItems.Add(mapInfo.mapId, newItem);
                });
            }
            else
            {
                //二次编辑时，还没打开素材背包界面，手动拉取封面
                CoroutineManager.Inst.StartCoroutine(LoadResCoverTexture(mapInfo));
            }
        }
    }

    private void CreateWeaponUiItem(MapInfo mapInfo, Action<UgcWeaponItem> createCb)
    {
        var itemPrefab = ResManager.Inst.LoadResNoCache<GameObject>("Prefabs/UI/Panel/WeaponItem");
        var item = Instantiate(itemPrefab, content, true);
        item.transform.SetAsFirstSibling();
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        var ugcCover = item.transform.Find("ResCover").GetComponent<RawImage>();
        var selectBg = item.transform.Find("SelectBg").gameObject;
        var btnCover = ugcCover.GetComponent<Button>();
        ugcCover.gameObject.SetActive(false);

        var newItem = new UgcWeaponItem()
        {
            itemObj = item,
            resCover = ugcCover,
            selectBg = selectBg,
            mapInfo = mapInfo,
        };

        newItem.selectBg.gameObject.SetActive(false);
        btnCover.onClick.AddListener(() => { OnWeaponItemClick(newItem); });

        if (createCb != null)
        {
            createCb(newItem);
        }
    }


    /// <summary>
    /// 打开背包选择UGC素材
    /// </summary>
    private void OpenUgcBagpackAndChoose()
    {
        LoggerUtils.Log($"{WEAPON_LOG} OpenUgcBagpackAndChoose");

#if UNITY_EDITOR
        ResStorePanel.SetItemCallback(ChoosePropCallback);
        ResStorePanel.Show();
#else
        ResBagManager.Inst.OpenResPage(ChoosePropCallback);
#endif
    }

    public UgcWeaponItem GetWeaponItemByMapInfo(MapInfo mapInfo)
    {
        if (mapInfo != null && ugcWeaponItems.ContainsKey(mapInfo.mapId))
        {
            return ugcWeaponItems[mapInfo.mapId];
        }

        return null;
    }

    public UgcWeaponItem GetWeaponItemByRd(string rId)
    {
        if (!string.IsNullOrEmpty(rId) && ugcWeaponItems.ContainsKey(rId))
        {
            return ugcWeaponItems[rId];
        }

        return null;
    }

    #region Create Weapons

    /// <summary>
    /// 选择了UGC素材后回调
    /// </summary>
    protected virtual void ChoosePropCallback(MapInfo mapInfo)
    {
        LoggerUtils.Log($"{WEAPON_LOG} ChoosePropCallback {mapInfo.mapId}");
        var oldPos = curBehav.transform.position;
        var lastPos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
        SecondCachePool.Inst.DestroyEntity(curBehav.gameObject);
        CreateWeaponItemUI(mapInfo);
        CreateUgcWeaponInScene(mapInfo, lastPos);

        RefreshUI();
    }

    /// <summary>
    /// 选择下发武器Panel的ugcItem
    /// </summary>
    public virtual void OnWeaponItemClick(UgcWeaponItem weaponItem)
    {
        var oldPos = curBehav.transform.position;
        var lastPos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
        SceneBuilder.Inst.DestroyEntity(curBehav.gameObject);
        CreateUgcWeaponInScene(weaponItem.mapInfo, lastPos, weaponItem);
        ChooseItem(weaponItem.mapInfo.mapId);
        RefreshUI();
    }

    /// <summary>
    /// 创建UGC武器，分别在从背包选择Ugc和点击panel下发item后触发
    /// </summary>
    protected void CreateUgcWeaponInScene(MapInfo mapInfo, Vector3 pos, UgcWeaponItem weaponItem = null)
    {
        var resItemDataDic = ResBagManager.Inst.resItemDataDic;
        var rId = "";
        var mapJsonContentStr = "";

        if (resItemDataDic != null && resItemDataDic.ContainsKey(mapInfo.mapId))
        {
            rId = resItemDataDic[mapInfo.mapId].mapInfo.mapId;
            mapJsonContentStr = resItemDataDic[mapInfo.mapId].mapJsonContent;
        }
        else if (weaponItem != null)
        {
            rId = weaponItem.mapInfo.mapId;
            mapJsonContentStr = weaponItem.mapJsonContent;
        }

        var nBehav = WeaponCreateUtils.Inst.CreateSingleUgcWeapon(pos, mapInfo, rId, mapJsonContentStr);

        if (nBehav)
        {
            AddDataToWeaponManager(nBehav, rId);
            if(PackPanel.Instance != null && PackPanel.Instance.gameObject.activeSelf)
            {
                MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
                return;
            }
            EditModeController.SetSelect?.Invoke(nBehav.entity);
        }
        LoggerUtils.Log($"{WEAPON_LOG} CreateUgcWeaponInScene {mapInfo.mapId}");
    }

    #endregion
}