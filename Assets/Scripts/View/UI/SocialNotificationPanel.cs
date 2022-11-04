/// <summary>
/// Author:WeiXin
/// Description:
/// Date: 2022/4/8 14:45:52
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocialNotificationPanel : BasePanel<SocialNotificationPanel>
{
    [SerializeField] private RawImage avatar;
    [SerializeField] private Text tips;
    [SerializeField] private Text txtFriend;
    [SerializeField] private Text txtFollow;
    [SerializeField] private Button btnFriend;
    [SerializeField] private Button btnFollow;
    [SerializeField] private Button btnBG;
    [SerializeField] private RectTransform bg;

    Coroutine cor;
    private float time = 7f;
    public SocialNotificationData data;

    public override void OnInitByCreate()
    {
        btnFriend.onClick.AddListener(FriendClick);
        btnFollow.onClick.AddListener(FollowClick);
        btnBG.onClick.AddListener(BGClick);
    }

    private void OnDestroy()
    {
        btnFriend.onClick.RemoveListener(FriendClick);
        btnFollow.onClick.RemoveListener(FollowClick);
        btnBG.onClick.RemoveListener(BGClick);
        if (cor != null)
        {
            StopCoroutine(cor);
            cor = null;
        }
    }

    public void InstertNotification()
    {
        if (data != null)
        {
            SocialNotificationManager.Inst.NotificationBack(data);
            data = null;
            StopCoroutine(cor);
            cor = null;
        }

        ShowNotification();
    }

    public void ShowNotification()
    {
        if (cor != null) return;

        var ntf = SocialNotificationManager.Inst.GetNotification();
        if (ntf == null)
        {
            data = null;
            cor = null;
            Hide();
        }
        else
        {
            if (ntf.show)
            {
                data = ntf;
                cor = StartCoroutine(SetPanel(ntf));
            }
            else
            {
                ShowNotification();
            }
        }
    }

    private IEnumerator SetPanel(SocialNotificationData data)
    {
        UpdateUI(data);
        yield return new WaitForSeconds(time);
        cor = null;
        ShowNotification();
    }

    public void UpdateUI(SocialNotificationData data)
    {
        if (!data.show && cor != null)
        {
            StopCoroutine(cor);
            cor = null;
            ShowNotification();
        }
        transform.SetAsLastSibling();
        LocalizationConManager.Inst.SetSystemTextFont(tips);
        tips.text = data.tips;
        // tips.SetTextWithEllipsis(data.tips);
        LocalizationConManager.Inst.SetLocalizedContent(txtFriend, data.friendStr);
        LocalizationConManager.Inst.SetLocalizedContent(txtFollow, data.followStr);
        btnFriend.gameObject.SetActive(data.showFriend);
        btnFollow.gameObject.SetActive(data.showFollow);
        btnFriend.interactable = data.enableFriend;
        btnFollow.interactable = data.enableFollow;
        var size = bg.sizeDelta;
        size.y = data.hight;
        bg.sizeDelta = size;
        SocialNotificationManager.Inst.LoadSprite(data.id, avatar);
        foreach (var v in transform.GetComponentsInChildren<RectTransform>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(v);
        }

        //若此时正在显示加好友emote的弹窗，社交模块移出屏幕外
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsStateEmoTipShow)
        {
            this.transform.GetComponent<RectTransform>().anchoredPosition = PlayerEmojiControl.Inst.v3OutScreen;
        }
        else
        {
            this.transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
    }

    private void AddFriend(string id)
    {
        // SocialNotificationManager.Inst.SendRequest(id, 1, (int) data.ffdata.friend);
        SocialNotificationManager.Inst.SendRequestWeb(id, 2);
    }

    private void Follow(string id)
    {
        // SocialNotificationManager.Inst.SendRequest(id, 2, (int) data.ffdata.follow);
        SocialNotificationManager.Inst.SendRequestWeb(id, 0);
    }

    private void FriendClick()
    {
        var id = data?.id;
        if (btnFollow.gameObject.activeSelf && data.enableFollow)
        {
            data.enableFriend = false;
            btnFriend.interactable = data.enableFriend;
            data.friendStr = data.ffdata.friend == UserProfilePanel.FriendshipEnum.None ? "Pending" : "Message";
            LocalizationConManager.Inst.SetLocalizedContent(txtFriend, data.friendStr);
        }
        else
        {
            if (cor != null)
            {
                StopCoroutine(cor);
                cor = null;
            }

            ShowNotification();
        }

        AddFriend(id);
    }

    private void FollowClick()
    {
        var id = data?.id;
        if (btnFriend.gameObject.activeSelf && data.enableFriend)
        {
            data.enableFollow = false;
            btnFollow.interactable = data.enableFollow;
            data.followStr = data.ffdata.follow == UserProfilePanel.SubscribedEnum.None ? "Following" : "Mutual";
            LocalizationConManager.Inst.SetLocalizedContent(txtFollow, data.followStr);
        }
        else
        {
            if (cor != null)
            {
                StopCoroutine(cor);
                cor = null;
            }

            ShowNotification();
        }

        Follow(id);
    }

    private void BGClick()
    {
        UserProfilePanel.Show();
        UserProfilePanel.Instance.OnOpenPanel(data?.id);
        if (cor != null)
        {
            StopCoroutine(cor);
            cor = null;
        }

        ShowNotification();
    }


    private void NewPanelOpened()
    {
        transform.SetAsLastSibling();
    }
}

public static class TextExtension
{
    
    public static void SetTextWithEllipsis(this Text textComponent, string value)
    {
        // create generator with value and current Rect
        var generator = new TextGenerator();
        var rectTransform = textComponent.GetComponent<RectTransform>();
        var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
        generator.Populate(value, settings);

        // truncate visible value and add ellipsis
        var characterCountVisible = generator.characterCountVisible;
        var updatedText = value;
        if (value.Length > characterCountVisible)
        {
            updatedText = value.Substring(0, characterCountVisible - 1);
            updatedText += "…";
        }

        // update text
        textComponent.text = updatedText;
    }
    
    public static string GetTextWithEllipsisByWidth(Text textComponent, string value)
    {
        // create generator with value and current Rect
        var generator = new TextGenerator();
        var rectTransform = textComponent.GetComponent<RectTransform>();
        var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
        generator.Populate(value, settings);

        // truncate visible value and add ellipsis
        var characterCountVisible = generator.characterCountVisible;
        var updatedText = value;
        if (value.Length > characterCountVisible)
        {
            updatedText = value.Substring(0, characterCountVisible - 1);
            updatedText += "…";
        }

        return updatedText;
    }

    /// <summary>
    ///  Unity 2019 版本以上
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="value"></param>
    public static void SetTextWithEllipsis(this Text textComponent, string value, int characterVisibleCount)
    {
        var updatedText = value;

        // 判断是否需要过长显示省略号
        if (value.Length > characterVisibleCount)
        {
            updatedText = value.Substring(0, characterVisibleCount - 1);
            updatedText += "…";
        }

        // update text
        textComponent.text = updatedText;
    }
}