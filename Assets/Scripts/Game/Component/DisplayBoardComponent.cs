using Newtonsoft.Json;

/// <summary>
/// Author:WenJia
/// Description:3D 展示板对应的存储数据的 Component，主要用于存储展板的数据，和进入场景时的Json还原
/// Date: 2022/1/5 17:48:26
/// </summary>

public class DisplayBoardComponent : IComponent
{
    public string userName;
    public string headUrl;
    public string userId;
    public IComponent Clone()
    {
        DisplayBoardComponent component = new DisplayBoardComponent();
        component.userId = userId;
        component.userName = userName;
        component.headUrl = headUrl;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        DisplayBoardData data = new DisplayBoardData
        {
            userId = userId,
            userName = userName,
            headUrl = headUrl
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.DisplayBoard,
            v = JsonConvert.SerializeObject(data)
        };
    }
}