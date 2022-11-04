using Assets.Scripts.Game.Core;
using UnityEngine;

public class VIPZoneBehaviour : ActorNodeBehaviour
{
    public bool canEnter = false;
    private ComponentData saveComponentData;
    
    private GameObject triggerHost;
    private BoxCollider checkTrigger;
    private CheckTriggerCall checkTriggerCall;
    private float externLen = 1.5f;

    private const string TRIGGER_HOST_NAME = "triggerHost";
    private const string TRIGGER_NAME = "trigger";
    
    public NodeBaseBehaviour ChangeDoorDefault(string key)
    {
        VIPDoorBehaviour vipDoorBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPDoorCreater, VIPDoorBehaviour>();
        VIPDoorCreater.SetDefaultData(vipDoorBehaviour);
        bool suc = VIPDoorCreater.UpdateModel(vipDoorBehaviour,key);
        if (!suc)
        {
            return null;
        }

        if (vipDoorBehaviour == null)
        {
            return null;
        }

        Transform oldDoor = FindDoor();
        if (oldDoor == null)
        {
            return null;
        }
        vipDoorBehaviour.transform.SetParent(oldDoor.parent);
        vipDoorBehaviour.transform.localPosition = oldDoor.localPosition;
        vipDoorBehaviour.transform.localRotation = oldDoor.localRotation;
        vipDoorBehaviour.transform.localScale = oldDoor.localScale;
        
        SceneBuilder.Inst.DestroyEntity(oldDoor.gameObject);
        
        return vipDoorBehaviour;
    }

    public Transform FindDoor()
    {
        VIPDoorBehaviour doorBehaviour = gameObject.GetComponentInChildren<VIPDoorBehaviour>();
        if (doorBehaviour != null)
        {
            return doorBehaviour.transform;
        }
        
        UGCCombBehaviour[] ugcs = gameObject.GetComponentsInChildren<UGCCombBehaviour>();
        foreach (var ugcCombBehaviour in ugcs)
        {
            if (ugcCombBehaviour.entity.HasComponent<VIPDoorComponent>())
            {
                return ugcCombBehaviour.transform;
            }
        }

        return null;
    }

    public NodeBaseBehaviour ChangeCheckDefault(string key)
    {
        VIPCheckBehaviour vipCheckBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPCheckCreater, VIPCheckBehaviour>();
        VIPCheckCreater.SetDefaultData(vipCheckBehaviour);
        bool suc = VIPCheckCreater.UpdateModel(vipCheckBehaviour,key);
        if (!suc)
        {
            return null;
        }
        
        Transform oldCheck = FindCheck();
        if (oldCheck == null)
        {
            return null;
        }
        vipCheckBehaviour.transform.SetParent(oldCheck.parent);
        vipCheckBehaviour.transform.localPosition = oldCheck.localPosition;
        vipCheckBehaviour.transform.localRotation = oldCheck.localRotation;
        vipCheckBehaviour.transform.localScale = oldCheck.localScale;
        vipCheckBehaviour.GetComponent<VIPCheckBoundControl>().UpdateEffectShow();

        SceneBuilder.Inst.DestroyEntity(oldCheck.gameObject);
        
        return vipCheckBehaviour;
    }

    private Transform FindCheck()
    {
        VIPCheckBehaviour checkBehaviour = gameObject.GetComponentInChildren<VIPCheckBehaviour>();
        if (checkBehaviour != null)
        {
            return checkBehaviour.transform;
        }
        
        UGCCombBehaviour[] ugcs = gameObject.GetComponentsInChildren<UGCCombBehaviour>();
        foreach (var ugcCombBehaviour in ugcs)
        {
            if (ugcCombBehaviour.entity.HasComponent<VIPCheckComponent>())
            {
                return ugcCombBehaviour.transform;
            }
        }

        return null;
    }

    public NodeBaseBehaviour ChangeDoorEffectDefault(string key)
    {
        VIPDoorEffectBehaviour vipDoorEffectBehaviour = SceneBuilder.Inst.CreateSceneNode<VIPDoorEffectCreater, VIPDoorEffectBehaviour>();
        VIPDoorEffectCreater.SetDefaultData(vipDoorEffectBehaviour);
        bool suc = VIPDoorEffectCreater.UpdateModel(vipDoorEffectBehaviour,key);
        if (!suc)
        {
            return null;
        }

        if (vipDoorEffectBehaviour == null)
        {
            return null;
        }

        Transform oldDoorEffect = FindDoorEffect();
        if (oldDoorEffect == null)
        {
            return null;
        }
        vipDoorEffectBehaviour.transform.SetParent(oldDoorEffect.parent);
        vipDoorEffectBehaviour.transform.localPosition = oldDoorEffect.localPosition;
        vipDoorEffectBehaviour.transform.localRotation = oldDoorEffect.localRotation;
        vipDoorEffectBehaviour.transform.localScale = oldDoorEffect.localScale;
        
        SceneBuilder.Inst.DestroyEntity(oldDoorEffect.gameObject);
        
        return vipDoorEffectBehaviour;
    }

