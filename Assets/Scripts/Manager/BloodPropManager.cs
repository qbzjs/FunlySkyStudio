using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using BudEngine.NetEngine;

/// <summary>
/// Author:WenJia
/// Description: 回血道具集中管理器
/// Date: 2022/5/18 11:36:54
/// </summary>


public class BloodPropManager : WeaponBaseManager<BloodPropManager>, IPVPManager, IUGCManager
{
    private Dictionary<NodeBaseBehaviour, BloodPropBase> allBloodPropDict = new Dictionary<NodeBaseBehaviour, BloodPropBase>();

    public const float RESTORE = 15.0f;
    private int MaxCount = 99;

    public void AddBloodPropBase(NodeBaseBehaviour behaviour, BloodPropBase bloodPropBase)
    {
        if (!allBloodPropDict.ContainsKey(behaviour))
        {
            allBloodPropDict.Add(behaviour, bloodPropBase);
        }
    }

    public void RemoveBloodPropBase(NodeBaseBehaviour behaviour)
    {
        if (allBloodPropDict.ContainsKey(behaviour))
        {
            allBloodPropDict.Remove(behaviour);
        }
    }

    public void ClearBloodPropDict()
    {
        allBloodPropDict.Clear();
    }

    public BloodPropBase GetBloodPropBase(NodeBaseBehaviour behaviour)
    {
        if (allBloodPropDict.ContainsKey(behaviour))
        {
            return allBloodPropDict[behaviour];
        }
        return null;
    }

    public override void Init()
    {
        base.Init();
        LoggerUtils.Log($"BloodPropManager : Init");
    }

    public override void AddWeaponComponent(NodeBaseBehaviour nb, string rId)
    {
        if (nb != null)
        {
            var cmp = nb.entity.Get<BloodPropComponent>();
            var gComp = nb.entity.Get<GameObjectComponent>();
            gComp.modId = (int)GameResType.BloodRestore;
            cmp.rId = rId;
            cmp.restore = RESTORE;
        }

        AddBloodPropComponent(nb, rId);
    }

    protected override void OnChangeMode(GameMode mode)
    {
        base.OnChangeMode(mode);
        LoggerUtils.Log($"BloodPropManager : OnChangeMode --> {mode}");

        switch (mode)
        {
            case GameMode.Edit:
                SetMeshColliderEnable(true);
                SetAllBloodPropVisible(true);
                ResetAllPlayersBloodEffect();
                break;
            case GameMode.Play:
            case GameMode.Guest:
                SetMeshColliderEnable(false);
                ResetAllPlayersBloodEffect();
                break;
        }
    }

    public void SetMeshColliderEnable(bool enable)
    {
        foreach (var list in allWeaponsDict.Values)
        {
            foreach (var weaponBev in list)
            {
                if (weaponBev != null)
                {
                    var bloodBase = BloodPropManager.Inst.GetBloodPropBase(weaponBev);
                    if (bloodBase != null)
                    {
                        bloodBase.SetMeshColliderEnable(enable);
                    }
                }
            }
        }
    }

    /// <summary>
    ///  非编辑状态，替换完ab之后，回血道具需要再次关闭MeshCollider
    ///  如果道具的包围盒不正确，需要更新道具的包围盒
    /// </summary>
    /// <param name="ugcCombBehaviour"> </param>
    public void UpdateBloodPropState(UGCCombBehaviour ugcCombBehaviour)
    {
        var bloodProp = BloodPropManager.Inst.GetBloodPropBase(ugcCombBehaviour);
        if (bloodProp != null)
        {
            if (GlobalFieldController.CurGameMode != GameMode.Edit)
            {
                bloodProp.SetMeshColliderEnable(false);
            }
            if (bloodProp.bloodBoxCollider != null &&
            bloodProp.bloodBoxCollider.size == Vector3.zero)
            {
                bloodProp.UpdateBoundBox(bloodProp.bloodBoxCollider);
            }
        }
    }

    public void SetAllBloodPropVisible(bool visible)
    {
        foreach (var list in allWeaponsDict.Values)
        {
            foreach (var weaponBev in list)
            {
                if (weaponBev != null)
                {
                    bool bloodIsVisible = visible;
                    if (BloodPropManager.Inst.GetBloodPropBase(weaponBev) != null)
                    {
                        BloodPropManager.Inst.GetBloodPropBase(weaponBev).propIsUsed = !visible;

                        // 被开关控制，且默认不可见，就隐藏
                        if (weaponBev.entity.HasComponent<ShowHideComponent>()
                        && weaponBev.entity.Get<ShowHideComponent>().defaultShow == 1)
                        {
                            bloodIsVisible = false;
                        }
                    }
                    weaponBev.gameObject.SetActive(bloodIsVisible);
                }
            }
        }
    }


