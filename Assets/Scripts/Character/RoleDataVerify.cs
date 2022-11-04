using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleDataVerify
{
    private const int defShoeId = 1001;
    public static RoleData defRoleData;
    public static RoleData DefRoleData
    {
        get
        {
            if (defRoleData == null)
            {
                defRoleData = new RoleData();
                defRoleData = ResManager.Inst.LoadJsonRes<RoleData>("Configs/RoleData/DefRoleData");
            }
            return defRoleData;
        }
    }

    private static DefClothDataConfig defClothDatas;
    private static DefClothDataConfig DefClothDatas
    {
        get
        {
            if (defClothDatas == null)
            {
                defClothDatas = new DefClothDataConfig();
                defClothDatas = ResManager.Inst.LoadJsonRes<DefClothDataConfig>("Configs/RoleData/DefClothData");
            }
            return defClothDatas;
        }
    }

    public static bool CheckRoleDataIsLegal(RoleData roleData)
    {
        if (roleData.eId == 0 ||
            roleData.bId == 0 ||
            roleData.mId == 0 ||
            roleData.bluId == 0||
            roleData.cloId == 0
            )
        {
            return false;
        }
        return true;
    }

    public static bool IsRoleDatalegal(RoleData roleData)
    {
        var fields = roleData.GetType().GetFields();
        foreach (var field in fields)
        {
            if(field.Name == "clothesJson")
            {
                continue;
            }
            var data = field.GetValue(roleData);
            if(data == null)
            {
                LoggerUtils.LogError("RoleData is Error ==> Uid = " + GameManager.Inst.ugcUserInfo.uid + " |RoleData = " + GameManager.Inst.ugcUserInfo.imageJson + " |Field = " + field.Name + "is Null" +  " |Time = " + GameUtils.GetTimeStamp());
                return false;
            }
        }
        return true;
    }
}
