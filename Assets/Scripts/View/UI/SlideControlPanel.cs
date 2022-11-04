using UnityEngine.UI;
public class SlideControlPanel : BasePanel<SlideControlPanel>
{
    public Button mCancelBtn;
    public Button mStartSlide;
    public PlayerSlidePipeControl mPlayerSlidePipeCtrl;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        mCancelBtn.onClick.AddListener(OnCancel);
        mStartSlide.onClick.AddListener(OnStart);
    }
    public void OnCancel()
    {
        if (mPlayerSlidePipeCtrl != null)
        {
            mPlayerSlidePipeCtrl.OnClickDownSlide();
        }
    }
    public void OnStart()
    {
        if (mPlayerSlidePipeCtrl != null)
        {
            mPlayerSlidePipeCtrl.OnClickStarSlide();
        }
    }
    public void SetrSlidePipeCtrl(PlayerSlidePipeControl ctrl)
    {
        mPlayerSlidePipeCtrl = ctrl;
    }
    public void SetSlideState(ESlidePipeMoveState state)
    {
        switch (state)
        {
            case ESlidePipeMoveState.None:
                break;
            case ESlidePipeMoveState.StartIdle:
                StartBtnIsVisible(true);
                CancelBtnIsVisible(true);
                break;
            case ESlidePipeMoveState.Start:
                StartBtnIsVisible(false);
                CancelBtnIsVisible(false);
                break;
            case ESlidePipeMoveState.Slide:
                StartBtnIsVisible(false);
                CancelBtnIsVisible(false);
                break;
            case ESlidePipeMoveState.EndIdle:
                StartBtnIsVisible(false);
                CancelBtnIsVisible(true);
                break;
            case ESlidePipeMoveState.End:
                break;
            default:
                break;
        }
    }
    public void CancelBtnIsVisible(bool isVisible)
    {
        mCancelBtn.gameObject.SetActive(isVisible);
    }
    public void StartBtnIsVisible(bool isVisible)
    {
        mStartSlide.gameObject.SetActive(isVisible);
    }
}