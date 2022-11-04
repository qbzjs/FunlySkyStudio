using System.Collections.Generic;
/// <summary>
/// Author: 熊昭
/// Description: 玩家血条控制组件
/// Date: 2022-04-07 18:08:17
/// </summary>
public class HPControlComponent : IComponent
{
    public int setHP;
    public List<int> dmgSrcs = new List<int>();
    public int customHP; // 自定义血量
    public IComponent Clone()
    {
        HPControlComponent component = new HPControlComponent();
        component.setHP = setHP;
        component.customHP = customHP;

        var dmgList = new List<int>();
        foreach (var src in dmgSrcs)
        {
            dmgList.Add(src);
        }   
        component.dmgSrcs = dmgList;     
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
