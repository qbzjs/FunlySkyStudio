using Cinemachine;
using System.Collections;
using System.IO;
using UnityEngine;
using static PreviewHandler;

public class ResPreviewCtr : MonoBehaviour
{
    public InputReceiver inputReceiver;
    CinemachineTransposer ct;
    public CinemachineVirtualCamera mainVirCam;
    public Camera mainCamera;
    public CinemachineTransposer CamTransposer
    {
        get
        {
            if (!ct) ct = mainVirCam.GetCinemachineComponent<CinemachineTransposer>();
            return ct;
        }
    }

    public void SetPreviewHandler()
    {
        var handler = GetHandler();
        inputReceiver.SetHandle(handler);
        var previewHandler = (PreviewHandler)handler;
        previewHandler.SetCamera(mainCamera, mainVirCam);
    }

    public InputHandler GetHandler()
    {
        return new PreviewHandler(this);
    }

}