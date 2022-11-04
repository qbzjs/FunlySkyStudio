using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PostProcessCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var dirGo = GameObject.Find("PostProcessVolume");
        var entity = world.NewEntityNoRecord();
        var postBehaviour = dirGo.AddComponent<T>();
        entity.Get<GameObjectComponent>().bindGo = postBehaviour.gameObject;
        postBehaviour.entity = entity;
        postBehaviour.OnInitByCreate();
        return postBehaviour;
    }


    public override GameObject Clone(GameObject target)
    {
        return null;
    }


    public static void SetDefault(PostProcessBehaviour behaviour)
    {
        var data = new PostProcessData();
        data.bInte = 5;
        data.bActive = 1;
        SetData(behaviour, data);
    }


    public static void SetData(PostProcessBehaviour behaviour, PostProcessData data)
    {
        if (behaviour == null)
        {
            UnityEngine.Debug.LogError("PostprocessBehaviour is null");
            return;
        }
        //1.13.0之前版本继续编辑默认值
        if (data == null)
        {
            data = new PostProcessData();
            data.bInte = 5;
            data.bActive = 0;
        }
        var dComp = behaviour.entity.Get<PostProcessComponent>();
        dComp.bloomActive = data.bActive;
        dComp.bloomIntensity = data.bInte;
        //behaviour.SetAmActive(data.amActive);
        behaviour.SetBloomActive(data.bActive);
        behaviour.ChangeBloomIntensity(data.bInte);
    }
}