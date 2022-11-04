using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIPortraitAutoFit : MonoBehaviour
{
    private float originalWidth = 1125;
    private float originalHeight = 2436;
    public bool isAutioFit = true;
    private RectTransform rectTransform;
    public UIAutoFitIgnore[] mIgnoreObjs;
    private void Awake()
    {
        AudioFit();
    }
    public void AudioFit()
    {
        if (isAutioFit == true)
        {
            rectTransform = transform.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            float screenWidth = Screen.currentResolution.width;
            float screenHeight = Screen.currentResolution.height;
            LoggerUtils.Log("screen:" + screenWidth + "/" + screenHeight);
            float screenRatio = screenHeight / screenWidth;//屏幕宽高比
            float minRatio = 1.78f;//缩放刚好合适时的宽高比=iphone6宽高比
            float zoomScale = screenRatio / minRatio;//缩放比例
            var canvasScaler = GetComponentInParent<CanvasScaler>();
            if (canvasScaler != null)
            {
                originalWidth = canvasScaler.referenceResolution.x;
                originalHeight = canvasScaler.referenceResolution.y;
            }
            float canvasHeight = originalWidth * screenRatio;
            float canvasWidth = originalWidth;
            if (screenRatio <= minRatio)
            {
                float widthOffset = (canvasWidth - canvasWidth * zoomScale) / 2 / zoomScale;//左右贴边值
                float heightOffset = (canvasHeight - canvasHeight * zoomScale) / 2 / zoomScale;//上下贴边值 
                rectTransform.localScale = new Vector3(zoomScale, zoomScale, zoomScale);
                rectTransform.offsetMax = new Vector2(widthOffset, heightOffset);
                rectTransform.offsetMin = new Vector2(-widthOffset, -heightOffset);

                //处理忽略缩放的GameObject
                UIAutoFitIgnore[] mIgnoreObjs = gameObject.GetComponentsInChildren<UIAutoFitIgnore>(true);
                int len = mIgnoreObjs.Length;
                for (int i = 0; i < len; i++)
                {
                    UIAutoFitIgnore obj = mIgnoreObjs[i];
                    obj.transform.localScale /= zoomScale;
                }
            }
           
        }
    }
}
