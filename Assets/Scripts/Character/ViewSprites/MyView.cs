using AvatarRedDotSystem;
using RedDot;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: My合集页面
/// Date: 2022/8/24 15:38:3
/// </summary>
public class MyView : BaseView
{
    public Toggle[] SubToggles;
    public GameObject[] Panels;
    public GameObject[] newImage;
    public GameObject toggleParent;
    public VNode mRWRedDotNode;
    public VNode mADRedDotNode;
    public RewardsView rewardsView;
    public AirdropView airdropView;
    private int curSelectTogIndex = 0;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitMyView);
    }

    public void InitMyView()
    {
        this.classifyType = ClassifyType.my;
        NewUserUiSetting(toggleParent, Panels);
        InitToggle();
    }

    private void InitToggle()
    {
        for (var i = 0; i < SubToggles.Length; i++)
        {
            int index = i;
            SubToggles[i].onValueChanged.AddListener((isOn) => { if (isOn) SelectSub(index); });
        }
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.SET_REWARDS)
        {
            var dcLists = GetAllDcPgcInfos();
            if(dcLists == null)
            {
                LoggerUtils.Log("MyView InitToggle Failed --> ugcClothInfo == null");
                return;
            }
            if (dcLists[0] == null || dcLists[0].Equals(default(PGCInfo)))
            {
                LoggerUtils.Log("MyView InitToggle Failed --> ugcClothInfo == null");
                return;
            }
            var type = (ClassifyType)dcLists[0].classifyType;
            var id = dcLists[0].pgcId;
            //选中指定分类
            var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(type, id);
            if (rcData == null)
            {
                return;
            }
            if (rcData.origin == (int)RoleOriginType.Rewards)
            {
                LoggerUtils.Log("My View InitToggle Success --> is Rewards");
                SubToggles[1].isOn = true;
                SelectSub(1);
            }
            else if (rcData.grading == (int)RoleResGrading.DC && rcData.origin == (int)RoleOriginType.Airdrop)
            {
                LoggerUtils.Log("My View InitToggle Success --> is Airdrop");
                SubToggles[2].isOn = true;
                SelectSub(2);
            }
        }
    }

    public override void OnSelect()
    {
        SelectSub(curSelectTogIndex);
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
        if (index == 1)
        {
            rewardsView.GetAllRewardsItemList();
            ClearRed(mRWRedDotNode, "rewards");
        }
        if (index == 2)
        {
            airdropView.GetAllAirdropItemList();
            ClearRed(mADRedDotNode, "airdrop");
        }
    }

    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.my, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("rewards"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[1].gameObject, (int)ENodeType.my, (int)ENodeType.rewards, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mRWRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("airdrop"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[2].gameObject, (int)ENodeType.my, (int)ENodeType.airdrop, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mADRedDotNode = vNode;
            }
        }
    }
}