using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Library<T> : ScriptableObject
{
    [SerializeField]
    protected List<T> source;

    public T Get(int id)
    {
        if (id >= source.Count || id < 0)
        {
            return default;
        }
        return source[id];
    }

    public int Size()
    {
        return source.Count;
    }

}