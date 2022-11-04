using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cmp;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public interface IExcuteSystem
{
    void Excute();
}

public interface IRunSystem
{
    void Run();
}

public class BaseSystem
{
    protected List<NodeBaseBehaviour> behvs = new List<NodeBaseBehaviour>();
    public virtual void SetBehaviours(List<NodeBaseBehaviour> tempBehvs)
    {
        behvs.Clear();
        behvs.AddRange(tempBehvs);
    }

    public virtual void Stop()
    {
        behvs.Clear();
    }
}

public class RotationSystem: BaseSystem, IRunSystem
{
    private List<Vector3> rots = new List<Vector3>();
    protected List<RPAnimComponent> comps = new List<RPAnimComponent>();

    public override void SetBehaviours(List<NodeBaseBehaviour> tempBehvs)
    {
        base.SetBehaviours(tempBehvs);
        rots.Clear();
        comps.Clear();
        tempBehvs.ForEach(x=>comps.Add(x.entity.Get<RPAnimComponent>()));
        for (var i = 0; i < tempBehvs.Count; i++)
        {
            rots.Add(tempBehvs[i].transform.eulerAngles);
        }
    }

    public void Run()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            if (behvs[i] == null)
                continue;
            var alreadyPicked = false;
            if (behvs[i].entity.HasComponent<PickablityComponent>())
            {
                alreadyPicked = behvs[i].entity.Get<PickablityComponent>().alreadyPicked;
            }
            if (comps[i].rSpeed != 0 && comps[i].tempAnimState == 0 && alreadyPicked == false)
            {
                switch (comps[i].rAxis)
                {
                    case 0:
                        behvs[i].OnSelfRotation(Vector3.up, GameConsts.rotSpeeds[comps[i].rSpeed - 1]);
                        break;
                    case 1:
                        behvs[i].OnSelfRotation(Vector3.right,GameConsts.rotSpeeds[comps[i].rSpeed - 1]);
                        break;
                    default:
                        behvs[i].OnSelfRotation(Vector3.forward,GameConsts.rotSpeeds[comps[i].rSpeed - 1]);
                        break;
                }
            }
        }
    }
    public override void Stop()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            behvs[i].transform.DOKill();
            behvs[i].transform.eulerAngles = rots[i];
        }
        base.Stop();
    }
}

public class UpDownSystem : BaseSystem, IExcuteSystem
{
    private RPAnimComponent comp;
    private float duration;
    private Ease upEase;
    private Ease downEase;
    private List<Vector3> positions = new List<Vector3>();
    // private List<int> animStates = new List<int>();
    // private List<RPAnimComponentTweenData> animTweenList = new List<RPAnimComponentTweenData>();

    // struct RPAnimComponentTweenData
    // {
    //     public NodeBaseBehaviour behv;
    //     public TweenerCore<Vector3, Vector3, VectorOptions> tween;
    // }

    public override void SetBehaviours(List<NodeBaseBehaviour> tempBehvs)
    {
        base.SetBehaviours(tempBehvs);
        positions.Clear();
        // animStates.Clear();
        // animTweenList.Clear();
        for (var i = 0; i < tempBehvs.Count; i++)
        {
            positions.Add(tempBehvs[i].transform.position);
            // var animState = tempBehvs[i].entity.Get<RPAnimComponent>().animState;
            // animStates.Add(animState);
        }
    }

