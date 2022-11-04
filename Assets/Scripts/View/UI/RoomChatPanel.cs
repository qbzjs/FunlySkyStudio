using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public enum RecChatType
{
    JoinRoom = 1001,
    LeaveRoom = 1002,
    TextChat = 1,
    Emo = 2,
    HitTrap = 3,//陷进盒伤害来源
    RoomAttrs = 4,
    GetItems = 5,
    Items = 6,//目前通用同步状态的SYNC_ITEMS


    /// <summary>
    /// ×Ô¶¨Òå×Ö¶Î£¬·þÎñÆ÷Ö»¹ã²¥£¬²»×öÈÎºÎ´¦Àí
    /// </summary>
    Custom = 7,
    //PVP
    PVPBoardCast = 8,
    PVPRoomState = 9,
    ChangeCloth = 10,  //场景内换装
    Portal = 11, //穿越传送门
    SwitchHoldItem = 12, //切换手持道具
    SendLatency = 13, // 延时 ping 值上报联机
    GetMsgs = 14,//断线重连拉取聊天信息
    ServerNotification = 16,//服务器主动通知客户端
    Promote = 17, // 场景内带货
    Firework = 18,//播放烟花
    UnFreeze = 19,//解冻
    Fishing = 20, // 钓鱼
    Seesaw = 23, // 跷跷板
    VIPZone = 24, // VIP 区域
    Sword = 250, //舞刀
    Swing = 26, // 跷跷板
    PlayerPos = 25,//更新玩家位置信息

    IceGem = 27, //冰晶宝石
}
public enum CustomType
{
    Jump = 1,
    Talk = 2,
    Bounceplank = 3
}
public class VerticalLayoutSetting
{
    public float paddingTop;
    public float gap;

    public VerticalLayoutSetting(float _top, float _gap)
    {
        paddingTop = _top;
        gap = _gap;
    }
}

public class RoomChatPanel : BasePanel<RoomChatPanel>
{
    public ScrollRect ChatFrame;
    public Image RedImg;
    public Button dialologBtn;
    public RectTransform content;
    public ScrollRect scroll;
    private const int maxMsgCount = 100;

    public List<ChatItemData> chatItemDatas = new List<ChatItemData>();
    private List<RoomChatItem> recycleItemPool = new List<RoomChatItem>();
    private List<RoomChatItem> currentShowingItem = new List<RoomChatItem>();
    private List<RoomChatItem> lastShowingItem = new List<RoomChatItem>();

