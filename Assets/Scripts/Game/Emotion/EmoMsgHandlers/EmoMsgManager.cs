using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public class EmoMsgManager : CInstance<EmoMsgManager>
{
    public EmoMsgHandlerBase GetEmoMsgHandler(bool isSelf, IPlayerController pCtrl, PlayerBaseControl pbCtrl, Item itemData, EmoItemData playEmoData, AnimationController ac, TextChatBehaviour tcBev)
    {
        var emoType = (EmoType) itemData.type;

        switch (emoType)
        {
            case EmoType.Mutual:
                return new EmoMsgMutualHandler(isSelf, pCtrl, pbCtrl, itemData, playEmoData, ac, tcBev);
            case EmoType.LoopMutual:
                return new EmoMsgLoopMutualHandler(isSelf, pCtrl, pbCtrl, itemData, playEmoData, ac, tcBev);
            case EmoType.StateEmo:
                return new EmoMsgStateEmoHandler(isSelf, pCtrl, pbCtrl, itemData, playEmoData, ac, tcBev);
            default:
                return new EmoMsgNormalHandler(isSelf, pCtrl, pbCtrl, itemData, playEmoData, ac, tcBev);
        }
    }

    public bool CallEmoMsgHandler(EmoMsgHandlerBase emoMsgHandler, OptType optType)
    {
        switch (optType)
        {
            case OptType.Release:
                return emoMsgHandler.OnEmoOptRelease();
            case OptType.Cancel:
                return emoMsgHandler.OnEmoOptCancel();
            case OptType.MutualFin:
                return emoMsgHandler.OnEmoOptMutualFin();
            case OptType.Interacting:
                return emoMsgHandler.OnEmoOptInteracting();
            default:
                return true;
        }
    }

    //处理表情相关的断线重连
    public void OnPlayerCustomData(PlayerCustomData playerCustomData, AnimationController animCtr, bool isTps=true)
    {
        LoggerUtils.Log("EmoMsgManager OnPlayerCustomData=>" + JsonConvert.SerializeObject(playerCustomData));
        if (playerCustomData != null)
        {
            bool containJoinHands = false;
            var items = playerCustomData.items;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item.type == (int) EmoType.LoopMutual)
                    {
                        containJoinHands = true;
                    }
                    EmoItemData emoItemData = JsonConvert.DeserializeObject<EmoItemData>(item.data);
                    if (item.type == (int) EmoType.Loop || (item.type == (int) EmoType.Mutual
                                                            && emoItemData.opt == (int) OptType.Release)
                                                        || (item.type == (int) EmoType.LoopMutual
                                                            && emoItemData.opt == (int) OptType.Release))
                    {
                        // 断线重连后可能再收到GetItem，之前已经再做动作的不应该再播动画
                        if (animCtr.isLooping && animCtr.loopingInfo.id == item.id)
                        {
                            return;
                        }

                        animCtr.PlayAnim(item.id);
                    }
                    else if (item.type == (int) EmoType.LoopMutual
                             && emoItemData.opt == (int) OptType.Interacting)
                    {
                        //断线重连双人牵手动作状态恢复
                        MutualManager.Inst.PlayersHoldingHands(emoItemData.startPlayerId, emoItemData.followPlayerId);
                    }
                    else if (item.type == (int) EmoType.StateEmo && item.id == (int) EmoName.EMO_ADD_FRIEND)
                    {
                        //断线重连恢复加好友状态，不恢复动作
                        if (playerCustomData.playerId == Player.Id)
                        {
                            if (emoItemData.opt== (int)OptType.Release)
                            {
                                //收到状态表情发起广播，进入某种状态，显示 UI
                                StateEmoPanel.Show();
                                StateEmoPanel.Instance.SetIconHide();
                                StateEmoPanel.Instance.SetIcon((EmoName)item.id);
                                StateEmoPanel.Instance.SetCancelStateBtnClick(() =>
                                {
                                    PlayerEmojiControl.Inst.CancelStateEmo(item.id);
                                });
                                StateEmoPanel.Instance.SetIsTps(isTps);//
                                
                            }
                            else
                            {
                                StateEmoPanel.Hide();
                            }
                            //隐藏拾取按钮
                            if (CatchPanel.Instance)
                            {
                                CatchPanel.Hide();
                            }
                            PlayerEmojiControl.Inst.SetCurEmoData(item.id);
                        }
                        else
                        {
                            if (emoItemData.opt == (int)OptType.Release)
                            {
                                //其他玩家显示头顶状态表情icon
                                EmoMsgManager.Inst.SetOtherPlayerTouchable(playerCustomData.playerId, item.id, true);
                            }
                            else
                            {
                                EmoMsgManager.Inst.SetOtherPlayerTouchable(playerCustomData.playerId, item.id, false);
                            }
                        }

                        animCtr.SetStateEmo((EmoName)item.id);
                    }
                }
            }

            if (Player.Id == playerCustomData.playerId && !containJoinHands)
            {
                // 断线重连回来自己没有双人牵手的相关消息，则放手
                var sPlayerId = MutualManager.Inst.SearchHoldingHandsPlayers(Player.Id);
                if (!string.IsNullOrEmpty(sPlayerId))
                {
                    LoggerUtils.Log("========= PlayersReleaseHands ========= playerCustomData.playerId :" + playerCustomData.playerId);
                    var fPlayerId = MutualManager.Inst.holdingHandsPlayersDict[sPlayerId];
                    MutualManager.Inst.PlayersReleaseHands(sPlayerId, fPlayerId);
                }
            }
        }
    }


    /**
    * 设置其他玩家是否可以交互
    */
    public void SetOtherPlayerTouchable(string playerId, int emoId, bool state)
    {
        var player = ClientManager.Inst.GetOtherPlayerComById(playerId);
        if (player)
        {
            var touchBev = player.GetComponent<PlayerTouchBehaviour>();
            touchBev.SetCanTouch(state, emoId);
        }
    }
}