    public void Excute()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            if (behvs[i] == null)
                continue;
            comp = behvs[i].entity.Get<RPAnimComponent>();
            if (comp.uSpeed != 0)
            {
                duration = GameConsts.updownDurations[comp.uSpeed - 1];
                upEase = GameConsts.upEases[comp.uSpeed - 1];
                downEase = GameConsts.downEases[comp.uSpeed - 1];

                behvs[i].OnUpDownMove(0.3f, duration, upEase, downEase);
                // RPAnimComponentTweenData data = new RPAnimComponentTweenData();
                // data.behv = behvs[i];
                // data.tween = tween;
                // animTweenList.Add(data);
                var bindGo = behvs[i].entity.Get<GameObjectComponent>().bindGo;
                if (!bindGo.activeSelf || comp.tempAnimState != 0)
                {
                    // tween.Pause();
                    behvs[i].transform.DOPause();
                }
            }
        }
    }

    /**
   * 手动刷新物体的旋转移动状态
   */
    public void RefreshAnimTweenAnim()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            var behv = behvs[i];
            if (behv)
            {
                var comp = behv.entity.Get<RPAnimComponent>();
                var bindGo = behv.entity.Get<GameObjectComponent>().bindGo;
                var alreadyPicked = false;
                if (behv.entity.HasComponent<PickablityComponent>())
                {
                    alreadyPicked = behv.entity.Get<PickablityComponent>().alreadyPicked;
                }
                if (bindGo.activeSelf && comp.tempAnimState == 0 && alreadyPicked == false)
                {
                    behv.transform.DOPlay();
                }
                else
                {
                    behv.transform.DOPause();
                }
            }
        }
    }

    public override void Stop()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            behvs[i].transform.DOKill();
            behvs[i].transform.position = positions[i];
            var comp = behvs[i].entity.Get<RPAnimComponent>();
            comp.tempAnimState = comp.animState;
        }
        base.Stop();
    }
}


public class MoveSystem : BaseSystem, IExcuteSystem
{
    private Transform cachePool;
    private bool isLookAt = true;
    private MovementComponent comp;
    private RPAnimComponent rComp;
    private List<Vector3> rots = new List<Vector3>();
    private List<Vector3> positions = new List<Vector3>();
    private List<MovementComponentTweenData> moveTweenList = new List<MovementComponentTweenData>();
    private List<GameObject> moveNodePool = new List<GameObject>();
    struct MovementComponentTweenData
    {
        public NodeBaseBehaviour behv;
        public TweenerCore<Vector3, Path, PathOptions> tween;
    }

    public MoveSystem(Transform cPool)
    {
        cachePool = cPool;
    }
    public override void SetBehaviours(List<NodeBaseBehaviour> tempBehvs)
    {
        base.SetBehaviours(tempBehvs);
        rots.Clear();
        positions.Clear();
        // moveStates.Clear();
        moveTweenList.Clear();
        for (var i = 0; i < tempBehvs.Count; i++)
        {
            rots.Add(tempBehvs[i].transform.eulerAngles);
            positions.Add(tempBehvs[i].transform.position);
        }
    }

    private GameObject GetMoveNode()
    {
        GameObject node = null;
        if (moveNodePool.Count > 0)
        {
            node = moveNodePool[0];
            moveNodePool.RemoveAt(0);
        }
        else
        {
            node = new GameObject("moveNode");
            node.AddComponent<DoTweenBehaviour>();
        }
        return node;
    }

    private void ReleaseMoveNode(GameObject node)
    {
        node.transform.SetParent(cachePool);
        moveNodePool.Add(node);
    }

    public void Excute()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            if (behvs[i] == null)
                continue;

