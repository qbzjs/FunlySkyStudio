using AvatarRedDotSystem;
using RedDot;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectView : BaseView
{
    private RoleColorAdjustView effectAdjustView;
    public RoleStyleView[] iconView;

    public Toggle[] SubToggles;
    public GameObject[] Panels;
    public GameObject[] newImage;
    public GameObject toggleParent;
    public VNode mDCRedDotNode;
    public VNode mOriginalRedDotNode;
    private int curSelectTogIndex = 0;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitEffectView);
    }
    public void InitEffectView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.effects;
        effectAdjustView = GetComponentInChildren<RoleColorAdjustView>(true);
        BindEffectAdjust();
        iconView[0].part = bodyPart;
        iconView[0].type = classifyType;
        NewUserUiSetting(toggleParent, Panels);
        InitToggle();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetEffectStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectEffectIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.effId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var effectStyleData = RoleConfigDataManager.Inst.GetEffectStyleDataById(itemId);
        if (effectStyleData != null)
        {
            effectStyleData.rc = roleStyleItem;
            OnSelectEffectIcon(effectStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
        SelectSub(curSelectTogIndex);
    }

    public override void UpdateSelectState()
    {
        if (effectAdjustView)
        {
            int index = Array.FindIndex(SubToggles, (tog) => tog.isOn);
            UpdateOnSelectSub(index);
        }
    }

    private void InitToggle()
    {
        for (var i = 0; i < SubToggles.Length; i++)
        {
            int index = i;
            SubToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    SelectSub(index);
                    UpdateOnSelectSub(index);
                }
            });
        }
    }

    private void UpdateOnSelectSub(int index)
    {
        iconView[index].GetAllItemList(InitListAction);
        iconView[index].curItem = null;
        iconView[index].SetSelect(roleData.effId);
    }

    private void SelectSub(int index)
    {
        for (var i = 0; i < Panels.Length; i++)
        {
            Panels[i].SetActive(false);
            SubToggles[i].GetComponent<Text>().color = new Color32(151, 151, 151, 255);
        }
        Panels[index].SetActive(true);
        SubToggles[index].GetComponent<Text>().color = new Color32(0, 0, 0, 255);
        newImage[index].gameObject.SetActive(false);
        curSelectTogIndex = index;
        if (index == 0)
        {
            ClearRed(mOriginalRedDotNode, "effectoriginal");
        }
        if (index == 1)
        {
            ClearRed(mDCRedDotNode, "dceffect");
        }
    }

    private void BindEffectAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.X_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Y_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Z_Rotation);
        effectAdjustView.Init(mAdjustItemContexts, AdjustItemValueChanged, OnAdjustViewResetCallBack);
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        EffectStyleData data = RoleConfigDataManager.Inst.GetEffectStyleDataById(id);
        SetAdjustView2Normal(effectAdjustView, data);
    }
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, EffectStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, roleData.effS, GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.effP, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, roleData.effP, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.effP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue(EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, roleData.effR, GetVecAxis(EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue(EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, roleData.effR, GetVecAxis(EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue(EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, roleData.effR, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, EffectStyleData data)
    {
        effectAdjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, data.sDef, GetVecAxis(EAdjustItemType.Size)));
        effectAdjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        effectAdjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, data.pDef, GetVecAxis(EAdjustItemType.Left_right)));
        effectAdjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        effectAdjustView.SetSliderValue(EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, data.rDef, GetVecAxis(EAdjustItemType.X_Rotation)));
        effectAdjustView.SetSliderValue(EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, data.rDef, GetVecAxis(EAdjustItemType.Y_Rotation)));
        effectAdjustView.SetSliderValue(EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, data.rDef, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        EffectStyleData data = RoleConfigDataManager.Inst.GetEffectStyleDataById(roleData.effId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.effS = GetValueBySlider(roleData.effS, data.scaleLimit, value);
                rController.SetEffectSca(roleData.effS);
                break;
            case EAdjustItemType.Up_down:
                roleData.effP = GetValueBySlider(roleData.effP, data.vLimit, value, VecAxis.X);
                rController.SetEffectPos(roleData.effP);
                break;
            case EAdjustItemType.Left_right:
                roleData.effP = GetValueBySlider(roleData.effP, data.hLimit, value, VecAxis.Z);
                rController.SetEffectPos(roleData.effP);
                break;
            case EAdjustItemType.Front_back:
                roleData.effP = GetValueBySlider(roleData.effP, data.fLimit, value, VecAxis.Y);
                rController.SetEffectPos(roleData.effP);
                break;
            case EAdjustItemType.X_Rotation:
                roleData.effR = GetValueBySlider(roleData.effR, data.xrotLimit, value, VecAxis.X);
                rController.SetEffectRot(roleData.effR);
                break;
            case EAdjustItemType.Y_Rotation:
                roleData.effR = GetValueBySlider(roleData.effR, data.zrotLimit, value, VecAxis.Z);
                rController.SetEffectRot(roleData.effR);
                break;
            case EAdjustItemType.Z_Rotation:
                roleData.effR = GetValueBySlider(roleData.effR, data.yrotLimit, value, VecAxis.Y);
                rController.SetEffectRot(roleData.effR);
                break;
            default:
                break;
        }
    }

    private void OnSelectEffectIcon(RoleIconData obj)
    {
        EffectStyleData data = obj as EffectStyleData;
        effectAdjustView.SetCurrentId(data.id);
        if (roleData.effId == data.id)
        {
            SetAdjustView2RoleData(effectAdjustView, data, roleData);
        }

        PlayLoadingAnime(obj, true);
        rController.SetStyle(BundlePart.Effect, data.texName, () => PlayLoadingAnime(obj, false), () => PlayLoadingAnime(obj, false));
        if (roleData.effId != data.id)
        {
            var effectData = RoleConfigDataManager.Inst.GetEffectStyleDataById(data.id);
            roleData.effP = effectData.pDef;
            roleData.effS = effectData.sDef;
            roleData.effR = effectData.rDef;
            roleData.effId = data.id;
            SetAdjustView2Normal(effectAdjustView, data);
        }
        effectAdjustView.ShowColorView(false);
    }
    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.effect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("effectoriginal"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[0].gameObject, (int)ENodeType.effect, (int)ENodeType.effectoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mOriginalRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("dceffect"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[1].gameObject, (int)ENodeType.effect, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mDCRedDotNode = vNode;
            }
        }
    }
    public VecAxis GetVecAxis(EAdjustItemType itemType)
    {
        switch (itemType)
        {
            case EAdjustItemType.Up_down:
                return VecAxis.X;
            case EAdjustItemType.Front_back:
                return VecAxis.Y;
            case EAdjustItemType.Left_right:
                return VecAxis.Z;
            case EAdjustItemType.X_Rotation:
                return VecAxis.X;
            case EAdjustItemType.Y_Rotation:
                return VecAxis.Z;
            case EAdjustItemType.Z_Rotation:
                return VecAxis.Y;
            default:
                break;
        }
        return VecAxis.None;
    }
}
