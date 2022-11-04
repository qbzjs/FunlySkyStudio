using System;
using Newtonsoft.Json;
/// <summary>
/// Author:Shaocheng
/// Description:攻击道具Cmp, UGC素材 和 默认的攻击道具 会携带此Cmp
/// Date: 2022-4-14 17:44:22
/// </summary>
[Serializable]
public struct AttackWeaponNodeData
{
    public string rId;
    public int wType;
    public float damage;
    public int oDur;
    public float hits;
}
public class AttackWeaponComponent : IComponent
{
    public string rId;
    public int wType;
    public float damage;
    public float hits = 20;
    public int openDurability = 1; // 是否开启耐力值 0 关 1 开 
    public float curHits; // 当前耐力值（不记录进 Json 文件）
    public IComponent Clone()
    {
        AttackWeaponComponent component = new AttackWeaponComponent();
        component.rId = rId;
        component.wType = wType;
        component.damage = damage;
        component.openDurability = openDurability;
        component.hits = hits;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        AttackWeaponNodeData data = new AttackWeaponNodeData
        {
            rId = rId,
            wType = wType,
            damage = damage,
            oDur = openDurability,
            hits = hits
        };

        return new BehaviorKV
        {
            k = (int) BehaviorKey.AttackWeapon,
            v = JsonConvert.SerializeObject(data)
        };
    }
}