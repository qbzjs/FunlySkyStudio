using System;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class FishingUgcPanel : MonoBehaviour
{
    public Action<MapInfo, string, string> onSelectUgc;

    private Button selectBtn;
    private Transform itemContent;
    private GameObject itemPrefab;

    private string _curSelectItem = "";
    private Dictionary<string, GameObject> _itemDic = new Dictionary<string, GameObject>();

    private void Awake()
    {
        var adapter = GetComponent<DataAdapter>();
        selectBtn = adapter.FindComponent<Button>("SelectBtn");
        itemContent = adapter.FindComponent<Transform>("ItemContent");

        itemPrefab = ResManager.Inst.LoadResNoCache<GameObject>("Prefabs/UI/Panel/WeaponItem");

        selectBtn.onClick.AddListener(() => { OpenResPage(); });
    }

    public void SetData(List<string> items)
    {
        _curSelectItem = "";

        foreach (var kvp in _itemDic)
            kvp.Value.SetActive(false);

        var downloadLst = new List<string>();
        if (items != null && items.Count > 0)
        {
            foreach (var rid in items)
            {
                if (_itemDic.ContainsKey(rid))
                {
                    _itemDic[rid].SetActive(true);
                    continue;
                }

                if (ResBagManager.Inst.resItemDataDic.ContainsKey(rid))
                {
                    var resItemData = ResBagManager.Inst.resItemDataDic[rid];
                    OnGetItemInfo(resItemData.mapInfo, resItemData.mapJsonContent, resItemData.mapCover);
                }
                else
                {
                    downloadLst.Add(rid);
                }
            }

            if (downloadLst.Count > 0)
            {
                var httpMapDataInfo = new HttpBatchMapDataInfo();
                httpMapDataInfo.mapIds = downloadLst.ToArray();
                httpMapDataInfo.dataType = 1;

                MapLoadManager.Inst.GetBatchMapInfo(httpMapDataInfo, (batchMapInfo) => {
                    var mapInfos = batchMapInfo.mapInfos;
                    if (mapInfos != null && mapInfos.Length > 0)
                    {
                        foreach (var mapInfo in mapInfos)
                        {
                            DownloadItemInfo(mapInfo, (mapInfo, mapJsonContent, mapCover) => {
                                OnGetItemInfo(mapInfo, mapJsonContent, mapCover);
                            });
                        }
                    }
                }, error => {
                    LoggerUtils.LogError(string.Format("Get batch map info failed: {0}", error));
                });
            }
        }
    }

    public void SelectItem(string rid)
    {
        _curSelectItem = rid;

        foreach (var kvp in _itemDic)
        {
            var itemRid = kvp.Key;
            var itemObj = kvp.Value;
            var selectBg = itemObj.transform.Find("SelectBg").gameObject;
            selectBg.SetActive(itemRid == _curSelectItem);
        }
    }

    private void OnGetItemInfo(MapInfo mapInfo, string mapJsonContent, Texture mapCover)
    {
        if (! _itemDic.ContainsKey(mapInfo.mapId))
            _itemDic.Add(mapInfo.mapId, CreateItem(mapInfo, mapJsonContent, mapCover));

        if (_curSelectItem == mapInfo.mapId)
        {
            SelectItem(_curSelectItem);
            onSelectUgc?.Invoke(mapInfo, mapInfo.mapId, mapJsonContent);
        }
    }

    private void OpenResPage()
    {
        ResBagManager.Inst.OpenResPage((mapInfo) => {
            _curSelectItem = mapInfo.mapId;

            if (ResBagManager.Inst.resItemDataDic.ContainsKey(mapInfo.mapId))
            {
                var resItemData = ResBagManager.Inst.resItemDataDic[mapInfo.mapId];
                OnGetItemInfo(mapInfo, resItemData.mapJsonContent, resItemData.mapCover);
            }
            else
            {
                DownloadItemInfo(mapInfo, (mapInfo, mapJsonContent, mapCover) => {
                    OnGetItemInfo(mapInfo, mapJsonContent, mapCover);
                });
            }
        });
    }

    private void DownloadItemInfo(MapInfo mapInfo, Action<MapInfo, string, Texture> callback)
    {
        CoroutineManager.Inst.StartCoroutine(ResBagManager.Inst.LoadTexture(mapInfo.mapCover, (texture) => {
            if (texture == null)
                return;

            string jsonUrl = mapInfo.propsJson;
            if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
            {
                StartCoroutine(GameUtils.GetByte(jsonUrl, (content) => {
                    callback?.Invoke(mapInfo, ZipUtils.SaveZipFromByte(content), texture);
                }, (error) => {
                    LoggerUtils.LogError(string.Format("GetResMapJson failed: url={0}, error={1}", jsonUrl, error));
                }));
            }
            else
            {
                StartCoroutine(GameUtils.GetText(jsonUrl, (content) => {
                    callback?.Invoke(mapInfo, content, texture);
                }, (error) => {
                    LoggerUtils.LogError(string.Format("GetResMapJson failed: url={0}, error={1}", jsonUrl, error));
                }));
            }
        }, (error) => {
            LoggerUtils.LogError(string.Format("LoadResCoverTexture failed: mapCover={0}, error={1}", mapInfo.mapCover, error));
        }));
    }

    private GameObject CreateItem(MapInfo mapInfo, string mapJsonContent, Texture coverTex)
    {
        var item = Instantiate(itemPrefab, itemContent);
        item.transform.SetAsFirstSibling();
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        var coverImg = item.transform.Find("ResCover").GetComponent<RawImage>();
        var selectBg = item.transform.Find("SelectBg").gameObject;
        var coverBtn = coverImg.GetComponent<Button>();

        coverImg.texture = coverTex;
        coverImg.gameObject.SetActive(true);
        selectBg.gameObject.SetActive(false);

        coverBtn.onClick.AddListener(() => {
            SelectItem(mapInfo.mapId);
            onSelectUgc?.Invoke(mapInfo, mapInfo.mapId, mapJsonContent);
        });

        return item;
    }
}
