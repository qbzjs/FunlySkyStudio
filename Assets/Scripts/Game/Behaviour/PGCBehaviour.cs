using System;
using Assets.Scripts.Game.Core;
using BudEngine.NetEngine;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using HLODSystem.Extensions;
using UnityEngine;
/// <summary>
/// Author:JayWill
/// Description:实现HighLight 方法 PGC物体高亮
/// </summary>
public class PGCBehaviour : BaseHLODBehaviour
{
    protected Color[] colors;
    private bool isDCRequesting;
    public bool isSoldOut;
    protected Bounds? bounds;

    public new GameObject assetObj
    {
        get => _assetObj;
        set
        {
            _assetObj = value;
            onSetAsset?.Invoke(this);
        }
    }
    private GameObject _assetObj;
    private Action<NodeBaseBehaviour> onSetAsset;
    
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        allRenderers = GetComponentsInChildren<Renderer>();
        IsOcclusion = isOcclusion;
        onSetAsset = null;
    }
    
    public void AddOnSetAssetAction(Action<NodeBaseBehaviour> ac)
    {
        onSetAsset += ac;
        if(_assetObj != null)ac?.Invoke(this);
    }
    
    public override Bounds? GetBounds()
    {
        if (gameObject == null || transform == null)
        {
            return null;
        }
        if (bounds == null)
        {
            bounds = transform.GetBounds();
        }
        return bounds;
    }


    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnEdit(isHigh, gameObject,ref colors,3.5f);
    }
    
    public override void OnRayEnter()
    {
        //双人牵手不让进行操作
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            return;
        }
        
        var uComp = entity.Get<UGCPropComponent>();
        if (uComp.isTradable == 1)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Dc);
            PortalPlayPanel.Instance.AddButtonClick(OnClickDCShop);
            PortalPlayPanel.Instance.SetTransform(transform);
        }
        base.OnRayEnter();
    }
    
    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }
    
    private void OnClickDCShop()
    {
        if (isDCRequesting)
        {
            return;
        }
        var dcComp = entity.Get<DcComponent>();
        var pgcComp = entity.Get<PGCSceneComponent>();
        DCResInfo info = new DCResInfo()
        {
            itemId = dcComp.dcId,
            budActId = dcComp.budActId,
            classifyType = pgcComp.classifyID,
            pgcId = pgcComp.pgcID
        };
        var ugcJson = JsonConvert.SerializeObject(info);
        if (!string.IsNullOrEmpty(ugcJson))
        {
            HttpUtils.MakeHttpRequest("/ugcmap/itemInfo", (int)HTTP_METHOD.GET, ugcJson, OnDCPlayerInfoSuccess, OnGetPlayerInfoFail);
            isDCRequesting = true;
        }
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
        if (entity.HasComponent<PGCSceneComponent>())
        {
            var c = entity.Get<PGCSceneComponent>();
            info.classifyType = c.classifyID;
            info.pgcId = c.pgcID;
        }
        var ugcJson = JsonConvert.SerializeObject(info);
        DCResPanel.Instance.SetOwnedPanel(string.IsNullOrEmpty(dcComp.budActId));
        DCResPanel.Instance.OnClickDC(ugcJson, transform);
        DCResPanel.Instance.InfoSuccess(socialResInfo);
        DCResPanel.Instance.HideOrShowPanel(false);

        DataLogUtils.DetailPageView(socialResInfo.mapInfo.mapId, socialResInfo.mapInfo.dcInfo, "prop");//上报素材详情页预览

    }
    public void OnSoldOut(string id, string address)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            DcSaveInfo info = new DcSaveInfo()
            {
                dcId = id,
                address = address
            };
            CustomData data = new CustomData();
            data.type = (int)ChatCustomType.PGCResSoldOut;
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
        if (!entity.HasComponent<UGCPropComponent>())
        {
            return;
        }
        var uComp = entity.Get<UGCPropComponent>();
        var childColliders = transform.GetComponentsInChildren<Collider>();
        foreach (var collider in childColliders)
        {
            collider.gameObject.layer = uComp.isTradable == 1 ? LayerMask.NameToLayer("Touch") : LayerMask.NameToLayer("Model");
        }
    }
    
    public void SetSoldOut()
    {
        if (isSoldOut == false)
        {
            var rlRoot = transform.Find("RLRoot");
            rlRoot.gameObject.SetActive(false);
            GameObject soldOutPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/DC/soldout");
            if (soldOutPrefab != null)
            {
                GameObject go = Instantiate(soldOutPrefab, transform);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                go.AddComponent<MeshCollider>();
            }
            isSoldOut = true;
        }
    }
    
    
    public override void SetLODStatus(HLODState state)
    {
        if (state == hlodState)
        {
            return;
        }

        hlodState = state;

        if (assetObj != null)
        {
            SwitchLOD();
        }
        else
        {
            PGCNodeCreater.LoadAsset(this, nodeData.id, (rl) =>
            {
                if (rl != null)
                {
                    var highTrans = rl.Find("High");
                    if (highTrans != null)
                    {
                        highObj = highTrans.gameObject;
                    }

                    var lowTrans = rl.Find("Low");
                    if (lowTrans != null)
                    {
                        lowObj = lowTrans.gameObject;
                    }
                    allRenderers = rl.GetComponentsInChildren<Renderer>();
                	IsOcclusion = isOcclusion;
                }

                SwitchLOD();
            });
        }
    }

    private void SwitchLOD()
    {
        if (assetObj == null) return;
        if (hlodState == HLODState.Cull)
        {
            ModelCachePool.Inst.Release(nodeData.id, assetObj);
            allRenderers = null;
            assetObj = null;
        }

        if (highObj == null || lowObj == null) return;
        highObj.SetActive(hlodState == HLODState.High);
        lowObj.SetActive(hlodState == HLODState.Low);
    }
}