    public Transform FindDoorEffect()
    {
        VIPDoorEffectBehaviour checkBehaviour = gameObject.GetComponentInChildren<VIPDoorEffectBehaviour>();
        if (checkBehaviour != null)
        {
            return checkBehaviour.transform;
        }
        
        return null;
    }

    public Transform FindArea()
    {
        VIPAreaBehaviour areaBehaviour = gameObject.GetComponentInChildren<VIPAreaBehaviour>();
        if (areaBehaviour != null)
        {
            return areaBehaviour.transform;
        }
        
        return null;
    }

    public Transform FindDoorWrap()
    {
        VIPDoorWrapBehaviour wrapBehaviour = gameObject.GetComponentInChildren<VIPDoorWrapBehaviour>();
        if (wrapBehaviour != null)
        {
            return wrapBehaviour.transform;
        }
        
        return null;
    }

    public void OnModeChange(GameMode mode)
    {
        VIPAreaBehaviour area = GetComponentInChildren<VIPAreaBehaviour>();
        var vipCheckBoundAdjust = FindCheck().GetComponent<VIPCheckBoundControl>();
        if (mode != GameMode.Edit)
        {
            //显示游玩光幕
            area.SwitchPlayMode();
            //调整区域的碰撞盒厚度
            area.AdjustColliderPosAndSize();
            //检测台检测开启
            vipCheckBoundAdjust.SetTriggerActive(true);
            //开启门的检测
            ActiveDoorTrigger();
            //关闭门特效的碰撞
            VIPDoorEffectBehaviour effect = GetComponentInChildren<VIPDoorEffectBehaviour>();
            effect.DisabelCollider();
            //区域的碰撞移到airWall层
            area.SwitchColliderLayer("Airwall");
        }
        else
        {
            ResetStatus();
        }
    }

    private void ResetStatus()
    {
        //恢复区域碰撞盒，不然选不中区域了
        VIPAreaBehaviour area = GetComponentInChildren<VIPAreaBehaviour>();
        area.SwitchAllColliderStatus(true);
        //删掉检测是否进入的触发器
        area.DeleteContainCollider();
        //显示编辑光幕
        area.SwitchEditMode();
        //检测台的检测触发失效
        var vipCheckBoundAdjust = FindCheck().GetComponent<VIPCheckBoundControl>();
        vipCheckBoundAdjust.SetTriggerActive(false);
        //恢复门的阻挡特效
        ResumeDoorInterceptStatus();
        //开启门特效的碰撞
        VIPDoorEffectBehaviour effect = GetComponentInChildren<VIPDoorEffectBehaviour>();
        effect.EnableCollider();
        //门的触发器失效
        InActiveDoorTrigger();
        //标记不能进
        canEnter = false;
        //区域的碰撞移到touch层
        area.SwitchColliderLayer("Touch");
    }

    public void EnableCollider(Vector3 center, Vector3 size)
    {
        InitColliderGameObject(size);
        triggerHost.transform.position = Vector3.zero;
        triggerHost.transform.rotation = Quaternion.identity;
        triggerHost.transform.localScale = Vector3.one / triggerHost.transform.parent.localScale.x;
        RefreshCheckColliderSize(center, size);
    }

    public void RefreshCheckColliderSize(Vector3 center, Vector3 size)
    {
        if (checkTrigger == null)
        {
            return;
        }
        Vector3 triggerCenter = center + Vector3.up * externLen / 2;
        float sizeXZ = Mathf.Max(size.x, size.z) * VIPZoneConstant.FACTOR_CHECK_EFFECT * VIPZoneConstant.FACTOR_CHECK_TRIGGER;
        Vector3 triggerSize = new Vector3(sizeXZ,size.y + externLen,sizeXZ);
        checkTrigger.center = triggerCenter;
        checkTrigger.size = triggerSize;
        checkTrigger.enabled = true;
        if (checkTriggerCall != null)
        {
            checkTriggerCall.SetSize(size);
        }
    }

