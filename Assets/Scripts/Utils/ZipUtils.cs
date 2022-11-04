using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using SavingData;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: zip压缩/解压工具类：提供Json等文件压缩/解压的相关功能
/// Date: 2021-12-30 10:47:26
/// </summary>
public class ZipUtils : MonoBehaviour
{
    private static readonly object locker = new object();
    public static string zipDir => Application.persistentDataPath + "/U3D/ZipFile/";
    public static string extJsonDir => Application.persistentDataPath + "/U3D/ExtJson/";

    public static string SaveZipJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            LoggerUtils.LogError("SaveZipJson -- Json is Empty");
            return null;
        }
        string jsonFilePath = DataUtils.SaveJsonAndGetPath(json);
        string zipFile = ZipFile(jsonFilePath, zipDir);
        return zipFile;
    }

    public static string SavePropZipJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            LoggerUtils.LogError("SavePropZipJson -- Json is Empty");
            return null;
        }
        string jsonFilePath = DataUtils.SavePropJsonAndGetPath(json);
        string zipFile = ZipFile(jsonFilePath, zipDir);
        return zipFile;
    }

    public static string SaveZipUgcClothJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            LoggerUtils.LogError("SaveZipUgcClothJson -- Json is Empty");
            return null;
        }
        string jsonFilePath = DataUtils.SaveUgcClothJsonAndGetPath(json);
        string zipFile = ZipFile(jsonFilePath, DataUtils.ugcClothesDataDir);
        return zipFile;
    }

    public static void SaveZipJsonLocal(string json, Action<string> onSuccess, Action<string> onFail)
    {
        if (string.IsNullOrEmpty(json))
        {
            LoggerUtils.LogError("LocalSaveZipJson -- Json is Empty");
            onFail?.Invoke(null);
            return;
        }
        string jsonFilePath = DataUtils.SaveJsonAndGetPath(json);
        SaveZipLocal(jsonFilePath, Data_Type.Map, (obj) =>
        {
            string zipFile = obj as string;
            if (string.IsNullOrEmpty(zipFile))
            {
                LoggerUtils.LogError("LocalSaveZipJson -- Zip File Failed");
                onFail?.Invoke(null);
                return;
            }
            GameManager.Inst.gameMapInfo.draftId++;
            GameManager.Inst.editSaveInfo.sMap = 1;
            onSuccess?.Invoke(zipFile);
        });
    }

    public static void SavePropZipJsonLocal(string json, Action<string> onSuccess, Action<string> onFail)
    {
        if (string.IsNullOrEmpty(json))
        {
            LoggerUtils.LogError("LocalSavePropZipJson -- Json is Empty");
            onFail?.Invoke(null);
            return;
        }
        string jsonFilePath = DataUtils.SavePropJsonAndGetPath(json);
        SaveZipLocal(jsonFilePath, Data_Type.Prop, (obj) =>
        {
            string zipFile = obj as string;
            if (string.IsNullOrEmpty(zipFile))
            {
                LoggerUtils.LogError("LocalSavePropZipJson -- Zip File Failed");
                onFail?.Invoke(null);
                return;
            }
            GameManager.Inst.editSaveInfo.sProp = 1;
            onSuccess?.Invoke(zipFile);
        });
    }

    private static void SaveZipLocal(string filePath, Data_Type type, Action<object> onFinish)
    {
        ZipThreadParams zipParam = new ZipThreadParams()
        {
            filePath = filePath,
            type = type,
            onFinish = onFinish
        };
        ThreadPool.QueueUserWorkItem(new WaitCallback(SaveZipFileThread), zipParam);
    }

    private static void SaveZipFileThread(object objParam)
    {
        lock (locker)
        {
            ZipThreadParams zipParam = objParam as ZipThreadParams;
            string zipFile = ZipFileToLocal(zipParam.filePath, zipParam.type);
            MainThreadDispatcher.Enqueue(new TaskRunner(zipFile, zipParam.onFinish));
        }
    }

    private static string ZipFileToLocal(string filePath, Data_Type type)
    {
        string zipFile = ZipFile(filePath, DataUtils.DraftPath);
        //压缩后文件路径
        string souPath = DataUtils.DraftPath + zipFile;
        if (string.IsNullOrEmpty(zipFile))
        {
            //压缩失败，删除空zip文件
            if (File.Exists(souPath))
            {
                File.Delete(souPath);
            }
            return null;
        }
        //压缩成功，覆盖原文件
        //目标路径
        string desPath = DataUtils.DraftPath + type.ToString().ToLower() + ".zip";
        try
        {
            if (File.Exists(desPath))
            {
                File.Delete(desPath);
            }
            File.Move(souPath, desPath);
            LoggerUtils.Log("local save zip file success -- desPath = " + desPath);
        }
        catch (Exception err)
        {
            LoggerUtils.Log("local save zip file exception");
            Debug.LogException(err);
            return null;
        }
        return type.ToString().ToLower() + ".zip";
    }

    public static void SaveClothZipLocal(List<string> files, Action<object> onFinish)
    {
        string extension = ".png";
        string inPath = DataUtils.ugcClothesDataDir;
        string zipName = DataUtils.GetClothImageZipName();
        ZipClothThreadParams zipParam = new ZipClothThreadParams()
        {
            files = files,
            extension = extension,
            inPath = inPath,
            zipName = zipName,
            onFinish = onFinish
        };
        ThreadPool.QueueUserWorkItem(new WaitCallback(SaveClothZipThread), zipParam);
    }

    private static void SaveClothZipThread(object objParam)
    {
        lock (locker)
        {
            ZipClothThreadParams zipParam = objParam as ZipClothThreadParams;
            string zipFile = ZipClothFileToLocal(zipParam.files, zipParam.extension, zipParam.inPath, zipParam.zipName);
            MainThreadDispatcher.Enqueue(new TaskRunner(zipFile, zipParam.onFinish));
        }
    }

    private static string ZipClothFileToLocal(List<string> files, string extension, string inPath, string zipName)
    {
        string zipFile = ZipFile(files, extension, inPath, zipName, DataUtils.DraftPath);
        //压缩后文件路径
        string souPath = DataUtils.DraftPath + zipFile;
        if (string.IsNullOrEmpty(zipFile))
        {
            //压缩失败，删除空zip文件
            if (File.Exists(souPath))
            {
                File.Delete(souPath);
            }
            return null;
        }
        //压缩成功，覆盖原文件
        //目标路径
        string desPath = DataUtils.DraftPath + "clothTex.zip";
        try
        {
            if (File.Exists(desPath))
            {
                File.Delete(desPath);
            }
            File.Move(souPath, desPath);
            LoggerUtils.Log("local save zip cloth tex success -- desPath = " + desPath);
        }
        catch (Exception err)
        {
            LoggerUtils.Log("local save zip cloth tex exception");
            Debug.LogException(err);
            return null;
        }
        return "clothTex.zip";
    }

    ///// <summary>
    ///// ZipFile
    ///// </summary>
    ///// <param name="FilePath">The File You Want To Zip</param>
    ///// <param name="DesPath">ZipFile Target Path</param>
    public static string ZipFile(string FilePath, string DesPath)
    {
        if (!File.Exists(FilePath))
        {
            LoggerUtils.LogError("The filePath to be compressed does not exist");
            return null;
        }

        if (!Directory.Exists(DesPath))
        {
            LoggerUtils.Log("!Directory.Exists(DesPath)");
            Directory.CreateDirectory(DesPath);
        }

        string oriFileName = Path.GetFileNameWithoutExtension(FilePath);
        //master环境中存在少量".json.zip"后缀得Json文件
        string zipFileFullPath = DesPath + oriFileName + ".zip";
        LoggerUtils.Log(zipFileFullPath);
        using (FileStream fs = File.Create(zipFileFullPath))
        {
            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
            {
                using (FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    //线上部分压缩文件内部Json文件没有后缀名，但解压后文件后缀名正常
                    ZipEntry zipEntry = new ZipEntry(oriFileName + ".json");
                    zipStream.SetLevel(9);
                    zipStream.PutNextEntry(zipEntry);
                    byte[] buffer = new byte[zipStream.Length];
                    int sizeRead = 0;
                    int sizeTemp = 0; //长度校验
                    try
                    {
                        do
                        {
                            sizeRead = stream.Read(buffer, 0, buffer.Length);
                            zipStream.Write(buffer, 0, sizeRead);
                            sizeTemp += sizeRead;
                        } while (sizeRead > 0);
                    }
                    catch (Exception err)
                    {
                        Debug.LogException(err);
                        return null;
                    }
                    //长度校验
                    if (sizeTemp < stream.Length)
                    {
                        LoggerUtils.LogError("Zip Incomplete --> sizeTemp : " + sizeTemp + "; totalLength : " + stream.Length);
                        return null;
                    }
                    stream.Close();
                    LoggerUtils.Log("Zip Length -- " + zipStream.Length / 1024 + " kb");
                    if (zipStream.Length <= 0)
                    {
                        LoggerUtils.LogError("Zip File Failed -- Length == 0");
                        return null;
                    }
                }
                zipStream.Finish();
                zipStream.Close();
            }
            fs.Close();
            File.Delete(FilePath);
            return oriFileName + ".zip";
        }
    }

    ///// <summary>
    ///// ZipFile
    ///// </summary>
    ///// <param name="FilePath">The File You Want To Zip</param>
    ///// <param name="DesPath">ZipFile Target Path</param>
    public static string ZipFile(List<string> files, string extension, string inPath, string zipName, string outPath)
    {
        
        for (var i = 0; i < files.Count; i++)
        {
            if (!File.Exists(inPath + files[i]))
            {
                LoggerUtils.LogError("The filePath to be compressed does not exist");
                return null;
            }
        }

        if (!Directory.Exists(outPath))
        {
            LoggerUtils.Log("!Directory.Exists(DesPath)");
            Directory.CreateDirectory(outPath);
        }

        string zipFileFullPath = outPath + zipName;
        LoggerUtils.Log(zipFileFullPath);
        string inFile = string.Empty;
        using (FileStream fs = File.Create(zipFileFullPath))
        {
            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
            {
                for (int i = 0; i < files.Count; i++)
                {
                    inFile = inPath + files[i];
                    using (FileStream stream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                    {
                        string oriFileName = Path.GetFileNameWithoutExtension(files[i]);
                        ZipEntry zipEntry = new ZipEntry(oriFileName + extension);
                        zipStream.SetLevel(9);
                        zipStream.PutNextEntry(zipEntry);
                        byte[] buffer = new byte[zipStream.Length];
                        int sizeRead = 0;
                        int sizeTemp = 0; //长度校验
                        try
                        {
                            do
                            {
                                sizeRead = stream.Read(buffer, 0, buffer.Length);
                                zipStream.Write(buffer, 0, sizeRead);
                                sizeTemp += sizeRead;
                            } while (sizeRead > 0);
                        }
                        catch (Exception err)
                        {
                            Debug.LogException(err);
                            return null;
                        }
                        //长度校验
                        if (sizeTemp < stream.Length)
                        {
                            LoggerUtils.LogError("Zip Incomplete --> sizeTemp : " + sizeTemp + "; totalLength : " + stream.Length);
                            return null;
                        }

                        stream.Close();
                        LoggerUtils.Log("Zip Length -- " + zipStream.Length / 1024 + " kb");

                        if (zipStream.Length <= 0)
                        {
                            LoggerUtils.LogError("Zip File Failed -- Length == 0");
                            return null;
                        }
                    }

                    File.Delete(inFile);

                }

                zipStream.Finish();
                zipStream.Close();
            }

            fs.Close();
            //return zipFileFullPath;
            return zipName;
        }
    }

    public static string SaveZipFromByte(byte[] ZipByte)
    {
        //bool result = true;
        FileStream fs = null;
        ZipInputStream zipStream = null;
        ZipEntry ent = null;
        string fileName = "";
        string fullPath = "";

        if (!Directory.Exists(extJsonDir))
        {
            LoggerUtils.Log("!Directory.Exists(DesPath)");
            Directory.CreateDirectory(extJsonDir);
        }
        if (ZipByte.Length <= 0)
        {
            LoggerUtils.LogError("Input ZipByte Length == 0");
            return null;
        }
        try
        {
            //直接使用 将byte转换为Stream，省去先保存到本地在解压的过程
            Stream stream = new MemoryStream(ZipByte);
            zipStream = new ZipInputStream(stream);
            LoggerUtils.Log("zipStream = " + zipStream);
            while ((ent = zipStream.GetNextEntry()) != null)
            {
                if (!string.IsNullOrEmpty(ent.Name))
                {
                    fileName = DataUtils.GetJsonName();
                    fullPath = Path.Combine(extJsonDir, fileName);
                    fullPath = fullPath.Replace('\\', '/');

                    if (fileName.EndsWith("/"))
                    {
                        Directory.CreateDirectory(fileName);
                        continue;
                    }

                    fs = File.Create(fullPath);

                    int size = 2048;
                    byte[] data = new byte[size];
                    while (true)
                    {
                        size = zipStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            fs.Write(data, 0, size);//解决读取不完整情况 
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            fs.Dispose();
            LoggerUtils.Log("UnZipFullPath = " + fullPath);
            string content = File.ReadAllText(fullPath);
            File.Delete(fullPath);
            return content;
        }
        catch (Exception e)
        {
            LoggerUtils.LogError(e.ToString());
            //result = false;
            return null;
        }
    }

    /// <summary> 
    /// 解压功能(下载后直接解压压缩文件到指定目录) 
    /// </summary> 
    /// <param name="wwwStream">www下载转换而来的Stream</param> 
    /// <param name="zipedFolder">指定解压目标目录(每一个Obj对应一个Folder)</param> 
    /// <param name="password">密码</param> 
    /// <returns>解压结果</returns> 
    public static Dictionary<string, byte[]> UnpackFiles(byte[] ZipByte)
    {
        //bool result = true;
        
        ZipInputStream zipStream = null;
        ZipEntry ent = null;
        string fileName = "";
        string fullPath = "";
        Dictionary<string,byte[]> allFiles = new Dictionary<string, byte[]>();
        try
        {
            //直接使用 将byte转换为Stream，省去先保存到本地在解压的过程
            Stream stream = new MemoryStream(ZipByte);
            zipStream = new ZipInputStream(stream);
            LoggerUtils.Log("zipStream = " + zipStream);
            while ((ent = zipStream.GetNextEntry()) != null)
            {
                if (!string.IsNullOrEmpty(ent.Name))
                {
                    using (MemoryStream fs = new MemoryStream())
                    {
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                fs.Write(data, 0, size); //解决读取不完整情况 
                            }
                            else
                            {
                                break;
                            }
                        }
                        var bytes = fs.GetBuffer();
                        allFiles.Add(ent.Name, bytes);
                    }
                }
            }
            return allFiles;
        }
        catch (Exception e)
        {
            LoggerUtils.Log(e.ToString());
            //result = false;
            return null;
        }
    }

    public static byte[] Unzip(byte[] zippedBuffer)
    {
        using (var zippedStream = new MemoryStream(zippedBuffer))
        {
            using (var archive = new ZipArchive(zippedStream))
            {
                var entry = archive.Entries.FirstOrDefault();

                if (entry != null)
                {
                    using (var unzippedEntryStream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            unzippedEntryStream.CopyTo(ms);
                            var unzippedArray = ms.ToArray();

                            return unzippedArray;
                        }
                    }
                }
                return null;
            }
        }
    }
    
    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="sourcePath">原文件夹路径</param>
    /// <param name="desZipPath">目标文件</param>
    /// <returns>MD5码</returns>
    public static string ZipDirectory(string sourcePath, string desZipPath)
    {
        FastZip fast = new FastZip();
        FileStream stream = null;
        try
        {
            stream = File.Create(desZipPath);
            fast.CreateZip(stream, sourcePath, true, null, null);
            stream?.Dispose();
            return DataUtils.GetMD5HashFromFile(desZipPath);
        }
        catch (Exception ex)
        {
            stream?.Dispose();
            LoggerUtils.LogError("ZipDirectory Failed! -- " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 解压目录文件
    /// </summary>
    /// <param name="sourceZipPath">原压缩文件</param>
    /// <param name="desPath">目标路径</param>
    public static void UnZipDirectory(string sourceZipPath, string desPath)
    {
        FastZip fast = new FastZip();
        try
        {
            fast.ExtractZip(sourceZipPath, desPath, null);
        }
        catch (Exception ex)
        {
            LoggerUtils.LogError("UnZipDirectory Failed! -- " + ex.Message);
        }
    }
}
