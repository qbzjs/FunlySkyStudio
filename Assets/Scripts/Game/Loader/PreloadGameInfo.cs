/// <summary>
/// Author:YangJie
/// Description: 进入场景预加载信息
/// Date: 2022年7月14日11:03:00
using System;
public class PreloadGameInfo
{
    private int maxPreloadCount = 3;
    private int preloadCount;
    public int PreloadCount
    {
        set
        {
            preloadCount = value;
            if (preloadCount >= maxPreloadCount)
            {
                preloadCallBack?.Invoke();
            }
        }
        get => preloadCount;
    }
    public bool isSessionSuccess;
    public string mapContent;
    private Action preloadCallBack;
    public PreloadGameInfo(int maxPreloadStep, Action callBack)
    {
        maxPreloadCount = maxPreloadStep;
        preloadCallBack = callBack;
    }
}
