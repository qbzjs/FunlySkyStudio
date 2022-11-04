using System.Collections.Generic;

public class SlideStateMachine
{
    private Dictionary<ESlidePipeMoveState, IState> mRegistedState = new Dictionary<ESlidePipeMoveState, IState>();
    // 状态堆栈
    private Stack<IState> mStateStack = new Stack<IState>();
    private IState mTargetState;
    public void RegisterState(ESlidePipeMoveState name, IState state)
    {
        if (mRegistedState.ContainsKey(name))
        {
            return;
        }

        mRegistedState.Add(name, state);
    }
    public IState ChangeState(ESlidePipeMoveState name)
    {
        IState state;
        if (!mRegistedState.TryGetValue(name, out state))
        {
            return default(IState);
        }

        return ChangeState(state);
    }
    public IState ChangeState(IState state)
    {
        if (state == null)
        {
            return default(IState);
        }

        mTargetState = state;

        IState oldState = default(IState);
        if (mStateStack.Count > 0)
        {
            oldState = mStateStack.Pop();
            oldState.OnStateLeave();
        }

        mStateStack.Push(state);
        state.OnStateEnter();

        return state;
    }
}
