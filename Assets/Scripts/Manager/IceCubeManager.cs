using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Lishuzhan
/// Description:
/// Date: 2022-07-14
/// </summary>
public class IceCubeManager : ManagerInstance<IceCubeManager>, IManager
{
    public List<NodeBaseBehaviour> bevs = new List<NodeBaseBehaviour>();
    private const int MaxCount = 999;
    public const string MAX_COUNT_TIP = "Up to 999 Ice Cubes can be set.";
    public override void Release()
    {
        base.Release();
        Clear();
    }

    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        if (newBev.entity.HasComponent<IceCubeComponent>())
        {
            AddItem(newBev);
        }
    }
    
    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<IceCubeBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<IceCubeBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + bevs.Count > MaxCount)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }
    
        return true;
    }

    public void AddItem(NodeBaseBehaviour b)
    {
        if (!bevs.Contains(b))
        {
            bevs.Add(b);
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.IceCube)
        {
            bevs.Remove(behaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.IceCube)
        {
            if(!bevs.Contains(behaviour))
            {
                bevs.Add(behaviour);
            }
        }
    }

    public void Clear()
    {
        if (bevs != null)
        {
            bevs.Clear();
        }
    }
    
    /************************************** 滑冰状态外部判断 **************************************/
    public bool IsPlayerStandOnIceCube(string playerId)
    {
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            if (PlayerStandonControl.Inst == null)
            {
                return false;
            }
            return PlayerStandonControl.Inst.IsStandOnIceCube();
        }
        else
        {
            var otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer == null)
            {
                return false;
            }
            return FrameStateManager.Inst.GetStandOnType(otherPlayer.CurrentAnimType) == StandOnType.IceCube;
        }
    }

    public bool IsOverMaxCount()//最大开关数量
    {
        if (bevs.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }

    //判断是单人脚步声还是双人牵手脚步声
    public FootSoundInfo IceCubeSimpleOrMult(FootSoundInfo info)
    {
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            info.switchState = StandOnAudioType.mutualSkateAudio;
            info.deltaTime = 0.4f;
        }
        else
        {
            info.switchState = StandOnAudioType.skateAudio;
            info.deltaTime = 1f;
        }
        return info;
    }

    public void EnterIceCube(StandOnType standOnType, GameObject standOnObj)
    {
        if (PlayerControlManager.Inst && 
            PlayerBaseControl.Inst)
        {
            if (StateManager.IsFishing)
            {
                FishingManager.Inst.ForceStopFishing();
            }
            SwordManager.Inst.forceInterrupt();
            PlayerBaseControl.Inst.Move(Vector3.zero);
            PlayerControlManager.Inst.ChangeAnimClips();
            PlayerBaseControl.Inst.mAnimStateManager.SwitchTo(EPlayerAnimState.Skate);
        }
    }
    
    public void LeaveIceCube(StandOnType standOnType, GameObject standOnObj)
    {
        
    }
}
