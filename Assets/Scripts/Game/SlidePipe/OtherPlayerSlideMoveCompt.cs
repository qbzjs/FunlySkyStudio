using UnityEngine;
using System;
using System.Collections;

public class OtherPlayerSlideMoveCompt
{
    public abstract class SlideMovementState : BaseState
    {
        public OtherPlayerSlideMoveCompt mParent;
        public string mClipName = "";
        public string mFaceClipName = "";
        public float mLength = 0;
        public ESlidePipeMoveState mMoveState;
        public SlideMovementState(OtherPlayerSlideMoveCompt parent)
        {
            mParent = parent;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            mParent.mPlayerCtrl.animCon.PlaySlideAnim(mFaceClipName);
            mParent.mPlayerCtrl.animCon.playerAnim.CrossFade(mClipName, 0.15f, 0);
        }
        public override void OnStateLeave()
        {
            base.OnStateLeave();
            mParent.mPlayerCtrl.animCon.ReleaseAndCancelLastEmo();
        }
    }
    public class NoneState: SlideMovementState
    {
        public NoneState(OtherPlayerSlideMoveCompt parent) :
          base(parent)
        {
            mMoveState = ESlidePipeMoveState.None;
        }
        public override void OnStateEnter()
        {
        }
        public override void OnStateLeave()
        {
        }
    }
    public class StartIdleState : SlideMovementState
    {
        public StartIdleState(OtherPlayerSlideMoveCompt parent) :
           base(parent)
        {
            mClipName = "SlidePipe.huati_idle";
            mFaceClipName = "SlidePipe.huati_idle";
            mMoveState = ESlidePipeMoveState.StartIdle;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
        }
    }
    public class StartState : SlideMovementState
    {
        public float mTime = 0;
        public StartState(OtherPlayerSlideMoveCompt parent) :
           base(parent)
        {
            mClipName = "SlidePipe.huati_start";
            mFaceClipName = "SlidePipe.huati_start";
            mMoveState = ESlidePipeMoveState.Start;
            mLength = 0.667f;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            mTime = 0;
            AKSoundManager.Inst.PostEvent("Play_Slideway_Lie_Down", mParent.mPlayerCtrl.gameObject);
        }
    }
    public class SlideState : SlideMovementState
    {
        public SlideState(OtherPlayerSlideMoveCompt parent) :
          base(parent)
        {
            mClipName = "SlidePipe.huati_centre";
            mFaceClipName = "SlidePipe.huati_centre";
            mMoveState = ESlidePipeMoveState.Slide;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            AKSoundManager.Inst.PostEvent("Play_Slideway_Slide_Loop_3P", mParent.mPlayerCtrl.gameObject);
            AKSoundManager.Inst.PostEvent("Play_Slideway_Voice_Scream", mParent.mPlayerCtrl.gameObject);
        }
        public override void OnStateLeave()
        {
            base.OnStateLeave();
            AKSoundManager.Inst.PostEvent("Stop_Slideway_Slide_Loop_3P", mParent.mPlayerCtrl.gameObject);
        }
    }
    public class EndState : SlideMovementState
    {
        public float mTime = 0;
        public EndState(OtherPlayerSlideMoveCompt parent) :
          base(parent)
        {
            mClipName = "SlidePipe.huati_end";
            mFaceClipName = "SlidePipe.huati_end";
            mMoveState = ESlidePipeMoveState.End;
            mLength = 1.267f;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            AKSoundManager.Inst.PostEvent("Play_Slideway_Jump_Up", mParent.mPlayerCtrl.gameObject);
        }
    }
    public class EndIdleState : SlideMovementState
    {
        public EndIdleState(OtherPlayerSlideMoveCompt parent) :
          base(parent)
        {
            mClipName = "SlidePipe.huati_idle";
            mMoveState = ESlidePipeMoveState.EndIdle;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
        }
    }
    public SlideStateMachine mMoveMode;
    public OtherPlayerCtr mPlayerCtrl;
    public IState mMoveState = null;
    public bool mIsStateFinished = false;
    private BudTimer mGotoStartTimer = null;
    private BudTimer mGotoEndTimer = null;
    public OtherPlayerSlideMoveCompt(OtherPlayerCtr playerCtrl)
    {
        mPlayerCtrl = playerCtrl;
        mMoveMode = new SlideStateMachine();
    }
    public void Init()
    {
        mMoveMode = new SlideStateMachine();
        mMoveMode.RegisterState(ESlidePipeMoveState.None, new NoneState(this));
        mMoveMode.RegisterState(ESlidePipeMoveState.StartIdle, new StartIdleState(this));
        mMoveMode.RegisterState(ESlidePipeMoveState.Slide, new SlideState(this));
        mMoveMode.RegisterState(ESlidePipeMoveState.Start, new StartState(this));
        mMoveMode.RegisterState(ESlidePipeMoveState.EndIdle, new EndIdleState(this));
        mMoveMode.RegisterState(ESlidePipeMoveState.End, new EndState(this));
        GotoState(ESlidePipeMoveState.None);
    }
    public void GotoState(ESlidePipeMoveState state)
    {
        mMoveState = mMoveMode.ChangeState(state);
    }
    
    public void GotoStart()
    {
        GotoState(ESlidePipeMoveState.Start);
        if (mGotoStartTimer!=null)
        {
            TimerManager.Inst.Stop(mGotoStartTimer);
            mGotoStartTimer = null;
        }
        if (mMoveState is StartState)
        {
            mGotoStartTimer=TimerManager.Inst.RunOnce("GotoStart", (mMoveState as StartState).mLength,
                () => { 
                    GotoState(ESlidePipeMoveState.StartIdle); 
                });
        }
        
    }
    public void GotoEnd()
    {
        GotoState(ESlidePipeMoveState.End);
        if (mGotoStartTimer != null)
        {
            TimerManager.Inst.Stop(mGotoEndTimer);
            mGotoEndTimer = null;
        }
        if (mMoveState is EndState)
        {
            mGotoEndTimer= TimerManager.Inst.RunOnce("GotoEnd", (mMoveState as EndState).mLength,
               () => {
                   OnMoveStateFinished(); 
               });
        }
    }
    public void OnMoveStateFinished()
    {
        GotoState(ESlidePipeMoveState.None);
        mIsStateFinished = true;
        mPlayerCtrl.animCon.playerAnim.CrossFade("idle", 0.15f, 0);
        DestroyTimer();
    }
    public bool IsOnSlide()
    {
        return (mMoveState!=null&&!(mMoveState is NoneState));
    }
    public void DestroyTimer()
    {
        if (mGotoStartTimer != null)
        {
            TimerManager.Inst.Stop(mGotoStartTimer);
            mGotoStartTimer = null;
        }
        if (mGotoStartTimer != null)
        {
            TimerManager.Inst.Stop(mGotoEndTimer);
            mGotoEndTimer = null;
        }
    }
    public void Destroy()
    {
        DestroyTimer();
    }
}
