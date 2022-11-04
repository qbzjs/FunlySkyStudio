using System.Collections;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
public class FreezePanel : BasePanel<FreezePanel>
{
    public GameObject mFreezeMask;
    public GameObject mCDRoot;
    public Text mCDText;
    public Image mCDImg;
    public bool mStartCD = false;
    public float mMaxCD = 0;
    public float mCurCD = 0;
    public StringBuilder mStringBuilder=new StringBuilder(64);
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        transform.SetAsFirstSibling();
        mCDRoot.SetActive(false);
        SetTps(PlayerBaseControl.Inst.IsTps);
    }
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        StopAllCoroutines();
        mCDRoot.SetActive(false);
    }
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        PlayerBaseControl.Inst.AddPlayerStateChangedObserver(PlayerStateChanged);
        SetTps(PlayerBaseControl.Inst.IsTps);
    }
    public void PlayerStateChanged(IPlayerCtrlMgr playerController)
    {
        //如果是第三人称则显示头顶加好友，如果是第一人称则不显示AddFriend图标s
        PlayerBaseControl player = playerController as PlayerBaseControl;
        SetTps(player.IsTps);
    }

    public void SetTps(bool isTps)
{
        mFreezeMask.SetActive(!isTps);
    }
    public void OnTimerTick(float time)
{
        mStartCD = true;
        mMaxCD = time;
        mCurCD = time; 
        mCDRoot.SetActive(true);
        SetTps(PlayerBaseControl.Inst.IsTps);
        UpdateButtonCdText(mMaxCD, mMaxCD,Mathf.CeilToInt(mMaxCD).ToString());
    }

    public void Update()
    {
        if (mStartCD)
        {
            mCurCD-= Time.deltaTime;
            if (mCurCD<=0)
            {
                mCurCD = 0;
                mStartCD = false;
            }
            mStringBuilder.Append($"{Mathf.CeilToInt(mCurCD)}s");
            UpdateButtonCdText(mCurCD,mMaxCD,mStringBuilder.ToString());
            mStringBuilder.Length = 0;
        }
    }
    public void UpdateButtonCdText(float cd, float maxCd, string str)
    {
        SetCDImgFillAmount(maxCd == 0 ? 0 : cd / maxCd);
        if (!mCDText)
            return;
        mCDText.text = str;
    }
    public void SetCDImgFillAmount(float fillAmount)
    {
        if (mCDImg!=null)
        {
            if (mCDImg)
                mCDImg.fillAmount = fillAmount;
        }
    }
    public void OnDestroy()
    {
        StopAllCoroutines();
        if (PlayerBaseControl.Inst!=null)
        {
            PlayerBaseControl.Inst.RemovePlayerStateChangedObserver(PlayerStateChanged);
        }
    }
}
