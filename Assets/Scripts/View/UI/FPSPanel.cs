/// <summary>
/// Author:YangJie
/// Description: Debug Pannel
/// Date: 2022/3/30 18:28:5
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using HLODSystem;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class FPSPanel :  MonoBehaviour
{

    private static FPSPanel sInstance;

    public static FPSPanel Instance
    {
        get
        {
            if (sInstance == null)
            {
                sInstance = FindObjectOfType<FPSPanel>(true);
                if (sInstance == null)
                {
                    var obj = new GameObject("FPSPanel");
                    //DontDestroyOnLoad(obj);
                    obj.DontDestroy();
                    sInstance = obj.AddComponent<FPSPanel>();
                    
                }
            }
            return sInstance;
        }
    }


    private float m_LastUpdateShowTime = 0f; //上一次更新帧率的时间;

    private float m_UpdateShowDeltaTime = 1f; //更新帧率的时间间隔;

    private int m_FrameUpdate = 0; //帧数;

    private float m_FPS = 0;

    private float ping;
    public float parseJsonTime;
    private float renderOfflineTime;

    private long drawCall = 0;
    ProfilerRecorder callRecorder;
    private ProfilerRecorder trianglesRecorder;
    private long triangles = 0;
    private bool isShow = false;
    private Vector2 scrollPos = new Vector2(0, 0);
    private float vScrollbarValue;
    public string showInfoDown = "Texture/ShowInfo/right";
    public string showInfoUp = "Texture/ShowInfo/left";
    public string showInfoBg = "Texture/ShowInfo/bg_info";

    public bool isActive = false;
    Texture tex;
    Texture texTag;
    Texture texBg;
    
    void Start()
    {
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
        tex = Resources.Load<Texture>(showInfoDown);
        texTag = Resources.Load<Texture>(showInfoUp);
        texBg = Resources.Load<Texture>(showInfoBg);
    ;
    }

    private void OnEnable()
    {
        isActive = true;
        callRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count");
    }

    private void OnDisable()
    {
        isActive = false;
        callRecorder.Dispose();
        trianglesRecorder.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        m_FrameUpdate++;
        if (m_FrameUpdate >= 60)
        {
            m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
            m_FrameUpdate = 0;
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
            drawCall = callRecorder.LastValue;
            triangles = trianglesRecorder.LastValue;
        } 
    }

    public void SetPingText(float pingValue)
    {
        ping = pingValue;
    }

    public void SetParseJsonTime(float time)
    {
        parseJsonTime = time;
    }

    public void SetRenderOfflineTime(float time)
    {
        renderOfflineTime = time;
    }

    void OnGUI()
    {
        int offsetY = 70;
        int offsetX = 0;
        int width = 256;
        float widthRatio = (float)UnityEngine.Screen.width / 2000;
        float heightRatio = (float)UnityEngine.Screen.height / 1125;
        
        GUIStyle style = new GUIStyle
        {
            border = new RectOffset(10, 10, 10, 10),
            fontSize = 40,
            fontStyle = FontStyle.BoldAndItalic,
        };
        GUIStyle fontStyle1 = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 40,
            normal =
            {
                textColor = Color.white,
                background = texBg as Texture2D
            },
            
        };
        if (isShow)
        {
            if (GUI.Button(new Rect(offsetX*widthRatio, offsetY*heightRatio, width/4, 126),tex,fontStyle1))
            {
                MessageHelper.Broadcast(MessageName.DebugStateChange, false);
                isShow = false;
            }
        } else
        {
            if (GUI.Button(new Rect(offsetX*widthRatio, offsetY*heightRatio, width/4, 126),texTag,fontStyle1))
            {
                scrollPos = Vector2.zero;
                MessageHelper.Broadcast(MessageName.DebugStateChange, true);
                isShow = true;
            }
        }
        if (!isShow)
        {
            return;
        }
        offsetY += 110;
        offsetX += 150;
        GUI.skin.scrollView = new GUIStyle()
        {
            normal =
            {
                background = Texture2D.grayTexture
            },
        };
        
        GUI.skin.horizontalScrollbar.fixedHeight = 40;
        GUI.skin.verticalScrollbar.fixedWidth = 40;
        GUI.skin.verticalScrollbar.fixedHeight = 0;
        GUI.skin.horizontalScrollbarThumb.fixedHeight = 38;
        GUI.skin.verticalScrollbarThumb.fixedWidth = 38;
        GUI.skin.verticalScrollbarThumb.fixedHeight = 0;
        
        scrollPos = GUI.BeginScrollView(new Rect(offsetX*widthRatio, offsetY*heightRatio, width*2.0f*widthRatio, 600*heightRatio), scrollPos,new Rect(0,0,width*3.0f*widthRatio,1200*heightRatio), false , true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar);
        offsetY = 10;
        offsetX = 10;
        int size = 36;
        if(UnityEngine.Screen.width < 1900)size = 28;
        //自定义宽度 ，高度大小 颜色，style
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "FPS:" + Mathf.Floor(m_FPS).ToString("f2")+ "</size></color>", style);
        offsetY += 50;
        if (ping > 500)
        {
            GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#ff0000><size="+size+">" + "Ping: 超时!(" + Mathf.Floor(ping).ToString("f2") + "ms)" + "</size></color>", style);
        }
        else
        {
            GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "Ping:" + Mathf.Floor(ping).ToString("f2") + "ms" + "</size></color>", style);
        }
        offsetY += 50;
        if ((GlobalFieldController.ugcNodeData.Count - UGCBehaviorManager.Inst.cannotRenderABResCount) > 0)
        {   
            var propDownCount = GlobalFieldController.offlineRenderDataDic.Count;
            foreach (var item in GlobalFieldController.offlineRenderDataDic)
            {
                if(item.Key.EndsWith("_1"))
                {
                    propDownCount --;
                    break;
                }
            }
            GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "Offline Prop Count(L/D/N/T) </size></color>", style);
            offsetY += 50;
            GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + ":" + UGCModelCachePool.Inst.GetPoolCount()+"/"+ propDownCount + "/" + (GlobalFieldController.ugcNodeData.Count - UGCBehaviorManager.Inst.cannotRenderABResCount)+"/"+ GlobalFieldController.ugcNodeData.Count + "</size></color>", style);
            offsetY += 50;
        }
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "Cache Prop Size:" + (LRUManager<FileLRUInfo>.Inst.GetCurSize() / 1024 / 1024).ToString("f2") + "M</size></color>", style);
        offsetY += 50;

        if (renderOfflineTime > 0)
        {
            GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "Render Offline Time:" + renderOfflineTime.ToString("f2") + "s</size></color>", style);
            offsetY += 50;
        }
        
        if (parseJsonTime > 0)
        {
            GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "Parse Json Time:" +  parseJsonTime.ToString("f2") + "s</size></color>", style);
            offsetY += 50;
        }
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "isInWhiteMask: " + GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.OfflineRender) + "</size></color>", style);
        offsetY += 50;
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "is HLOD: " + HLOD.Inst.IsValid + "</size></color>", style);
        offsetY += 50;
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "isOpenPostProcess: " + GlobalFieldController.isOpenPostProcess + "</size></color>", style);
        offsetY += 50;
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "isOcclusionEnable: " + MapRenderManager.Inst.isOcclusionEnable + "/" + MapRenderManager.Inst.occlusionCount + "</size></color>", style);
        offsetY += 50;  
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "Draw Call: " + drawCall + "</size></color>", style);
        offsetY += 50;
        GUI.Label(new Rect(offsetX, offsetY, width, 50), "<color=#00ff00><size="+size+">" + "triangles: " + triangles + "</size></color>", style);
        offsetY += 50;
        GUI.EndScrollView();
        



    }


    
}
