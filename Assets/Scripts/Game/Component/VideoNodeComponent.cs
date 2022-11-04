using System;
using Newtonsoft.Json;

[Serializable]
public struct VideoNodeData
{
    public string vUrl;
    public int sRange;
}
/// <summary>
/// Author:Shaocheng
/// Description:视频道具Cmp
/// Date: 2022-3-30 19:43:08
/// </summary>
public class VideoNodeComponent : IComponent
{
    public string videoUrl = "";
    public int soundRange;

    public IComponent Clone()
    {
        VideoNodeComponent component = new VideoNodeComponent();
        component.videoUrl = videoUrl;
        component.soundRange = soundRange;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        VideoNodeData data = new VideoNodeData
        {
            vUrl = videoUrl,
            sRange = soundRange
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.VideoNode,
            v = JsonConvert.SerializeObject(data)
        };
    }
}