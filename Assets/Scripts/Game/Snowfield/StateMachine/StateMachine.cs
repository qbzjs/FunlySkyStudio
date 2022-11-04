#if false
using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachine
{
    public string Name { get; private set; }

    private Dictionary<string, State> _stateDic = new Dictionary<string, State>();
    private List<StateTransition> _transLst = new List<StateTransition>();
    private Dictionary<string, List<StateTransition>> _transDic = new Dictionary<string, List<StateTransition>>();

    private bool _running = false;
    private string _startState = null;
    private string _currentStateName = null;
    private State _currentState { get { return _stateDic[_currentStateName]; } }

    public StateMachine(string name)
    {
        Name = name;
    }

    public State NewState(string name)
    {
        var state = new State(name);
        _stateDic[name] = state;
        return state;
    }

    public StateTransition Trans()
    {
        var st = new StateTransition();
        _transLst.Add(st);
        return st;
    }

    public void Start()
    {
        Prepare();
        _running = true;
    }

    public void Destroy()
    {
        _running = false;
        _transDic.Clear();
        _currentStateName = null;
    }

    public void Pause()
    {
        _running = false;
    }

    public void Resume()
    {
        _running = true;
    }

    public void Run()
    {
        if (! _running)
            return;

        if (_currentStateName == null)
            _currentStateName = _startState;

        CheckTransition();
        _currentState.DoRun();
    }

    private void Prepare()
    {
        _transDic.Clear();

        foreach (var st in _transLst)
        {
            if (st.FromState == null || st.ToState == null || ! _stateDic.ContainsKey(st.FromState) || ! _stateDic.ContainsKey(st.ToState))
                throw new Exception(String.Format("Invalid state transition: {0} => {1}", st.FromState != null ? st.FromState : "null", st.ToState != null ? st.ToState : "null"));

            List<StateTransition> lst;
            if (! _transDic.TryGetValue(st.FromState, out lst))
            {
                lst = new List<StateTransition>();
                _transDic.Add(st.FromState, lst);
            }

            lst.Add(st);
        }

        foreach (var s in _stateDic.Values.ToArray())
        {
            if (s.IsDefault)
            {
                if (_startState != null)
                    _stateDic[_startState].AsDefault(false);

                _startState = s.Name;
            }
        }

        if (_startState == null)
            throw new Exception("StartState is null since it's not set or the StateMachine has been destroyed.");
    }

    private bool CheckTransition()
    {
        if (! _running)
            return false;

        StateTransition tToWork = null;
        foreach (var t in _transDic[_currentStateName])
        {
            if (t.Condition != null && t.Condition(_currentState))
            {
                tToWork = t;
            }
        }

        if (tToWork == null)
            return false;

        _currentState.RunOut?.Invoke();
        _currentStateName = tToWork.ToState;
        _currentState.RunIn?.Invoke();

        return true;
    }
}
#endif