    private VerticalLayoutSetting verticalLayout;
    #region 刷新关键参数
    int startIndex = -1;
    float startYPos = -1;
    float contentYPos = 0;
    float yPos = 0;
    float maxYPos = 0; 
    #endregion
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        verticalLayout = new VerticalLayoutSetting(6, 7.2f);
        scroll.onValueChanged.AddListener(OnScrollChanged);
        dialologBtn.onClick.AddListener(OnDialogBtnClick);
        RedImg.gameObject.SetActive(false);
        ChatFrame.gameObject.SetActive(true);
        ChatFrame.verticalNormalizedPosition = 0f;
        dialologBtn.gameObject.SetActive(true);
    }
    public void OnScrollChanged(Vector2 pos)
    {
        RefreshContent();
    }
    /// <summary>
    /// 聊天按钮点击事件
    /// </summary>
    private void OnDialogBtnClick()
    {
        if (ChatFrame.gameObject.activeSelf)
        {
            ChatFrame.gameObject.SetActive(false);
        }
        else
        {
            ResizeContentArea();
            RefreshContent();
            ChatFrame.gameObject.SetActive(true);
            RedImg.gameObject.SetActive(false);
            ChatFrame.verticalNormalizedPosition = 0f;
        }
    }
    /// <summary>
    /// 下一帧刷新scrollView滑到底部
    /// </summary>
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        ChatFrame.verticalNormalizedPosition = 0f;
    }
    public void SetRecChat(RecChatType recType, string userName, string content = "")
    {
        switch (recType)
        {
            case RecChatType.JoinRoom:
                var joinMsg = LocalizationConManager.Inst.GetLocalizedText("joined the server");
                AddMessage(joinMsg, GetUserName(userName));
                break;
            case RecChatType.LeaveRoom:
                var leftMsg = LocalizationConManager.Inst.GetLocalizedText("left the server");
                AddMessage(leftMsg, GetUserName(userName));
                break;
            case RecChatType.TextChat:
                string str = DataUtils.FilterNonStandardText(content);
                str = DataUtils.ReplaceRichText(str);
                if (string.IsNullOrEmpty(str))
                {
                    //修复 显示空消息
                    return;
                }
                AddMessage(str, GetUserName(userName));
                break;
            case RecChatType.Emo:
                string  stPlayerName = null;
                var itemData = JsonConvert.DeserializeObject<Item>(content);
                var playEmoData = JsonConvert.DeserializeObject<EmoItemData>(itemData.data);
                if (playEmoData.opt == (int)OptType.Cancel)
                {
                    return; //循环动作取消，不在聊天框显示任何消息
                }
                var emoData = MoveClipInfo.GetAnimName(itemData.id);
                if (emoData == null)
                {
                    return;
                }
                var iconId = emoData.emoIcon;
                if (playEmoData.opt == (int)OptType.MutualFin || playEmoData.opt == (int)OptType.Interacting)
                {
                    //完成双人动作，聊天框消息特殊情况
                    var stPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playEmoData.startPlayerId);
                    stPlayerName = stPlayerInfo == null ? " " : stPlayerInfo.userName; //发起者名字
                    iconId = emoData.emoMutualIcon;
                }
                if (string.IsNullOrEmpty(emoData.spriteName))
                {
                    return; //排除不在面板显示的emo
                }
                var emoName= LocalizationConManager.Inst.GetLocalizedText(emoData.spriteName);
                AddMessage(emoName, GetUserName(userName, stPlayerName), iconId);
                break;
            case RecChatType.PVPBoardCast:
                string[] players = content.Split('|');
                string defeats = " " + LocalizationConManager.Inst.GetLocalizedText("defeats") + " ";
                if (players[0] != null && players[1] != null)
                {
                    var msg = ": " + players[0] + defeats + players[1];
                    AddMessage(msg, "", (int)global::RoomChatItem.EmoIcon.BoardCast);
                }
                break;
            case RecChatType.HitTrap:
                string outsmsg = ": " + content + " " + LocalizationConManager.Inst.GetLocalizedText("out!");
                AddMessage(outsmsg, "", (int)global::RoomChatItem.EmoIcon.BoardCast);
                break;
            case RecChatType.Promote:
                string promotemsg = LocalizationConManager.Inst.GetLocalizedText("is promoting");
                AddMessage(promotemsg, GetUserName(userName));
                break;
            case RecChatType.Sword:
                string swordemsg = LocalizationConManager.Inst.GetLocalizedText("is playing sword");
                AddMessage(swordemsg, GetUserName(userName));
                break;
            default:
                //修复 避免展示非预期内消息
                return;
        }
        if (gameObject.activeSelf)
        {
            StartCoroutine("ScrollToBottom");
        }
        if (!ChatFrame.gameObject.activeSelf)
        {
            RedImg.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 获取userName的显示
    /// </summary>
    private string GetUserName(string name, string stPlayerName=null)
    {
        string userName;
        string limitName=name;
        if (name.Length>=15)
        {
            limitName = name.Substring(0, 15) + "...";//限制userName长度
        }
        var playerName= GameManager.Inst.ugcUserInfo.userName;
        userName = name == playerName ? "<color=#B5ABFF>" + "[" + limitName + "]: " + "</color>" : "[" + limitName + "]: ";
        if (stPlayerName!=null)
        {
            var limitstPlayerName = stPlayerName;
            if (stPlayerName.Length>=15)
            {
                limitstPlayerName = stPlayerName.Substring(0, 15) + "...";
            }
            var twoName = limitstPlayerName +"\u00A0"+"&" +"\u00A0"+ limitName;
            userName = name == playerName || stPlayerName== playerName ? "<color=#B5ABFF>"+"["+ twoName +"]: "+ "</color>" : "[" + twoName + "]: ";
        }
        return userName;
    }

    public float GetItemHeight(GameObject go, string msg)
    {
        RectTransform itemRect = go.GetComponent<RectTransform>();
        Text msgText = go.GetComponent<Text>();
        LocalizationConManager.Inst.SetLocalizedContent(msgText, "{0}", msg);
        float height = msgText.preferredHeight;
        itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        return height;
    }
    /// <summary>
    /// 添加新消息数据
    /// </summary>
    public void AddMessage(string msg, string userName, int iconId = -1)
    {
        GameObject go;
        if (recycleItemPool.Count > 0)
            go = recycleItemPool[0].gameObject;
        else
        {
            GameObject itemPrefab = Resources.Load("Prefabs/UI/Panel/RoomChatItem") as GameObject;
            go = Instantiate(itemPrefab, content.transform);
            RoomChatItem item = go.GetComponent<RoomChatItem>();
            recycleItemPool.Add(item);
        }
        var message = iconId == -1 ? userName + msg : userName + "      " + msg;
        float height = GetItemHeight(go, message);
        go.SetActive(false);
        ChatItemData data = new ChatItemData(message, userName, height, iconId);
        chatItemDatas.Add(data);
        if (chatItemDatas.Count > maxMsgCount)
        {
            chatItemDatas.RemoveAt(0);
        }
        ResizeContentArea();
        RefreshContent();
    }
    /// <summary>
    /// 在回收池中通过index得到Item
    /// </summary>
    public RoomChatItem GetItem(int index)
    {
        RoomChatItem item;
        for (int i = 0; i < lastShowingItem.Count; ++i)
        {
            if (lastShowingItem[i].data == chatItemDatas[index])
            {
                item = lastShowingItem[i];
                lastShowingItem.RemoveAt(i);
                return item;
            }
        }
        if (recycleItemPool.Count > 0)
        {
            for (int i = 0; i < recycleItemPool.Count; ++i)
            {
                if (recycleItemPool[i].data == chatItemDatas[index])
                {
                    item = recycleItemPool[i];
                    recycleItemPool.RemoveAt(i);
                    return item;
                }
            }
            item = recycleItemPool[0];
            recycleItemPool.RemoveAt(0);
        }
        else
        {
            GameObject itemPrefab = Resources.Load("Prefabs/UI/Panel/RoomChatItem") as GameObject;
            GameObject go = Instantiate(itemPrefab, content);
            go.transform.SetParent(content);
            go.transform.localPosition = Vector3.zero;
            item = go.GetComponent<RoomChatItem>();
        }
        item.data = chatItemDatas[index];
        return item;
    }

    /// <summary>
    /// 刷新Scroll内容显示
    /// </summary>
    public void RefreshContent()
    {
        ResizeContentArea();
        contentYPos = content.localPosition.y;
        if (contentYPos < 0)
            contentYPos = 0;
        else
        {   
            if (contentYPos > content.rect.height - scroll.viewport.rect.height)
                contentYPos = content.rect.height - scroll.viewport.rect.height;
        }
        startIndex = -1;
        startYPos = -1;
        yPos = verticalLayout.paddingTop;
        maxYPos = contentYPos + scroll.viewport.rect.height;
        for (int i = 0; i < chatItemDatas.Count; ++i)
        {
            yPos += chatItemDatas[i].height;
            if (yPos > contentYPos)
            {
                startIndex = i;
                startYPos = yPos - chatItemDatas[i].height;
                break;
            }
            yPos += verticalLayout.gap;
            if (yPos > contentYPos)
            {
                if (yPos >= maxYPos)
                {
                    startIndex = -1;
                    break;
                }
                else
                {
                    startIndex = i + 1;
                    startYPos = yPos;
                    break;
                }
            }
        }
        lastShowingItem.Clear();
        for (int i = 0; i < currentShowingItem.Count; ++i)
        {
            lastShowingItem.Add(currentShowingItem[i]);
        }
        currentShowingItem.Clear();

        if (startIndex != -1)
        {

            while (startYPos < maxYPos && startIndex < chatItemDatas.Count)
            {
                RoomChatItem item = GetItem(startIndex);
                item.gameObject.SetActive(true);
                item.UpdateItem();
                item.transform.localPosition = new Vector3(0, -startYPos, 0);
                currentShowingItem.Add(item);
                ++startIndex;
                startYPos += item.data.height;
                startYPos += verticalLayout.gap;
            }
        }
        for (int i = 0; i < lastShowingItem.Count; ++i)
        {
            recycleItemPool.Add(lastShowingItem[i]);
            lastShowingItem[i].gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 计算Content高度
    /// </summary>
    public void ResizeContentArea()
    {
        float totalHeight = verticalLayout.paddingTop;
        float count = chatItemDatas.Count;
        for (int i = 0; i < count; ++i)
        {
            totalHeight += chatItemDatas[i].height;
            if (i < count - 1)           
                totalHeight += verticalLayout.gap;
        }
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }
    /// <summary>
    /// 断线重连时刷新数据
    /// </summary>
    public void GetChatMsgData(GetMsgsRsp getMsgsRsp)
    {
        chatMsgs[] chatMsgs = getMsgsRsp.chatMsgs.Clone() as chatMsgs[];
        Array.Sort(chatMsgs, (msga, msgb) => (int)msga.id - (int)msgb.id);
        LoggerUtils.Log("===getchatMsgs.Length" + chatMsgs.Length);
        for (int i = 0; i < chatMsgs.Length; i++)
        {
            if (chatMsgs[i].id > ChatPanelManager.Inst.lastChatId)
            {
                RecChatType recChatType = (RecChatType)chatMsgs[i].type;
                string content = "";
                string userName = "";
                switch (recChatType)
                {
                    case RecChatType.JoinRoom:
                    case RecChatType.LeaveRoom:
                        userName = chatMsgs[i].msg;//msg为playerName(服务器主动推送消息)
                        break;
                    case RecChatType.TextChat:
                        userName = chatMsgs[i].playerName;//玩家名
                        content = chatMsgs[i].msg;//msg为玩家所发消息内容
                        break;
                    case RecChatType.PVPBoardCast:
                    case RecChatType.HitTrap:
                        content = chatMsgs[i].msg;//msg为玩家名
                        break;
                    default:
                        break;
                }
                if (userName != null && content != null)
                {
                    SetRecChat(recChatType, userName, content);
                }
                ChatPanelManager.Inst.lastChatId = chatMsgs[i].id;
            }
        }
        LoggerUtils.Log("===chatItemDatas.Count" + chatItemDatas.Count);
    }

    public void ChangeSiblingIndex(int siblingIndex)
    {
        transform.SetSiblingIndex(siblingIndex);
    }
    public void SetChatFormActive(bool isActive)
    {
        ChatFrame.gameObject.SetActive(isActive);
    }
}
