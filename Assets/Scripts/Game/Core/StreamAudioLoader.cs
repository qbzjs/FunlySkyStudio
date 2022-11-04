
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

class StreamAudioLoader: MonoManager<StreamAudioLoader>
{
    public IEnumerator LoadAudio(string url, UnityAction<AudioClip> onLoaded, UnityAction onFailure, UnityAction<AudioClip> onFinish)
    {
        bool isStart = false;
        url = url.Replace("buddy-app-bucket.s3.us-west-1.amazonaws.com", "cdn.joinbudapp.com");
        LoggerUtils.Log("LoadStreamBGAudio:" + url);
        var fileName = url.Split('\\', '/').LastOrDefault() ?? "ugcAudio.mp3";
        using var request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        ((DownloadHandlerAudioClip) request.downloadHandler).streamAudio = true;
        request.SendWebRequest();
        // 等待1s 增加音频缓存
        yield return new WaitForSeconds(1.0f);
        AudioClip audioClip = null;
        while (!request.isDone)
        {
            // 为确保AudioClip 正确解析, 添加 10% 缓冲时, 
            if (request.downloadProgress <= 0.1f)
            {
                yield return null;
                continue;
            }
            if ( audioClip == null || audioClip.loadState != AudioDataLoadState.Loaded)
            {
                isStart = false;
                // 需要重新获取 audioClip，Unity内部会执行音频解析逻辑
                audioClip = ((DownloadHandlerAudioClip) request.downloadHandler).audioClip;
                yield return null;
            }
            if (audioClip.loadState == AudioDataLoadState.Loaded && !isStart )
            {
                // 需要等待一帧
                yield return null;
                isStart = true;
                audioClip.name = fileName;
                onLoaded?.Invoke(audioClip);
            }
            yield return null;
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            onFailure?.Invoke();
            LoggerUtils.Log("PlayStreamAudioClip Error:[url]" + request.error);
        }
        else
        {
            //下载下来之后保存到本地 再加载重新创建 AudioClip, 保证音频不串流，卡顿, 有优化空间
            var filePath = DataUtils.SaveAudio(fileName,  request.downloadHandler.data);
            var fullPath = filePath;
            if (Application.isEditor)
            {
                fullPath = filePath;
            } else {
                fullPath = "file://" + filePath;
            }
            LoggerUtils.Log("fullPath" + fullPath);
            yield return ResManager.Inst.GetAudioClip(fullPath, onFinish, onFailure);
            File.Delete(filePath);
            LoggerUtils.Log("PlayStreamAudioClip Finish:[url]" + url);
        }
    }

}