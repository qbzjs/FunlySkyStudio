using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CenterTriggerController : MonoBehaviour
{
    private Ray ray;
    private Vector3 centerPoint;
    private Transform triggerGo;
    
    private NodeBaseBehaviour nBehaviour;
    private NodeBaseBehaviour pBehaviour;
    private Dictionary<int, float> distanceDic = new Dictionary<int, float>();

    private PlayerBaseControl player;
    private UGCDetectHandler ugcHandler;
    private float emoTrigDis = 2.7f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        player = this.GetComponent<PlayerBaseControl>();
        centerPoint = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        ugcHandler = new UGCDetectHandler();
        MessageHelper.AddListener(MessageName.ReleaseTrigger, ReleaseTrigger);
    }

    private void Update()
    {
        if (ReferManager.Inst.isRefer) { return; }
        float minValue = !player.isTps ? 10 : 5;
        Collider[] curHits = Physics.OverlapSphere(this.transform.position, minValue);
        HandleWaterCube(curHits);
        HandleTouchProp(curHits);
        HandlePickablityProp(curHits);
    }
    private void HandleWaterCube(Collider[] curHits)
    {
        List<Collider> waterCubeHitList = new List<Collider>();

        foreach (var hit in curHits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("WaterCube"))
            {
                waterCubeHitList.Add(hit);
            }
        }
        bool containsInWater = false;
        if (waterCubeHitList.Count > 0)
        {
            bool isSwimming = false;
            if (PlayerSwimControl.Inst)
            {
                isSwimming = PlayerSwimControl.Inst.isSwimming;
            }
            containsInWater = WaterCubeManager.Inst.ContainsPlayer(waterCubeHitList, player, isSwimming);
        }

        if (containsInWater)
        {
            var playerSwimCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerSwimControl>();
            if (!playerSwimCtrl)
            {
                // 添加游泳控制脚本
                playerSwimCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerSwimControl>();
            }
            //跳伞到水里,打断跳伞
            if (StateManager.IsParachuteUsing)
            {
                PlayerParachuteControl.Inst.IntoWater();
            }
            else if (StateManager.IsSnowCubeSkating)
            {
                PlayerSnowSkateControl.Inst.ForceStopSkating();
            }
            SwordManager.Inst.forceInterrupt();
        }
        if (PlayerSwimControl.Inst)
        {
            PlayerSwimControl.Inst.IsInWater(containsInWater);
        }

    }

    NodeBaseBehaviour[] lastDecthList;
    Collider lastCollider;
    private void HandleTouchProp(Collider[] curHits)
    {
        float minValue;
        minValue = !player.isTps ? 10 : 5;
        List<Collider> touchHitList = new List<Collider>();

        foreach (var hit in curHits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Touch"))
            {
                touchHitList.Add(hit);
            }

        }
        Collider[] touchHits = touchHitList.ToArray();
        //init temp value
        int index = -1;
        if (touchHits.Length > 0)
        {
            for (int i = 0; i < touchHits.Length; i++)
            {
                if (!touchHits[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                var nBehavList = touchHits[i].GetComponentsInParent<NodeBaseBehaviour>();
                foreach (var nBehav in nBehavList)
                {
                    if (nBehav == null)
                    {
                        continue;
                    }
                    
                    if (!IsInView(nBehav))
                    {
                        continue;
                    }

                    if (nBehav.isCanClick == false)
                    {
                        continue;
                    }
                    float dis = GetDis(transform.position, nBehav);
                    if (!IsCanShow(nBehav, dis))
                    {
                        continue;
                    }
                    if (dis < minValue)
                    {
                        minValue = dis;
                        index = i;
                    }

                }
            }
            if(index != -1)
            {
                var listNode = touchHits[index].GetComponentsInParent<NodeBaseBehaviour>();
                if(lastCollider != touchHits[index])
                {
                    OnRayExitDecthList(lastDecthList);
                    lastDecthList = listNode;
                    lastCollider = touchHits[index];
                    OnRayEnterDecthList(listNode);
                }
                return;
            }
            if(index == -1 && lastDecthList != null)
            {
                OnRayExitDecthList(lastDecthList);
                lastDecthList = null;
            }
        }
        ReleaseTrigger();
    }
    private float GetDis(Vector3 pos,NodeBaseBehaviour nBehav)
    {
        if (nBehav is LadderBehaviour)
        {
            LadderBehaviour ladderBv = nBehav as LadderBehaviour;
            return ladderBv.GetRayDis(pos);
        }
        else
        {
            return Vector3.Distance(pos, nBehav.transform.position);
        }
        

    }
    private void OnRayExitDecthList(NodeBaseBehaviour[] DecthList)
    {
        if(DecthList != null)
        {
            for (int i = 0; i < DecthList.Length; i++)
            {
                DecthList[i].OnRayExit();
            }
        }
    }

    private void OnRayEnterDecthList(NodeBaseBehaviour[] DecthList)
    {
        if (DecthList != null)
        {
            for (int i = 0; i < DecthList.Length; i++)
            {
                DecthList[i].OnRayEnter();
            }
        }
    }

    private void HandlePickablityProp(Collider[] curHits)
    {
        //init temp value
        float minValue;
        minValue = !player.isTps ? 10 : 5;
        int index = -1;
        if (curHits.Length > 0)
        {
            for (int i = 0; i < curHits.Length; i++)
            {
                if (!curHits[i].gameObject.activeInHierarchy)
                {
                    continue;
                }
                var entity = SceneObjectController.GetCanControllerNode(curHits[i].gameObject);
                if(entity == null)
                {
                    continue;
                }
                var gComp = entity.Get<GameObjectComponent>();
                var obj = gComp.bindGo;
                if (obj && !IsInView(obj.transform.position))
                {
                    continue;
                }
                if (!entity.HasComponent<PickablityComponent>())
                {
                    continue;
                }
                if (entity.Get<PickablityComponent>().isPicked || entity.Get<PickablityComponent>().canPick == (int)PickableState.Unpickable)
                {
                    continue;
                }
                float dis = Vector3.Distance(transform.position, curHits[i].transform.position);
                if (dis < minValue)
                {
                    minValue = dis;
                    index = i;
                }
            }
            if (index != -1)
            {
                if (pBehaviour != null)
                {
                    PickablityController.Inst.OnRayExit();
                    pBehaviour = null;
                }

                var basBevs = curHits[index].transform.GetComponentsInParent<NodeBaseBehaviour>();
                foreach(var bev in basBevs)
                {
                    if (bev == null || bev.entity == null) continue;
                    if (bev.entity.HasComponent<PickablityComponent>())
                    {
                        var gComp = bev.entity.Get<GameObjectComponent>();
                        var obj = gComp.bindGo;
                        pBehaviour = obj.GetComponent<NodeBaseBehaviour>();
                    }
                }

                if (pBehaviour != null)
                {
                    PickablityController.Inst.OnRayEnter(pBehaviour);
                }
                return;
            }
        }
        if (index == -1 && pBehaviour != null)
        {
            PickablityController.Inst.OnRayExit();
            pBehaviour = null;
        }
        PickablityController.Inst.OnRayExit();
    }

    private bool IsInView(Vector3 worldPos)
    {
        Transform camTransform = mainCamera.transform;
        Vector2 viewPos = mainCamera.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - camTransform.position).normalized;
        float dot = Vector3.Dot(camTransform.forward, dir); //判断物体是否在相机前面
        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
        {
            return true;
        }
        return false;
    }

    private bool IsInView(NodeBaseBehaviour nBehav)
    {
        if (nBehav is PlayerTouchBehaviour)
        {
            return IsInView((nBehav as PlayerTouchBehaviour).touchPos.position);
        }
        else if (nBehav is LadderBehaviour)
        {
            return true;
        }
        else
        {
            return IsInView(nBehav.transform.position);
        }
    }
    //public bool IsIgnoreDis(NodeBaseBehaviour nBehav)
    //{
    //    return (nBehav is LadderBehaviour);
    //}
    private bool IsCanShow(NodeBaseBehaviour nBehav, float dis)
    {
        float emoMinValue = !player.isTps ? emoTrigDis * 2 : emoTrigDis;
        if (nBehav is PlayerTouchBehaviour)
        {
            if (dis > emoMinValue)
            {
                return false;
            }
        }
        return true;
    }

    public void ReleaseTrigger()
    {
        if (lastDecthList != null)
        {
            OnRayExitDecthList(lastDecthList);
            lastDecthList = null;
        }
        lastCollider = null;
    }

    public void ReleasePickTrigger()
    {
        if(pBehaviour != null)
        {
            PickablityController.Inst.OnReleaseTrigger();
            pBehaviour = null;
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Hide();
            AttackWeaponCtrlPanel.Hide();
            ShootWeaponCtrlPanel.Hide();
        }
    }

    private void OnDisable()
    {
        ReleaseTrigger();
        ReleasePickTrigger();
    }

    private void OnDestroy()
    {
        MessageHelper.RemoveListener(MessageName.ReleaseTrigger, ReleaseTrigger);
    }
}
