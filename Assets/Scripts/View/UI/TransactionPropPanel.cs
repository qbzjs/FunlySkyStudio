using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: 场景内素材可购买设置操作面板
/// Date: 2021-12-11 23:26:09
/// </summary>
public class TransactionPropPanel : MonoBehaviour
{
    private SceneEntity curEntity;

    [SerializeField]
    private Toggle allowToggle;

    public void Init()
    {
        allowToggle.onValueChanged.AddListener(OnToggleSelect);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        if (curEntity.HasComponent<UGCPropComponent>())
        {
            UGCPropComponent cmp = curEntity.Get<UGCPropComponent>();
            allowToggle.isOn = cmp.isTradable == 1;
        }
        else
        {
            allowToggle.isOn = false;
        }
    }

    private void OnDestroy()
    {
        allowToggle.onValueChanged.RemoveListener(OnToggleSelect);
    }

    private void OnToggleSelect(bool isOn)
    {
        UGCPropComponent cmp = curEntity.Get<UGCPropComponent>();
        cmp.isTradable = isOn ? 1 : 0;

        var gComp = curEntity.Get<GameObjectComponent>();

        if (curEntity.HasComponent<UGCClothItemComponent>())
        {
            //UGC衣服道具
            var ugcClothBev = gComp.bindGo.GetComponent<UgcClothItemBehaviour>();
            if(ugcClothBev != null)
            {
                ugcClothBev.SetCanBuyInMap();
            }
            else
            {
                LoggerUtils.LogError("TransactionPropPanel ugcClothBev == null");
            }
        }
        else if (curEntity.HasComponent<PGCSceneComponent>())
        {
            var pgcBev = gComp.bindGo.GetComponent<PGCBehaviour>();
            if(pgcBev != null)
            {
                pgcBev.SetCanBuyInMap();
            }
            else
            {
                LoggerUtils.LogError("TransactionPropPanel pgcBev == null");
            }
        }
        else
        {
            //UGC素材
            var cBehav = gComp.bindGo.GetComponent<UGCCombBehaviour>();
            if(cBehav != null)
            {
                cBehav.SetCanBuyInMap();
            }
            else
            {
                LoggerUtils.LogError("TransactionPropPanel cBehav == null");
            }
        }
    }
}