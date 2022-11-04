#if false
using System;

public class StateTransition
{
    public string FromState { get; private set; }
    public string ToState { get; private set; }

    public Func<State, bool> Condition = null;

    public StateTransition From(string state)
    {
        FromState = state;
        return this;
    }

    public StateTransition To(string state)
    {
        ToState = state;
        return this;
    }

    public StateTransition When(Func<State, bool> condition)
    {
        Condition = condition;
        return this;
    }
}
#endif