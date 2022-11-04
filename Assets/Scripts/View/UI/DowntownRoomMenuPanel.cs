using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DowntownRoomMenuPanel : MonoBehaviour
{
    public Button btnLeaveRoom;
    public Button btnHide;
    public Transform downtownContent;

    private void OnEnable()
    {
        btnLeaveRoom.gameObject.SetActive(GlobalFieldController.curMapMode == MapMode.Downtown);
    }

    public void InitData()
    {
        btnHide.onClick.AddListener(OnHideBtnClick);
        btnLeaveRoom.onClick.AddListener(ExitRoom);
    }

    private void OnHideBtnClick()
    {
        RoomMenuPanel.Instance.HidePanel();
    }

    private void OnBackToDowntownClick()
    {
        RoomMenuPanel.Instance.HidePanel();
    }

    private void ExitRoom()
    {
        RoomMenuPanel.Instance.HidePanel();
        ClientManager.Inst.SendPlayerLastPos();
        RoomMenuPanel.Instance.OnExitBtnClick();
    }
}
