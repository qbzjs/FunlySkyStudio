using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description: 场景节点的UID管理
/// Date: 2022-3-30 19:43:08
/// </summary>
public class UidManager : CInstance<UidManager>
{
    public int curMaxUid = 0;

    public int GetUid(int uid = 0)
    {
        int newUid;
        if (uid == 0)
        {
            newUid = ++curMaxUid;
        }
        else
        {
            newUid = uid;
        }

        if (newUid > curMaxUid)
        {
            curMaxUid = newUid;
        }

        return newUid;
    }

    public int GetUid(NodeData data)
    {
        int newUid;

        if (data == null)
        {
            //prop edit mode, not set uid
            newUid = 0;
        }
        else if (data.uid == 0)
        {
            newUid = UidManager.Inst.GetUid(0);
        }
        else
        {
            newUid = data.uid;
            if (newUid > curMaxUid)
            {
                curMaxUid = newUid;
            }
        }

        return newUid;
    }

}