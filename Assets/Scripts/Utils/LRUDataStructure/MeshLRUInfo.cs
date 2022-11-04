using System;
using System.IO;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>
public class MeshLRUInfo: BaseLRUInfo
{
    public const ulong MaxSize = 1024 * 1024 * 100; // 1024 * 1024 * 100
    
    public string cachePath;
    public override void Delete()
    {
        var path = Path.Combine(CombineUtils.CacheFolder, cachePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public override bool Equals(object obj)
    {
        if (!base.Equals(obj))
        {
            return false;
        }
        return cachePath == (obj as MeshLRUInfo)?.cachePath;
    }

}
