using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEmoteBehaviour
{
    public int id;
    public BundlePart part;
    public GameObject selfObj;
    public string modelName;
    public int emoteId;
    public SpecialEmotePropsType specialEmoteType;
    public int isOwner;

    public void SetBehavInfo(int id,BundlePart part,GameObject selfObj,string modelName,int emoteId, SpecialEmotePropsType specialEmoteType)
    {
        this.id = id;
        this.part = part;
        this.selfObj = selfObj;
        this.modelName = modelName;
        this.emoteId = emoteId;
        this.specialEmoteType = specialEmoteType;
    }
}
