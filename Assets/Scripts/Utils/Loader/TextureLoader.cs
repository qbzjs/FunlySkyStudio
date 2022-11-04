using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TextureLoader : MonoBehaviour
{
    public bool isIdle = true;
    private uint _index = 0;
    private float _timeOut = 30f;
    private Coroutine loader;
    public TextureQueuedLoader.TextureDetail loaderInfo { get; private set; }
    private Action<Texture2D, uint,TextureQueuedLoader.TextureDetail> _onComplete;
    /// <summary> A callback to fire when failing to load an image, returns the ErrorType and URL(or message) in the callback. (Register this callback manually if needed) </summary>
    public Action<ErrorType, TextureQueuedLoader.TextureDetail> m_OnImageLoadError;
    
    public enum ErrorType
    {
        /// <summary> Failed to download the image due to Network/Connection/HTTP error when using UnityWebRequest/WWW. </summary>
        NetworkError,

        /// <summary> Failed to download the image within the provided time limit! Is the provided timeout value too small? Is the network connection good? </summary>
        TimeOut,

        /// <summary> Failed to load the local cached image because filename not provided! </summary>
        MissingFilename,

        /// <summary> Failed to create texture from the loaded byte array. Is it a Unity supported image format? </summary>
        InvalidImageData,
    }

    public static TextureLoader Create()
    {
        TextureLoader loader = new GameObject("[ImageLoader]").AddComponent<TextureLoader>();
        return loader;
    }


    /// <summary>
    /// Start to load an image using specific cache and load settings, return a Texture2D and the given index in the onComplete callback.
    /// </summary>
    /// <param name="index"> A number specified by you. Can be used to indicate the identity or purpose of this Imageloader. It will be returned with the onComplete callback when finished. </param>
    /// <param name="url"> The url or local path of the image. </param>
    /// <param name="fileName"> The filename for storing(caching) the image file. </param>
    /// <param name="folderName"> The target folder for storing(caching) the image file. </param>
    /// <param name="cacheMode"> The behavior for handling Load and Cache files. (NoCache: do not auto save the image; UseCached: use the locally cached file if exist; Replace: download and replace the locally cached file is exist)  </param>
    /// <param name="onComplete"> The callback for returning the loaded Texture2D and Index. </param>
    /// <param name="retry"> How many time to retry if fail to load the file. </param>
    /// <param name="timeOut"> The maximum duration in seconds for waiting for a load. </param>
    public void Load(uint index, TextureQueuedLoader.TextureDetail info,
        Action<Texture2D, uint,TextureQueuedLoader.TextureDetail> onComplete = null, uint retry = 0, float timeOut = 30f)
    {
        _timeOut = timeOut;
        _index = index;
        _onComplete = onComplete;
        isIdle = false;
        _LoadFile(index, info, retry, onComplete);
    }

    private void _LoadFile(uint index, TextureQueuedLoader.TextureDetail info, uint retry = 0,
        Action<Texture2D, uint,TextureQueuedLoader.TextureDetail> onComplete = null)
    {
        loader = StartCoroutine(_LoadRoutine(index, info, false, retry, onComplete));
    }

    private bool isRetry = false;

    private IEnumerator _LoadRoutine(uint index, TextureQueuedLoader.TextureDetail info, bool isLoadLocal, uint retry = 0,
        Action<Texture2D, uint,TextureQueuedLoader.TextureDetail> onComplete = null)
    {
        if (string.IsNullOrEmpty(info.m_URL))
        {
            _StopDownload();
            yield break;
        }

        loaderInfo = info;
        var texInfo = info;
        if (_timeOut > 0) Invoke("_TimeOut", _timeOut);
        using (UnityWebRequest uwr = UnityWebRequest.Get(loaderInfo.m_URL)) // UnityWebRequestTexture.GetTexture(url))
        {
            DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            uwr.downloadHandler = texDl;
            yield return uwr.SendWebRequest();
            if (_timeOut > 0) CancelInvoke("_TimeOut");
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError ||
                uwr.result == UnityWebRequest.Result.DataProcessingError)
            {
                if (retry > 0)
                {
                    yield return new WaitForSeconds(1);
                    retry--;
                    isRetry = true;
                    loader = StartCoroutine(_LoadRoutine(index, loaderInfo, isLoadLocal, retry, onComplete));
                }
                else
                {
                    if (m_OnImageLoadError != null) m_OnImageLoadError(ErrorType.NetworkError, loaderInfo);
                    _StopDownload();
                }
            }
            else if(uwr.result == UnityWebRequest.Result.Success)
            {
                if (texDl.texture == null && m_OnImageLoadError != null)
                {
                    m_OnImageLoadError(ErrorType.InvalidImageData, loaderInfo);
                }
                if (onComplete != null)
                {
                    texDl.texture.Compress(true);
                    onComplete(texDl.texture, index,texInfo);
                    ReleaseLoader();
                }
            }
            texDl.Dispose();
            uwr.Dispose();
        }
    }

    private void _TimeOut()
    {
        if (m_OnImageLoadError != null) m_OnImageLoadError(ErrorType.TimeOut, loaderInfo);
        _StopDownload();
    }

    private void _StopDownload()
    {
        if (_onComplete != null)
        {
            _onComplete(null, _index,loaderInfo);
            ReleaseLoader();
        }
        StopCoroutine(loader);
    }
    
    public void ReleaseLoader()
    {
        isIdle = true;
    }
}