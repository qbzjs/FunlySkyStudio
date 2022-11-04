using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PurchaseText : MonoBehaviour
{
    public SuperTextMesh source;
    public SuperTextMesh purchaseText;
    public SuperTextMesh goods;
    public SpriteRenderer clothSpriteRenderer;

    public int limitLen;
    public void SetContent(string source, string goods, string coverUrl)
    {
        clothSpriteRenderer.sprite = null;
        source = DataUtils.FilterNonStandardText(source);
        goods = DataUtils.FilterNonStandardText(goods);
        
        LocalizationConManager.Inst.SetSystemTextFont(this.source);
        LocalizationConManager.Inst.SetSystemTextFont(purchaseText);
        LocalizationConManager.Inst.SetSystemTextFont(this.goods);
        
        this.source.text = GetLimitString(source);
        this.goods.text = GetLimitString(goods);
        purchaseText.text = LocalizationConManager.Inst.GetLocalizedText("purchased");
        this.source.Rebuild();
        this.goods.Rebuild();
        purchaseText.Rebuild();
        
        StartCoroutine(ResManager.Inst.GetTexture(coverUrl, (texture) =>
        {
            clothSpriteRenderer.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }, () => {
            LoggerUtils.LogError("");
        }));
        GetComponent<PurchasedLayout>().Rebuild();
    }

    private string GetLimitString(string s)
    {
        if (s.Length > limitLen)
        {
            return s.Substring(0, limitLen) + "...";
        }
        return s;
    }
    

}
