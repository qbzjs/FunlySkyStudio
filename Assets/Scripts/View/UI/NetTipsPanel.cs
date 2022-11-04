using DG.Tweening;
using UnityEngine;
public class NetTipsPanel : BasePanel<NetTipsPanel>
{
    public GameObject wifiImg;
    private Sequence animTweenSeq;

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
    }

    private void Start()
    {
        playAlphaAnim();
    }

    private void playAlphaAnim()
    {
        // Tweener tweener = wifiImg.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.6f);
        // tweener.SetEase(Ease.OutSine);
        // tweener.SetLoops(-1);
        if(animTweenSeq !=null)
        {
            animTweenSeq.Kill();
        }
        animTweenSeq = DOTween.Sequence();
        float scaleTime = 0.5f;
        animTweenSeq.Append(wifiImg.transform.DOScale(new Vector3(2.6f, 2.6f, 2.6f), scaleTime));//延迟
        animTweenSeq.Append(wifiImg.transform.DOScale(new Vector3(2f, 2f, 2f), scaleTime));
        animTweenSeq.SetLoops(-1);
    }


}
