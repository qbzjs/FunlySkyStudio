using AvatarRedDotSystem;
using DG.Tweening;
using RedDot;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RoleClassifyItem : MonoBehaviour
{
    public Image ClassifyIcon;
    public Image newImage;
    private string spritename;
    public SpriteAtlas Atlas;
    public Button ClassifyBtn;
    
    private Action<ClassifyType> selectClick;
    private ViewType viewType;
    public int classifyIndex;
    public ClassifyType classifyType;
    public BodyPartType bodyPartType;
    //水平方向边界：
    private float leftX=-4.0f;
    private float rightX=4.0f;
    public VNode mVNode;

    // Start is called before the first frame update
    void Start()
    {
        initAtlas();
        ClassifyBtn.onClick.AddListener(OnSelectClick);
    }

    void initAtlas()
    {
        Atlas = Atlas == null ? ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/ClassifyIcon") : Atlas;
    }

    public void SetClassifyName(int index, string spritename, Action<ClassifyType> act, ViewType viewType, ClassifyType classifyType, BodyPartType partType)
    {
        classifyIndex = index;
        this.spritename = spritename;
        initAtlas();
        ClassifyIcon.sprite = Atlas.GetSprite(spritename);
        selectClick = act;
        this.viewType = viewType;
        this.classifyType = classifyType;
        bodyPartType = partType;
        newImage.gameObject.SetActive(false);
    }

    public void OnSelectClick()
    {
        var oItem = RoleClassifiyView.Ins.curSelectItem;
        initAtlas();
        if (oItem)
        {
            //还原上次选中图标
            oItem.ClassifyIcon.sprite = Atlas.GetSprite(oItem.spritename);
        }
        ClassifyIcon.sprite = Atlas.GetSprite(this.spritename+"_select");
        RoleClassifiyView.Ins.curSelectItem = this;
        AutoSlide();
        selectClick?.Invoke(classifyType);
        RoleMenuController.Ins.SetCameraZoom(viewType);
        ClearItemsNew();
        if (mVNode!=null&&mVNode.mLogic!=null&&mVNode.mLogic.Count>0)
        {
            int old= mVNode.mLogic.Count;
            mVNode.mLogic.ChangeCount(old-1);
            ClearRed();
        }
    }
    private void ClearItemsNew()
    {
        newImage.gameObject.SetActive(false);
        var otherItems = RoleClassifiyView.Ins.roleClassifyItems.FindAll(x => x.bodyPartType != bodyPartType && x.classifyType == classifyType);
        foreach (var item in otherItems)
        {
            item.newImage.gameObject.SetActive(false);
        }
    }
    public void ClearRed()
    {
        RoleMenuView.Ins.mAvatarRedDotManager.ReqCleanRedDot(classifyType);
      
    }
    private void AutoSlide()
    {
        ScrollRect scrollRect = transform.parent.parent.parent.GetComponent<ScrollRect>();
        float oneItemProportion = (transform.GetComponent<RectTransform>().sizeDelta.x + scrollRect.content.GetComponent<HorizontalLayoutGroup>().spacing) / (scrollRect.content.rect.xMax - scrollRect.viewport.rect.size.x);
        if (transform.position.x > rightX && scrollRect.horizontalNormalizedPosition < 1 && scrollRect.content.childCount >= 6)
        {
            DOTween.To(() => scrollRect.horizontalNormalizedPosition,
                lerpValue => scrollRect.horizontalNormalizedPosition = lerpValue,
                scrollRect.horizontalNormalizedPosition + oneItemProportion, 0.5f).SetEase(Ease.OutQuint);
        }
        if (transform.position.x < leftX && scrollRect.horizontalNormalizedPosition > 0 && scrollRect.content.childCount >= 6)
        {
            DOTween.To(() => scrollRect.horizontalNormalizedPosition,
                lerpValue => scrollRect.horizontalNormalizedPosition = lerpValue,
                scrollRect.horizontalNormalizedPosition - oneItemProportion, 0.5f).SetEase(Ease.OutQuint);
        }
    }
    public void AttachRedDot(RedDotTree tree)
    {
        ENodeType nodeType;
        if (RoleClassifiyView.Ins.mClassifyType2TreeNodeTypeMapping.TryGetValue(classifyType,out nodeType))
        {
            if (bodyPartType == BodyPartType.body)
            {
                mVNode= tree.AddRedDot(gameObject, (int)ENodeType.Body, (int)nodeType, ERedDotPrefabType.Type3);
            }
            else if (bodyPartType == BodyPartType.face)
            {
                mVNode= tree.AddRedDot(gameObject, (int)ENodeType.Face, (int)nodeType, ERedDotPrefabType.Type3);
            }
            if (mVNode!=null&&mVNode.mLogic!=null)
            {
                mVNode.mLogic.ChangeCount(1);
            }
        }
    }
}
