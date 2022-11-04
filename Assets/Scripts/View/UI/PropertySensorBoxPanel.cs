using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:JayWill
/// Description:属性面板中：感应盒操作面板
/// </summary>
public class PropertySensorBoxPanel : MonoBehaviour
{
    private GameObject proItem;
    private SceneEntity curEntity;

    [SerializeField]
    private Toggle activeToggle;
    [SerializeField]
    private GameObject sensorBoxScrollView;

    public Transform proParent;
    private Dictionary<int, CommonButtonItem> itemScripts = new Dictionary<int, CommonButtonItem>();

    private PropControlType _ctrlType;

    public PropControlType CtrlType { private get; set; }

    public void Init()
    {
        proItem = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "PropertiesButtonItem");
        activeToggle.onValueChanged.AddListener(OnToggleSensorBoxActive);
        sensorBoxScrollView.SetActive(false);
    }


    private void OnDestroy()
    {
    }

    public void OnSwitchEdited(SwitchAction switchAction, int sid)
    {
        if (switchAction == SwitchAction.Add)
        {
            CreateItem(sid, sid.ToString());
        }
        else
        {
            Destroy(itemScripts[sid].gameObject);
            itemScripts.Remove(sid);
        }
    }

    private void OnToggleSensorBoxActive(bool isOn)
    {
        if (isOn)
        {
            if (itemScripts.Count <= 0)
            {
                TipPanel.ShowToast("Please add a Sensor Box first");
                activeToggle.isOn = false;
                return;
            }
            sensorBoxScrollView.SetActive(true);
        }
        else
        {
            OnItemClick(0);
            sensorBoxScrollView.SetActive(false);
        }
    }

    private void CreateItem(int id, string text)
    {
        var rItem = GameObject.Instantiate(proItem, proParent);
        var rScript = rItem.AddComponent<CommonButtonItem>();
        rScript.Init();
        rScript.SetText(text);
        rScript.AddClick(() => OnItemClick(id));
        itemScripts.Add(id, rScript);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        RefreshUI();
    }

    public void RefreshUI()
    {
        RefreshItemState();

        float alpha = 1;
        if (itemScripts.Count <= 0)
        {
            alpha = 0.5f;
        }
        activeToggle.targetGraphic.color = new Color(1, 1, 1, alpha);
    }

    public void SelectSensorBoxes(bool isSelected)
    {
        activeToggle.isOn = isSelected;
        sensorBoxScrollView.SetActive(isSelected);
    }

    private void RefreshNoneButtonState()
    {
        foreach (var item in itemScripts.Values)
        {
            if (item.GetSelectState() == true)
            {
                // itemScripts[0].SetSelectState(false);
                return;
            }
        }
        // itemScripts[0].SetSelectState(true);
    }

    private void DictionarySort(Dictionary<int, string> dic)
    {
        if (dic.Count > 0)
        {
            List<KeyValuePair<int, string>> lst = new List<KeyValuePair<int, string>>(dic);
            lst.Sort(delegate (KeyValuePair<int, string> s1, KeyValuePair<int, string> s2)
            {
                return s1.Key.CompareTo(s2.Key);
            });
            dic.Clear();

            foreach (KeyValuePair<int, string> kvp in lst)
            {
                dic.Add(kvp.Key, kvp.Value);
            }
        }
    }

    private void RefreshItemState()
    {
        foreach (var item in itemScripts.Values)
        {
            Destroy(item.gameObject);
        }
        itemScripts.Clear();

        // CreateItem(0, "None");

        var tempDic = SensorBoxManager.Inst.GetIndexDict();
        DictionarySort(tempDic);
        foreach (var sid in tempDic.Keys)
        {
            if (!itemScripts.ContainsKey(sid))
            {
                CreateItem(sid, sid.ToString());
            }
        }
        tempDic.Clear();

        foreach (var sid in itemScripts.Keys)
        {
            itemScripts[sid].SetSelectState(false);
        }
        if (!curEntity.HasComponent<SensorControlComponent>())
        {
            // itemScripts[0].SetSelectState(true);
            SelectSensorBoxes(false);
            return;
        }
        List<int> uids = null;
        if (CtrlType == PropControlType.VISIBLE_CONTROL)
        {
            uids = curEntity.Get<SensorControlComponent>().visibleSensorUids;
        }
        else if (CtrlType == PropControlType.MOVEMENT_CONTROL)
        {
            uids = curEntity.Get<SensorControlComponent>().moveSensorUids;
        }
        else if (CtrlType == PropControlType.SOUNDPLAY_CONTROL)
        {
            uids = curEntity.Get<SensorControlComponent>().soundSensorUids;
        }
        else if (CtrlType == PropControlType.ANIMATION_CONTROL)
        {
            uids = curEntity.Get<SensorControlComponent>().animSensorUids;
        }
        else if (CtrlType == PropControlType.FIREWORK_CONTROL)
        {
            uids = curEntity.Get<SensorControlComponent>().fireworkSensorUids;
        }


        if (uids == null || uids.Count == 0)
        {
            // itemScripts[0].SetSelectState(true);
            SelectSensorBoxes(false);
            return;
        }

        LoggerUtils.Log("@@@@@@当前感应盒uids:"+uids);
        LoggerUtils.Log("@@@@@@当前感应盒uids.Count:"+uids.Count);

        bool isControlBySensorBox = false;
        foreach (var index in itemScripts.Keys)
        {
            if (index != 0)
            {
                int uid = SensorBoxManager.Inst.GetBoxUidByIndex(index);
                if (uids.Contains(uid))
                {
                    itemScripts[index].SetSelectState(true);
                    isControlBySensorBox = true;
                }
            }
        }

        SelectSensorBoxes(isControlBySensorBox);
    }


    public void OnItemClick(int sid)
    {
        //sid = 0 : click none button
        if (sid == 0)
        {
            foreach (var k in itemScripts.Keys)
            {
                if (k != 0)
                {
                    int sUid = SensorBoxManager.Inst.GetBoxUidByIndex(k);
                    if (CtrlType == PropControlType.VISIBLE_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToVisible(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.MOVEMENT_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToMove(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.SOUNDPLAY_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToSound(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.ANIMATION_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToAnim(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.FIREWORK_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToFirework(curEntity, sUid);
                    }

                    SensorBoxManager.Inst.RemoveControlledId(sUid, curEntity.Get<GameObjectComponent>().uid, (int)CtrlType);
                }
                itemScripts[k].SetSelectState(false);
            }
        }
        else
        {
            if (itemScripts.ContainsKey(sid))
            {
                int sUid = SensorBoxManager.Inst.GetBoxUidByIndex(sid);
                if (itemScripts[sid].GetSelectState() == true)
                {
                    //cancel bind 
                    if (CtrlType == PropControlType.VISIBLE_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToVisible(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.MOVEMENT_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToMove(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.SOUNDPLAY_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToSound(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.ANIMATION_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToAnim(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.FIREWORK_CONTROL)
                    {
                        SensorBoxManager.Inst.UnBindEntityToFirework(curEntity, sUid);
                    }
                    SensorBoxManager.Inst.RemoveControlledId(sUid, curEntity.Get<GameObjectComponent>().uid, (int)CtrlType);

                    itemScripts[sid].SetSelectState(false);
                }
                else
                {
                    //bind
                    if (CtrlType == PropControlType.VISIBLE_CONTROL)
                    {
                        SensorBoxManager.Inst.BindEntityToVisible(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.MOVEMENT_CONTROL)
                    {
                        SensorBoxManager.Inst.BindEntityToMove(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.SOUNDPLAY_CONTROL)
                    {
                        SensorBoxManager.Inst.BindEntityToSound(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.ANIMATION_CONTROL)
                    {
                        SensorBoxManager.Inst.BindEntityToAnim(curEntity, sUid);
                    }
                    else if (CtrlType == PropControlType.FIREWORK_CONTROL)
                    {
                        SensorBoxManager.Inst.BindEntityToFirework(curEntity, sUid);
                    }
                    SensorBoxManager.Inst.AddControlledId(sUid, curEntity.Get<GameObjectComponent>().uid, (int)CtrlType);

                    itemScripts[sid].SetSelectState(true);
                }
            }
            else
            {
                LoggerUtils.Log("[Click NOT Exist Switch] sid=>" + sid);
            }
        }

        // RefreshNoneButtonState();
    }

}
