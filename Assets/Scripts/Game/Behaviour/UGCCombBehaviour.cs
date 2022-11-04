using System;
using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HLODSystem;
using UnityEngine;
using BudEngine.NetEngine;

public class UGCCombBehaviour : CombineBehaviour
{
    private Collider[] childColliders;
    private int[] oldLayers;
    private Color[] oldColor;
    private Dictionary<Material, Color> hColorDic;
    private Dictionary<Material, Color> oColorDic;
    
    [SerializeField]
    public UGCModelType modelType = UGCModelType.None;
    
    [SerializeField]
    private UGCModelType targetModelType = UGCModelType.None;



    private bool isDCRequesting;
    public bool isSoldOut;
    public override void OnRayEnter()
    {
        if (entity.HasComponent<FireworkComponent>())
        {
            var fireworComp = entity.Get<FireworkComponent>();
            if (fireworComp.isControl == (int)FireworkControl.NOT_SUPPORT)
            {
                PortalPlayPanel.Show();
                PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Firework);
                PortalPlayPanel.Instance.AddButtonClick(OnClickFirework);
                PortalPlayPanel.Instance.SetTransform(transform);
                return;
            }
        }
        if (entity.HasComponent<SeesawSeatComponent>() && !entity.Get<SeesawSeatComponent>().isFull)
        {
            if (SeesawManager.Inst.CanUseSeesaw())
            {
                PortalPlayPanel.Show();
                PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Seesaw);
                PortalPlayPanel.Instance.SetTransform(transform);
                PortalPlayPanel.Instance.AddButtonClick(OnClickSeesawSeat, true);
                return;
            }
        }
        var uComp = entity.Get<UGCPropComponent>();
        if (uComp.isTradable == 1)
        {
            var dcComp = entity.Get<DcComponent>();
            if (dcComp != null && dcComp.isDc == (int)IsDC.True)
            {
                PortalPlayPanel.Show();
                PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Dc);
                PortalPlayPanel.Instance.AddButtonClick(OnClickDCShop);
                PortalPlayPanel.Instance.SetTransform(transform);
            }
            else
            {
                PortalPlayPanel.Show();
                PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Shopping);
                PortalPlayPanel.Instance.AddButtonClick(OnClickShop);
                PortalPlayPanel.Instance.SetTransform(transform);
            }
        }
        base.OnRayEnter();
    }

    private void OnClickSeesawSeat()
    {
        if (!StateManager.Inst.CheckCanSitOnSeesaw())
        {
            return;
        }
        
        int index = entity.Get<SeesawSeatComponent>().index;

        SeesawManager.Inst.PlayerSendOnSeesaw(GetHashCode(), index == 1);
        PortalPlayPanel.Hide();
    }

    private void OnClickShop()
    {
        NativeResInfo ugcMapInfo = new NativeResInfo
        {
            type = 1,
            mapId = entity.Get<GameObjectComponent>().resId
        };
        string ugcJson = JsonConvert.SerializeObject(ugcMapInfo);
        
        StorePanel.Show();
        StorePanel.Instance.BindEntity(entity);
        StorePanel.Instance.ClickShoping(ugcJson,transform);
        DataLogUtils.DetailPageView(ugcMapInfo.mapId, null, "prop");//上报素材详情页预览
    }
    private void OnClickDCShop()
    {
        if (isDCRequesting)
        {
            return;
        }
        var dcComp = entity.Get<DcComponent>();
        DCResInfo info = new DCResInfo()
        {
            itemId = dcComp.dcId,
            budActId = dcComp.budActId
        };
        var ugcJson = JsonConvert.SerializeObject(info);
        if (!string.IsNullOrEmpty(ugcJson))
        {
            HttpUtils.MakeHttpRequest("/ugcmap/itemInfo", (int)HTTP_METHOD.GET, ugcJson, OnDCPlayerInfoSuccess, OnGetPlayerInfoFail);
            isDCRequesting = true;
        }
    }
    private void OnClickFirework()
    {
        var go = entity.Get<GameObjectComponent>().bindGo;
        var behv = go.GetComponentInChildren<NodeBaseBehaviour>();
        FireworkManager.Inst.OnTriggerFirework(behv);
    }
    private void OnDCPlayerInfoSuccess(string content)
    {
        isDCRequesting = false;
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        StoreResData socialResInfo = JsonConvert.DeserializeObject<StoreResData>(hResponse.data);
        LoggerUtils.Log("content:" + hResponse.data);
        if (socialResInfo.mapInfo == null || socialResInfo.mapInfo.mapCreator == null || socialResInfo.mapInfo.dcInfo == null)
        {
            LoggerUtils.LogError($"mapInfo data error:{socialResInfo}");
            return;
        }
        if (socialResInfo.mapInfo.dcInfo.itemStatus == (int)DCItemStatus.Sold)
        {
        
            SetSoldOut();
            OnSoldOut(socialResInfo.mapInfo.dcInfo.itemId, socialResInfo.mapInfo.dcInfo.walletAddress);
            isSoldOut = true;
            return;
        }
        DCResPanel.Show();
        var dcComp = entity.Get<DcComponent>();
        DCResInfo info = new DCResInfo()
        {
            itemId = dcComp.dcId,
            budActId = dcComp.budActId
        };
        var ugcJson = JsonConvert.SerializeObject(info);
        DCResPanel.Instance.SetOwnedPanel(string.IsNullOrEmpty(dcComp.budActId));
        DCResPanel.Instance.OnClickDC(ugcJson, transform);
        DCResPanel.Instance.InfoSuccess(socialResInfo);
        DCResPanel.Instance.HideOrShowPanel(false);

        DataLogUtils.DetailPageView(socialResInfo.mapInfo.mapId, socialResInfo.mapInfo.dcInfo, "prop");//上报素材详情页预览

    }
    private void OnSoldOut(string id, string address)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            DcSaveInfo info = new DcSaveInfo()
            {
                dcId = id,
                address = address
            };
            CustomData data = new CustomData();
            data.type = (int)ChatCustomType.DCResSoldOut;
            data.data = JsonConvert.SerializeObject(info);
            RoomChatData roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Custom,
                data = JsonConvert.SerializeObject(data),
            };
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }
    }
    private void OnGetPlayerInfoFail(string content)
    {
        LoggerUtils.LogError($"OnGetPlayerInfoFail:{content}");
        isDCRequesting = false;

    }
    public void SetCanBuyInMap()
    {
        var uComp = entity.Get<UGCPropComponent>();
        bool hasEdibility = entity.HasComponent<EdibilityComponent>();
        if (childColliders == null || childColliders.Length == 0 || childColliders.Any(tmp => tmp == null))
        {
            InitChildLayers();
        }
        for (int i = 0; i < childColliders.Length; i++)
        {
            GameObject childGO = childColliders[i].gameObject;
            if (childGO != null)
            {
                childGO.layer = (uComp.isTradable == 1 || hasEdibility) ? LayerMask.NameToLayer("Touch") : oldLayers[i];
            }
        }
    }


    public void InitChildLayers()
    {
        childColliders = transform.GetComponentsInChildren<Collider>();
        oldLayers = new int[childColliders.Length];
        for (int i = 0; i < oldLayers.Length; i++)
            oldLayers[i] = childColliders[i].gameObject.layer;
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }
    
    public void SetRedRender()
    {
        if (modelType != UGCModelType.Json)
        {
            return;
        }
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        var mpb = new MaterialPropertyBlock();
        var zeroColor = new Color(0, 0, 0, 0);
        oldColor = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].GetPropertyBlock(mpb);
                oldColor[i] = mpb.GetColor("_Color");
                if (oldColor[i] == zeroColor)
                {
                    oldColor[i] = renderers[i].material.GetColor("_Color");
                }
                mpb.SetColor("_Color", Color.red);
                renderers[i].SetPropertyBlock(mpb);
            }
        }
    }

    public void SetNormalRender()
    {
        if (modelType != UGCModelType.Json)
        {
            return;
        }
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        var mpb = new MaterialPropertyBlock();
        if (oldColor != null)
        {
            for (int i = 0; i < oldColor.Length; i++)
            {
                if (renderers[i].material.HasProperty("_Color"))
                {
                    renderers[i].GetPropertyBlock(mpb);
                    mpb.SetColor("_Color",oldColor[i]);
                    renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        var nBevhList = GetComponentsInChildren<NodeBaseBehaviour>();
        if(nBevhList.Length == 1)
        {
            if (hColorDic == null || oColorDic == null) GetHightLightColor();
            HighLightUtils.HighLightOnOffLineUgc(isHigh, gameObject, oColorDic, hColorDic);
        }
    }

    private void GetHightLightColor()
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        hColorDic = new Dictionary<Material, Color>();
        oColorDic = new Dictionary<Material, Color>();
        for (int i = 0; i < renderers.Length; i++)
        {
            var materials = renderers[i].materials;
            foreach (var mat in materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    var oColor = mat.GetColor("_Color");
                    var hColor = DataUtils.GetHighlightColor(oColor);
                    hColorDic.Add(mat, hColor);
                    oColorDic.Add(mat, oColor);
                }
            }
        }
    }

    public override void OnReset()
    {
        base.OnReset();
        Release();
        hlodState = HLODState.NoThing;
        targetModelType = UGCModelType.None;
        modelType = UGCModelType.None;
    }

    private void Release()
    {
        if (assetObj == null)
        {
            return;
        }
        if (modelType == UGCModelType.Json)
        {
            foreach (var tmpNodeBase in assetObj.GetComponentsInChildren<NodeBaseBehaviour>())
            {
                SceneBuilder.Inst.RemoveNodeBehaviour(tmpNodeBase);
            }
            UGCModelCachePool.Inst.DelOriginObj(rid, modelType, assetObj);
            Destroy(assetObj);
        }
        else
        {
            UGCModelCachePool.Inst.Release(rid, modelType, assetObj);
        }

        allRenderers = null;
        assetObj = null;
    }

    // 异步切换状态,若本地无缓存，则加载缓存
    public void SetLODStatusAsync(HLODState state)
    {
        if (state == hlodState || isSoldOut)
        { 
            return;
        }
        hlodState = state;
        switch (hlodState)
        {
            case HLODState.NoThing:
                targetModelType = UGCModelType.None;
                modelType = UGCModelType.None;
                break;
            case HLODState.Cull:
                AsyncChangeTo(UGCModelType.None);
                if (!UGCModelCachePool.Inst.IsContains(rid, UGCModelType.Low))
                {
                    OfflineResManager.Inst.LoadFileAsync(rid, UGCModelType.Low, null, 100, 2);  
                }
                break;
            case HLODState.Low:
                AsyncChangeTo(UGCModelType.Low);
                break;
            case HLODState.High:
                AsyncChangeTo(UGCModelType.High);
                break;
        }
    }

    // 同步状态切换 若本地无缓存，需要降级显示
    public override void SetLODStatus(HLODState state)
    {
        if (state == hlodState || isSoldOut)
        { 
            return;
        }
        hlodState = state;

        switch (hlodState)
        {
            case HLODState.NoThing:
                targetModelType = UGCModelType.None;
                modelType = UGCModelType.None;
                break;
            case HLODState.Cull:
                AsyncChangeTo(UGCModelType.None);
                if (!UGCModelCachePool.Inst.IsContains(rid, UGCModelType.Low))
                {
                    OfflineResManager.Inst.LoadFileAsync(rid, UGCModelType.Low, null, 100, 2);  
                }
                break;
            case HLODState.Low:
                ChangeToLow();
                break;
            case HLODState.High:
                ChangeToHigh();
                break;
        }
    }

    
    /// <summary>
    /// 异步状态转换, 每次状态转换时，若本地无缓存，则下载并加载缓存
    /// </summary>
    /// <param name="type"></param>
    public void AsyncChangeTo(UGCModelType type)
    {
        targetModelType = type;
        if (targetModelType == modelType)
        {
            return;
        }
        if (targetModelType == UGCModelType.None)
        {
            Release();
            modelType = targetModelType;
            return;
        }
        
        var tmpAssetObj = UGCModelCachePool.Inst.Get(rid, targetModelType, transform);
        if (tmpAssetObj == null)
        {
            if (targetModelType == UGCModelType.Json)
            {
                ChangeToJson();
            }
            else
            {
                OfflineResManager.Inst.LoadFileAsync(rid, targetModelType, (isSuccess) =>
                {
                    if (modelType == targetModelType) return;
                    if (isSuccess)
                    {
                        // 加载成功，则切换到对应 模型
                        AsyncChangeTo(targetModelType);
                    }
                    else
                    {
                        // 加载失败，则降低一级模型 High -> Low -> Json
                        AsyncChangeTo(targetModelType + 1);
                    }
                });
            }
        }
        else
        {
            SetAssetObj(tmpAssetObj);
            modelType = targetModelType;
            if (!IsMatchModelStatus())
            {
                OfflineRenderData renderData = null;
                if (!string.IsNullOrEmpty(rid))
                {
                    GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out renderData);
                }
                if (renderData != null)
                {
                    if (hlodState == HLODState.High && modelType != UGCModelType.High)
                    {
                        AsyncChangeTo(UGCModelType.High);
                    } else if (hlodState == HLODState.Low && modelType != UGCModelType.Low)
                    {
                        AsyncChangeTo(UGCModelType.Low);
                    }
                }
            }
        }
    }

    private bool IsMatchModelStatus()
    {
        return hlodState == HLODState.Low && modelType == UGCModelType.Low || hlodState == HLODState.High && modelType == UGCModelType.High || hlodState == HLODState.Cull && modelType == UGCModelType.None;
    }

    private void ChangeToJson()
    {
        targetModelType = UGCModelType.Json;
        if (targetModelType == modelType)
        {
            return;
        }
        CreateUgcSubNode();
        OfflineRenderData renderData = null;
        if (!string.IsNullOrEmpty(rid))
        {
            GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out renderData);
        }
        if (renderData != null)
        {
            AsyncChangeTo(UGCModelType.Low);
        }
    }
    public override void ChangeToLow()
    {
        targetModelType = UGCModelType.Low;
        if (targetModelType == modelType)
        {
            return;
        }
        OfflineRenderData renderData = null;
        if (!string.IsNullOrEmpty(rid))
        {
            GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out renderData);
        }
        UGCModelType tmpTargetModelType;
        if (renderData == null)
        {
            tmpTargetModelType = UGCModelType.Json;
        }
        else
        {
            tmpTargetModelType = UGCModelType.Low;
            if (renderData.IsCache(UGCModelType.Low))
            {
                tmpTargetModelType = UGCModelType.Low;
            } else if (renderData.IsCache(UGCModelType.High) && modelType != UGCModelType.High)
            {
                tmpTargetModelType = UGCModelType.High;
            }
        }
        AsyncChangeTo(tmpTargetModelType);
    }

    public override void ChangeToHigh()
    {
        targetModelType = UGCModelType.High;
        if (targetModelType == modelType)
        {
            return;
        }
        OfflineRenderData renderData = null;
        if (!string.IsNullOrEmpty(rid))
        {
            GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out renderData);
        }
        UGCModelType tmpTargetModelType;
        if (renderData == null)
        {
            tmpTargetModelType = UGCModelType.Json;
        }
        else
        {
            tmpTargetModelType = UGCModelType.High;
            for (var cacheModelType = tmpTargetModelType; cacheModelType < modelType; cacheModelType++)
            {
                if (renderData.IsCache(cacheModelType) || cacheModelType == UGCModelType.Json) 
                {
                    tmpTargetModelType = cacheModelType;
                    break;
                }
            }
        }
        AsyncChangeTo(tmpTargetModelType);
    }


    private void CreateUgcSubNode()
    {
        var ugcCacheObj = UGCModelCachePool.Inst.Get(rid, UGCModelType.Json, transform);
        if (ugcCacheObj == null)
        {
            ugcCacheObj = CombineUtils.DeSerializeNodeData(transform, nodeData);
            var tmpChildBehaviours = ugcCacheObj.GetComponentsInChildren<NodeBaseBehaviour>();
            foreach (var tmpChildBehaviour in tmpChildBehaviours)
            {
                var tmpComp = tmpChildBehaviour.entity.Get<GameObjectComponent>();
                tmpComp.uid = 0;
            }
            UGCModelCachePool.Inst.SetOriginObj(rid, UGCModelType.Json, ugcCacheObj);
        }
        SetAssetObj(ugcCacheObj);
        modelType = UGCModelType.Json;
    }

    public void SetAssetObj(GameObject tmpObj)
    {
        if (assetObj != null)
        {
            Release();
        }

        if (tmpObj == null)
        {
            return;
        }
        assetObj = tmpObj;
        assetObj.transform.SetParent(transform);
        assetObj.transform.localScale = Vector3.one;
        assetObj.transform.localEulerAngles = Vector3.zero;
        assetObj.transform.localPosition = Vector3.zero;
        assetObj.SetActive(true);
        InitChildLayers();
        SetCanBuyInMap();

        allRenderers = assetObj.GetComponentsInChildren<Renderer>();
        // 状态重置
        IsOcclusion = IsOcclusion;
        UGCBehaviorManager.Inst.OnUGCChangeStatus(this);
        HLOD.Inst.OnHLODBehaviourStatusChange?.Invoke(this);
    }
    public void SetSoldOut()
    {
        if (isSoldOut == false)
        {
            Release();
            GameObject soldOutPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/DC/soldout");
            if (soldOutPrefab != null)
            {
                GameObject go = Instantiate(soldOutPrefab, transform);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                go.AddComponent<MeshCollider>();
            }
            isSoldOut = true;
            UGCBehaviorManager.Inst.OnUGCChangeStatus(this);
            HLOD.Inst.OnHLODBehaviourStatusChange?.Invoke(this);
        }

    }
}
