using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertySwitchPanel : MonoBehaviour
{
    private GameObject proItem;
    private SceneEntity curEntity;
    public Transform proParent;

    [SerializeField]
    private Toggle activeToggle;
    [SerializeField]
    private GameObject switchScrollView;
    private Dictionary<int, CommonButtonItem> itemScripts = new Dictionary<int, CommonButtonItem>();

    private SwitchControlType _ctrlType;

    public SwitchControlType CtrlType { private get; set; }

    public void Init()
    {
        proItem = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "PropertiesButtonItem");
        activeToggle.onValueChanged.AddListener(OnToggleSwitchActive);
        switchScrollView.SetActive(false);
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

    private void OnToggleSwitchActive(bool isOn)
    {
        if (isOn)
        {
            if (itemScripts.Count <= 0)
            {
                TipPanel.ShowToast("Please add a Switch Button first");
                activeToggle.isOn = false;
                return;
            }
            switchScrollView.SetActive(true);
        }
        else
        {
            OnItemClick(0);
            switchScrollView.SetActive(false);
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

    public void SelectSwitches(bool isSelected)
    {
        activeToggle.isOn = isSelected;
        switchScrollView.SetActive(isSelected);
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

        bool isControlBySwitch = false;

        // CreateItem(0, "None");

        var tempDic = new Dictionary<int, string>();
        foreach (var sid in SwitchManager.Inst.switchBevs.Keys)
        {
            if (!tempDic.ContainsKey(sid))
            {
                tempDic.Add(sid, sid.ToString());
            }   
        }
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
        if (CtrlType == SwitchControlType.VISIBLE_CONTROL)
        {
            if (!curEntity.HasComponent<ShowHideComponent>())
            {
                SelectSwitches(false);
                return;
            }
            var uids = curEntity.Get<ShowHideComponent>().switchUids;
            if (uids.Count == 0)
            {
                // itemScripts[0].SetSelectState(true);
                SelectSwitches(false);
                return;
            }
            foreach (var sid in itemScripts.Keys)
            {
                if (sid != 0)
                {
                    int uid = SwitchManager.Inst.GetSwtichUidBySId(sid);
                    if (uids.Contains(uid))
                    {
                        itemScripts[sid].SetSelectState(true);
                        isControlBySwitch = true;
                    }
                }
            }
        }
        else
        {
            if (!curEntity.HasComponent<SwitchControlComponent>())
            {
                SelectSwitches(false);
                return;
            }

            var switchCtrlCmp = curEntity.Get<SwitchControlComponent>();
            var uids = switchCtrlCmp.switchUids;
            if (CtrlType == SwitchControlType.SOUNDPLAY_CONTROL)
            {
                uids = switchCtrlCmp.switchSoundUids;
            }
            else if (CtrlType == SwitchControlType.ANIMATION_CONTROL)
            {
                uids = switchCtrlCmp.switchAnimUids;
            }
            else if (CtrlType == SwitchControlType.FIREWORK_CONTROL)
            {
                uids = switchCtrlCmp.switchFireworkUids;
            }

            if (uids.Count == 0)
            {
                // itemScripts[0].SetSelectState(true);
                SelectSwitches(false);
                return;
            }
            foreach (var sid in itemScripts.Keys)
            {
                if (sid != 0)
                {
                    int uid = SwitchManager.Inst.GetSwtichUidBySId(sid);
                    if (uids.Contains(uid))
                    {
                        itemScripts[sid].SetSelectState(true);
                        isControlBySwitch = true;
                    }
                }
            }
        }

        SelectSwitches(isControlBySwitch);
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
                    int sUid = SwitchManager.Inst.GetSwtichUidBySId(k);
                    if (CtrlType == SwitchControlType.VISIBLE_CONTROL)
                    {
                        ShowHideManager.Inst.RemoveSwitchId(curEntity, sUid);
                    }
                    else if (CtrlType == SwitchControlType.SOUNDPLAY_CONTROL)
                    {
                        SwitchControlManager.Inst.RemoveSoundSwitchId(curEntity, sUid);
                    }
                    else if (CtrlType == SwitchControlType.ANIMATION_CONTROL)
                    {
                        SwitchControlManager.Inst.RemoveAnimSwitchId(curEntity, sUid);
                    }
                    else if (CtrlType == SwitchControlType.FIREWORK_CONTROL)
                    {
                        SwitchControlManager.Inst.RemoveFireworkSwitchId(curEntity, sUid);
                    }
                    else
                    {
                        SwitchControlManager.Inst.RemoveSwitchId(curEntity, sUid);
                    }

                    SwitchManager.Inst.RemoveControlledId(k, curEntity.Get<GameObjectComponent>().uid, (int)CtrlType);
                }
                itemScripts[k].SetSelectState(false);
            }
        }
        else
        {
            if (itemScripts.ContainsKey(sid))
            {
                int sUid = SwitchManager.Inst.GetSwtichUidBySId(sid);
                if (itemScripts[sid].GetSelectState() == true)
                {
                    //cancel bind switch
                    if (CtrlType == SwitchControlType.VISIBLE_CONTROL)
                    {
                        ShowHideManager.Inst.RemoveSwitchId(curEntity, sUid);
                    }
                    else if (CtrlType == SwitchControlType.SOUNDPLAY_CONTROL)
                    {
                        SwitchControlManager.Inst.RemoveSoundSwitchId(curEntity, sUid);
                    }
                    else if (CtrlType == SwitchControlType.ANIMATION_CONTROL)
                    {
                        SwitchControlManager.Inst.RemoveAnimSwitchId(curEntity, sUid);
                    }
                    else if (CtrlType == SwitchControlType.FIREWORK_CONTROL)
                    {
                        SwitchControlManager.Inst.RemoveFireworkSwitchId(curEntity, sUid);
                    }
                    else
                    {
                        SwitchControlManager.Inst.RemoveSwitchId(curEntity, sUid);
                    }
                    SwitchManager.Inst.RemoveControlledId(sid, curEntity.Get<GameObjectComponent>().uid, (int)CtrlType);

                    itemScripts[sid].SetSelectState(false);
                }
                else
                {
                    //bind switch
                    if (CtrlType == SwitchControlType.VISIBLE_CONTROL)
                    {
                        ShowHideManager.Inst.AddSwitchId(curEntity, sUid);
                    }
                    else
                    {
                        var switchCtrlCmp = curEntity.Get<SwitchControlComponent>();
                        if (CtrlType == SwitchControlType.SOUNDPLAY_CONTROL)
                        {
                            // switchCtrlCmp.controlPlaySound = (int)SoundControl.SUPPORT_CTRL_MUSIC;
                            SwitchControlManager.Inst.AddSoundSwitchId(curEntity, sUid);
                        }
                        else if (CtrlType == SwitchControlType.ANIMATION_CONTROL)
                        {
                            // switchCtrlCmp.switchControlType = (int)CtrlType;
                            SwitchControlManager.Inst.AddAnimSwitchId(curEntity, sUid);
                        }
                        else if (CtrlType == SwitchControlType.FIREWORK_CONTROL)
                        {
                            // switchCtrlCmp.switchControlType = (int)CtrlType;
                            SwitchControlManager.Inst.AddFireworkSwitchId(curEntity, sUid);
                        }
                        else
                        {
                            // switchCtrlCmp.switchControlType = (int)CtrlType;
                            SwitchControlManager.Inst.AddSwitchId(curEntity, sUid);
                        }

                    }
                    SwitchManager.Inst.AddControlledId(sid, curEntity.Get<GameObjectComponent>().uid, (int)CtrlType);


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
