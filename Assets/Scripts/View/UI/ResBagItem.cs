using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class ResBagItem : MonoBehaviour
{
    //public Animator loading;
    public RawImage CoverImg;
    public Button ResBtn;

    [HideInInspector]
    public MapInfo mapInfo;

    private Dictionary<string,ResItemData> resItemDataDic = new Dictionary<string, ResItemData>();
    private bool isLoading = true;

    private Action<MapInfo> outItemClick; //外部调用素材背包，设置特殊的点击事件

    private void Start()
    {
        ResBtn.onClick.AddListener(OnResBtnClick);
    }

    private void OnDisable()
    {
        CoverImg.gameObject.SetActive(false);
        //loading.gameObject.SetActive(true);
        isLoading = true;
    }

    public void SetBagItemInfo(MapInfo mapInfo)
    {
        resItemDataDic = ResBagManager.Inst.resItemDataDic;
        var AlreadyShowResItem = ResBagManager.Inst.AlreadyShowResItem;
        this.mapInfo = mapInfo;
        if (gameObject.activeSelf)
        {
            if (AlreadyShowResItem.Contains(mapInfo.mapId) && !resItemDataDic.ContainsKey(mapInfo.mapId))
            {
                ResBagManager.Inst.LoadSingleResItemData(mapInfo);
                return;
            }
            if (resItemDataDic.ContainsKey(mapInfo.mapId))
            {
                CoverImg.texture = resItemDataDic[mapInfo.mapId].mapCover;
                //loading.gameObject.SetActive(false);
                isLoading = false;
                CoverImg.gameObject.SetActive(true);
                return;
            }
        }
    }

    [Obsolete("此方法仅 Unity PC 端使用，其余情况不适用")]
    private void OnResBtnClick()//弃用！！！！该方法体移动至resbagmanager
    {
        // if (loading.gameObject.activeSelf == true)
        // {
        //    LoggerUtils.Log("loading.gameObject.activeSelf");
        //    return;
        // }
        if (isLoading)
        {
           LoggerUtils.Log("ResItem isLoading");
           return;
        }
        if (resItemDataDic.ContainsKey(mapInfo.mapId) && resItemDataDic[mapInfo.mapId].mapJsonContent != null)
        {
           //外部调用素材背包要进行的操作
           if (outItemClick != null)
           {
               outItemClick.Invoke(mapInfo);
               return;
           }
            
           var rId = resItemDataDic[mapInfo.mapId].mapInfo.mapId;
           var pos = CameraUtils.Inst.GetCreatePosition();

           // 将离线数据加入当前数据缓存
        //    bool canOfflineRender = false;
           if (mapInfo.renderList != null)
           {
            UGCBehaviorManager.Inst.AddOfflineRenderData(mapInfo.renderList);
            //    var renderData = mapInfo.renderList[0]?.abList[0];
            //    if (renderData != null)
            //    {
            //        renderData.version = mapInfo.renderList[0]?.version;
            //        if (!GlobalFieldController.offlineRenderDataDic.ContainsKey(renderData.mapId))
            //        {
            //            GlobalFieldController.offlineRenderDataDic.Add(renderData.mapId, renderData);
            //        }

            //        if (GlobalFieldController.CurMapInfo != null)
            //        {
            //            if (GlobalFieldController.CurMapInfo.renderList == null)
            //            {
            //                GlobalFieldController.CurMapInfo.renderList = new[] {new OfflineRenderListObj()
            //                {
            //                    version = renderData.version,
            //                    abList = new []{ renderData}
            //                }};
            //            }
            //            else
            //            {
            //                var offlineRenderList = GlobalFieldController.CurMapInfo.renderList;
            //                var tmpRenderList = new List<OfflineRenderListObj>(offlineRenderList);
            //                var renderListObj = tmpRenderList.Find(tmp => tmp.version == renderData.version);
            //                if (renderListObj == null)
            //                {
            //                    renderListObj = new OfflineRenderListObj() {version = renderData.version};
            //                    tmpRenderList.Add(renderListObj);
            //                }
                            
            //                var tmpAbList = new List<OfflineRenderData>(renderListObj.abList);
            //                if (tmpAbList.Find(tmp => tmp.mapId == renderData.mapId) == null)
            //                {
            //                    tmpAbList.Add(renderData);
            //                }
            //                renderListObj.abList = tmpAbList.ToArray();
            //                GlobalFieldController.CurMapInfo.renderList = offlineRenderList.ToArray();
            //            }
                //    }
                //    canOfflineRender = true;
            //    }
           }

           var nBehav = SceneBuilder.Inst.ParsePropAndBuild(resItemDataDic[mapInfo.mapId].mapJsonContent, pos, rId);
           EditModeController.SetSelect?.Invoke(nBehav.entity);

            ResStorePanel.Hide();
        }
    }

    public void SetOutsideItemClick(Action<MapInfo> itemClick)
    {
        outItemClick = itemClick;
    }
}
