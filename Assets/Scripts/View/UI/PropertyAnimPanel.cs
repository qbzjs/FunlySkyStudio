using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:WenJia
/// Description: 属性设置之旋转设置面板
/// Date: 2022/4/22 18:6:25
/// </summary>


public class PropertyAnimPanel : MonoBehaviour
{
    public Transform RotParent;
    public Transform AxisParent;
    public Transform VerParent;
    public GameObject[] Panels;
    public Toggle activeToggle;
    public Toggle inActiveToggle;
    private List<Text> toggleNames;
    private SceneEntity curEntity;
    private GameObject proItem;
    private string[] itemNames = { "Still", "Slow", "Medium", "Fast" };
    private readonly string[] axisItemNames = { "y-axis", "x-axis", "z-axis" };
    private int rIndex = 0;
    private int uIndex = 0;
    private int aIndex = 0;
    private List<CommonButtonItem> rotScripts = new List<CommonButtonItem>();
    private List<CommonButtonItem> updownScripts = new List<CommonButtonItem>();
    private List<CommonButtonItem> axisScripts = new List<CommonButtonItem>();
    private PropertySwitchPanel switchPanel;
    private PropertyCollectiblesPanel collectiblesPanel;
    private PropertySensorBoxPanel sensorBoxPanel;

    public void Init()
    {
        proItem = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "PropertiesButtonItem");
        for (int i = 0; i < itemNames.Length; i++)
        {
            int index = i;
            var rItem = GameObject.Instantiate(proItem, RotParent);
            var rScript = rItem.AddComponent<CommonButtonItem>();
            rScript.Init();
            rScript.SetText(itemNames[i]);
            rScript.AddClick(() => OnRotClick(index));
            rotScripts.Add(rScript);

            var vItem = GameObject.Instantiate(proItem, VerParent);
            var vScript = vItem.AddComponent<CommonButtonItem>();
            vScript.Init();
            vScript.SetText(itemNames[i]);
            vScript.AddClick(() => OnVerClick(index));
            updownScripts.Add(vScript);
        }

        for (int i = 0; i < axisItemNames.Length; i++)
        {
            int index = i;

            var axisItem = GameObject.Instantiate(proItem, AxisParent);
            var axisScript = axisItem.AddComponent<CommonButtonItem>();
            if (i == 1)
            {
                // X 轴排最前面
                axisItem.transform.SetAsFirstSibling();
            }
            axisScript.Init();
            axisScript.SetText(axisItemNames[i]);
            axisScript.AddClick(() => OnAxisClick(index));
            axisScripts.Add(axisScript);
        }

        activeToggle.onValueChanged.AddListener(OnToggleActive);
        inActiveToggle.onValueChanged.AddListener(OnToggleInactive);
        switchPanel = Panels[0].GetComponent<PropertySwitchPanel>();
        switchPanel.CtrlType = SwitchControlType.ANIMATION_CONTROL;
        switchPanel.Init();
        collectiblesPanel = Panels[1].GetComponent<PropertyCollectiblesPanel>();
        collectiblesPanel.CtrlType = CollectControlType.ANIMATION_CONTROL;
        collectiblesPanel.Init();
        sensorBoxPanel = Panels[2].GetComponent<PropertySensorBoxPanel>();
        sensorBoxPanel.CtrlType = PropControlType.ANIMATION_CONTROL;
        sensorBoxPanel.Init();
    }

    private void OnToggleActive(bool isOn)
    {
        if (isOn)
        {
            var comp = curEntity.Get<RPAnimComponent>();
            comp.animState = 0;
            comp.tempAnimState = comp.animState;
        }
    }

    private void OnToggleInactive(bool isOn)
    {
        if (isOn)
        {
            var comp = curEntity.Get<RPAnimComponent>();
            comp.animState = 1;
            comp.tempAnimState = comp.animState;
        }
    }

    private void OnRotClick(int index)
    {
        rotScripts[rIndex].SetSelectState(false);
        rIndex = index;
        rotScripts[rIndex].SetSelectState(true);
        curEntity.Get<RPAnimComponent>().rSpeed = index;
    }

    private void OnAxisClick(int index)
    {
        if (aIndex == index)
            return;
        axisScripts[aIndex].SetSelectState(false);
        aIndex = index;
        axisScripts[aIndex].SetSelectState(true);
        curEntity.Get<RPAnimComponent>().rAxis = index;
    }

    private void OnVerClick(int index)
    {
        updownScripts[uIndex].SetSelectState(false);
        uIndex = index;
        updownScripts[uIndex].SetSelectState(true);
        curEntity.Get<RPAnimComponent>().uSpeed = index;
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var comp = curEntity.Get<RPAnimComponent>();
        rIndex = comp.rSpeed;
        uIndex = comp.uSpeed;
        aIndex = comp.rAxis;
        //兼容旧数据，物体上下移动静止且旋转静止，则物体静止
        if (rIndex == 0 && uIndex == 0)
        {
            comp.animState = 1;
        }
        comp.tempAnimState = comp.animState;

        activeToggle.SetIsOnWithoutNotify(comp.animState == 0);
        inActiveToggle.SetIsOnWithoutNotify(comp.animState == 1);
        OnRotClick(rIndex);
        OnVerClick(uIndex);
        axisScripts[aIndex].SetSelectState(true);
        switchPanel.SetEntity(entity);
        collectiblesPanel.SetEntity(entity);
        sensorBoxPanel.SetEntity(entity);
    }

    public void ResetSelectItems()
    {
        rotScripts[rIndex].SetSelectState(false);
        updownScripts[uIndex].SetSelectState(false);
        axisScripts[aIndex].SetSelectState(false);
    }
}
