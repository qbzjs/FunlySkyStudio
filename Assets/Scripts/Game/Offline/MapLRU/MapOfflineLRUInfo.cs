using System.IO;
using UnityEngine;

public class MapOfflineLRUInfo : FileLRUInfo
{
    protected override string localSavePath => Application.persistentDataPath + "/Offline/Map/";
    public const ulong MaxSize = 1024 * 1024 * 100;

}