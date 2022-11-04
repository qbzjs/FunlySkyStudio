using BudEngine.NetEngine;

/// <summary>
/// Author:Shaocheng
/// Description: 双人循环表情动作(双人牵手)的广播处理 原双人牵手@wenjia
/// Date: 2022-7-7 18:18:57
/// </summary>
public class EmoMsgLoopMutualHandler : EmoMsgHandlerBase
{
    public EmoMsgLoopMutualHandler()
    {
    }

    public EmoMsgLoopMutualHandler(bool isSelf, IPlayerController pCtrl, PlayerBaseControl pBase, Item iData, EmoItemData emoItemData, AnimationController animCtrl, TextChatBehaviour textChatBev)
        : base(isSelf, pCtrl, pBase, iData, emoItemData, animCtrl, textChatBev)
    {
    }

    public override bool OnEmoOptRelease()
    {
        // 自己做动作客户端直接表现，只处理其他玩家的牵手动作发起广播
        if (IsSelf == false)
        {
            animCon.PlayAnim(itemData.id, playEmoData.random);
        }

        return true;
    }

    public override bool OnEmoOptCancel()
    {
        //牵手取消，需要做取消处理
        // PlayerControl.Inst.EndMutual();
        MutualManager.Inst.PlayersReleaseHands(playEmoData.startPlayerId, playEmoData.followPlayerId);

        if (IsSelf == false)
        {
            animCon.RecStopLoop();
        }

        return true;
    }

    public override bool OnEmoOptMutualFin()
    {
        return true;
    }

    public override bool OnEmoOptInteracting()
    {
        if (IsSelf)
        {
            /********************************收到自己的响应牵手广播*******************************/

            if (playEmoData.startPlayerId != Player.Id
                && MutualManager.Inst.holdingHandsPlayersDict.ContainsKey(playEmoData.startPlayerId)
                && MutualManager.Inst.holdingHandsPlayersDict[playEmoData.startPlayerId] != Player.Id)
            {
                var stPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playEmoData.startPlayerId);
                var stPlayerName = stPlayerInfo == null ? " " : stPlayerInfo.userName;
                TipPanel.ShowToast("{0} has cancelled or interacted with other players!", stPlayerName);
                return false;
            }
        }
        else
        {
            /********************************收到其他人的响应牵手广播*******************************/

            if (animCon.isInteracting)
            {
                return false;
            }

            if (MagneticBoardManager.Inst.IsOtherPlayerOnBoard(playerCtrl as OtherPlayerCtr))
            {
                return false;
            }

            if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(playerCtrl as OtherPlayerCtr))
            {
                return false;
            }

            var iPlayerController = playEmoData.startPlayerId == Player.Id ? (IPlayerController) PlayerEmojiControl.Inst : ClientManager.Inst.GetOtherPlayerComById(playEmoData.startPlayerId);
            if (iPlayerController == null || !iPlayerController.GetEmoInteractState())
            {
                return false;
            }
        }

        MutualManager.Inst.PlayersHoldingHands(playEmoData.startPlayerId, playEmoData.followPlayerId);

        return true;
    }
}