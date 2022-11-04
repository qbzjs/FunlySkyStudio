/// <summary>
/// Author:Mingo-LiZongMing
/// Description:子弹对象池，子弹创建，和让子弹跑Update
/// Date: 2022-5-5 17:44:22 
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBulletManager : InstMonoBehaviour<WeaponBulletManager>
{
    public bool isInit = false;
    private List<BulletDefaultBehaviour> BulletPool;
    private GameObject bulletPrefab;
    private Transform bulletParent;
    private const int DefaultBulletAmount = 10;
    private List<BulletDefaultBehaviour> CurMovingBullets = new List<BulletDefaultBehaviour>();

    public void Init()
    {
        InitBulletPool();
        isInit = true;
    }

    /// <summary>
    /// 初始化Bullet缓存池
    /// </summary>
    private void InitBulletPool()
    {
        BulletPool = new List<BulletDefaultBehaviour>();
        for (int i = 0; i < DefaultBulletAmount; i++)
        {
            var bulletBev = CreateOneBullet();
            BulletPool.Add(bulletBev);
        }
    }

    /// <summary>
    /// 从Pool中获取一个子弹
    /// </summary>
    /// <returns></returns>
    public BulletDefaultBehaviour GetBullet()
    {
        BulletDefaultBehaviour bulletBev = null;
        if (BulletPool.Count > 0)
        {
            bulletBev = BulletPool[0];
            BulletPool.RemoveAt(0);
        }
        else
        {
            bulletBev = CreateOneBullet();
        }
        return bulletBev;
    }

    /// <summary>
    /// 将用完的子弹压入Pool中
    /// </summary>
    /// <param name="bullet"></param>
    public void PushItem(BulletDefaultBehaviour bulletBev)
    {
        RemoveBulletInMovingList(bulletBev);
        bulletBev.transform.position = Vector3.zero;
        BulletPool.Add(bulletBev);
        var bulletGo = bulletBev.gameObject;
        bulletGo.SetActive(false);
    }

    /// <summary>
    ///  设置子弹射击行为
    /// </summary>
    /// <param name="shootPoint"></param>
    /// <param name="targetPos"></param>
    /// <param name="OnHitCallBack"></param>
    public void InitShootBehaviour(Vector3 shootPoint,Vector3 targetPos)
    {
        var bulletBev = GetBullet();
        bulletBev.transform.SetPositionAndRotation(shootPoint, Quaternion.identity);
        bulletBev.gameObject.SetActive(true);
        bulletBev.Initialization(shootPoint, targetPos.normalized);
        AddBulletInMovingList(bulletBev);
    }

    /// <summary>
    /// 创建一个子弹
    /// </summary>
    /// <returns></returns>
    private BulletDefaultBehaviour CreateOneBullet()
    {
        if (bulletPrefab == null)
        {
            bulletPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Special/ShootBullet");
        }
        if(bulletParent == null)
        {
            bulletParent = new GameObject("BulletPool").transform;
        }
        var bullet = GameObject.Instantiate(bulletPrefab, bulletParent);
        var bulletBev = bullet.GetComponent<BulletDefaultBehaviour>();
        bullet.SetActive(false);
        return bulletBev;
    }

    /// <summary>
    /// 将子弹压入子弹移动队列
    /// </summary>
    /// <param name="bulletBev"></param>
    private void AddBulletInMovingList(BulletDefaultBehaviour bulletBev)
    {
        if (bulletBev == null) {
            return;
        }
        if (!CurMovingBullets.Contains(bulletBev))
        {
            CurMovingBullets.Add(bulletBev);
        }
    }

    /// <summary>
    /// 将子弹从移动队列中移除
    /// </summary>
    /// <param name="bulletBev"></param>
    private void RemoveBulletInMovingList(BulletDefaultBehaviour bulletBev)
    {
        if (bulletBev == null) {
            return;
        }
        if (CurMovingBullets.Contains(bulletBev))
        {
            CurMovingBullets.Remove(bulletBev);
        }
    }

    /// <summary>
    /// 在UpData中一直刷新子弹位置,并且让子弹按照弹道移动
    /// </summary>
    private void Update()
    {
        if (CurMovingBullets != null && CurMovingBullets.Count > 0)
        {
            for (int i = 0; i < CurMovingBullets.Count; i++)
            {
                var bullet = CurMovingBullets[i];
                if (bullet != null && bullet.isActiveAndEnabled)
                {
                    bullet.OnUpdate();
                }
            }
        }
    }

    private void OnDestroy()
    {
        inst = null;
    }
}
