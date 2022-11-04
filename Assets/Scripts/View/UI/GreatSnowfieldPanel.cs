using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GreatSnowfieldPanel : MonoBehaviour
{
    [SerializeField] private Button crystalCollectCompleteBtn;
    [SerializeField] private Transform guestModeIceCrystal;
    [SerializeField] private Transform playModeIceCrystal;
    [SerializeField] private GameObject iceCrystalGridPrefab;
    [SerializeField] private ParticleSystemListener effetListner;

    private List<IceCrystalGrid> iceCrystalList = new List<IceCrystalGrid>();
    private int iceCrystalCount = 0;
    private Action collectCompleteCallback;

    private void Awake()
    {
        crystalCollectCompleteBtn.onClick.AddListener(OnCrystalCollectCompleteBtnClick);
        effetListner.CompleteAction = RewardEffectComplete;
    }

    #region public
    /// <summary>
    /// 打开冰晶界面
    /// </summary>
    public void ShowIceCrystal(int maxCount, int collectedCount)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            InitPlayModeIceCrystalGrid(maxCount, collectedCount);
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            InitGuestModeIceCrystalGird(maxCount, collectedCount);
        }

        crystalCollectCompleteBtn.gameObject.SetActive(false);
    }

    /// <summary>
    /// 收集一个冰晶
    /// </summary>
    public void CollectIceCrystal(Action callback)
    {
        if (iceCrystalCount >= iceCrystalList.Count)
        {
            return;
        }

        AKSoundManager.Inst.PostEvent("Play_GreatSnowfield_Ice_Gem_UI_Storage", PlayerBaseControl.Inst.gameObject);

        iceCrystalList[iceCrystalCount].ShowIceCrystal(true, true, callback);
        iceCrystalCount++;
    }

    /// <summary>
    /// 收集完成，显示已收集完成的按钮
    /// </summary>
    public void ShowCollectedComplete()
    {
        AKSoundManager.Inst.PostEvent("Play_GreatSnowfield_Ice_Gem_UI_Fusion", PlayerBaseControl.Inst.gameObject);

        for (int i = 0; i < iceCrystalList.Count; i++)
        {
            iceCrystalList[i].PlayCollectedCompleteAni(crystalCollectCompleteBtn.transform.position, 0.8f, 0.2f, CollectedCompleteAniEnd);
        }
    }

    /// <summary>
    /// 显示礼物按钮
    /// </summary>
    /// <param name="spriteName"></param>
    public void ShowCrystalCollectCompleteBtn()
    {
        crystalCollectCompleteBtn.gameObject.SetActive(true);
    }

    public void SetCollectCompleteCallback(Action callback)
    {
        collectCompleteCallback = callback;
    }
    #endregion

    #region private
    /// <summary>
    /// 初始化游玩模式冰晶收集数量
    /// </summary>
    /// <param name="count">数量</param>
    private void InitGuestModeIceCrystalGird(int count, int collectedCount)
    {
        iceCrystalCount = collectedCount;

        InitIceCrystal(guestModeIceCrystal, count, collectedCount);
    }

    /// <summary>
    /// 初始化试玩模式冰晶收集数量
    /// </summary>
    private void InitPlayModeIceCrystalGrid(int maxCount, int collectedCount)
    {
        iceCrystalCount = 0;

        if (iceCrystalList.Count != 0)
        {
            for (int i = 0; i < iceCrystalList.Count; i++)
            {
                iceCrystalList[i].ShowIceCrystal(false, false);
            }
        }
        else
        {
            InitIceCrystal(playModeIceCrystal, maxCount, collectedCount);
        }
    }

    /// <summary>
    /// 初始化冰晶状态
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="maxCount"></param>
    /// <param name="collectedCount"></param>
    private void InitIceCrystal(Transform parent, int maxCount,int collectedCount)
    {
        if (iceCrystalList.Count == 0)
        {
            CreateIceCrystal(parent, maxCount);
        }
        else
        {
            int curCount = iceCrystalList.Count;
            if (curCount > maxCount)
            {
                for (int i = 1; i <= curCount - maxCount; i++)
                {
                    IceCrystalGrid iceCrystal = iceCrystalList[curCount - i];
                    iceCrystalList.Remove(iceCrystal);
                    Destroy(iceCrystal.gameObject);
                }
            }
            else
            {
                CreateIceCrystal(parent, maxCount - curCount);
            }
        }

        for (int i = 0; i < iceCrystalList.Count; i++)
        {
            if (i < collectedCount)
            {
                iceCrystalList[i].ShowIceCrystal(true, false);
            }
            else
            {
                iceCrystalList[i].ShowIceCrystal(false, false);
            }
        }
    }

    /// <summary>
    /// 生成冰晶
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="count"></param>
    private void CreateIceCrystal(Transform parent, int count)
    {
        for (int i = 0; i < count; i++)
        {
            IceCrystalGrid iceCrystal = Instantiate(iceCrystalGridPrefab, parent).GetComponent<IceCrystalGrid>();
            iceCrystalList.Add(iceCrystal);
        }
    }

    /// <summary>
    /// 收集完成动画播放结束
    /// </summary>
    private void CollectedCompleteAniEnd()
    {
        if (crystalCollectCompleteBtn.gameObject.activeSelf)
        {
            return;
        }
        guestModeIceCrystal.gameObject.SetActive(false);
        crystalCollectCompleteBtn.gameObject.SetActive(true);
    }

    private void RewardEffectComplete()
    {
        effetListner.transform.parent.gameObject.SetActive(false);
        OnCrystalCollectCompleteBtnClick();
    }

    private void OnCrystalCollectCompleteBtnClick()
    {
        UIControlManager.Inst.CallUIControl("snowfield_reward_enter");
        CrystalStoneRewardPanel.Instance.OpenRewardItem();
    }
    #endregion
}
