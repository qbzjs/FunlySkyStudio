/// <summary>
/// 目前IOS关闭后无法释放静态内存
/// </summary>
/// <typeparam name="T"></typeparam>
public class CInstance<T> : BaseInstance where T : BaseInstance, new()
{
    private static T _instance;
    private static object lockObj = new object();
    public static T Inst
    {
        get
        {
            _instance = CInstanceManager.CreateInstance<T>(typeof(T).FullName);
            return _instance;
        }
    }

    public override void Release()
    {
        base.Release();
        _instance = null;
    }
}

public class BaseInstance
{
    public virtual void Release() { }
}