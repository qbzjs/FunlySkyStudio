using Cinemachine;
using UnityEngine;

public class Test : MonoBehaviour
{
    public PlayModePanel _playModePanel;

    void Start()
    {
        var playController = new PlayModeController();
        var gameMode = GameMode.Play;
        var mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        var PlayVirCamera = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();

        GlobalFieldController.CurGameMode = gameMode;
        InputReceiver.locked = false;

        playController.joyStick = _playModePanel.joyStick;
        playController.SetCamera(mainCamera, PlayVirCamera);
        PlayVirCamera.enabled = true;
    }
}
