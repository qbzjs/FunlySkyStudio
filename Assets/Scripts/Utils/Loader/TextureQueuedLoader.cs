using System;
using System.Collections.Generic;
using System.Linq;
using SuperScrollView;
using UnityEngine;

public class TextureQueuedLoader : MonoBehaviour
{
    [Serializable]
    public struct TextureDetail
    {
        public string m_URL;
        public int scrollDir;//0:向下滑动 1:向上滑动
        public int tempArg;

        public TextureDetail(string url, int arg,int dir = 0)
        {
            m_URL = url;
            tempArg = arg;
            scrollDir = dir;
        }
    }
    
    /// <summary> Destroy this GameObject when all loading tasks finished. </summary>
    [Tooltip("Destroy this GameObject when all loading tasks finished.")]
    public bool m_DestroyOnComplete = false;

    /// <summary> Max number of ImageLoaders to use concurrently. </summary>
    [Tooltip("Max number of ImageLoaders to use concurrently.")]
    public uint m_MaxLoaderNum = 3;

    /// <summary> Max number of loading tasks allowed to queue in the URL/path(task) list. If the tasks exceeded this limit, the older waiting tasks will be removed. 0 = no limit (default). </summary>
    [Tooltip(
        "Max number of loading tasks allowed to queue in the URL/path(task) list. If the tasks exceeded this limit, the older waiting tasks will be removed. 0 = no limit (default).")]
    public uint m_MaxQueueSize;

    [Space]
    /// <summary> Image URLs or local paths to be loaded. </summary>
    [Tooltip("Image URLs or local paths to be loaded.")]
    public List<TextureDetail> m_ImageUrls = new List<TextureDetail>();

    public bool AutoClearMemoryCachedTexture = true;
    public List<TextureLoader> currentLoaders = new List<TextureLoader>();

    /// <summary> A callback to fire when each image URL load is finished. (Register this callback manually if needed) </summary>
    public Action<Result> m_OnImageLoaded;

    /// <summary> A callback to fire when all the image URL loading tasks are finished. (Register this callback manually if needed) </summary>
    public Action m_OnAllImagesLoaded;

    /// <summary> A callback to fire when failing to load an image, returns the ErrorType and URL(or message) in the callback. (Register this callback manually if needed) </summary>
    public Action<TextureLoader.ErrorType, TextureDetail> m_OnImageLoadError;

    private Action<float> _onProgressCallback = null;
    private Results _results;
    private uint _maxMemoryCacheNum = 0;
    private uint _currentIndex = 0;
    private float _progress = 0f;
    private int _finishCount = 0;
    
    /// <summary>
    /// Create an ImageQueuedLoader to load images with a specific number of ImageLoaders.
    /// </summary>
    /// <param name="maxLoaderNum"> Max number of ImageLoaders to use concurrently. (Requires at least 1 to load the images) </param>
    /// <param name="maxQueueSize"> Max number of loading tasks allowed to queue in the URL/path(task) list.
    /// If the tasks exceeded this limit, the oldest waiting tasks will be removed. 0 = no limit (default). </param>
    public static TextureQueuedLoader Create(uint maxLoaderNum, uint maxMemoryCacheNum, uint maxQueueSize = 0)
    {
        TextureQueuedLoader loader = new GameObject("[ImageQueuedLoader]").AddComponent<TextureQueuedLoader>();
        loader.m_MaxLoaderNum = maxLoaderNum;
        loader.m_MaxQueueSize = maxQueueSize;
        loader._results = new Results(maxMemoryCacheNum);
        return loader;
    }

    public Texture2D GetImageByUrl(string imageUrl,int arg,ScrollDirection sdir, bool loadIfNotFound = false)
    {
        TextureDetail detail = new TextureDetail(imageUrl, arg,(int)sdir);
        return GetMemoryCacheItemByUrl(detail, loadIfNotFound)?.m_Texture;
    }


    private Result GetMemoryCacheItemByUrl(TextureDetail info, bool loadIfNotFound = false)
    {
        var result = _results.GetResultByUrl(info);
        if (result != null)
        {
            if (result.m_Texture)
            {
                return result;
            }
            else
            {
                _results.Remove(result); // Remove the item if its texture is null (maybe destroyed from outside!!)
            }
        }
        else if (loadIfNotFound)
        {
            Add(info);
        }

        return null;
    }

    /// <summary> Add an image URL or local path to the loading list. Optional to provide a specific filename for it. </summary>
    /// <param name="imageUrl"> Image URL or local path. </param>
    /// <param name="overrideFilename"> The specific filename for this image, instead of using the sequential file naming feature in LoaderManagement. Please make sure no duplication. </param>
    private void Add(TextureDetail info, string overrideFilename = null)
    {
        if (m_ImageUrls == null) m_ImageUrls = new List<TextureDetail>();
        m_ImageUrls.Add(info);
    }

