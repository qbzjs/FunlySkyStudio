#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Amazon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public struct UploadTask
{
    public string destFileName;
    public string sourceFile;
    public string destPath;
}


public class S3UploadRuntimeUtils : MonoBehaviour
{
    public static bool isRunning = false;
    public static S3UploadRuntimeUtils instance;
    private int fileCount;
    private int upLoaded;
    private int failCount;
    private Action<List<UploadTask>, Action> currUploadTaskSuccBack;
    private List<UploadTask> uploadingFiles = new List<UploadTask>();
    
    public static void EnterPlaymode()
    {
        if (!Application.isPlaying)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/AWSUpLoadScene.unity");
            EditorApplication.EnterPlaymode();
        }
        else
        {
            EditorUtility.DisplayDialog("错误", "正在运行模式,请先退出", "确定");
        }
       
    }
    
    void Start()
    {
        UnityInitializer.AttachToGameObject(gameObject);
        MainThreadDispatcher.Init();
        isRunning = true;
        instance = this;
    }

    public void BeginUpload(List<UploadTask> upLoadFiles, Action<List<UploadTask>, Action> ac = null)
    {
        
        uploadingFiles.Clear();
        uploadingFiles.AddRange(upLoadFiles);
        upLoaded = 0;
        failCount = 0;
        fileCount = uploadingFiles.Count;
        currUploadTaskSuccBack = ac;
        EditorUtility.DisplayProgressBar("upload files",$"{upLoaded}/{fileCount} uploading...", upLoaded * 1.0f / fileCount );
    }
    
    private void Update()
    {
        if (S3UploadRuntimeUtils.isRunning && uploadingFiles.Count > 0)
        {
            foreach (var f in uploadingFiles)
            {
                AWSUtill.UpLoadRes(f.destFileName,
                    f.sourceFile,
                    f.destPath,
                    OnSuccess, OnFail);
                
                S3UploadRuntimeUtils.isRunning = false;
            }
        }
    }
    
    private void OnSuccess(string url)
    {
        UpdateProgress();
    }

    private void OnFail(string url)
    {
        failCount++;
        Debug.LogError($"file upload fail ${url}");
        
        //这边又是个坑  成功的回调是在主线程 但是网络失败的不在
        MainThreadDispatcher.Enqueue(UpdateProgress);
    }
    
    void UpdateProgress()
    {
        upLoaded++;
        EditorUtility.DisplayProgressBar("upload files",$"{upLoaded}/{fileCount} uploading...", upLoaded * 1.0f / fileCount );
        if (upLoaded >= uploadingFiles.Count)
        {
            EditorUtility.ClearProgressBar();
          

            if (failCount == 0)
            {
                EditorUtility.DisplayDialog("上传结束", $"上传成功{upLoaded - failCount}个 失败{failCount}个 详见console面板","确定");
                currUploadTaskSuccBack?.Invoke(uploadingFiles, () =>
                {
                    if (Application.isPlaying)
                    {
                        EditorApplication.ExitPlaymode();
                    }
                });
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "有文件上传失败了 请重新上传","确定");
                if (Application.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
            }
        }
    }


}
#endif