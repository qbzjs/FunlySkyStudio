using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// wait for Update
/// </summary>
[Serializable]
public class SceneEntity
{
    //禁止使用该Id做业务开发
    public int Id;
    public Dictionary<Type, IComponent> Components = new Dictionary<Type, IComponent>();

    public void SetID(int id)
    {
        this.Id = id;
    }

    public T Get<T>() where T : IComponent, new()
    {
        Type tType = typeof(T);
        if (!Components.ContainsKey(tType))
        {
            var comp = new T();
            Components.Add(tType,comp);
        }
        return (T)Components[tType];
    }

    public void Remove<T>() where T : IComponent, new()
    {
        Type tType = typeof(T);
        if (Components.ContainsKey(tType))
        {
            Components.Remove(tType);
        }
    }

    public Dictionary<Type, IComponent> CloneComponent()
    {
        Dictionary<Type, IComponent> comps = new Dictionary<Type, IComponent>();
        foreach (var val in Components)
        {
            comps.Add(val.Key,val.Value.Clone());
        }
        return comps;
    }

    public void Destroy()
    {
        Id = -1;
        Components.Clear();
    }

    public bool HasComponent<T>() where T : IComponent
    {
        return Components.ContainsKey(typeof(T));
    }
}

public static class EntityExt
{
    public static void Des(this SceneEntity entity,ECSWorld world)
    {
        world.DestroyEntity(entity);
    }
    
    public static bool TryGet<T>(this SceneEntity entity, out T component) where T : IComponent
    {
        Type tType = typeof(T);
        if (entity.Components.ContainsKey(tType))
        {
            component = (T) entity.Components[tType];
            return true;
        }
        component = default;
        return false;
    }
}