    public void DontDestroyOnLoad()
    {
        gameObject.DontDestroy();
        //DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        int total = m_ImageUrls.Count;
        if (_currentIndex >= total)
        {
            // No more Url/Path.
            if (m_DestroyOnComplete)
            {
                _CleanLoaderList();
                if (currentLoaders.Count == 0)
                {
                    Destroy(gameObject);
                }
            }

            return;
        }

        if (m_MaxQueueSize > 0 && (total - (int) _currentIndex) > (int) m_MaxQueueSize)
        {
            _currentIndex =
                (uint) total -
                m_MaxQueueSize; // (60 - 30 > 25) >> (60 - 25 = 35) >> (index:30 += 35 = 65)index out of range.. (fix: index = 60 - 25 = 35)
        }

        TextureLoader loader = GetIdleLoader();
        if (loader != null)
        {
            loader.m_OnImageLoadError = m_OnImageLoadError;
            _currentIndex++;
            TextureDetail tInfo = m_ImageUrls[(int) _currentIndex - 1];

            loader.Load(_currentIndex - 1, tInfo, (texture, index,info) =>
            {
                _finishCount++;
                _progress = (float) _finishCount / total; //(float)(_results.Count + 1) / total;
                Result result = new Result(texture, index, info.tempArg,info.m_URL, _progress);

                if (_onProgressCallback != null) _onProgressCallback(_progress);

                if (m_OnImageLoaded != null && texture != null)
                {
                    _results.AddResult(result,info.scrollDir == 0);
                    m_OnImageLoaded(result);
                    
                }

                _CleanLoaderList();

                if (currentLoaders.Count == 1 && _currentIndex >= m_ImageUrls.Count)
                {
                    if (m_OnAllImagesLoaded != null) m_OnAllImagesLoaded();
                }
            });
        }
    }

    private TextureLoader GetIdleLoader()
    {
        if (currentLoaders.Count < m_MaxLoaderNum)
        {
            TextureLoader loader = TextureLoader.Create();
            currentLoaders.Add(loader);
            return loader;
        }
        else
        {
            for (var i = 0; i < currentLoaders.Count; i++)
            {
                if (currentLoaders[i] != null &&currentLoaders[i].isIdle)
                {
                    return currentLoaders[i];
                }
            }
        }

        return null;
    }


    /// <summary> Remove null references from the loader list. </summary>
    private void _CleanLoaderList()
    {
        for (int i = 0; i < currentLoaders.Count; i++)
        {
            if (currentLoaders[i] == null) currentLoaders.RemoveAt(i);
        }
    }

    private void Init()
    {
        for (int i = 0; i < currentLoaders.Count; i++)
        {
            if (currentLoaders[i])
            {
                Destroy(currentLoaders[i].gameObject);
            }
        }

        currentLoaders = new List<TextureLoader>();

        m_ImageUrls = new List<TextureDetail>();
        _currentIndex = 0;
        _finishCount = 0;
    }

    private void OnDestroy()
    {
        Init();
    }


    public class Result
    {
        /// <summary> Loaded texture. Null if the image does not exist or there are network issues. (Check Null before using) </summary>
        public Texture2D m_Texture;

        /// <summary> The index of the image, the same as in the image URL/path list. </summary>
        public uint m_Index;

        /// <summary> The URL/path that is used to load the image. </summary>
        public string m_URL;

        /// <summary> The loading progress when this load is done. </summary>
        public float m_Progress;

        public int tempArg;

        public Result(Texture2D texture, uint index,int arg, string imageUrl, float progress)
        {
            m_Texture = texture;
            m_Index = index;
            tempArg = arg;
            m_URL = imageUrl;
            m_Progress = progress;
        }
    }

    public class Results
    {
        /// <summary> A dictionary contains all the loaded Results and textures. </summary>
        public List<Result> _resultList = new List<Result>();

        public uint _maxMemoryCacheNum;

        public Results(uint maxMemoryCacheNum)
        {
            _maxMemoryCacheNum = maxMemoryCacheNum;
        }
        public void AddResult(Result result,bool isDown)
        {
            if (Count >= _maxMemoryCacheNum)
            {
                if (isDown)
                {
                    var item = _resultList.First();
                    if (item.m_Texture != null)
                    {
                        UnityEngine.Object.Destroy(item.m_Texture);
                    }
                    _resultList.RemoveAt(0);
                }
                else
                {
                    var item = _resultList.Last();
                    if (item.m_Texture != null)
                    {
                        UnityEngine.Object.Destroy(item.m_Texture);
                    }
                    _resultList.RemoveAt(_resultList.Count - 1);
                }
            }
            _resultList.Add(result);
            _resultList.Sort((x, y) =>
            {
                if (x.tempArg == y.tempArg)
                    return 0;
                return x.tempArg <= y.tempArg ? -1 : 1;
            });

        }
        

        public void ClearTextures()
        {
            var list = GetResultList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].m_Texture != null)
                    UnityEngine.Object.Destroy(list[i].m_Texture);
            }
        }

        /// <summary> The total number of loaded image Result objects. </summary>
        public int Count
        {
            get { return _resultList.Count; }
        }

        /// <summary> Get all loaded results in a list. (Reminded to check null) </summary>
        public List<Result> GetResultList()
        {
            return _resultList;
        }


        /// <summary> Get all textures in a list. (Reminded to check null) </summary>
        public List<Texture2D> GetTextureList()
        {
            if (_textureList == null)
            {
                _textureList = new List<Texture2D>();
                for (int i = 0; i < _resultList.Count; i++)
                {
                    _textureList.Add(_resultList[i].m_Texture);
                }
            }

            return _textureList;
        }

        private List<Texture2D> _textureList;

        /// <summary> Get a loaded result object by its URL/path that is used to load the image. (Reminded to check null) </summary>
        public Result GetResultByUrl(TextureDetail info)
        {
            List<Result> list = GetResultList();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].m_URL == info.m_URL && list[i].tempArg == info.tempArg) 
                        return list[i];
                }
            }

            return null;
        }
        

        public void Remove(Result item)
        {
            _resultList.Remove(item);
        }
    }
}