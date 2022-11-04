using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BudEngine.NetEngine;

/// <summary>
/// Author:WenJia
/// Description:UGC 回血道具行为,包括Trigger 和包围盒计算更新
/// Date: 2022/5/19 16:10:58
/// </summary>


public class BloodPropBase : IBloodProp
{
    public string Rid { get; set; }
    public NodeBaseBehaviour bloodPropBehaviour { get; set; }
    public float Restore { get; set; } //回血量

    public GameObject bloodEffectPrefab, bloodDisappearPrefab, bloodEffectNodePrefab;
    public BloodPropCheck bloodCheck;
    public BoxCollider bloodBoxCollider;
    private GameObject effectNode, effectParticleNode;

    public bool propIsUsed = false;

    public BloodPropBase(NodeBaseBehaviour behv)
    {
        bloodPropBehaviour = behv;
        Rid = behv.entity.Get<BloodPropComponent>().rId;
        Restore = behv.entity.Get<BloodPropComponent>().restore;
        if (bloodCheck)
        {
            bloodCheck.OnTrigger = BloodPropOnTrigger;
        }
    }

    public void OnCreate(NodeBaseBehaviour behv)
    {

    }

    public void OnDisappear()
    {
        propIsUsed = true;
        if (bloodDisappearPrefab != null)
        {
            bloodDisappearPrefab.SetActive(true);
        }
        CoroutineManager.Inst.StartCoroutine(HideBloodProp());
    }

    IEnumerator HideBloodProp()
    {
        yield return new WaitForSeconds(0.03f);
        bloodPropBehaviour.gameObject.SetActive(false);
    }

    public void AddBloodEffect(NodeBaseBehaviour behaviour)
    {
        if (!effectNode)
        {
            effectNode = new GameObject("EffectNode");
        }
        effectNode.transform.localPosition = Vector3.zero;
        effectNode.transform.SetParent(behaviour.transform);

        bloodEffectPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Effect/BloodEffect");
        UnityEngine.Object.Instantiate(bloodEffectPrefab, effectNode.transform);

        if (!effectParticleNode)
        {
            effectParticleNode = new GameObject("EffectParticleNode");
        }
        effectParticleNode.transform.localPosition = Vector3.zero;
        effectParticleNode.transform.SetParent(behaviour.transform);

        bloodEffectNodePrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Effect/BloodEffectNode");
        UnityEngine.GameObject.Instantiate(bloodEffectNodePrefab, effectParticleNode.transform);

        bloodDisappearPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Effect/BloodPropDisappear");
        UnityEngine.GameObject.Instantiate(bloodDisappearPrefab, effectNode.transform);
        bloodDisappearPrefab.SetActive(false);
    }

    public void SetBloodEffectSize(Vector3 scale)
    {
        if (!effectNode) return;
        if (scale.x < 1)
        {
            scale.x = 1;
        }
        if (scale.y < 1)
        {
            scale.y = 1;
        }
        if (scale.z < 1)
        {
            scale.z = 1;
        }

        effectNode.transform.localScale = scale * 1.3f;

        var max = Math.Max(scale.x, scale.y);
        max = Math.Max(max, scale.z);
        if (!effectParticleNode) return;
        effectParticleNode.transform.localScale = new Vector3(max, max, max);
    }

    public void UpdateBloodPropBehaviour(NodeBaseBehaviour nBehav, bool isClone = false)
    {
        GameObject bCheck = null;
        var bCheckTrans = nBehav.transform.Find("bloodCheck");
        if (bCheckTrans == null)
        {
            bCheck = new GameObject("bloodCheck");
            bCheck.transform.localPosition = Vector3.one;
            bCheck.transform.SetParent(nBehav.transform);
            bCheck.transform.localPosition = Vector3.zero;
        }
        else
        {
            bCheck = bCheckTrans.gameObject;
        }

        bloodCheck = bCheck.GetComponent<BloodPropCheck>();
        if (bloodCheck == null)
        {
            bloodCheck = bCheck.AddComponent<BloodPropCheck>();
        }
        bloodCheck.OnTrigger = BloodPropOnTrigger;

        bloodBoxCollider = bCheck.GetComponent<BoxCollider>();
        if (bloodBoxCollider == null)
        {
            bloodBoxCollider = bCheck.AddComponent<BoxCollider>();
            bloodBoxCollider.isTrigger = true;
        }

        var bloodRigidbody = bCheck.GetComponent<Rigidbody>();
        if (bloodRigidbody == null)
        {
            bloodRigidbody = bCheck.AddComponent<Rigidbody>();
            bloodRigidbody.isKinematic = false;
            bloodRigidbody.useGravity = false;
        }

        bloodCheck.gameObject.SetActive(true);
        bloodCheck.enabled = true;
        bloodCheck.gameObject.layer = LayerMask.NameToLayer("TriggerModel");
        if (!isClone)
        {
            UpdateBoundBox(bloodBoxCollider);
        }
    }


