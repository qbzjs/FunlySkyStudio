using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WhiteListMask
{

    public WhiteListMask(int mask)
    {
        _mask = mask;
    }

    /// <summary>
    /// 白名单类型, 分别按照 mask 二进制位数控制, 比如 :
    /// 1(二进制01)， 在离线渲染白名单中,
    /// 2(二进制10), 在显示调试信息白名单中,
    /// 3(二进制11), 既在离线渲染白名单中，也在调试信息白名单中 
    /// </summary>
    public enum WhiteListType
    {
        /// <summary>
        /// 离线渲染
        /// </summary>
        OfflineRender = 1,
        
        /// <summary>
        /// 调试信息
        /// </summary>
        DevInfo = 2,
        
        /// <summary>
        /// Alpha 白名单，后端使用
        /// </summary>
        Alpha = 4,
        
    }
    
    private int _mask;

    public static int GetMask(params WhiteListType[] types)
    {
        int num = 0;
        foreach (WhiteListType type in types)
        {
            num |= (int)type;
        }
        return num;
    }

    public void SetInWhiteList(WhiteListType type)
    {
        _mask = (_mask | (int)type);
    }

    public bool IsInWhiteList(WhiteListType type)
    {
        return (_mask & (int)type) == (int)type;
    }

}
