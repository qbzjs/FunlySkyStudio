using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

//public class DTextData
//{
//    public string con;
//    public string col;
//}
public class DTextComponent : IComponent
{
    public string content;
    public Color col;
    public int colorId;
    public IComponent Clone()
    {
        var comp = new DTextComponent();
        comp.content = content;
        comp.col = col;
        comp.colorId = colorId;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        DTextData data = new DTextData
        {
            textcol = DataUtils.ColorToString(col),
            tex = content
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.DText,
            v = JsonConvert.SerializeObject(data)
        };
    }
}