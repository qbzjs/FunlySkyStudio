using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FollowModeManager : ManagerInstance<FollowModeManager>, IManager, IPVPManager
{
    private Dictionary<int, NodeBaseBehaviour> followBoxs = new Dictionary<int, NodeBaseBehaviour>();
    private Dictionary<int, int> targetMoveStates = new Dictionary<int, int>();
    private List<GameObject> nouseBoxs = new List<GameObject>();
    private List<GameObject> useBoxs = new List<GameObject>();


    private void ClearFollowList()
    {
        followBoxs.Clear();
    }
    

    public void AddFolowBox(int tId, NodeBaseBehaviour go)
    {
        if (!followBoxs.ContainsKey(tId))
        {
            followBoxs.Add(tId, go);
        }
    }
    

    public void RemoveFolowBox(int tId)
    {
        if (followBoxs.ContainsKey(tId))
        {
            followBoxs.Remove(tId);
        }
    }

    public NodeBaseBehaviour GetFolowBoxGo(int tId)
    {
        if (followBoxs.ContainsKey(tId))
        {
            return followBoxs[tId];
        }
        return null;
    }
    
    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            //ResetTargetMoveType();
            foreach (var item in followBoxs)
            {
                var temp = item.Value.GetComponent<FollowModeBehaviour>();
                var collider = item.Value.GetComponentInChildren<Collider>();
                if (temp == null)
                {
                    continue;
                }
                temp.bRenderer.enabled = true;
                collider.enabled = false;
            }
        }
        if(mode == GameMode.Play || mode == GameMode.Guest)
        {
            foreach (var item in followBoxs)
            {
                var temp = item.Value.GetComponent<FollowModeBehaviour>();
                var collider = item.Value.GetComponentInChildren<Collider>();
                if (temp == null)
                {
                    continue;
                }
                temp.bRenderer.enabled = false;
                collider.enabled = true;
            }
        }
    }
    
        public void UpdateFollowYAxis()
        {
            foreach (var followBehv in followBoxs.Values)
            {
                var tempBehv = followBehv as FollowModeBehaviour;
                tempBehv.UpdateFollowYAxis();
                var collider = tempBehv.GetComponentInChildren<Collider>();
                //PVP重置OnTigger触发
                collider.enabled = false;
                collider.enabled = true;
            }
        }

    public void StopAllFollowMove()
    {
        foreach (var followBehv in followBoxs.Values)
        {
            var tempBehv = followBehv as FollowModeBehaviour;
            tempBehv.StopFollowMove();
        }
    }

    public override void Release()
    {
        base.Release();
        ClearFollowList();
    }

    public void Clear()
    {
        ClearFollowList();
    }

    public void DistoryFollowBox(GameObject curTarget)
    {
        if(curTarget == null)
        {
            return;
        }
        var behav = curTarget.GetComponent<NodeBaseBehaviour>();
        int nodeId = behav.entity.Get<GameObjectComponent>().uid;
        if (GetFolowBoxGo(nodeId) == null)
        {
            return;
        }
        var bing = GetFolowBoxGo(nodeId).entity.Get<GameObjectComponent>().bindGo;
        bing.SetActive(false);
        nouseBoxs.Add(bing);
        RemoveFolowBox(nodeId);
    }
    

    public bool IsContainSpecialEntity(SceneEntity entity)
    {
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        var nodeBehvs = bindGo.GetComponentsInChildren<NodeBaseBehaviour>();
        for (var i = 0; i < nodeBehvs.Length; i++)
        {
            ResType resType = nodeBehvs[i].entity.Get<GameObjectComponent>().type;
            NodeModelType modeType = nodeBehvs[i].entity.Get<GameObjectComponent>().modelType;
            if (nodeBehvs[i].entity.HasComponent<ParachuteComponent>())
            {
                return false;
            }
            if (resType != ResType.UGC
                && resType != ResType.PGC
                && modeType != NodeModelType.BaseModel
                && modeType != NodeModelType.TrapBox
                && modeType != NodeModelType.DText
                && modeType != NodeModelType.NewDText
                && modeType != NodeModelType.CommonCombine
                && modeType != NodeModelType.BloodRestore
                && modeType != NodeModelType.PGCPlant
                && modeType != NodeModelType.FreezeProps
                && modeType != NodeModelType.FireProp
                && modeType != NodeModelType.PGCEffect
                )
            {
                return false;
            }
        }
        return true;
    }

    public GameObject BuildFollowBox(NodeBaseBehaviour target)
    {
        GameObject box = null;
        if (nouseBoxs.Count > 0)
        {
            box = nouseBoxs.First();
            box.SetActive(true);
            var behav = box.GetComponent<FollowModeBehaviour>();
            behav.bRenderer.enabled = true;
            FollowBoxCreater.SetData(behav, target);
            nouseBoxs.Remove(box);
        }
        else
        {
            box = SceneBuilder.Inst.CreateFollowBox(target).gameObject;
        }
        useBoxs.Add(box);
        return box;
    }

    public void SetFollowBoxTrans(GameObject target)
    {
        var behav = target.GetComponent<NodeBaseBehaviour>();
        if (behav == null)
        {
            return;
        }
        var uid = behav.entity.Get<GameObjectComponent>().uid;
        var box = GetFolowBoxGo(uid);
        if (box == null)
        {
            return;
        }

        var boxTrans = GetGameObjComp(box).bindGo.transform;
        boxTrans.position = target.transform.position;
        boxTrans.eulerAngles = target.transform.eulerAngles;
    }
    

    public GameObjectComponent GetGameObjComp(NodeBaseBehaviour behav)
    {
        return behav.entity.Get<GameObjectComponent>();
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var comp = behaviour.entity.Get<GameObjectComponent>();
        DistoryFollowBox(comp.bindGo);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<FollowableComponent>())
        {
            if (behaviour.entity.Get<FollowableComponent>().moveType == (int)MoveMode.Follow)
            {
                BuildFollowBox(behaviour);
            }
        }
    }
    public void SetFollowBoxVisable(NodeBaseBehaviour nodeBehav, bool isActive)
    {
        var uid = nodeBehav.entity.Get<GameObjectComponent>().uid;
        var followBox = GetFolowBoxGo(uid);
        if (followBox)
        {
            followBox.gameObject.SetActive(isActive);
        }
    }

    public void OnCombineNode(SceneEntity entity)
    {
        if (entity.HasComponent<FollowableComponent>())
        {
            DistoryFollowBox(entity.Get<GameObjectComponent>().bindGo);
        }
    }

    public void OnReset()
    {

    }
}
