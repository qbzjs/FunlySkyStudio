using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public struct LayoutGroup
{
    public Component go;
    public float width;
}

public class PurchasedLayout : MonoBehaviour
{
    public float spacing = 0;
    public float magin = 0;
    private SpriteRenderer rootSpriteRenderer;
    private List<LayoutGroup> layoutGroups = new List<LayoutGroup>();

    

    private float GetAllContentWidth()
    {
        float to = 0;
        layoutGroups.Clear();
        foreach (Transform item in transform)
        {
            if (item.GetComponent<SuperTextMesh>() is { } stm)
            {
                var acWidth = stm.preferredWidth;
                layoutGroups.Add(new LayoutGroup()
                {
                    go = stm,
                    width = acWidth
                });
                to += acWidth;
            }
            else if (item.GetComponent<SpriteRenderer>() is { } spriteRenderer)
            {
                var acWidth = spriteRenderer.size.x * item.transform.localScale.x;
                layoutGroups.Add(new LayoutGroup()
                {
                    go = spriteRenderer,
                    width = acWidth
                });
                to += acWidth;
            }

            to += spacing;
        }

        to -= spacing;
        return to;
    }
    
    
    public void Rebuild()
    {
        rootSpriteRenderer = GetComponent<SpriteRenderer>();
        var pos = transform.localPosition;
        var newWidth = GetAllContentWidth();
        
        var start = - newWidth / 2;
        float offset = 0;
        for (int i = 0; i < layoutGroups.Count; ++i)
        {
            var ts = layoutGroups[i].go.gameObject.transform;
            var position = new Vector3(start + offset + layoutGroups[i].width / 2, 0, 0);
            ts.localPosition = position;
            offset += layoutGroups[i].width;
            if (i != layoutGroups.Count - 1)offset += spacing;
        }
        var nWidth = offset + magin;
        var diff = nWidth - rootSpriteRenderer.size.x;
        rootSpriteRenderer.size = new Vector2(nWidth, rootSpriteRenderer.size.y);
        transform.localPosition = new Vector3(pos.x - (diff/2) * transform.localScale.x, pos.y, pos.z);
    }


}
