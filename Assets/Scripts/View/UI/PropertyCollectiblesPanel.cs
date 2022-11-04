/*
* @Author: YangJie
 * @LastEditors: wenjia
* @Description: Collectibles Panel (doc: https://pointone.feishu.cn/docs/doccnW7wavIb2jzzjryIVYBFOHe#)
* @Date: ${YEAR}-${MONTH}-${DAY} ${TIME}
* @Modify:
*/

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//收藏控制的类型
public enum CollectControlType
{
    VISIBLE_CONTROL, //显隐控制
    MOVEMENT_CONTROL, // 移动控制
    SOUNDPLAY_CONTROL, // 声音播放控制
    ANIMATION_CONTROL, // 旋转移动控制
    FIREWORK_CONTROL,//烟花播放控制
}

public class PropertyCollectiblesPanel : MonoBehaviour
{
    
    private SceneEntity curEntity;
    
    [SerializeField]
    private Toggle activeToggle;
    private CollectControlType _ctrlType;
    private List<PropStarBehaviour> propStarBehaviours = new List<PropStarBehaviour>();

    public CollectControlType CtrlType { private get; set; }

    public void Init()
    {
        activeToggle.onValueChanged.AddListener(OnToggleActive);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        propStarBehaviours = SceneSystem.Inst.FilterNodeBehaviours<PropStarBehaviour>(SceneBuilder.Inst.allControllerBehaviours);

        if (curEntity.HasComponent<CollectControlComponent>())
        {
            CollectControlComponent cmp = curEntity.Get<CollectControlComponent>();
            if (CtrlType == CollectControlType.MOVEMENT_CONTROL)
            {
                activeToggle.isOn = cmp.moveActive;
            }
            else if (CtrlType == CollectControlType.SOUNDPLAY_CONTROL)
            {
                activeToggle.isOn = cmp.playSound == (int)SoundControl.SUPPORT_CTRL_MUSIC;
            }
            else if (CtrlType == CollectControlType.ANIMATION_CONTROL)
            {
                activeToggle.isOn = cmp.animActive == (int)AnimControl.SUPPORT_CTRL_ANIM;
            }
            else if (CtrlType == CollectControlType.FIREWORK_CONTROL)
            {
                activeToggle.isOn = cmp.playfirework == (int)FireworkControl.SUPPORT_CTRL_Firework;
            }
            else
            {
                activeToggle.isOn = cmp.isControl == 1;
            }
        }
        else
        {
            activeToggle.isOn = false;
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        float alpha = 1;

        if (propStarBehaviours.Count <= 0)
        {
            alpha = 0.5f;
            activeToggle.isOn = false;
        }
        activeToggle.targetGraphic.color = new Color(1, 1, 1, alpha);
    }


    private void OnDestroy()
    {
        activeToggle.onValueChanged.RemoveListener(OnToggleActive);
    }

    private void OnToggleActive(bool isOn)
    {
        if (isOn)
        {
            if (propStarBehaviours.Count <= 0)
            {
                TipPanel.ShowToast("Please add Collectibles first");
                activeToggle.isOn = false;
                return;
            }
            var cmp = curEntity.Get<CollectControlComponent>();
            if (CtrlType == CollectControlType.MOVEMENT_CONTROL)
            {
                cmp.moveActive = true;
            }
            else if (CtrlType == CollectControlType.SOUNDPLAY_CONTROL)
            {
                cmp.playSound = (int)SoundControl.SUPPORT_CTRL_MUSIC;
            }
            else if (CtrlType == CollectControlType.ANIMATION_CONTROL)
            {
                cmp.animActive = (int)AnimControl.SUPPORT_CTRL_ANIM;
            }
            else if (CtrlType == CollectControlType.FIREWORK_CONTROL)
            {
                cmp.playfirework = (int)FireworkControl.SUPPORT_CTRL_Firework;
            }
            else
            {
                cmp.isControl = 1;
            }
        }
        else
        {
            if (curEntity.HasComponent<CollectControlComponent>())
            {
                var cmp = curEntity.Get<CollectControlComponent>();
                if (CtrlType == CollectControlType.MOVEMENT_CONTROL)
                {
                    cmp.moveActive = false;
                }
                else if (CtrlType == CollectControlType.SOUNDPLAY_CONTROL)
                {
                    cmp.playSound = (int)SoundControl.NOT_SUPPORT;
                }
                else if (CtrlType == CollectControlType.ANIMATION_CONTROL)
                {
                    cmp.animActive = (int)AnimControl.NOT_SUPPORT;
                }
                else if (CtrlType == CollectControlType.FIREWORK_CONTROL)
                {
                    cmp.playfirework = (int)FireworkControl.NOT_SUPPORT;
                }
                else
                {
                    cmp.isControl = 0;
                }
            }
        }
    }
}