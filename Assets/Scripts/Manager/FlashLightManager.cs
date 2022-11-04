using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightManager : ManagerInstance<FlashLightManager>, IManager
{
    private Dictionary<int, FlashLightBehaviour> propDict = new Dictionary<int, FlashLightBehaviour>();

    public NodeBaseBehaviour CreateBySelected(Vector3 pos)
    {
        FlashLightBehaviour nBev = SceneBuilder.Inst.CreateSceneNode<FlashLightCreator, FlashLightBehaviour>();
        if (!nBev) return null;

        FlashLightData data = GetDefaultData();        

        nBev.transform.position = pos;

        FlashLightCreator.SetData(nBev, data);
        AddNode(nBev);
        SceneBuilder.Inst.allControllerBehaviours.Add(nBev);

        return nBev;
    }

    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener(MessageName.OnForeground, OnForeGround);
    }

    public override void Release()
    {
        base.Release();
        Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener(MessageName.OnForeground, OnForeGround);
    }

    public void Clear()
    {
        propDict.Clear();
    }

    public void AddNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour == null) return;
        FlashLightBehaviour behav = behaviour.GetComponent<FlashLightBehaviour>();
        if (behav == null) return;
        propDict.Add(behaviour.entity.Get<GameObjectComponent>().uid, behav);
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour == null) return;
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        if (propDict.ContainsKey(uid))
        {
            propDict.Remove(uid);
        }      
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        AddNode(behaviour);
    }

    public void OnChangeMode(GameMode mode)
    {
        foreach(FlashLightBehaviour behav in propDict.Values)
        {
            behav.OnChangeMode(mode);
        }
    }

    public void OnForeGround()
    {
        if(GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            EnterPlayMode();
        }       
    }


    public void EnterPlayMode()
    {
        foreach (FlashLightBehaviour behav in propDict.Values)
        {
            behav.EnterColorPlayMode(true);
        }
    }

    public void OnFixedTime()
    {
        foreach (FlashLightBehaviour behav in propDict.Values)
        {
            behav.OnFixUpdate();
        }
    }

    public void SetFixScale(GameObject target)
    {
        List<FlashLightBehaviour> behavs = GameUtils.GetBehaviourInFirstLayer<FlashLightBehaviour>(target.transform);
        for(int i = 0; i < behavs.Count; ++i)
        {
            behavs[i].FixScale();
        }
    }

    

    public FlashLightData GetDefaultData()
    {
        return new FlashLightData
        {
            id = (int)GameResType.FlashLight,
            type = 0,
            range = 2.85f,
            inten = 0.455f,
            radius = 4f,
            colors = new List<string>() { DataUtils.ColorToString(Color.white) },
            mode = 0,
            time = 3,
            isReal = 0
        };
    }

}
