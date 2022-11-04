#if false
using System;

public class State
{
    public string Name { get; private set; }
    public Action DoRun { get; private set; }
    public Action RunIn { get; private set; }
    public Action RunOut { get; private set; }
    public bool IsDefault { get; set; }

    public State(string name)
    {
        Name = name;
    }

    public State Run(Action doRun)
    {
        DoRun = doRun;
        return this;
    }

    public State OnRunIn(Action runIn)
    {
        RunIn = runIn;
        return this;
    }

    public State OnRunOut(Action runOut)
    {
        RunOut = runOut;
        return this;
    }

    public State AsDefault(bool isDefault = true)
    {
        IsDefault = isDefault;
        return this;
    }
}
#endif