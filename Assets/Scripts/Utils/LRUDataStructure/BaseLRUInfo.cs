/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>
public abstract class BaseLRUInfo
{
    public string key;
    public ulong size;

    public abstract void Delete();

    public override bool Equals(object obj)
    {
        var tmpValue = obj as BaseLRUInfo;
        if (tmpValue == null)
        {
            return false;
        }
        return key == tmpValue.key;
    }
}
