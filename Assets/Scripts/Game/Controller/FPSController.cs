using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FPSController:BMonoBehaviour<FPSController>
{
    private float m_LastUpdateShowTime = 0f; //上一次更新帧率的时间;

    private float m_UpdateShowDeltaTime = 0.5f; //更新帧率的时间间隔;

    private int m_FrameCount = 0; //帧数;

    private float m_FPS = 0;

    private List<float> allFPS = new List<float>();
    
    // Use this for initialization

    public void StartCollectFPS()
    {
        m_FrameCount = 0;
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
        allFPS.Clear();
    }

    public float GetAverageFPS()
    {
        float deltaTime = Time.realtimeSinceStartup - m_LastUpdateShowTime;
        LoggerUtils.Log("deltaTime===="+ deltaTime);
        LoggerUtils.Log("m_FrameCount=" + m_FrameCount);
        m_FPS = m_FrameCount/deltaTime;
        return m_FPS;
    }
    // Update is called once per frame
    void Update()
    {
        m_FrameCount++;
        if (m_FrameCount >= int.MaxValue)
        {
            m_FrameCount = 0;
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
        }
    }
    public void ResetTime()
    {
        m_FrameCount = 0;
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
    }
}