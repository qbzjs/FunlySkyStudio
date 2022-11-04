#if false
using System.Collections.Generic;
using UnityEngine;

namespace Snowfield
{
    public class SnowfieldRoom
    {
        public static SnowfieldRoom Instance = new SnowfieldRoom();

        private Dictionary<string, OtherPlayer> _others;

        public SnowfieldRoom()
        {
            _others = new Dictionary<string, OtherPlayer>();
        }

        public void Init()
        {
            _others.Clear();

            MessageHelper.AddListener<string, GameObject>(MessageName.OnRoomAddPlayer, HandleAddPlayer);
            MessageHelper.AddListener<string>(MessageName.OnRoomRemovePlayer, HandleRemovePlayer);
            MessageHelper.AddListener<string, int>(MessageName.OnRoomSyncFrameData, HandleSyncFrameData);
        }

        public void Destroy()
        {
            _others.Clear();

            MessageHelper.RemoveListener<string, GameObject>(MessageName.OnRoomAddPlayer, HandleAddPlayer);
            MessageHelper.RemoveListener<string>(MessageName.OnRoomRemovePlayer, HandleRemovePlayer);
            MessageHelper.RemoveListener<string, int>(MessageName.OnRoomSyncFrameData, HandleSyncFrameData);
        }

        private void HandleAddPlayer(string playId, GameObject model)
        {
            var animator = model.transform.GetComponent<Animator>();
            var player = new OtherPlayer(model, animator);
            var playerModel = model.AddComponent<OtherPlayerModel>();
            playerModel.SetData(player);

            _others[playId] = player;
        }

        private void HandleRemovePlayer(string playerId)
        {
            _others.Remove(playerId);
        }

        private void HandleSyncFrameData(string playerId, int state)
        {
            OtherPlayer player;
            if (_others.TryGetValue(playerId, out player))
                player.Get<PlayerFrameHander>().Sync(state);
        }
    }
}
#endif