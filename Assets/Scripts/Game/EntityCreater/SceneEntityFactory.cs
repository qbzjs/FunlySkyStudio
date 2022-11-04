using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Object = System.Object;

public class SceneEntityFactory
{
    private ECSWorld ecsWorld;
    private Dictionary<string,SceneEntityCreater> creaters;

    public SceneEntityFactory(ECSWorld world)
    {
        ecsWorld = world;
        creaters = new Dictionary<string, SceneEntityCreater>();
    }

    public T BindCreater<T>() where T: SceneEntityCreater,new()
    {
        string className = typeof(T).Name;
        if (!creaters.ContainsKey(className))
        {
            T creater = new T();
            creater.world = ecsWorld;
            creaters.Add(className, creater);
        }
        return (T)creaters[className];
    }
}


public abstract class SceneEntityCreater
{
    public ECSWorld world;
    public abstract T Create<T>() where T : NodeBaseBehaviour;
    public abstract GameObject Clone(GameObject target);
}

