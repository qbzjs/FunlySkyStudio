using Newtonsoft.Json;

/// <summary>
/// Author: 熊昭
/// Description: 音乐板道具数据类
/// Date: 2021-12-03 14:51:34
/// </summary>
public class MusicBoardComponent : IComponent
{
    public int[] audioIDs = new int[] { 0, 0, 0 }; //[0] leftID; [1] middleID; [2] rightID; (0~36)

    public IComponent Clone()
    {
        MusicBoardComponent component = new MusicBoardComponent();
        for (int i = 0; i < audioIDs.Length; i++)
        {
            component.audioIDs[i] = this.audioIDs[i];   
        }
        return component;
    }

    public BehaviorKV GetAttr()
    {
        MusicIDData data = new MusicIDData
        {
            lID = audioIDs[0],
            mID = audioIDs[1],
            rID = audioIDs[2],
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.MusicBoard,
            v = JsonConvert.SerializeObject(data)
        };
    }
}