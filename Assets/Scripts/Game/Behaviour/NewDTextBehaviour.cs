/// <summary>
/// Author:LiShuZhan
/// Description:新版3d文字本体逻辑
/// Date: 2022-6-2 17:44:22
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using HLODSystem;
using Assets.Scripts.Game.Core;

[SerializeField]
public class NewDTextData
{
    public string tex;
    public string textcol;
}
/// <summary>
/// 修改此方法时要考虑旧版文字DTextBehaviour
/// </summary>
public class NewDTextBehaviour : BaseHLODBehaviour
{
    private SuperTextMesh textPro;
    private GameObject bgImg;
    private BoxCollider textCollider;
    private SpriteRenderer selectSprite;
    private float maxWidth = 20f;
    private float maxHeight = 2f;
    public override void SetUp()
    {
        textPro = assetObj.transform.GetChild(0).GetComponent<SuperTextMesh>();
        selectSprite = assetObj.transform.GetChild(1).GetComponent<SpriteRenderer>();
        textCollider = textPro.GetComponent<BoxCollider>();
        bgImg = assetObj.transform.GetChild(1).gameObject;
        var mComp = this.entity.Get<NewDTextComponent>();
        SetText(mComp.content, mComp.col);
    }

    public void SetText(string con, Color col)
    {
        SetContent(con);
        textPro.color = col;
    }

    public void SetContent(string content)
    {
        textPro.text = content;
        textPro.alignment = SuperTextMesh.Alignment.Center;
        var width = textPro.preferredWidth;
        var height = textPro.preferredHeight;
        if (width > maxWidth || height> maxHeight)
        {
            textPro.alignment = SuperTextMesh.Alignment.Left;
        }
        textPro.Rebuild();
        width = Math.Min(width, maxWidth);
        textCollider.size = new Vector3(width, textPro.preferredHeight,0);
        selectSprite.size = new Vector3(width / 3, textPro.preferredHeight/3, 0);
    }

    public void SetColor(Color color)
    {
        textPro.color = color;
        textPro.Rebuild();
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        selectSprite.gameObject.SetActive(isHigh);
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        SetInteractBtnOnClickByIconName();
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    public void SetInteractBtnOnClickByIconName()
    {
        var playIconEnum = PortalPlayPanel.IconName.None;
        if (entity.HasComponent<EdibilityComponent>())
        {
            playIconEnum = PortalPlayPanel.IconName.Eat;
        }
        switch (playIconEnum)
        {
            case PortalPlayPanel.IconName.Eat:
                SetInteractBtnOnClickByIconName(playIconEnum, () => { EdibilitySystemController.Inst.OnSceneNodeFoodBtnClick(this); });
                break;
        }
    }

    public void SetInteractBtnOnClickByIconName(PortalPlayPanel.IconName iconName, Action onClick)
    {

        if (PortalPlayPanel.Instance == null || !PortalPlayPanel.Instance.gameObject.activeSelf)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(iconName);
            PortalPlayPanel.Instance.SetTransform(this.transform);
            PortalPlayPanel.Instance.AddButtonClick(() => { onClick?.Invoke(); });
        }
        else
        {
            PortalPlayPanel.Instance.AddExtraIcon(iconName, onClick);
        }
    }
}