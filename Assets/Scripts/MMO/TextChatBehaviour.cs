using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

public enum ChatType
{
    TextChat,
    Purchased
}

public class TextChatBehaviour : MonoBehaviour
{
    private const float contentW = 5.8f;
    private const float contentH = 1.4f;
    private const float lineWidth = 5.07f;
    private GameObject chatAxis;
    private Transform chatContent;
    private SuperTextMesh charText;
    private UserInfo syncPlayerInfo;
    private SpriteRenderer chatAxisContent;
    private AnimationController animCtr;
    private GameObject chatRoot;
    private GameObject purchaseRoot;
    private void Awake()
    {
        chatAxis = transform.Find("chataxis").gameObject;
        chatRoot = chatAxis.transform.Find("chat").gameObject;
        purchaseRoot = chatAxis.transform.Find("purchased").gameObject;
        charText = chatRoot.GetComponentInChildren<SuperTextMesh>();
        chatAxisContent = charText.transform.parent.GetComponent<SpriteRenderer>();
        chatContent = charText.transform.parent.GetComponent<Transform>();
        chatAxis.gameObject.SetActive(false);
        animCtr = transform.GetComponentInChildren<AnimationController>();
        var playerDataCom = transform.GetComponentInChildren<PlayerData>();
        if(playerDataCom == null)
        {
            playerDataCom = transform.parent.GetComponentInChildren<PlayerData>();
        }
        syncPlayerInfo = playerDataCom.syncPlayerInfo;
    }

    public void OnRecChat(RoomChatResp data)
    {
        RoomChatData roomChatData = JsonConvert.DeserializeObject<RoomChatData>(data.Msg);
        
        
        switch ((RecChatType)roomChatData.msgType)
        {
            case RecChatType.TextChat:
                DefeatMsg defeatMsg = JsonConvert.DeserializeObject<DefeatMsg>(roomChatData.data);
                if (defeatMsg == null)
                {
                    return;
                }
                if(animCtr != null)
                {
                    ResizeChatAxis(defeatMsg.msg);
                }
                ChatPanelManager.Inst.UpdataeLastChatId(defeatMsg.chatId);
                break;
            case RecChatType.Emo:
                chatAxis.SetActive(false);
                CancelInvoke("HideText");
                Invoke("HideText", 5);
                break;
        }
    }
    
    private void ActiveChat(ChatType recChatType)
    {
        chatRoot.SetActive(recChatType == ChatType.TextChat);
        purchaseRoot.SetActive(recChatType == ChatType.Purchased);
    }

    private void HideText()
    {
        chatAxis.gameObject.SetActive(false);
    }

    public void ResizeChatAxis(string data)
    {
        string str = DataUtils.FilterNonStandardText(data);
        if (string.IsNullOrEmpty(str))
        {
            //修复 显示空消息
            return;
        }
        ActiveChat(ChatType.TextChat);
        chatAxis.SetActive(true);
        CancelInvoke("HideText");
        Invoke("HideText", 5);
        //根据系统语言切换字体
        LocalizationConManager.Inst.SetLocalizedContent(charText, "{0}", str);
        ReChatProperty(charText.preferredHeight);
    }

    public void SetPurchaseText(string data)
    {
        chatAxis.SetActive(true);
        ActiveChat(ChatType.Purchased);
        var purchaseData = JsonConvert.DeserializeObject<PurchasedTextData>(data);
        purchaseRoot.GetComponent<PurchaseText>()
            .SetContent(purchaseData.source, purchaseData.goods, purchaseData.imgUrl);
        CancelInvoke("HideText");
        Invoke("HideText", 5);
    }
    
    public void ReChatProperty(float row)
    {
        if (row <= 1)
        {
            chatAxisContent.size = new Vector2(contentW, 1.4f);
            chatContent.localPosition = new Vector3(-1.453f, 1.036f, chatContent.localPosition.z);
        }
        else if (1 < row && row <= 2)
        {
            chatAxisContent.size = new Vector2(contentW, 1.85f);
            chatContent.localPosition = new Vector3(-1.453f, 0.976f, chatContent.localPosition.z);
        }
        else if (2 < row && row <= 3)
        {
            chatAxisContent.size = new Vector2(contentW, 2.33f);
            chatContent.localPosition = new Vector3(-1.453f, 0.916f, chatContent.localPosition.z);
        }
        else if (3 < row && row <= 4)
        {
            chatAxisContent.size = new Vector2(contentW, 2.8f);
            chatContent.localPosition = new Vector3(-1.453f, 0.861f, chatContent.localPosition.z);
        }
        else if (4 < row && row <= 5)
        {
            chatAxisContent.size = new Vector2(contentW, 3.25f);
            chatContent.localPosition = new Vector3(-1.453f, 0.806f, chatContent.localPosition.z);
        }
        else if (5 < row && row <= 6)
        {
            chatAxisContent.size = new Vector2(contentW, 3.71f);
            chatContent.localPosition = new Vector3(-1.453f, 0.75f, chatContent.localPosition.z);
        }
    }


}