    public void InitColliderGameObject(Vector3 size)
    {
        //克隆的恢复逻辑
        if (triggerHost == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.name.Contains(TRIGGER_HOST_NAME))
                {
                    triggerHost = child.gameObject;
                    for (int j = 0; j < child.childCount; j++)
                    {
                        var grandChild = child.GetChild(j);
                        if (grandChild.name.Contains(TRIGGER_NAME))
                        {
                            checkTrigger = grandChild.gameObject.GetComponent<BoxCollider>();
                        }
                    }
                    break;
                }
            }
        }
        if (triggerHost == null)
        {
            triggerHost = new GameObject(TRIGGER_HOST_NAME);
            triggerHost.transform.SetParent(transform);
        }
        if (checkTrigger == null)
        {
            checkTrigger = AddCheckCollider(TRIGGER_NAME,size);
        }
        else
        {
            if (checkTriggerCall != null)
            {
                checkTriggerCall.SetSize(size);
            }
        }
    }

    private BoxCollider AddCheckCollider(string name,Vector3 size)
    {
        GameObject collider = new GameObject(name);
        collider.transform.SetParent(triggerHost.transform);
        BoxCollider boxCollider = collider.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.enabled = false;
        checkTriggerCall = collider.AddComponent<CheckTriggerCall>();
        checkTriggerCall.SetSize(size);
        return boxCollider;
    }

    public void DisableCollider()
    {
        if (checkTrigger != null)
        {
            checkTrigger.enabled = false;
        }
    }

    public void SignCanEnterDoor()
    {
        var doorEffect = GetComponentInChildren<VIPDoorEffectBehaviour>(true);
        doorEffect.gameObject.SetActive(false);

        AKSoundManager.Inst.PostEvent("Stop_VipArea_Door_Loop", doorEffect.gameObject);
        canEnter = true;
    }

    public void ResumeDoorInterceptStatus()
    {
        var doorEffect = GetComponentInChildren<VIPDoorEffectBehaviour>(true);
        doorEffect.gameObject.SetActive(true);
        if(GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            AKSoundManager.Inst.PostEvent("Play_VipArea_Door_Loop", doorEffect.gameObject);
        }
        canEnter = false;
    }

    public void InActiveDoorTrigger()
    {
        Transform door = FindDoor();
        var doorCollider = door.GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            return;
        }

        doorCollider.enabled = false;
    }

    private void ActiveDoorTrigger()
    {
        Transform door = FindDoor();
        BoxCollider doorCollider = door.GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            doorCollider = door.gameObject.AddComponent<BoxCollider>();
            doorCollider.center = new Vector3(0,1.5f,0.5f);
            doorCollider.size = new Vector3(1,3,2);
            doorCollider.isTrigger = true;
        }

        doorCollider.enabled = true;
        if (door.gameObject.GetComponent<DoorTriggerCall>() == null)
        {
            door.gameObject.AddComponent<DoorTriggerCall>();
        }
    }

    public void DisableFaceWallCollider()
    {
        VIPAreaBehaviour vipAreaBehaviour = GetComponentInChildren<VIPAreaBehaviour>();
        vipAreaBehaviour.SwitchFaceColliderStatus(false);
    }

    public void EnableFaceWallCollider()
    {
        VIPAreaBehaviour vipAreaBehaviour = GetComponentInChildren<VIPAreaBehaviour>();
        vipAreaBehaviour.SwitchFaceColliderStatus(true);
    }

    public void DisableAllWallCollider()
    {
        VIPAreaBehaviour vipAreaBehaviour = GetComponentInChildren<VIPAreaBehaviour>();
        vipAreaBehaviour.SwitchAllColliderStatus(false);
    }

    public void EnableAllWallCollider()
    {
        VIPAreaBehaviour vipAreaBehaviour = GetComponentInChildren<VIPAreaBehaviour>();
        vipAreaBehaviour.SwitchAllColliderStatus(true);
    }

    public bool  IsPlayerInArea(Vector3 pos)
    {
        VIPAreaBehaviour vipAreaBehaviour = GetComponentInChildren<VIPAreaBehaviour>();
        bool isInArea = vipAreaBehaviour.CheckPlayerInArea(pos);
        return isInArea;
    }

    public void SaveComponentData()
    {
        ComponentData componentData = new ComponentData();
        var areaTrans = FindArea();
        componentData.areaPos = areaTrans.localPosition;
        componentData.areaRot = areaTrans.localRotation;
        componentData.areaScale = areaTrans.localScale;
        var doorTrans = FindDoor();
        componentData.doorPos = doorTrans.localPosition;
        componentData.doorRot = doorTrans.localRotation;
        componentData.doorScale = doorTrans.localScale;
        var doorWrapTrans = FindDoorWrap();
        componentData.doorWrapPos = doorWrapTrans.localPosition;
        componentData.doorWrapRot = doorWrapTrans.localRotation;
        componentData.doorWrapScale = doorWrapTrans.localScale;
        var doorEffectTrans = FindDoorEffect();
        componentData.doorEffectPos = doorEffectTrans.localPosition;
        componentData.doorEffectRot = doorEffectTrans.localRotation;
        componentData.doorEffectScale = doorEffectTrans.localScale;
        var checkTrans = FindCheck();
        componentData.checkPos = checkTrans.localPosition;
        componentData.checkRot = checkTrans.localRotation;
        componentData.checkScale = checkTrans.localScale;
        saveComponentData = componentData;
    }

    public void ResumeComponentData()
    {
        var areaTrans = FindArea();
        areaTrans.localPosition = saveComponentData.areaPos;
        areaTrans.localRotation = saveComponentData.areaRot;
        areaTrans.localScale = saveComponentData.areaScale;
        var doorTrans = FindDoor();
        doorTrans.localPosition = saveComponentData.doorPos;
        doorTrans.localRotation = saveComponentData.doorRot;
        doorTrans.localScale = saveComponentData.doorScale;
        var doorWrapTrans = FindDoorWrap();
        doorWrapTrans.localPosition = saveComponentData.doorWrapPos;
        doorWrapTrans.localRotation = saveComponentData.doorWrapRot;
        doorWrapTrans.localScale = saveComponentData.doorWrapScale;
        var doorEffectTrans = FindDoorEffect();
        doorEffectTrans.localPosition = saveComponentData.doorEffectPos;
        doorEffectTrans.localRotation = saveComponentData.doorEffectRot;
        doorEffectTrans.localScale = saveComponentData.doorEffectScale;
        var checkTrans = FindCheck();
        checkTrans.localPosition = saveComponentData.checkPos;
        checkTrans.localRotation = saveComponentData.checkRot;
        checkTrans.localScale = saveComponentData.checkScale;
        VIPCheckBoundControl vipCheckBoundControl = checkTrans.GetComponent<VIPCheckBoundControl>();
        if (vipCheckBoundControl != null)
        {
            vipCheckBoundControl.UpdateEffectShow();
        }
    }

    public void ResetComponentData()
    {
        var areaTrans = FindArea();
        areaTrans.localPosition = VIPZoneManager.Inst.vipAreaSrcPosition;
        areaTrans.localRotation = Quaternion.identity;
        areaTrans.localScale = Vector3.one;
        var doorTrans = FindDoor();
        doorTrans.localPosition = VIPZoneManager.Inst.doorSrcPosition;
        doorTrans.localRotation = Quaternion.identity;
        doorTrans.localScale = Vector3.one;
        var doorWrapTrans = FindDoorWrap();
        doorWrapTrans.localPosition = VIPZoneManager.Inst.doorSrcPosition;
        doorWrapTrans.localRotation = Quaternion.identity;
        doorWrapTrans.localScale = Vector3.one;
        var doorEffectTrans = FindDoorEffect();
        doorEffectTrans.localPosition = VIPZoneManager.Inst.doorSrcPosition;
        doorEffectTrans.localRotation = Quaternion.identity;
        doorEffectTrans.localScale = Vector3.one;
        var checkTrans = FindCheck();
        checkTrans.localPosition = VIPZoneManager.Inst.checkSrcPosition;
        checkTrans.localRotation  = Quaternion.identity;
        checkTrans.localScale = Vector3.one;
        VIPCheckBoundControl vipCheckBoundControl = checkTrans.GetComponent<VIPCheckBoundControl>();
        if (vipCheckBoundControl != null)
        {
            vipCheckBoundControl.UpdateEffectShow();
        }
    }

    private class ComponentData
    {
        public Vector3 areaPos;
        public Quaternion areaRot;
        public Vector3 areaScale;
        
        public Vector3 doorPos;
        public Quaternion doorRot;
        public Vector3 doorScale;
        
        public Vector3 doorWrapPos;
        public Quaternion doorWrapRot;
        public Vector3 doorWrapScale;
        
        public Vector3 checkPos;
        public Quaternion checkRot;
        public Vector3 checkScale;
        
        public Vector3 doorEffectPos;
        public Quaternion doorEffectRot;
        public Vector3 doorEffectScale;
    }
}