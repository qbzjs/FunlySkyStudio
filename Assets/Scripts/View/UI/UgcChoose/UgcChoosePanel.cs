using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

/// <summary>
/// Author:Shaocheng
/// Description:提供选择UGC素材作为道具的基础能力。原来最早是武器中使用到，但和武器Manager耦合太紧，所以独立出来，供其他道具也可以使用
/// Date: 2022-7-26 15:03:01
/// </summary>
public class UgcChooseItem
{
    public GameObject itemObj;
    public RawImage resCover; //Ugc素材封面图
    public GameObject selectBg;
    public MapInfo mapInfo;
    public string mapJsonContent;
}

public abstract class UgcChoosePanel<T> : CommonMatColorPanel<T> where T : CommonMatColorPanel<T>
{
    #region UI

    public GameObject goNoPanel;
    public GameObject goHasPanel;
    public Button closePanelBtn;

    public Button btnChoosePropInNoPanel;
    public Button btnChoosePropInHasPanel;

    public RectTransform content;

    #endregion

    //已经选过的UGC,显示在panel下方供选择, key-rid
    protected Dictionary<string, UgcChooseItem> UgcChooseItems = new Dictionary<string, UgcChooseItem>();

    //当前选中的item rid
    private string curChooseItemRid = string.Empty;

    protected string UGC_CHOOSE_LOG = "UgcChoosePanel:";
    protected NodeBaseBehaviour curBehav;

    //上一次选择的item
    protected UgcChooseItem lastChooseItem;

    #region 各道具按需实现

    /// <summary>
    /// 获取这个道具所有使用到的ugc素材rid
    /// </summary>
    /// <returns></returns>
    protected virtual List<string> GetAllUgcRidList()
    {
        return null;
    }

    /// <summary>
    /// UGC素材在场景中创建完毕，后续业务模块override这个方法进行behaviour操作，比如挂自己的component等等
    /// </summary>
    protected virtual void AfterUgcCreateFinish(NodeBaseBehaviour bev, string rId)
    {
    }

    /// <summary>
    /// 刷新ui
    /// </summary>
    protected virtual void RefreshUI()
    {
    }
    
    /// <summary>
    /// 是不是要在创建了UGC之后删掉原来的节点
    /// </summary>
    /// <returns></returns>
    protected virtual bool DestroySelf()
    {
        return true;
    }
    
    /// <summary>
    /// 是不是要在创建了UGC之后选中UGC
    /// </summary>
    /// <returns></returns>
    protected virtual bool SelectUgc()
    {
        return true;
    }

    #endregion

    #region 生命周期

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (btnChoosePropInNoPanel != null)
        {
            btnChoosePropInNoPanel.onClick.AddListener(OpenUgcBagpackAndChoose);
        }

        if (btnChoosePropInHasPanel != null)
        {
            btnChoosePropInHasPanel.onClick.AddListener(OpenUgcBagpackAndChoose);
        }

