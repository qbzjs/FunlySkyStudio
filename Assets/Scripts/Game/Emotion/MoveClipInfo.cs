using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveClipInfo
{
    public static readonly int StartTalkAnim = 1001;
    public static readonly int StopTalkAnim = 1002;
    private static List<EmoIconData> emoicons;
    public static  List<EmoIconData> emoIcons
    {
        get
        {
            if (emoicons == null)
            {
                emoicons = ResManager.Inst.LoadJsonRes<EmoConfigData>("Configs/Emotion/EmoConfigData").emoIcons;

            }
            return emoicons;
        }

    }
    private static List<MutualFinEmoData> mutualFinEmos;
    public static List<MutualFinEmoData> MutualFinEmos
    {
        get
        {
            if (mutualFinEmos == null)
            {
                mutualFinEmos = ResManager.Inst.LoadJsonRes<MutualFinEmoConfig>("Configs/Emotion/MutualFinEmoConfig").mutualFinEmos;
            }
            return mutualFinEmos;
        }
    }

    public static EmoIconData GetAnimName(int anim)
    {
     
        for (int i = 0; i < emoIcons.Count; i++)
        {
            if (emoIcons[i].id == anim)
            {
                return emoIcons[i];
            }
            
        }
        return null;
        //switch (info.name)
        //{
        //    //case "doubt":
        //    case "sleep":
        //    case "laugh":
        //    case "pudency":
        //    case "rejoice_02":
        //        info.loopTimes = 2;
        //        break;
        //}
        
    }
    public static EmoIconData GetAnimName(string anim)
    {

        for (int i = 0; i < emoIcons.Count; i++)
        {
            if (emoIcons[i].name == anim)
            {
                return emoIcons[i];
            }

        }
        return null;
       

    }
    public static MutualFinEmoData GetMutualFinAnim(int anim)
    {
        for (int i = 0; i < MutualFinEmos.Count; i++)
        {
            if (MutualFinEmos[i].id == anim)
            {
                return MutualFinEmos[i];
            }
        }
        return null;
    }

    public static string GetName(int id)
    {
        for (int i = 0; i < emoIcons.Count; i++)
        {
            if (emoIcons[i].id == id)
            {
                return emoIcons[i].spriteName;
            }

        }
        return "";
    }
}


public class EmoIconData
{
    public int id;
    public string name;
    public string spriteName;
    public BandBody[] bandBody;
    public float delateTime;
    public int hasFaceAnim;
    public int normalMove; // 0-普通动作 1-特殊动作(显示角标)

    public int effectCount = 1; //特效数量，国内已支持多特效
    public int randomCount;
    public int noLoop;  //0-普通动作 1-循环动作
    
    public int emoIcon;  //动作EmoIcon编号：默认0
    public int emoType; // 表情分类
    public bool collected; // 是否收藏
    public float faceEndTime;//表情结束时间

    public int emoMutualIcon; // 双人交互表情，交互成功时的聊天框显示的 icon
    public int emoPortalIcon; // 动作发起显示交互icon(仅限双人交互动作)

    public float moveEndTime;//动作结束时间（目前动态读取赋值，无需配置）（特效暂时使用该字段）

    public EmoIconData[] moveInfos;//一组动作的子动作数组，
    public EmoIconData[] endMoveInfos;//一组动作的结束子动作数组，
    public EmoIconData nextEmoInfo;//下一个动作

    public int isBodyLoop;//该动作是否循环  0-否，1-是
    public int isFaceLoop;//该表情是否循环  0-否，1-是
    public int isEffectLoop;//该特效是否循环  0-否，1-是
    public string playEffectAnim;//该特效是否需要播放指定动画，无则播放默认
    public int isNew;//是否是新表情，用来控制红点显示  TODO:红点系统上线后可优化  0-否，1-是
    public int actionProtect; //动画保护时间，服务器根据这个时间来控制同一时间只有一次交互

    public int specialType; //1舞刀 2收集宝石

    public bool isAbRes; //是否动态加载动画

    public EmoType GetEmoType()
    {
        if (id == (int)EmoName.EMO_JOIN_HAND) //目前仅双人牵手是双人循环动作,故单独判断双人牵手(暂时写死)
        {
            return EmoType.LoopMutual;  //双人循环动作
        }
        if (emoType == 3)
        {
            return EmoType.Mutual;  //双人动作
        }
        if (emoType == 5)
        {
            return EmoType.StateEmo;  //状态表情动作
        }
        if (noLoop == 1)
        {
            return EmoType.Loop;
        }
        else if (randomCount > 0)
        {
            return EmoType.Random;
        }
        
        return EmoType.Normal;
    }
}

public class MutualFinEmoData
{
    public int id;
    public string interactPos;  //动作交互位置(以发起者为参照)
    public string interactRot;  //动作交互旋转(以发起者为参照)
    public string strEndName;  //发起者动画结尾名称
    public string finEndName;  //完成者动画结尾名称
}


public class EmoConfigData
{
    public List<EmoIconData> emoIcons = new List<EmoIconData>();
}
public class MutualFinEmoConfig
{
    public List<MutualFinEmoData> mutualFinEmos = new List<MutualFinEmoData>();
}
public class BandBody
{
    public int id;
    public int bandNode;
    public Vec3 r;
    public Vec3 s;
    public Vec3 p;
}
public enum BodyNode
{
    RightHand = 1,
    LeftHand = 2,
    PickNode = 3,
    PickPos = 4,
    FoodNode = 5,
    BackNode = 6,//降落伞背包节点位置
    LEffectNode = 7,
    BackDeckNode = 8,//背包节点位置
}
public enum EmoType
{
    Normal,
    Loop,
    Random,
    Mutual,
    LoopMutual, // 双人循环(例如：牵手)
    StateEmo,//唤起Emo状态，非立即执行
}
