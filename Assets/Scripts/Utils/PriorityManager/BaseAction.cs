using System;

public abstract class BaseAction
{
    protected bool isUsed = false;
    public BaseAction()
    {

    }

    public BaseAction(Action<BaseAction, object, string> callBack, int priority = 0)
    {
        isUsed = true;
        CallBack = callBack;
        CallBackManager = null;
        Priority = priority;
    }

    public abstract void Do();

    public int Priority
    {
        get; set;
    }

    public Action<BaseAction, object, string> CallBack
    {
        get;
        set;
    }

    public Action<BaseAction, object, string> CallBackManager
    {
        get;
        set;
    }

    public bool IsUsed => isUsed;


    protected virtual void OnAssetCallBack(object tAsset, string err = null)
    {
        if (CallBackManager != null)
            CallBackManager?.Invoke(this, tAsset, err);
        else
            CallBack?.Invoke(this, tAsset, err);
    }

    public virtual void Dispose()
    {
        CallBack = null;
        CallBackManager = null;
        isUsed = false;
    }
}
