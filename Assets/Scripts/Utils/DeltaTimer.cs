using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaTimer
{
    public float TimeElapsed { get; private set; }
    public float TimeToRing { get; private set; }

    private int digit; //保留几位小数，校准浮点精度引起的误差
    public DeltaTimer()
    {
        TimeElapsed = 0;
        TimeToRing = 0;
        digit = 4; //默认保留4位
    }

    public DeltaTimer(float time)
    {
        TimeElapsed = 0;
        TimeToRing = time;
    }

    public void Elapse(float time)
    {
        TimeElapsed += time;
    }

    public bool Ring()
    {
        return KeepDigit(TimeElapsed, digit) >= KeepDigit(TimeToRing, digit);
    }

    public void Reset()
    {
        TimeElapsed = 0;
    }

    public void ResetTimeToRing(float timeToRing)
    {
        TimeToRing = timeToRing;
    }

    public void SetFloatAccuracy(int d)
    {
        if(d >= 0)
        {
            digit = d;
        }
    }

    private float KeepDigit(float value, int d)
    {
        int mult = Mathf.RoundToInt(Mathf.Pow(10, d));
        return Mathf.Round(value * mult) / mult;
    }
}
