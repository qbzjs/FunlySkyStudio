/// <summary>
/// Author:MeiMei—LiMei
/// Description:管理场景内聊天窗口的单条消息数据
/// Date: 2021-02-24
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public class ChatItemData
{
    public string Message;//消息
    public string UserName;//玩家名
    public float height; //该item所使用的prefab的总高度
    public int iconId = -1;//表情ID,-1表示无表情
    //public bool isSelf = false;//是否为自己发送
    public ChatItemData(string _msg, string _name, float _height, int _iconId)
    {
        UserName = _name;
        Message = _msg;
        iconId = _iconId;
        height = _height;
    }
}
public class RoomChatItem : MonoBehaviour
{
    public Text Message;
    public Image EmoImg;
    public Text UserName;
    private SpriteAtlas priAtlas;
    private EmoIcon curIcon=EmoIcon.Default;
    public ChatItemData data;

    private readonly Dictionary<EmoIcon, string> iconDic = new Dictionary<EmoIcon, string>()
    {
        {EmoIcon.Default, "emoji_icon"},
        {EmoIcon.OneHand, "onehand"},
        {EmoIcon.TwoHand, "twohand"},
        {EmoIcon.WaitHand, "ic_onehand"},
        {EmoIcon.JoinHand, "ic_twohands"},
        {EmoIcon.BoardCast, "ic_notice"}
    };
    public enum EmoIcon
    {
        Default,
        OneHand,
        TwoHand,
        WaitHand, // 等待牵手
        JoinHand, // 玩家牵手
        BoardCast // 广播Icon
    }
    public void UpdateItem()//更新Item的UI元素
    {
        LocalizationConManager.Inst.SetSystemTextFont(Message);
        UserName.text = data.UserName;
        Message.text = data.Message;
        EmoImg.gameObject.SetActive(data.iconId!=-1); 
        if (data.iconId != -1)
        {
            Vector3 vector3 = new Vector3(UserName.preferredWidth + 25f, -24.5f, 0f);
            if (UserName.preferredWidth >= 567)//换一行
            {
                vector3= new Vector3(UserName.preferredWidth - 567f + 25f, -83.5f, 0f);
            }
            EmoImg.transform.localPosition = vector3;
            var emoIcon = (EmoIcon)data.iconId;
            if (curIcon == emoIcon) return;
            if (priAtlas == null)
            {
                priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
            }
            EmoImg.sprite = priAtlas.GetSprite(iconDic[emoIcon]);
            curIcon = emoIcon;
        }
    }
}
