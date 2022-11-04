/// <summary>
/// Author:LiShuZhan
/// Description:新版3d文字component
/// Date: 2022-6-2 17:44:22
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
/// <summary>
/// 修改此脚本时要考虑旧版3d文字DTextComponent
/// </summary>
public class NewDTextComponent : IComponent
{
    public string content;
    public Color col;
    public int colorId;
    public IComponent Clone()
    {
        var comp = new NewDTextComponent();
        comp.content = content;
        comp.col = col;
        comp.colorId = colorId;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        NewDTextData data = new NewDTextData
        {
            textcol = DataUtils.ColorToString(col),
            tex = content
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.NewDTextData,
            v = JsonConvert.SerializeObject(data)
        };
    }
}