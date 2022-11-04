public interface IPlayerController
{
    void OnRoomChat(RoomChatResp resp);
    void OnRoomCustom(string playerId, RoomChatCustomData customData);
    void OnGetPlayerCustomData(PlayerCustomData playerCustomData);

    void SetEmoInteractState(bool state);
    bool GetEmoInteractState();
}