using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public struct PGCSceneData
{
    public int classifyID;
    public int pgcID;
}


public class PGCSceneComponent : IComponent
{
    public int classifyID;
    public int pgcID;

    public IComponent Clone()
    {
        var comp = new PGCSceneComponent();
        comp.classifyID = this.classifyID;
        comp.pgcID = this.pgcID;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        PGCSceneData data = new PGCSceneData
        {
            classifyID = this.classifyID,
            pgcID = this.pgcID
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.PgcScene,
            v = JsonConvert.SerializeObject(data)
        };
    }
}