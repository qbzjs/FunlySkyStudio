using System;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src.BattleGame;
using Newtonsoft.Json;
using UnityEngine.UI;

public class PVPWaitPanel : BasePVPGamePanel
{
    public Button RoomPlayers;
    public Text CurPlayerCountText;
    public Text MaxPlayerCountText;

    private void Start()
    {  
        MessageHelper.AddListener(MessageName.PlayerCreate, UpdatePlayerCount);
        MessageHelper.AddListener(MessageName.StartGameOnLine, UpdatePlayerCount);
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, LeaveUpdatePlayerCount);
        RoomPlayers.onClick.AddListener(OnRoomPlayerClick);
    }

    private void OnDestroy()
    {
        MessageHelper.RemoveListener(MessageName.PlayerCreate, UpdatePlayerCount);
        MessageHelper.RemoveListener(MessageName.StartGameOnLine, UpdatePlayerCount);
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, LeaveUpdatePlayerCount);
    }

    public override void Enter(PVPGameConnectEnum connect)
    {
        UpdatePlayerCount();
        RoomPlayers.gameObject.SetActive(true);
    }

    private void OnRoomPlayerClick()
    {
        RoomMenuPanel.Show();
    }
    private void LeaveUpdatePlayerCount(string count)
    {
        UpdatePlayerCount();
    }
    private void UpdatePlayerCount()
    {
        if (Global.Room!= null && Global.Room.RoomInfo != null && Global.Room.RoomInfo.PlayerList != null)
        {
            int count = Global.Room.RoomInfo.PlayerList.Count;
            CurPlayerCountText.text = count.ToString();
            MaxPlayerCountText.text = Global.Room.RoomInfo.MaxPlayers.ToString();
        }
    }
        
    public override void Leave()
    {
        RoomPlayers.gameObject.SetActive(false);
    }
    
}