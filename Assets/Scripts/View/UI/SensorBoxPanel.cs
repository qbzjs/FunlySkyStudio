using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Author:JayWill
/// Description:感应盒专属属性面板
/// </summary>
public class SensorBoxPanel : InfoPanel<SensorBoxPanel>
{
    [SerializeField]
    private Toggle OnceToggle;

    [SerializeField]
    private Toggle UnlimitedToggle;
    private SceneEntity curEntity;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        OnceToggle.onValueChanged.AddListener(OnOnceSelect);
        UnlimitedToggle.onValueChanged.AddListener(OnUnlimitedSelect);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var tComp = entity.Get<SensorBoxComponent>();

        if(tComp.boxTimes == 1){
            OnceToggle.isOn = true;
        }else{
            UnlimitedToggle.isOn = true;
        }
    }

    private void OnOnceSelect(bool isOn)
    {
        if(curEntity != null)
        {
            var tComp = curEntity.Get<SensorBoxComponent>();
            tComp.boxTimes = 1;
        }
    }

    private void OnUnlimitedSelect(bool isOn)
    {
        if(curEntity != null)
        {
            var tComp = curEntity.Get<SensorBoxComponent>();
            tComp.boxTimes = -1;
        }
    }

}