    public void UpdateBoundBox(BoxCollider bCollider)
    {
        var cNode = bloodPropBehaviour.transform;
        Vector3 postion = cNode.position;
        Quaternion rotation = cNode.rotation;
        Vector3 scale = cNode.localScale;
        cNode.position = Vector3.zero;
        cNode.rotation = Quaternion.Euler(Vector3.zero);
        Vector3 center = Vector3.zero;
        Renderer[] renders = cNode.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer child in renders)
        {
            // 道具特效不算包围盒
            if (child.gameObject.GetComponent<ParticleSystem>())
            {
                continue;
            }
            center += child.bounds.center;
        }
        if (renders.Length != 0)
        {
            center /= renders.Length;
        }
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer child in renders)
        {
            // 道具特效不算包围盒
            if (child.gameObject.GetComponent<ParticleSystem>())
            {
                continue;
            }
            bounds.Encapsulate(child.bounds);
        }

        bCollider.center = bounds.center - cNode.position;
        bCollider.size = bounds.size;
        cNode.position = postion;
        cNode.rotation = rotation;
        cNode.localScale = scale;

        if (bCollider.size != Vector3.zero)
        {
            AddBloodEffect(bloodPropBehaviour);
            var EffectSize = new Vector3(bCollider.size.x * bCollider.transform.localScale.x, bCollider.size.y * bCollider.transform.localScale.y, bCollider.size.z * bCollider.transform.localScale.z);
            SetBloodEffectSize(EffectSize);
            effectNode.transform.localPosition = bCollider.center;
            effectParticleNode.transform.localPosition = bCollider.center;
        }
    }

    public void SetMeshColliderEnable(bool enabled)
    {
        var bloodMeshColliders = bloodPropBehaviour.transform.GetComponentsInChildren<MeshCollider>(true);
        for (var i = 0; i < bloodMeshColliders.Length; i++)
        {
            var bloodMeshCollider = bloodMeshColliders[i];
            if (bloodMeshCollider)
            {
                bloodMeshCollider.enabled = enabled;
            }
        }
    }

    public void BloodPropOnTrigger(GameObject other)
    {
        if (!StateManager.IsBloodTrigger)
        {
            return;
        }

        //玩家已死亡不处理
        if (PlayerManager.Inst.GetPlayerDeathState(Player.Id))
        {
            return;
        }

        LoggerUtils.Log("-------- BloodProp  OnTrigger --------");

        // 试玩模式的表现（只展示特效）
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            // 加血特效
            OnDisappear();
            // 试玩加血操作
            PVPManager.Inst.AddPlayerHpShow(Player.Id, bloodPropBehaviour.entity.Get<BloodPropComponent>().restore);
            BloodPropManager.Inst.SetBloodEffectVisible(Player.Id, true);
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            //发送使用回血道具请求
            BloodPropAffectPlayerData affectData = new BloodPropAffectPlayerData();
            affectData.PlayerId = Player.Id;
            if (bloodPropBehaviour.entity.HasComponent<BloodPropComponent>())
            {
                affectData.restore = bloodPropBehaviour.entity.Get<BloodPropComponent>().restore;
            }
            BloodPropItemData bloodPropItemData = new BloodPropItemData();
            bloodPropItemData.affectPlayers = new[]
            {
                affectData,
            };
            var uid = bloodPropBehaviour.entity.Get<GameObjectComponent>().uid;
            BloodPropManager.Inst.SendBloodRestoreReq(uid, bloodPropItemData, (errorCode, msg) =>
            {
                LoggerUtils.Log("BloodProp SendBloodRestoreReq CallBack errorCode ==>" + errorCode);
                LoggerUtils.Log("BloodProp SendBloodRestoreReq CallBack msg ==>" + msg);
                if (errorCode == 0)
                {
                    OnDisappear();
                }
            });
        }
    }
}