            GameObject moveNode = GetMoveNode();
            moveNode.transform.SetParent(behvs[i].transform.parent);
            moveNode.transform.position = behvs[i].transform.position;
            behvs[i].transform.SetParent(moveNode.transform);
            comp = behvs[i].entity.Get<MovementComponent>();
            rComp = behvs[i].entity.Get<RPAnimComponent>();
            if (comp.pathPoints != null && comp.pathPoints.Count != 0)
            {
                bool isLook = rComp.rSpeed == 0;
                bool isTurn = (comp.turnAround == 1);
                float speed = GameConsts.moveSpeed[comp.speedId];
                float turnDur = GameConsts.rotDelTime[comp.speedId];
                List<Vector3> tempPath = new List<Vector3>();
                tempPath.Add(behvs[i].transform.position);
                tempPath.AddRange(comp.pathPoints);
                var rot = behvs[i].transform.rotation;
                moveNode.transform.LookAt(comp.pathPoints[0]);
                behvs[i].transform.rotation = rot;
                float dur = CalculateMoveTime(tempPath, speed);
                int addArg = 0;
                bool isBack = false;
                var tween = moveNode.transform.DOPath(tempPath.ToArray(), dur).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).OnWaypointChange(
                    (index) =>
                    {
                        if (index == 0)
                        {
                            isBack = false;
                            addArg = 1;
                        }
                        else if (index == tempPath.Count - 1)
                        {
                            isBack = true;
                            addArg = -1;
                        }
                        Vector3 offSet = Vector3.zero;
                        if (!isTurn)
                        {
                            offSet = isBack
                                ? tempPath[index] - tempPath[index - 1]
                                : tempPath[index + 1] - tempPath[index];
                           
                        }
                        else
                        {
                            int nextIndex = index + addArg;
                            offSet = tempPath[nextIndex] - tempPath[index];
                        }
                        if (isLook)
                        {
                            moveNode.transform.DOLookAt(tempPath[index] + offSet, turnDur);
                        }
                    });
                MovementComponentTweenData data = new MovementComponentTweenData();
                data.behv = behvs[i];
                data.tween = tween;
                moveTweenList.Add(data);
                var bindGo = behvs[i].entity.Get<GameObjectComponent>().bindGo;
                if (!bindGo.activeSelf || comp.tempMoveState != 0)
                {
                    tween.Pause();
                }
            }
        }
    }

    /**
    * 手动刷新物体的可移动状态
    */
    public void RefreshMoveTweenAnim()
    {
        foreach (var data in moveTweenList)
        {
            if (data.tween != null)
            {
                var comp = data.behv.entity.Get<MovementComponent>();
                var bindGo = data.behv.entity.Get<GameObjectComponent>().bindGo;
                var alreadyPicked = false;
                if (data.behv.entity.HasComponent<PickablityComponent>())
                {
                    alreadyPicked = data.behv.entity.Get<PickablityComponent>().alreadyPicked;
                }
                if (bindGo.activeSelf && comp.tempMoveState == 0 && alreadyPicked == false)
                {
                    data.tween.Play();
                }
                else
                {
                    data.tween.Pause();
                }
            }
        }
    }

    private float CalculateMoveTime(List<Vector3> allPoints,float speed)
    {
        float distance = 0;
        for (int i = 1; i < allPoints.Count; i++)
        {
            distance += (allPoints[i] - allPoints[i - 1]).magnitude;
        }

        return distance / speed;
    }

    public override void Stop()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            if (behvs[i] == null)
                continue;
            behvs[i].transform.DOKill();
            var moveNode = behvs[i].transform.parent;
            behvs[i].transform.SetParent(moveNode.parent);
            behvs[i].transform.eulerAngles = rots[i];
            behvs[i].transform.position = positions[i];
            moveNode.DOKill();
            var comp = behvs[i].entity.Get<MovementComponent>();
            comp.tempMoveState = comp.moveState;
            ReleaseMoveNode(moveNode.gameObject);
            //GameObject.Destroy(moveNode.gameObject);
        }
        base.Stop();
    }
}

public class FollowSystem : BaseSystem
{
    private Transform cachePool;
    private List<Vector3> rots = new List<Vector3>();
    private List<Vector3> positions = new List<Vector3>();
    private List<GameObject> followNodePool = new List<GameObject>();
    
    public FollowSystem(Transform cPool)
    {
        cachePool = cPool;
    }
    public override void SetBehaviours(List<NodeBaseBehaviour> tempBehvs)
    {
        base.SetBehaviours(tempBehvs);
        rots.Clear();
        positions.Clear();
        for (var i = 0; i < tempBehvs.Count; i++)
        {
            rots.Add(tempBehvs[i].transform.eulerAngles);
            positions.Add(tempBehvs[i].transform.position);
        }
    }
    
