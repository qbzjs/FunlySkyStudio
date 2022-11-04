/// <summary>
/// Author:MeiMei—LiMei
/// Description: UI适配ipad+低端机
/// Date: 2022-02-28
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAutoFit: MonoBehaviour
{
    private float originalWidth = 2436;
    private float originalHeight = 1125;
    public bool isAutioFit = true;
    private RectTransform rectTransform;
    private void Start()
    {
        AudioFit();
    }
    public void AudioFit()
    {
        if (isAutioFit==true)
        {
            if (Screen.orientation == ScreenOrientation.Portrait) 
                return;//竖屏时不做调整
            rectTransform = transform.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            float screenWidth = Screen.currentResolution.width;
            float screenHeight = Screen.currentResolution.height;
            if (screenWidth < screenHeight)
            {
                Debug.LogError("UI AutoFit Error");
                screenWidth = Screen.currentResolution.height;
                screenHeight = Screen.currentResolution.width;
            }
            LoggerUtils.Log("screen:" + screenWidth + "/" + screenHeight);
            float screenRatio = screenWidth / screenHeight;//屏幕宽高比
            float minRatio = 1.78f;//缩放刚好合适时的宽高比=iphone6宽高比
            float zoomScale = screenRatio / minRatio;//缩放比例
            var canvasScaler = GetComponentInParent<CanvasScaler>();
            if (canvasScaler!=null)
            {
                originalWidth = canvasScaler.referenceResolution.x;
                originalHeight = canvasScaler.referenceResolution.y;
            }
            float canvasHeight = originalHeight;
            float canvasWidth = originalHeight *screenRatio;
            if (screenRatio <= minRatio)
            {
                float widthOffset = (canvasWidth - canvasWidth * zoomScale) / 2/zoomScale;//左右贴边值
                float heightOffset = (canvasHeight - canvasHeight*zoomScale) / 2/zoomScale;//上下贴边值 
                rectTransform.localScale = new Vector3(zoomScale, zoomScale, zoomScale);
                rectTransform.offsetMax = new Vector2(widthOffset, heightOffset);
                rectTransform.offsetMin = new Vector2(-widthOffset, -heightOffset);
            }
        }       
    }
}
