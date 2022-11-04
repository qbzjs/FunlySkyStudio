using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:攻击道具武器
/// Date: 2022-4-14 17:44:22
/// </summary>
public class MeleeAttackWeapon : WeaponBase
{
    public WeaponAttackCheck weaponCheck;
    private BoxCollider weaponBoxCollider;
    private Rigidbody weaponRigidbody;
    private MeshCollider[] weaponColliders;
    public int weaponUid;
    public Action<Collider> OnTrigger;
    public ParticleSystem effectDestroy;
    public float Durability { get; set; } //武器耐力值
    public int OpenDurability { get; set; } // 武器是否开启耐力值
    public float CurDurability { get; set; }// 武器当前耐力值
    public MeleeAttackWeapon(NodeBaseBehaviour behv):base(behv)
    {
        Rid = behv.entity.Get<AttackWeaponComponent>().rId;
        Damage = behv.entity.Get<AttackWeaponComponent>().damage;
        Durability = behv.entity.Get<AttackWeaponComponent>().hits;
        OpenDurability = behv.entity.Get<AttackWeaponComponent>().openDurability;
        CurDurability = behv.entity.Get<AttackWeaponComponent>().curHits;
    }

    public override void OnCreate()
    {
        if (weaponCheck == null)
        {
            GameObject wCheck = new GameObject("attackCheck");
            wCheck.transform.SetParent(weaponBehaviour.transform);
            wCheck.transform.localPosition = Vector3.zero;
            wCheck.transform.localScale = Vector3.one;
            weaponCheck = wCheck.AddComponent<WeaponAttackCheck>();
            weaponCheck.OnTrigger = OnAttackTrigger;
            weaponColliders = wCheck.GetComponentsInChildren<MeshCollider>(true);
            weaponBoxCollider = wCheck.AddComponent<BoxCollider>();
            weaponRigidbody = wCheck.AddComponent<Rigidbody>();
            weaponRigidbody.isKinematic = false;
            weaponRigidbody.useGravity = false;
        }
        weaponCheck.gameObject.SetActive(true);
        weaponCheck.gameObject.layer = LayerMask.NameToLayer("Weapon");
        weaponCheck.enabled = true;
        OnBindWeapon();
    }
    private void OnBindWeapon()
    {
        SetMeshColliderVisible(false);
        PropSizeUtill.UpdateBoundBox(weaponBoxCollider);
        weaponBoxCollider.enabled = false;
        weaponBoxCollider.isTrigger = true;
    }
    public void UnBindWeapon()
    {
        if (weaponCheck != null)
        {
            weaponCheck.gameObject.layer = LayerMask.NameToLayer("Default");
            weaponCheck.gameObject.SetActive(false);
            SetMeshColliderVisible(true);
        }
    }

    private void SetMeshColliderVisible(bool visible)
    {
        if (weaponColliders != null)
        {
            for (var i = 0; i < weaponColliders.Length; i++)
            {
                weaponColliders[i].enabled = visible;
            }
        }
    }

    public override void OnAttack()
    {
        weaponBoxCollider.enabled = true;
    }

    public override void OnEndAttack()
    {
        weaponBoxCollider.enabled = false;
    }

    private void OnAttackTrigger(Collider other)
    {
        OnTrigger?.Invoke(other);
    }

    public IEnumerator PlayWeaponDestroyEffect()
    {
        yield return new WaitForSeconds(0.15f);
        if (effectDestroy == null)
        {
            GameObject smokeEffect = ResManager.Inst.LoadRes<GameObject>("Effect/destory_smoke/destory_smoke");
            var effect = GameObject.Instantiate(smokeEffect, weaponBehaviour.transform.parent);
            effect.transform.localPosition = weaponBehaviour.transform.localPosition;
            effectDestroy = effect.GetComponentInChildren<ParticleSystem>(true);
            var EffectSize = new Vector3(weaponBoxCollider.size.x * weaponBoxCollider.transform.localScale.x,
            weaponBoxCollider.size.y * weaponBoxCollider.transform.localScale.y,
            weaponBoxCollider.size.z * weaponBoxCollider.transform.localScale.z);
            SetDestroyEffectSize(EffectSize);
        }
        effectDestroy.gameObject.SetActive(true);
        effectDestroy.Play();
        HideDestroyEffect();

    }

    public void SetDestroyEffectSize(Vector3 scale)
    {
        if (!effectDestroy) return;
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

        var max = Math.Max(scale.x, scale.y);
        max = Math.Max(max, scale.z);
        if (!effectDestroy) return;
        effectDestroy.transform.parent.localScale = new Vector3(max, max, max);
    }


    private void HideDestroyEffect()
    {
        ParticleSystemListener listenerComp = effectDestroy.gameObject.GetComponent<ParticleSystemListener>();
        if (listenerComp == null)
        {
            listenerComp = effectDestroy.gameObject.AddComponent<ParticleSystemListener>();
        }
        ParticleSystem.MainModule mainModule = effectDestroy.main;
        mainModule.loop = false;
        mainModule.stopAction = ParticleSystemStopAction.Callback;
        listenerComp.CompleteAction = () =>
        {
            if (effectDestroy != null)
            {
                GameObject.Destroy(effectDestroy.transform.parent.gameObject);
            }
        };
    }
}