using Cinemachine;
using UnityEngine;

public interface BaseModeController
{
    void Init();
    void SetCamera(Camera cam, CinemachineVirtualCamera vCam);
}