    public override NodeBaseBehaviour CreateDefaultNode()
    {
        var newBev = SceneBuilder.Inst.CreateSceneNode<BloodPropCreater, BloodPropBehaviour>();
        BloodPropCreater.SetData((BloodPropBehaviour)newBev, new BloodPropData()
        {
            rId = DEFAULT_MODEL,
            restore = RESTORE,
        });
        return newBev;
    }

    public override void Release()
    {
        base.Release();
        ClearBloodPropDict();
        LoggerUtils.Log($"BloodPropManager : Release");
    }

    public override void RevertNode(NodeBaseBehaviour behaviour)
    {
        base.RevertNode(behaviour);
        if (behaviour.entity.HasComponent<BloodPropComponent>())
        {
            var rid = behaviour.entity.Get<BloodPropComponent>().rId;
            AddUgcWeaponItem(rid, behaviour);
        }
    }

    public override void RemoveNode(NodeBaseBehaviour behaviour)
    {
        base.RemoveNode(behaviour);
    }

    /**
    * 进房、断线重连获取回血道具信息
    */

    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========BloodPropManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[BloodPropManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                if (getItemsRsp.items == null)
                {
                    LoggerUtils.Log("[BloodPropManager.OnGetItemsCallback]getItemsRsp.items is null");
                    return;
                }

                for (int i = 0; i < getItemsRsp.items.Length; i++)
                {
                    Item item = getItemsRsp.items[i];
                    if (item.type != (int)ItemType.BLOOD_PROP)
                    {
                        continue;
                    }

                    var uid = item.id;
                    // 回血道具同步消失
                    var behaviour = GetBloodPropByUid(uid);
                    if (behaviour)
                    {
                        behaviour.gameObject.SetActive(false);

                        var bloodBase = BloodPropManager.Inst.GetBloodPropBase(behaviour);
                        if (bloodBase != null)
                        {
                            bloodBase.propIsUsed = true;
                        }
                    }

                    if (string.IsNullOrEmpty(item.data))
                    {
                        LoggerUtils.Log("[BloodPropManager.OnGetItemsCallback]getItemsRsp.item.Data is null");
                        continue;
                    }

                    BloodPropItemData itemDatas = JsonConvert.DeserializeObject<BloodPropItemData>(item.data);

                    if (itemDatas != null && itemDatas.affectPlayers != null)
                    {
                        foreach (var itemData in itemDatas.affectPlayers)
                        {
                            LoggerUtils.Log("[BloodPropManager.OnGetItemsCallback]getItemsRsp.itemData : " + itemData);
                            // 更新玩家血量和回血道具使用
                            PVPManager.Inst.UpdatePlayerHpShow(itemData.PlayerId, itemData.CurBlood);
                        }
                    }
                }
            }
        }
    }

    /**
    * 发送回血请求给服务端
    */
    public void SendBloodRestoreReq(int bloodpropUid, object bloodPropData, Action<int, string> callBack)
    {
        Item[] itemsArray =
        {
            new Item()
            {
                id = bloodpropUid,
                type = (int) ItemType.BLOOD_PROP,
                data = JsonConvert.SerializeObject(bloodPropData),
            }
        };

        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };


        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log($"SendBloodRestoreReq => {JsonConvert.SerializeObject(roomChatData)}");
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), callBack);
    }

    /**
    * 接收服务端回血消息
    */
    public bool OnReceiveServer(string sendPlayerId, string msg)
    {
        LoggerUtils.Log($"BloodPropManager OnReceiveServer ==> => senderPlayer:{sendPlayerId}, msg:{msg}");
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.BLOOD_PROP)
                {
                    if (string.IsNullOrEmpty(item.data))
                    {
                        LoggerUtils.Log("[BloodPropManager.OnGetItemsCallback]getItemsRsp.item.Data is null");
                        continue;
                    }

                    var uid = item.id;
                    // 回血道具同步消失
                    var behaviour = GetBloodPropByUid(uid);
                    if (behaviour)
                    {
                        behaviour.gameObject.SetActive(false);
                        var bloodBase = BloodPropManager.Inst.GetBloodPropBase(behaviour);
                        if (bloodBase != null)
                        {
                            bloodBase.propIsUsed = true;
                        }
                    }

                    BloodPropItemData itemDatas = JsonConvert.DeserializeObject<BloodPropItemData>(item.data);

                    if (itemDatas != null && itemDatas.affectPlayers != null)
                    {
                        foreach (var itemData in itemDatas.affectPlayers)
                        {
                            LoggerUtils.Log("[BloodPropManager.OnGetItemsCallback]getItemsRsp.itemData : " + itemData);
                            // 加血操作
                            PVPManager.Inst.UpdatePlayerHpShow(itemData.PlayerId, itemData.CurBlood);
                            SetBloodEffectVisible(itemData.PlayerId, true);
                        }
                    }
                }
            }
        }
        return true;
    }

    // 播放玩家加血特效
    public void SetBloodEffectVisible(string playerId, bool visible)
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return;
        }

        GameObject bloodEffect = null;
        Transform bloodEffectTrans = null;
        if (playerId == Player.Id)
        {
            bloodEffectTrans = PlayerBaseControl.Inst.playerAnim.transform.Find("BloodRestore(Clone)");
            if (bloodEffectTrans == null)
            {
                var bloodEffectObj = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Effect/BloodRestore");
                bloodEffect = UnityEngine.Object.Instantiate(bloodEffectObj, PlayerBaseControl.Inst.playerAnim.transform);
            }
            else
            {
                bloodEffect = bloodEffectTrans.gameObject;
            }
        }
        else if (ClientManager.Inst.GetOtherPlayerComById(playerId))
        {
            var otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer)
            {
                bloodEffectTrans = otherPlayer.transform.Find("BloodRestore(Clone)");
            }
            if (bloodEffectTrans == null)
            {
                var bloodEffectObj = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Effect/BloodRestore");
                bloodEffect = UnityEngine.Object.Instantiate(bloodEffectObj, otherPlayer.transform);
            }
            else
            {
                bloodEffect = bloodEffectTrans.gameObject;
            }

        }

        if (bloodEffect)
        {
            //播放人物加血特效
            var particleEffect = bloodEffect.GetComponentInChildren<ParticleSystem>(true);
            if (particleEffect)
            {
                bloodEffect.gameObject.SetActive(visible);
                if (visible)
                {
                    particleEffect.Play();
                    AKSoundManager.Inst.PostEvent("Play_Healing_Props", bloodEffect.transform.parent.gameObject);
                }
                else
                {
                    particleEffect.Stop();
                }
            }
            // 加血特效播放完毕就隐藏
            CoroutineManager.Inst.StartCoroutine(HideEffectNode(bloodEffect));
        }
    }

    public IEnumerator HideEffectNode(GameObject effectNode)
    {
        yield return new WaitForSeconds(1.8f);

        if (effectNode != null)
        {
            effectNode.SetActive(false);
        }
    }

    public bool IsOverMaxCount()
    {
        int curNum = 0;
        foreach (var weaponList in allWeaponsDict.Values)
        {
            curNum += weaponList.Count;
        }

        if (curNum >= MaxCount)
        {
            return true;
        }
        return false;
    }

    /**
    * 重置地图数据
    */

    public void OnReset()
    {
        SetAllBloodPropVisible(true);
        ResetAllPlayersBloodEffect();
        SetDefaultModeShow(false);
    }

    public void ResetAllPlayersBloodEffect()
    {
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.playerAnim)
        {
            var selfBloodEffect = PlayerBaseControl.Inst.playerAnim.transform.Find("BloodRestore(Clone)");
            if (selfBloodEffect != null)
            {
                selfBloodEffect.gameObject.SetActive(false);
            }
        }

        var otherDict = ClientManager.Inst.otherPlayerDataDic;
        foreach (var otherPlayer in otherDict.Values)
        {
            var otherBloodEffect = otherPlayer.transform.Find("BloodRestore(Clone)");
            if (otherBloodEffect != null)
            {
                otherBloodEffect.gameObject.SetActive(false);
            }
        }
    }

    public void AddBloodPropComponent(NodeBaseBehaviour nb, string rId)
    {
        if (nb != null)
        {
            var cmp = nb.entity.Get<BloodPropComponent>();
            cmp.rId = rId;
            cmp.restore = RESTORE;
        }
    }

    public NodeBaseBehaviour GetBloodPropByUid(int uid)
    {
        foreach (var list in allWeaponsDict.Values)
        {
            foreach (var weaponBev in list)
            {
                if (weaponBev != null)
                {
                    var gComp = weaponBev.entity.Get<GameObjectComponent>();
                    if (uid == gComp.uid)
                    {
                        return weaponBev;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 当前回血道具是否被使用
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool IsBloodPropUsed(NodeBaseBehaviour behaviour)
    {
        var bloodProp = BloodPropManager.Inst.GetBloodPropBase(behaviour);
        if (bloodProp != null && bloodProp.propIsUsed)
        {
            return true;
        }

        // 未添加 UGC 素材的道具的显隐不应该被控制
        var defalutModels = GetAllDefaultNodeBeav();
        if (defalutModels != null && defalutModels.Count > 0)
        {
            if (defalutModels.Contains(behaviour))
            {
                return true;
            }
        }

        return false;
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        UpdateBloodPropState(ugcCombBehaviour);
    }
}
