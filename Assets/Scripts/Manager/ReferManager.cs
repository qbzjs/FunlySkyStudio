using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:LiShuZhan
/// Description:人物参照模式管理类，可以在编辑模式里使用人物
/// Date: 2022.03.02
/// </summary>
public class ReferManager : ManagerInstance<ReferManager>, IManager
{
    public bool isRefer;
    public bool isHafeRefer;
    public bool isReferPlay;
    public bool isUndoRedo;
    private const int reyHigh = 1000;
    private const float playerHigh = 0.95f;
    public void Clear()
    {
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
    }

    public void EnterReferPlay()
    {
        if (ReferPanel.Instance && isReferPlay)
        {
            ReferPanel.Show();
            ReferPanel.Instance.OnReferInvoke();
            isReferPlay = false;
        }
    }

    public void OnReferPlay()
    {
        if (ReferPanel.Instance && isRefer)
        {
            ReferPanel.Instance.OnEnterReferInvoke();
            isReferPlay = true;
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }

    public void CheckSpawnPos(Transform player)
    {
        RaycastHit hit;
        var cam = ReferPanel.Instance.editCam;
        Vector3 tempV3 = new Vector3(cam.position.x, reyHigh, cam.position.z);
        bool isHit = Physics.Raycast(tempV3, -Vector3.up, out hit,2* reyHigh,
            1 << LayerMask.NameToLayer("Terrain"));
        if (isHit)
        {
            if (player.position != cam.position + new Vector3(0, playerHigh, 0))
            {
                player.position = hit.point + new Vector3(0, playerHigh, 0);
            }
        }
    }

    public void OnChangeMode(GameMode gameMode)
    {
        if(gameMode == GameMode.Edit)
        {
            PlayerBaseControl.Inst.isFastRun = false;
            PlayerBaseControl.Inst.SetEndFlyPlayerPos();
            PlayerBaseControl.Inst.PlayerResetIdle();
        }
    }
    //保存地图完成时，根据参照模式的状态来显示完整的参照模式还是只显示人物
    public void OnSaveChangeReferState()
    {
        if (!isRefer) return;
        if (PlayerBaseControl.Inst && isHafeRefer)
        {
            PlayerBaseControl.Inst.gameObject.SetActive(true);
        }
        else
        {
            EnterReferPlay();
        }
    }
}
