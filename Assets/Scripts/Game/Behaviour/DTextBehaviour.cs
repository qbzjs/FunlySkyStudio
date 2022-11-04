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
public class DTextData
{
    public string tex;
    public string textcol;
}
public class DTextBehaviour : BaseHLODBehaviour
{
    private TextMeshPro textPro;
    private GameObject bgImg;
    private BoxCollider textCollider;
    private SpriteRenderer selectSprite;
    private float maxWidth = 5.7f;
    private float maxHeight = 0.45f;
    public override void SetUp()
    {
        textPro = assetObj.transform.GetChild(0).GetComponent<TextMeshPro>();
        selectSprite = assetObj.transform.GetChild(1).GetComponent<SpriteRenderer>();
        textCollider = textPro.GetComponent<BoxCollider>();
        bgImg = assetObj.transform.GetChild(1).gameObject;
        AddDTextAttribute();
    }

    private void AddDTextAttribute()
    {
        var dComp = entity.Get<DTextComponent>();
        SetText(dComp.content, dComp.col);
    }

    public void SetText(string con, Color col)
    {
        SetContent(con);
        textPro.color = col;
    }

    public void SetContent(string content)
    {
        textPro.text = content;
        textPro.alignment = TextAlignmentOptions.Center;
        var width = textPro.preferredWidth;
        var height = textPro.preferredHeight;
        if (width > maxWidth || height> maxHeight)
        {
            textPro.alignment = TextAlignmentOptions.Left;
        }
        width = Math.Min(width, maxWidth);
        textCollider.size = new Vector3(width, textPro.preferredHeight,0);
        selectSprite.size = new Vector3(width, textPro.preferredHeight, 0);
    }

    public void SetColor(Color color)
    {
        textPro.color = color;
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        selectSprite.gameObject.SetActive(isHigh);
    }
}