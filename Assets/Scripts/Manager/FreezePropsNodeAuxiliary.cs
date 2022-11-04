using System;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Entitas;
using UnityEngine;
//Node的辅助类：处理计算UGC的最大包围盒等
public class FreezePropsNodeAuxiliary
{
    public NodeBaseBehaviour mNode;
    public BoxCollider mCollider;
    public NodeTriggerChecker mTriggerChecker;
    public FreezePropsManager mManager;
    public bool propIsUsed = false;
    private GameObject effectNode, effectParticleNode;
    public GameObject freezeEffectPrefab, bloodDisappearPrefab, bloodEffectNodePrefab;
    public FreezePropsNodeAuxiliary(NodeBaseBehaviour node,FreezePropsManager manager)
    {
        mNode = node;
        mManager = manager;
    }
    public void UpdatePropBehaviour(NodeBaseBehaviour nBehav, bool isClone = false)
    {
        GameObject bCheck = null;
        var bCheckTrans = mNode.transform.Find("Checker");
        if (bCheckTrans == null)
        {
            bCheck = new GameObject("Checker");
            bCheck.transform.SetParent(mNode.transform);
            bCheck.transform.localPosition = Vector3.zero;
        }
        else
        {
            bCheck = bCheckTrans.gameObject;
        }

        mTriggerChecker = bCheck.GetComponent<NodeTriggerChecker>();
        if (mTriggerChecker == null)
        {
            mTriggerChecker = bCheck.AddComponent<NodeTriggerChecker>();
        }
        mTriggerChecker.OnTrigger = OnTrigger;

        mCollider = bCheck.GetComponent<BoxCollider>();
        if (mCollider == null)
        {
            mCollider = bCheck.AddComponent<BoxCollider>();
            mCollider.isTrigger = true;
        }

        var bloodRigidbody = bCheck.GetComponent<Rigidbody>();
        if (bloodRigidbody == null)
        {
            bloodRigidbody = bCheck.AddComponent<Rigidbody>();
            bloodRigidbody.isKinematic = false;
            bloodRigidbody.useGravity = false;
        }

        mTriggerChecker.gameObject.SetActive(true);
        mTriggerChecker.enabled = true;
        mTriggerChecker.gameObject.layer = LayerMask.NameToLayer("TriggerModel");

        if (!isClone)
        {
            UpdateBoundBox();
        }
    }
    //计算包围盒
    public Bounds CalcBoundingVolume(List<Renderer> renderers)
    {
        int count = renderers.Count;
        Vector3 center = Vector3.zero;
        Bounds bounds = new Bounds();
        for (int i = 0; i < count; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer.gameObject.GetComponent<ParticleSystem>())
            {
                continue;
            }
            center += renderer.bounds.center;
            bounds.Encapsulate(renderer.bounds);
        }
        if (count != 0)
        {
            center /= count;
        }
        bounds.center = center;
        return bounds;
    }
    public void OnTrigger(GameObject obj)
    {
        //玩家已死亡不处理
        if (PlayerManager.Inst.GetPlayerDeathState(Player.Id))
        {
            return;
        }
        // 试玩模式的表现（只展示特效）
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            //如果冻结中，不再触发
            if (mManager.mPlayFreezeManager.CheckerPlayerIsFreeze(Player.Id))
            {
                return;
            }
            float freezeTime = 0;
            if (mNode.entity.HasComponent<FreezePropsComponent>())
            {
                freezeTime = mNode.entity.Get<FreezePropsComponent>().mFreezeTime;
            }
            mManager.FreezePlyaerWithPlay(Player.Id, freezeTime);
            mNode.gameObject.SetActive(false);
            propIsUsed = true;
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
           
            mManager.mSession.ReqTriggerFreezeProps(mNode.entity);
        }
    }
    public void UpdateBoundBox()
    {
        var cNode = mNode.transform;
        Vector3 postion = cNode.position;
        Quaternion rotation = cNode.rotation;
        Vector3 scale = cNode.localScale;
        cNode.position = Vector3.zero;
        cNode.rotation = Quaternion.Euler(Vector3.zero);
        Renderer[] renders = cNode.GetComponentsInChildren<Renderer>(true);
        List<Renderer> renderers = new List<Renderer>(renders);
        Bounds bounds = CalcBoundingVolume(renderers);
        mCollider.center = bounds.center - cNode.position;
        mCollider.size = bounds.size;
        cNode.position = postion;
        cNode.rotation = rotation;
        cNode.localScale = scale;
        mTriggerChecker.enabled = true;
        mTriggerChecker.gameObject.SetActive(true);
        mTriggerChecker.gameObject.layer = LayerMask.NameToLayer("TriggerModel");

        if (mCollider.size != Vector3.zero)
        {
            AddUGCFreezeEffect(mNode);
            var EffectSize = new Vector3(mCollider.size.x * mCollider.transform.localScale.x, mCollider.size.y * mCollider.transform.localScale.y, mCollider.size.z * mCollider.transform.localScale.z);
            SetBloodEffectSize(EffectSize);
            effectNode.transform.localPosition = mCollider.center;
        }
    }
    public void SetMeshColliderEnable(bool enabled)
    {
        var meshColliders = mNode.transform.GetComponentsInChildren<MeshCollider>(true);
        for (var i = 0; i < meshColliders.Length; i++)
        {
            var meshCollider = meshColliders[i];
            if (meshCollider)
            {
                meshCollider.enabled = enabled;
            }
        }
    }
    public void AddUGCFreezeEffect(NodeBaseBehaviour behaviour)
    {
        if (!effectNode)
        {
            effectNode = new GameObject("EffectNode");
        }
        effectNode.transform.localPosition = Vector3.zero;
        effectNode.transform.SetParent(behaviour.transform);

        freezeEffectPrefab = ResManager.Inst.LoadRes<GameObject>("Effect/freeze/freeze_duration");
        UnityEngine.Object.Instantiate(freezeEffectPrefab, effectNode.transform);
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
    }
}
