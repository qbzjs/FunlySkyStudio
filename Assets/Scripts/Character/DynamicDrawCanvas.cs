using System.Collections;
using System.Collections.Generic;
using Amazon.S3.Model;
using UnityEngine;
using UnityEngine.UI;

public class DynamicDrawCanvas : MonoBehaviour
{
    public RawImage mRawImage;
    public Camera mDrawCamera;
    public Transform mDrawPanel;
    public void SetTargetTexture(RectTransform mDrawBoard, CanvasScaler mMainCanvas,RenderTexture rt)
    {
        var realHeight = mMainCanvas.referenceResolution.x * Screen.currentResolution.height / Screen.currentResolution.width;
        var ratio = (float) mDrawBoard.rect.height /realHeight;
        mDrawCamera.orthographicSize *= ratio;
        mDrawCamera.targetTexture = rt;
        mDrawCamera.Render();
        var scaleRatio = (float)mDrawCamera.targetTexture.height / mDrawBoard.rect.height;
        mDrawPanel.localScale = new Vector3(scaleRatio, scaleRatio, 1);
    }

    public void SetRawImage(RenderTexture rt)
    {
        mRawImage.texture = rt;
    }
    
}
