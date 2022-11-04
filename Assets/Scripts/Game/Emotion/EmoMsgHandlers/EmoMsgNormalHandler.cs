/// <summary>
/// Author:Shaocheng
/// Description: 普通单人表情广播接收，包括: 单人表情、循环表情
/// Date: 2022-7-7 18:16:22
/// </summary>
public class EmoMsgNormalHandler : EmoMsgHandlerBase
{
    public EmoMsgNormalHandler()
    {
    }

    public EmoMsgNormalHandler(bool isSelf, IPlayerController pCtrl, PlayerBaseControl pBase, Item iData, EmoItemData emoItemData, AnimationController animCtrl, TextChatBehaviour textChatBev)
        : base(isSelf, pCtrl, pBase, iData, emoItemData, animCtrl, textChatBev)
    {
    }

    public override bool OnEmoOptRelease()
    {
        if (IsSelf == false)
        {
            if ((OptType) playEmoData.opt != OptType.Cancel && PlayerMutualControl.Inst
                                                            && playEmoData.startPlayerId == PlayerMutualControl.Inst.startPlayerId
                                                            && (itemData.type != (int) EmoType.Mutual && itemData.type != (int) EmoType.LoopMutual))
            {
                //被牵手时，和发起者做相同的表情动作
                PlayerMutualControl.Inst.SelfPlayerFollowEmote(itemData.id);
            }
            animCon.PlayAnim(itemData.id, playEmoData.random);
        }
        
        return true;
    }

    public override bool OnEmoOptCancel()
    {
        //单人动作结束客户端直接表现，所以回包时只用处理其他人的
        if (IsSelf == false)
        {
            animCon.RecStopLoop();
            return true;
        }

        return true;
    }

    public override bool OnEmoOptMutualFin()
    {
        return true;
    }

    public override bool OnEmoOptInteracting()
    {
        return true;
    }
}