        if (closePanelBtn != null)
        {
            closePanelBtn.onClick.AddListener(OnCloseBtnClick);
        }

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
        InitUgcChoosePanel();
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        StopAllCoroutines();
        curChooseItemRid = string.Empty;
    }

    #endregion

    #region 再次进入编辑模式，初始化UIpanel

    protected void InitUgcChoosePanel()
    {
        var allUsedUgcs = GetAllUgcRidList();

        if (allUsedUgcs != null && allUsedUgcs.Count > 0)
        {
            var ugcToReqArray = new List<string>();
            foreach (var usedUgc in allUsedUgcs)
            {
                if (!UgcChooseItems.ContainsKey(usedUgc))
                {
                    ugcToReqArray.Add(usedUgc);
                }
            }

            if (ugcToReqArray.Count > 0)
            {
                LoggerUtils.Log($"{UGC_CHOOSE_LOG} InitUgcChoosePanel ugcToReqArray：{ugcToReqArray}");

                var httpMapDataInfo = new HttpBatchMapDataInfo
                {
                    mapIds = ugcToReqArray.ToArray(),
                    dataType = 1
                };

                MapLoadManager.Inst.GetBatchMapInfo(httpMapDataInfo, getMapInfo =>
                {
                    LoggerUtils.Log($"{UGC_CHOOSE_LOG} InitUgcChoosePanel GetBatchMapInfo Success:{JsonConvert.SerializeObject(getMapInfo)}");

                    var mapInfos = getMapInfo.mapInfos;
                    if (mapInfos != null && mapInfos.Length > 0)
                    {
                        foreach (var mapInfo in mapInfos)
                        {
                            CreateItem(mapInfo);
                        }
                    }
                }, error => { LoggerUtils.LogError($"{UGC_CHOOSE_LOG} GetBatchMapInfo faild:{error}"); });
            }
        }
    }

    private IEnumerator LoadResCoverTexture(MapInfo mapInfo)
    {
        yield return ResBagManager.Inst.LoadTexture(mapInfo.mapCover, (texture) =>
        {
            if (texture)
            {
                if (UgcChooseItems.ContainsKey(mapInfo.mapId))
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
                        // LoggerUtils.Log($"{UGC_CHOOSE_LOG}-->LoadTexture and get json success-->" + content);
                        string jsonStr = ZipUtils.SaveZipFromByte(content);
                        CreateItemUI(mapInfo, (newItem) =>
                        {
                            newItem.resCover.texture = texture;
                            newItem.resCover.gameObject.SetActive(true);
                            newItem.mapJsonContent = jsonStr;
                            UgcChooseItems.Add(mapInfo.mapId, newItem);
                        });
                        ChooseItem(curChooseItemRid);
                    }, (error) => { LoggerUtils.LogError($"{UGC_CHOOSE_LOG} Get ResMapJson Fail"); }));
                }
                else
                {
                    StartCoroutine(GetText(jsonUrl, (content) =>
                    {
                        // LoggerUtils.Log($"{UGC_CHOOSE_LOG}-->LoadTexture and get GetText json success-->" + content);
                        CreateItemUI(mapInfo, (newItem) =>
                        {
                            newItem.resCover.texture = texture;
                            newItem.resCover.gameObject.SetActive(true);
                            newItem.mapJsonContent = content;
                            UgcChooseItems.Add(mapInfo.mapId, newItem);
                        });
                        ChooseItem(curChooseItemRid);
                    }, (error) => { LoggerUtils.LogError($"{UGC_CHOOSE_LOG} Get ResMapJson Fail"); }));
                }
            }
        }, (error) => { LoggerUtils.LogError($"{UGC_CHOOSE_LOG} LoadResCoverTexture Fail: {mapInfo.mapCover},error:{error}"); });
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
        curChooseItemRid = rid;

        foreach (var item in UgcChooseItems.Values)
        {
            if (item != null)
            {
                item.selectBg.SetActive(false);
            }
        }

        if (!string.IsNullOrEmpty(rid) && UgcChooseItems.ContainsKey(rid))
        {
            var selectBg = UgcChooseItems[rid].selectBg;
            if (selectBg) selectBg.gameObject.SetActive(true);
            lastChooseItem = GetUgcChooseItemByRd(rid);
        }
    }

    /// <summary>
    /// 创建UGC UI item
    /// </summary>
    /// <param name="mapInfo"></param>
    protected void CreateItem(MapInfo mapInfo)
    {
        if (!UgcChooseItems.ContainsKey(mapInfo.mapId))
        {
            if (ResBagManager.Inst.resItemDataDic.ContainsKey(mapInfo.mapId))
            {
                //直接从素材背包数据获取封面
                var resItemInfo = ResBagManager.Inst.resItemDataDic[mapInfo.mapId];
                CreateItemUI(mapInfo, (newItem) =>
                {
                    newItem.resCover.texture = resItemInfo.mapCover;
                    newItem.resCover.gameObject.SetActive(true);
                    newItem.mapJsonContent = ResBagManager.Inst.resItemDataDic[mapInfo.mapId].mapJsonContent;
                    UgcChooseItems.Add(mapInfo.mapId, newItem);
                });
            }
            else
            {
                //二次编辑时，还没打开素材背包界面，手动拉取封面
                CoroutineManager.Inst.StartCoroutine(LoadResCoverTexture(mapInfo));
            }
        }
    }

    //TODO:暂未做缓存池或循环列表，待优化
    private void CreateItemUI(MapInfo mapInfo, Action<UgcChooseItem> createCb)
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

        var newItem = new UgcChooseItem()
        {
            itemObj = item,
            resCover = ugcCover,
            selectBg = selectBg,
            mapInfo = mapInfo,
        };

        newItem.selectBg.gameObject.SetActive(false);
        btnCover.onClick.AddListener(() => { OnUgcChooseItemClick(newItem); });

        if (createCb != null)
        {
            createCb(newItem);
        }
    }


    /// <summary>
    /// 打开背包选择UGC素材，现在是打开端上选择ugc素材页面
    /// </summary>
    private void OpenUgcBagpackAndChoose()
    {
        LoggerUtils.Log($"{UGC_CHOOSE_LOG} OpenUgcBagpackAndChoose");

#if UNITY_EDITOR
        ResStorePanel.SetItemCallback(ChooseUgcCallback);
        ResStorePanel.Show();
#else
        ResBagManager.Inst.OpenResPage(ChooseUgcCallback);
#endif

        if (SelectUgc())
        {
            Hide();
        }
    }

    public UgcChooseItem GetUgcChooseItemByRd(string rId)
    {
        if (!string.IsNullOrEmpty(rId) && UgcChooseItems.ContainsKey(rId))
        {
            return UgcChooseItems[rId];
        }

        return null;
    }

    #region 选择ugc作为道具

    /// <summary>
    /// 从端上选择了UGC素材后回调
    /// </summary>
    protected virtual void ChooseUgcCallback(MapInfo mapInfo)
    {
        LoggerUtils.Log($"{UGC_CHOOSE_LOG} ChooseUgcCallback {mapInfo.mapId}");
        var oldPos = curBehav.transform.position;
        var lastPos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
        if (DestroySelf())
        {
            SecondCachePool.Inst.DestroyEntity(curBehav.gameObject);
        }

        CreateItem(mapInfo);
        CreateNewPropInSceneByUgcChoose(mapInfo, lastPos);
        RefreshUI();
        SetLastChooseUgcItem(lastChooseItem);
    }

    /// <summary>
    /// 点击Item，切换UGC素材，用新素材来展现道具
    /// </summary>
    public virtual void OnUgcChooseItemClick(UgcChooseItem item)
    {
        var oldPos = curBehav.transform.position;
        var lastPos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
        if (DestroySelf())
        {
            SceneBuilder.Inst.DestroyEntity(curBehav.gameObject);
        }

        CreateNewPropInSceneByUgcChoose(item.mapInfo, lastPos, item);
        ChooseItem(item.mapInfo.mapId);
        RefreshUI();
        SetLastChooseUgcItem(lastChooseItem);
    }
    public virtual void SetLastChooseUgcItem(UgcChooseItem item)
    {

    }
    /// <summary>
    /// 用选中的ugc创建道具，分别在从选择Ugc后和点击panel的item后触发
    /// </summary>
    protected void CreateNewPropInSceneByUgcChoose(MapInfo mapInfo, Vector3 pos, UgcChooseItem item = null)
    {
        var resItemDataDic = ResBagManager.Inst.resItemDataDic;
        var rId = string.Empty;
        var mapJsonContentStr = string.Empty;

        if (resItemDataDic != null && resItemDataDic.ContainsKey(mapInfo.mapId))
        {
            rId = resItemDataDic[mapInfo.mapId].mapInfo.mapId;
            mapJsonContentStr = resItemDataDic[mapInfo.mapId].mapJsonContent;
        }
        else if (item != null)
        {
            rId = item.mapInfo.mapId;
            mapJsonContentStr = item.mapJsonContent;
        }

        var nBehav = UgcChooseManager.Inst.CreateSingleUgcAsProp(pos, mapInfo, rId, mapJsonContentStr);
        if (nBehav)
        {
            AfterUgcCreateFinish(nBehav, rId);
            if(PackPanel.Instance != null && PackPanel.Instance.gameObject.activeSelf)
            {
                MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
                return;
            }

            if (SelectUgc())
            {
                EditModeController.SetSelect?.Invoke(nBehav.entity);
            }

            LoggerUtils.Log($"{UGC_CHOOSE_LOG} CreateNewPropInSceneByUgcChoose {mapInfo.mapId}");
        }
        else
        {
            LoggerUtils.Log($"{UGC_CHOOSE_LOG} CreateNewPropInSceneByUgcChoose Error !!!{mapInfo.mapId}");
        }
    }

    #endregion
}