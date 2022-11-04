using System.Collections.Generic;
using UnityEngine;
public class SlidePipeMovementCompt
{
    
    public abstract class SlideMovementState : BaseState
    {
        public SlidePipeMovementCompt mParent;
        public string mClipName = "";
        public string mFaceClipName = "";
        public float mLength = 0;
        public ESlidePipeMoveState mMoveState;
        public SlideMovementState(SlidePipeMovementCompt parent)
        {
            mParent = parent;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            if (SlideControlPanel.Instance)
            {
                SlideControlPanel.Instance.SetSlideState(mMoveState);
            }
            mParent.mSlidePipeControl.playerBase.animCon.PlaySlideAnim(mFaceClipName);
            mParent.mSlidePipeControl.playerBase.animCon.playerAnim.CrossFade(mClipName, 0.15f, 0);
        }
        public override void OnStateLeave()
        {
            base.OnStateLeave();
            mParent.mSlidePipeControl.playerBase.animCon.ReleaseAndCancelLastEmo();
        }
    }
    public class NoneState : SlideMovementState
    {
        public NoneState(SlidePipeMovementCompt parent) : base(parent)
        {
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
        public StartIdleState(SlidePipeMovementCompt parent) :
           base(parent)
        {
            mClipName = "SlidePipe.huati_idle";
            mFaceClipName = "SlidePipe.huati_idle";
            mMoveState = ESlidePipeMoveState.StartIdle;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            // mParent.mSlidePipeControl.playerBase.animCon.PlaySlideAnim("SlidePipe.huati_idle");
        }
    }
    public class StartState : SlideMovementState
    {
        public float mTime = 0;
        public StartState(SlidePipeMovementCompt parent) :
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
            mParent.mGhostObj = new GameObject("GhostObj");
            mParent.mGhostObj.transform.position = mParent.mTargetPosition;
            mParent.mGhostObj.transform.rotation = mParent.mTargetRotation;
            mParent.mCurrentRotation = mParent.mTargetRotation;
            mParent.mSlidePipeControl.playerBase.SetPosToNewPointWithoutIniPos(mParent.mGhostObj.transform.position, mParent.mGhostObj.transform.rotation);
            //mParent.mPlayerControl.Character.enabled = false;
            AKSoundManager.Inst.PostEvent("Play_Slideway_Lie_Down", mParent.mSlidePipeControl.playerBase.gameObject);
        }
    }
    public class SlideState : SlideMovementState
    {
        public SlideState(SlidePipeMovementCompt parent) :
          base(parent)
        {
            mClipName = "SlidePipe.huati_centre";
            mFaceClipName = "SlidePipe.huati_centre";
            mMoveState = ESlidePipeMoveState.Slide;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            AKSoundManager.Inst.PostEvent("Play_Slideway_Slide_Loop_1P", mParent.mSlidePipeControl.playerBase.gameObject);
            AKSoundManager.Inst.PostEvent("Play_Slideway_Voice_Scream", mParent.mSlidePipeControl.playerBase.gameObject);
        }
        public override void OnStateLeave()
        {
            base.OnStateLeave();
            AKSoundManager.Inst.PostEvent("Stop_Slideway_Slide_Loop_1P", mParent.mSlidePipeControl.playerBase.gameObject);
        }
    }
    public class EndState : SlideMovementState
    {
        public float mTime = 0;
        public EndState(SlidePipeMovementCompt parent) :
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
            AKSoundManager.Inst.PostEvent("Play_Slideway_Jump_Up", mParent.mSlidePipeControl.playerBase.gameObject);
        }
    }
    public class EndIdleState : SlideMovementState
    {
        public EndIdleState(SlidePipeMovementCompt parent) :
          base(parent)
        {
            mClipName = "SlidePipe.huati_idle";
            mFaceClipName = "SlidePipe.huati_idle";
            mMoveState = ESlidePipeMoveState.EndIdle;
        }
        public override void OnStateEnter()
        {
            base.OnStateEnter();
        }
    }
    
    
    public float mSpeed = 6;
    public PlayerSlidePipeControl mSlidePipeControl;
    public SlidePipeWaypoint[] mMyWaypoints;
    public SlidePipeWaypoint mCurWaypoint;
    public Vector3 mTargetPosition;
    public Quaternion mTargetRotation;
    public bool mIsExcuteMoving = false;
    public bool mIsStateFinished = false;
    public bool mIsNegDir = false;
    public IState mMoveState = null;
    public SlideStateMachine mMoveMode;
    public GameObject mGhostObj;

    public SlidePipeMovementCompt(PlayerSlidePipeControl slidePipeCtrl)
    {
        mSlidePipeControl = slidePipeCtrl;
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
        mIsStateFinished = false;
    }
    public SlidePipeWaypoint[] WayPoints
    {
        get { return mMyWaypoints; }
    }
    public SlidePipeWaypoint StartPoint
    {
        get { return WayPoints != null && WayPoints.Length > 0 ? WayPoints[0] : null; }
    }
    public SlidePipeWaypoint EndPoint
    {
        get { return WayPoints != null && WayPoints.Length > 0 ? WayPoints[WayPoints.Length - 1] : null; }
    }

    public void Update(float deltaTime)
    {
        if (mIsStateFinished) return;
        if (mMoveState != null)
        {
            if (mMoveState is StartState)
            {
                StartState state = mMoveState as StartState;
                state.mTime += deltaTime;
                if (state.mTime >= state.mLength)
                {
                    GotoState(ESlidePipeMoveState.StartIdle);
                }
            }
            if (mMoveState is EndState)
            {
                EndState state = mMoveState as EndState;
                state.mTime += deltaTime;
                if (state.mTime >= state.mLength)
                {
                    //结束
                    OnMoveStateFinished();
                }
            }
        }

        if (mIsExcuteMoving)
        {
            MoveToTarget(deltaTime);
            mSlidePipeControl.playerBase.SetPosToNewPointWithoutIniPos(mGhostObj.transform.position, mGhostObj.transform.rotation);
        }
    }

    private float mRotateInterval = 0;
    private Quaternion mCurrentRotation = Quaternion.identity;
    public void MoveToTarget(float deltaTime)
    {
        Vector3 curLoc = mGhostObj.transform.position;
        Vector3 newPos =MoveTowards(curLoc, mTargetPosition, mSpeed * deltaTime);
        Move(newPos-curLoc);
        mRotateInterval += deltaTime*mSpeed;
        mRotateInterval = Mathf.Clamp01(mRotateInterval);
        mGhostObj.transform.rotation = Quaternion.Lerp(mCurrentRotation, mTargetRotation, mRotateInterval);
        if (IsOnTarget())
        {
            SlidePipeWaypoint nextPoint = GetNextPoint(mCurWaypoint, mIsNegDir);
            if (nextPoint != null)
            {
                mCurWaypoint = nextPoint;
                mCurrentRotation = mGhostObj.transform.rotation;
                SetMoveParam(mCurWaypoint.mPosition, mCurWaypoint.mRotation,mCurWaypoint.mSpeed);
            }
            else
            {
                mIsExcuteMoving = false;
                StopMove();
            }
        }
    }
    public Vector3 MoveTowards(Vector3 from, Vector3 to, float dt)
    {
        if ((to - from).sqrMagnitude <= dt * dt)
        {
            return to;
        }
        else
        {
            Vector3 dir = to - from;
            return from + dir.normalized * dt;
        }
    }
    public virtual void Move(Vector3 delta)
    {
        mGhostObj.transform.position = mGhostObj.transform.position + delta;
    }
    public bool IsOnTarget()
    {
        Vector3 dist = mGhostObj.transform.position - mTargetPosition;
        if (dist.magnitude <= 0.0001f)
            return true;
        return false;
    }
    public bool ShouldStop()
    {
        Vector3 dist = mGhostObj.transform.position - mTargetPosition;
        if (dist == Vector3.zero)
            return true;
        return false;
    }
    public void ExcuteMove()
    {
        mIsExcuteMoving = true;
        //切换到滑动状态
        GotoState(ESlidePipeMoveState.Slide);
    }
    public void SetMoveParam(Vector3 inVector, Quaternion rot,float speed)
    {
        mTargetPosition = inVector;
        mTargetRotation = rot;
        mSpeed = speed;
        mRotateInterval = 0;
    }
    public void StopMove()
    {
        mSlidePipeControl.ReqSlideEnd();
        GotoState(ESlidePipeMoveState.EndIdle);
    }
    public void SetMoveWaypoint(List<SlidePipeWaypoint> waypoints,bool isNegDir)
    {
        mIsNegDir = isNegDir;
        mMyWaypoints = waypoints.ToArray();
        if (mMyWaypoints.Length > 0)
        {
            int startIndex = 0;
            if (mIsNegDir)
            {
                startIndex = mMyWaypoints.Length - 1;
            }
            mCurWaypoint = mMyWaypoints[startIndex];
            SetMoveParam(mCurWaypoint.mPosition, mCurWaypoint.mRotation,mCurWaypoint.mSpeed);
        }
    }
    public void PuaseMove()
    {

    }
    public SlidePipeWaypoint GetNextPoint(SlidePipeWaypoint inPoint, bool isNegDir)
    {
        if (inPoint == null)
        {
            return null;
        }
        int nextIndex = 0;
        if (isNegDir)
        {
            if (inPoint.mIndex > 0)
            {
                nextIndex = inPoint.mIndex - 1;
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (inPoint.mIndex < WayPoints.Length - 1)
            {
                nextIndex = inPoint.mIndex + 1;
            }
            else
            {
                return null;
            }
        }
        return WayPoints[nextIndex];
    }
    public void GotoState(ESlidePipeMoveState state)
    {
        mMoveState = mMoveMode.ChangeState(state);
    }
    public void OnMoveStateFinished()
    {
        mIsStateFinished = true;
        mSlidePipeControl.OnDownSlidePipeCompleted();

    }
    public void ForceAbort()
    {
        mIsStateFinished = true;
        GotoState(ESlidePipeMoveState.None);
    }
    public  void OnChangeTps()
    {
        if (mMoveState is SlideMovementState)
        {
            SlideMovementState state = (SlideMovementState)mMoveState;
            mSlidePipeControl.playerBase.SetPosToNewPointWithoutIniPos(mGhostObj.transform.position,mGhostObj.transform.rotation);
            mSlidePipeControl.playerBase.animCon.playerAnim.Play(state.mClipName, 0,1.0f);
        }
    }
    public void Exit()
    {
        if (mGhostObj != null)
        {
            GameObject.Destroy(mGhostObj);
            mGhostObj = null;
        }
    }
}
