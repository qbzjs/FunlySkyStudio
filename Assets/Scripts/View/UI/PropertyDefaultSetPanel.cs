using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyDefaultSetPanel : MonoBehaviour
{
    private SceneEntity curEntity;
    public GameObject ToggleView;
    public Toggle activeToggle;
    public Toggle inActiveToggle;

    public int IsDefaultShow;
    private PropertySwitchPanel switchPanel;
    private PropertyCollectiblesPanel collectiblesPanel;
    private PropertySensorBoxPanel sensorBoxPanel;
    public GameObject[] Panels;


    public void Init()
    {
        activeToggle.onValueChanged.AddListener(OnDefaultOpenClick);
        inActiveToggle.onValueChanged.AddListener(OnDefaultHideClick);
        switchPanel = Panels[0].GetComponent<PropertySwitchPanel>();
        switchPanel.CtrlType = SwitchControlType.VISIBLE_CONTROL;
        switchPanel.Init();

        collectiblesPanel = Panels[1].GetComponent<PropertyCollectiblesPanel>();
        collectiblesPanel.CtrlType = CollectControlType.VISIBLE_CONTROL;
        collectiblesPanel.Init();

        sensorBoxPanel = Panels[2].GetComponent<PropertySensorBoxPanel>();
        sensorBoxPanel.CtrlType = PropControlType.VISIBLE_CONTROL;
        sensorBoxPanel.Init();
    }

    private void OnDestroy()
    {
        activeToggle.onValueChanged.RemoveAllListeners();
        inActiveToggle.onValueChanged.RemoveAllListeners();
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;

        if (curEntity.HasComponent<ShowHideComponent>())
        {
            IsDefaultShow = curEntity.Get<ShowHideComponent>().defaultShow;
            activeToggle.isOn = IsDefaultShow == 0;
            inActiveToggle.isOn = IsDefaultShow == 1;
        }else
        {
            activeToggle.isOn = true;
            inActiveToggle.isOn = false;
        }

        switchPanel.SetEntity(entity);
        collectiblesPanel.SetEntity(entity);
        sensorBoxPanel.SetEntity(entity);
    }

    private void OnDefaultOpenClick(bool isOn)
    {
        if (curEntity.HasComponent<ShowHideComponent>() && isOn)
        {
            //Debug.Log("Set ShowHideComponent defaultShow=0");

            ShowHideComponent cmp = curEntity.Get<ShowHideComponent>();
            cmp.defaultShow = 0;
            ShowHideManager.Inst.UpdateShowHideCmpInEntity(curEntity);
        }
    }

    private void OnDefaultHideClick(bool isOn)
    {
        if (isOn)
        {
            curEntity.Get<ShowHideComponent>().defaultShow = 1;
            ShowHideManager.Inst.AddShowHideEntityToDict(curEntity);

            //Debug.Log("Set ShowHideComponent defaultShow=1");
        }
    }

}
