using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixUiSizeToParent : MonoBehaviour
{
    public bool isFillWidth = false;
    public bool isFillHeight = false;
    
    void Start()
    {
       FitWidth();
    }

   private void OnEnable() 
   {
        FitWidth();
   }

   private void FitWidth()
   {    
        if(this.transform.parent == null)
        {
            return;
        }

        RectTransform rtf = this.gameObject.GetComponent<RectTransform>();
        RectTransform parentRtf = this.transform.parent.GetComponent<RectTransform>();
        
        if(rtf == null || parentRtf == null)
        {
            return;
        }

        float width = rtf.rect.width;
        float height = rtf.rect.height;

        float parentWidth = parentRtf.rect.width;
        float parentHeight = parentRtf.rect.height;

        // rectTransform.rect.width = parentRectTransform.rect.width;
        if(isFillWidth)
        {
            width = parentWidth;
        }

        if(isFillHeight)
        {
            height = parentHeight;
        }
        rtf.sizeDelta = new Vector2(width, height);

        LoggerUtils.Log("####当前节点宽度："+ rtf.rect.width);
   }
}
