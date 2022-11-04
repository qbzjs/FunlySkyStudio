using System;

public class ObjectValue<T>
{
    private Action<T> onValueChange;
    private T cur;
    public T Value
    {
        set
        {
            if (!value.Equals(cur))
            {
                onValueChange?.Invoke(value);
                cur = value;
            }
        }
        get { return cur; }
    }

    public ObjectValue(T val)
    {
        cur = val;
    }
    public void Bind(Action<T> onChange)
    {
        onValueChange = onChange;
    }
}