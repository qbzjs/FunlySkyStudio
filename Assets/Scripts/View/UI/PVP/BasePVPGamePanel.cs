using UnityEngine;
public enum PVPGameConnectEnum
{
    Normal = 0,
    ReConnect = 1
}
public abstract class BasePVPGamePanel : MonoBehaviour
{
    public abstract void Enter(PVPGameConnectEnum connect);
    public abstract void Leave();
}