    public void Excute()
    {
        FollowModeManager.Inst.UpdateFollowYAxis();
        for (var i = 0; i < behvs.Count; i++)
        {
            if (behvs[i] == null)
                continue;
            GameObject moveNode = GetFollowNode();
            moveNode.transform.SetParent(behvs[i].transform.parent);
            moveNode.transform.position = behvs[i].transform.position;
            behvs[i].transform.Rotate(Vector3.up,180,Space.World);
            behvs[i].transform.SetParent(moveNode.transform);
            moveNode.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    

    public override void Stop()
    {
        FollowModeManager.Inst.StopAllFollowMove();
        for (var i = 0; i < behvs.Count; i++)
        {
            if (behvs[i] == null)
                continue;
            behvs[i].transform.DOKill();
            var moveNode = behvs[i].transform.parent;
            behvs[i].transform.SetParent(moveNode.parent);
            behvs[i].transform.eulerAngles = rots[i];
            behvs[i].transform.position = positions[i];
            moveNode.DOKill();
            var comp = behvs[i].entity.Get<MovementComponent>();
            comp.tempMoveState = comp.moveState;
            ReleaseFollowNode(moveNode.gameObject);
        }
        base.Stop();
    }
    public void RefreshMoveTweenAnim()
    {
        for (var i = 0; i < behvs.Count; i++)
        {
            var moveNode = behvs[i].transform.parent;
            var moveComp = behvs[i].entity.Get<MovementComponent>();
            int uid = behvs[i].entity.Get<GameObjectComponent>().uid;
            var tempFollow = FollowModeManager.Inst.GetFolowBoxGo(uid);
            if(tempFollow == null)
            {
                return;
            }
            var follow = tempFollow as FollowModeBehaviour;
            if (behvs[i].gameObject.activeSelf && moveComp.tempMoveState == 0)
            {
                follow.StartFollowMove();
            }
            else
            {
                follow.PauseFollowMove();
            }
        }
        
    }
    
    private GameObject GetFollowNode()
    {
        GameObject node = null;
        if (followNodePool.Count > 0)
        {
            node = followNodePool[0];
            followNodePool.RemoveAt(0);
        }
        else
        {
            node = new GameObject("followNode");
            node.AddComponent<DoTweenBehaviour>();
        }
        node.transform.position = Vector3.zero;
        node.transform.eulerAngles = Vector3.zero;
        return node;
    }

    private void ReleaseFollowNode(GameObject node)
    {
        node.transform.SetParent(cachePool);
        node.transform.position = Vector3.zero;
        node.transform.eulerAngles = Vector3.zero;
        followNodePool.Add(node);
    }
}

public class SceneSystem : InstMonoBehaviour<SceneSystem>
{
    private bool isStartExcute = false;
    private RotationSystem rotSystem;
    private UpDownSystem upDownSystem;
    private MoveSystem moveSystem;
    private FollowSystem followSystem;
    void Awake()
    {
        rotSystem = new RotationSystem();
        upDownSystem = new UpDownSystem();
        moveSystem = new MoveSystem(this.transform);
        followSystem = new FollowSystem(this.transform);
    }

    public void Init() { }

    public void StartSystem()
    {
        isStartExcute = true;
        //move require create parent node,Must be at the front
        var moveBehv = FilterBehaviours<MovementComponent>(SceneBuilder.Inst.allControllerBehaviours);
        var hasAttMoveBehv = new List<NodeBaseBehaviour>();
        for (var i = 0; i < moveBehv.Count; i++)
        {
            var comp = moveBehv[i].entity.Get<MovementComponent>();
            if (comp.pathPoints != null && comp.pathPoints.Count > 0 && comp.speedId > 0)
            {
                hasAttMoveBehv.Add(moveBehv[i]);
            }
        }
        moveSystem.SetBehaviours(hasAttMoveBehv);
        moveSystem.Excute();

        var followBehv = FilterBehaviours<FollowableComponent>(SceneBuilder.Inst.allControllerBehaviours);
        followSystem.SetBehaviours(followBehv);
        followSystem.Excute();

        // 需要获取到localPosition位置，避免位置偏差，需要放到最后执行
        var rotBehv = FilterBehaviours<RPAnimComponent>(SceneBuilder.Inst.allControllerBehaviours);
        rotSystem.SetBehaviours(rotBehv);
        upDownSystem.SetBehaviours(rotBehv);
        upDownSystem.Excute();
    }

    /**
    * 手动运行移动系统，刷新物体的可移动状态
    */
    public void ExcuteMoveSystem()
    {
        moveSystem.RefreshMoveTweenAnim();
        followSystem.RefreshMoveTweenAnim();
    }

    /**
    * 恢复物体的可移动状态，包括Move, UpDown 和 Follow 移动
    * 当物体由不可见变为可见时，需要恢复相关的移动状态
    */

    public void RestoreSystemState()
    {
        ExcuteMoveSystem();
        ExcuteUpDownSystem();
    }

    /**
    * 手动运行上下移动系统，刷新物体的可移动状态
    */
    public void ExcuteUpDownSystem()
    {
        upDownSystem.RefreshAnimTweenAnim();
    }

    public void StopSystem()
    {
        isStartExcute = false;
        rotSystem.Stop();
        upDownSystem.Stop();
        moveSystem.Stop();
        followSystem.Stop();
    }


    private void Update()
    {
        if (isStartExcute)
        {
            rotSystem.Run();
        }
    }

    public List<NodeBaseBehaviour> FilterBehaviours<T>(List<NodeBaseBehaviour> all) 
        where T:IComponent
    {
        var tmpComponents = new List<NodeBaseBehaviour>();
        if (all != null)
        {
            foreach (var baseBehaviour in all)
            {
                if (baseBehaviour != null && baseBehaviour.entity.HasComponent<T>())
                {
                    tmpComponents.Add(baseBehaviour);
                }
            }
        }
        return tmpComponents;
    }

    public List<T> FilterNodeBehaviours<T>(List<NodeBaseBehaviour> all) where T:NodeBaseBehaviour
    {
        var tmpComponents = new List<T>();
        if (all != null)
        {
            foreach (var baseBehaviour in all)
            {
                if (baseBehaviour != null && baseBehaviour.TryGetComponent<T>(out var targetBehaviour))
                {
                    tmpComponents.Add(targetBehaviour);
                }
            }
        }
        return tmpComponents;
    }

    public int FilterNodeBehavioursCount<T>(List<NodeBaseBehaviour> all) where T:NodeBaseBehaviour
    {
        int count = 0;
        if (all != null)
        {
            foreach (var baseBehaviour in all)
            {
                if (baseBehaviour != null && baseBehaviour.TryGetComponent<T>(out var targetBehaviour))
                {
                    count++;
                }
            }
        }
        return count;
    }

    public float GetParticlesCount(List<NodeBaseBehaviour> all) 
    {
        float count = 0;
        if (all != null)
        {
            foreach (var baseBehaviour in all)
            {
                if (baseBehaviour != null)
                {
                    var particles = baseBehaviour.GetComponentsInChildren<ParticleSystem>(true);
                    if (particles != null && particles.Length > 0)
                    {
                        for (int i = 0; i < particles.Length; i++)
                        {
                            var particle = particles[i];
                            count += particle.emission.rateOverTime.constantMax + particle.emission.rateOverDistance.constantMax + particle.emission.burstCount;
                        }
                    }
                }
            }
        }
        return count;
    }

    public List<NodeBaseBehaviour> FilterBehaviours<T1, T2>(List<NodeBaseBehaviour> all)
        where T1 : IComponent
        where T2 : IComponent
    {
        var tmpComponents = new List<NodeBaseBehaviour>();
        if (all != null)
        {
            foreach (var baseBehaviour in all)
            {
                if (baseBehaviour != null && baseBehaviour.entity.HasComponent<T1>() && baseBehaviour.entity.HasComponent<T2>())
                {
                    tmpComponents.Add(baseBehaviour);
                }
            }
        }
        return tmpComponents;
    }


    void OnDestroy()
    {
        inst = null;
    }

}