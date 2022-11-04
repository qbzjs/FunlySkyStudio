using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class PVPWaitAreaManager : ManagerInstance<PVPWaitAreaManager>, IManager
{
    public bool IsPVPGameStart = false;
    public bool IsSelfDeath = false;
    public PVPWaitAreaBehaviour PVPBehaviour;
    private List<IPVPManager> allPvpManagers = new List<IPVPManager>();
    private Action OnRComplete;

    public void Clear()
    {
        PVPBehaviour = null;
    }
    /// <summary>
    /// 后续需要移植到PVPManager中 需要考虑Manager调用的先后顺序
    /// </summary>
    public void Init()
    {
        allPvpManagers.Add(SensorBoxManager.Inst);
        allPvpManagers.Add(SwitchManager.Inst);
        allPvpManagers.Add(CollectControlManager.Inst);
        allPvpManagers.Add(BaggageManager.Inst);
        allPvpManagers.Add(ShootWeaponManager.Inst);
        allPvpManagers.Add(ParachuteManager.Inst);
        allPvpManagers.Add(PickabilityManager.Inst);
        allPvpManagers.Add(SteeringWheelManager.Inst);
        allPvpManagers.Add(ShowHideManager.Inst);
        allPvpManagers.Add(LockHideManager.Inst);
        allPvpManagers.Add(MagneticBoardManager.Inst);
        allPvpManagers.Add(PlayerManager.Inst);
        allPvpManagers.Add(BloodPropManager.Inst);
        allPvpManagers.Add(AttackWeaponManager.Inst);
        allPvpManagers.Add(ClosetClientManager.Inst);
        allPvpManagers.Add(EdibilityManager.Inst);
        allPvpManagers.Add(PromoteManager.Inst);
        allPvpManagers.Add(SeesawManager.Inst);
        allPvpManagers.Add(SlidePipeManager.Inst);
        allPvpManagers.Add(VIPZoneManager.Inst);
        allPvpManagers.Add(FreezePropsManager.Inst);
    }

    public void AddResetComplete(Action complete)
    {
        OnRComplete = complete;
        PickabilityManager.Inst.AddCompleteListener(OnMapResetComplete);
    }

    public void OnMapResetComplete()
    {
        OnRComplete?.Invoke();
        OnRComplete = null;
    }

    /// <summary>
    /// 后续需要移植到PVPManager中
    /// </summary>
    public void OnReset()
    {
        allPvpManagers.ForEach(x=>x.OnReset());
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (PVPBehaviour == behaviour)
        {
            PVPBehaviour = null;
            LeaderBoardManager.Inst.OnPVPClose();
            SceneBuilder.Inst.UpdateBronPointTeamIDState(null);
        }
    }

    public void SetMeshAndBoxVisible(bool isShowMesh,bool isShowBox)
    {
        if (PVPBehaviour != null)
        {
            PVPBehaviour.meshShow.SetActive(isShowMesh);
            PVPBehaviour.boxShow.SetActive(isShowBox);
        }
    }

    public bool IsSameGameMode(PVPServerTaskType pvpType)
    {
        if (PVPBehaviour != null)
        {
            var comp = PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            return (PVPServerTaskType)comp.gameMode == pvpType;
        }
        return false;
    }

    public bool IsCompleteCondition(int id)
    {
        var comp = PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
        //默认Race，开关获胜条件需要特殊判断(taskArga == 1)
        if (comp.gameMode == (int)PVPServerTaskType.Race)
        {
            return comp.raceData.taskArga == 1 && comp.raceData.taskArg == id;
        }
        else
        {
            return comp.raceData.taskArg == id;
        }

    }
    

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if(behaviour is PVPWaitAreaBehaviour)
        {
            PVPBehaviour = behaviour as PVPWaitAreaBehaviour;
        }
    }


}