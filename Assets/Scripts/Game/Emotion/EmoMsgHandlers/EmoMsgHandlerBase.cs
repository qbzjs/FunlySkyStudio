/// <summary>
/// Author:Shaocheng
/// Description: 
/// Date: 
/// </summary>
public class EmoMsgHandlerBase
{
    public IPlayerController playerCtrl;
    public PlayerBaseControl playerBase;
    public Item itemData;
    public EmoItemData playEmoData;
    public AnimationController animCon;
    public TextChatBehaviour textCharBev;
    public bool IsSelf; //区分自己和OtherPlayer的广播

    public EmoMsgHandlerBase()
    {
    }

    public EmoMsgHandlerBase(bool isSelf, IPlayerController pCtrl, PlayerBaseControl pBase, Item iData, EmoItemData emoItemData, AnimationController animCtrl, TextChatBehaviour textChatBev)
    {
        this.playerCtrl = pCtrl;
        this.playerBase = pBase;
        this.playEmoData = emoItemData;
        this.itemData = iData;
        this.IsSelf = isSelf;
        this.animCon = animCtrl;
        this.textCharBev = textChatBev;
    }

    public virtual bool OnEmoOptRelease()
    {
        return true;
    }

    public virtual bool OnEmoOptCancel()
    {
        return true;
    }

    public virtual bool OnEmoOptMutualFin()
    {
        return true;
    }

    public virtual bool OnEmoOptInteracting()
    {
        return true;
    }
    public virtual bool IsNeedShowInChatWnd()
    {
        return true;
    }
    protected void CleanCurEmoData()
    {
        PlayerEmojiControl emojiCtrl = playerCtrl as PlayerEmojiControl;
        EmoIconData emoData = MoveClipInfo.GetAnimName(itemData.id);
        if (emoData == null)
        {
            return;
        }
        if (emojiCtrl.mCurEmoDataNormal != null
            && emojiCtrl.mCurEmoDataNormal.emoType == emoData.emoType)
        {
            emojiCtrl.mCurEmoDataNormal = null;
            emojiCtrl.mCurEmoData = null;
        }
    }
}