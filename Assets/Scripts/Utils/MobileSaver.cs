using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SavingData;

public class MobileSaver
{

    public static string dataDir;

    public static string SaveProfileImg(byte[] img)
    {

        string fileName = "Test.png";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        File.WriteAllBytes(dataDir + fileName, img);
        string fullPath = dataDir + fileName;
        return fullPath;
    }
}
