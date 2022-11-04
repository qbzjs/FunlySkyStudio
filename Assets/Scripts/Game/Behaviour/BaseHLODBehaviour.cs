using System;
using System.Linq;
using HLODSystem;
using HLODSystem.Extensions;
using UnityEngine;
using Assets.Scripts.Game.Core;

public class BaseHLODBehaviour : NodeBaseBehaviour {
    
    [NonSerialized]
    protected NodeData nodeData;

    public  GameObject assetObj;


    
    protected GameObject highObj;
    protected GameObject lowObj;
     
    [SerializeField]
    private string m_hlodId;
    private Type[] hlodTypes = new []{typeof(MovementComponent), typeof(UGCPropComponent)};
    private Type[] ignoreTypes = new []{typeof(GameObjectComponent), typeof(MaterialComponent)};
    protected bool isCanHLOD = false;
    
    
    protected Renderer[] allRenderers;
    public string rid;
    
    public string HLODID
    {
        get
        {
            if (string.IsNullOrEmpty(m_hlodId) && nodeData != null)
            {
                m_hlodId = nodeData.uid + "_" + nodeData.ToHash();
            }
            return m_hlodId;
        }
        set => m_hlodId = value;
    }

    [SerializeField]
    protected bool isOcclusion = false;
    public virtual bool IsOcclusion { 
        get => isOcclusion;
        set
        {
            isOcclusion = value;
            if (allRenderers == null) return;
            foreach (var tmpRender in allRenderers)
            {
                tmpRender.forceRenderingOff = isOcclusion;
            }
        }
    }

    public virtual Bounds? GetBounds()
    {
        return MapNodeData.Get(nodeData.ToHash()).GetBounds();
    }
    
    

    public HLODState hlodState = HLODState.NoThing;
    
    
    public NodeData data
    {
        set
        {
            if (value != null)
            {
                rid = value.rid;
            }
            nodeData = value; 
        }
    }

    public override void OnReset()
    {
        hlodState = HLODState.NoThing;
        isCanHLOD = false;
        IsOcclusion = false;
        m_hlodId = null;
        base.OnReset();
    }
    
    public virtual void SetLODStatus(HLODState state)
    {
        if (state == hlodState)
        {
            return;
        }

        hlodState = state;
        switch (hlodState)
        {
            case HLODState.Cull:
                ChangeToCull();
                break;
            case HLODState.High:
                ChangeToHigh();
                break;
            case HLODState.Low:
                ChangeToLow();
                break;
        } 
    }

    public virtual void ChangeToLow()
    {
        if (assetObj == null)
        {
            SetAssetObj(nodeData.id);
            SetUp();
        }
    }

    public virtual void ChangeToHigh()
    {
        if (assetObj == null)
        {
            SetAssetObj(nodeData.id);
            SetUp();
        }
    }

    public virtual void ChangeToCull()
    {
        if (assetObj != null)
        {
            ModelCachePool.Inst.Release(nodeData.id, assetObj);
            assetObj = null;
        }
    }

    public virtual void SetUp()
    {

    }
    /// <summary>
    /// 判断节点是否是移动节点, 仅位置移动
    /// </summary>
    /// <param name="behaviour"></param>
    /// <returns></returns>
    public bool IsMoving(BaseHLODBehaviour behaviour)
    {
        var isMoving = CheckMovingParent(behaviour) || CheckMovingSelf(behaviour) ||
                       CheckMovingChildren(behaviour);
        return isMoving;
    }
    
    private bool CheckMovingSelf(BaseHLODBehaviour behaviour)
    {
        var isMoving = false;
        if(behaviour.entity.HasComponent<MovementComponent>())
        {
            isMoving = behaviour.entity.Get<MovementComponent>().pathPoints.Count != 0;
        }
        return isMoving || behaviour.entity.HasComponent<FollowableComponent>() ||
               behaviour.entity.HasComponent<PickablityComponent>() ||
               behaviour.entity.HasComponent<SteeringWheelComponent>();
    }
    public bool CheckMovingParent(BaseHLODBehaviour behaviour)
    {
        BaseHLODBehaviour parentBehaviour = behaviour.transform.parent.GetComponentInParent<BaseHLODBehaviour>();
        return parentBehaviour != null && IsMoving(parentBehaviour);
    }
    
    private bool CheckMovingChildren(BaseHLODBehaviour behaviour)
    {
        bool isMoving = false;
        if (behaviour.transform.childCount == 0)
        {
            return false;
        }

        var childrenBehaviours = behaviour.transform.GetComponentsInChildren<SteeringWheelBehaviour>(true);
        if(childrenBehaviours.Length != 0)isMoving = true;

        return isMoving;
    }

    protected virtual void SetAssetObj(int id)
    {
        assetObj = ModelCachePool.Inst.Get(id);
        if(assetObj == null)return;
        assetObj.transform.SetParent(transform);
        assetObj.transform.localPosition = Vector3.zero;
        assetObj.transform.localEulerAngles = Vector3.zero;
        assetObj.transform.localScale = Vector3.one;
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        SetInteractBtnOnClickByIconName();
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    public void SetInteractBtnOnClickByIconName()
    {
        var playIconEnum = PortalPlayPanel.IconName.None;
        if (entity.HasComponent<EdibilityComponent>())
        {
            playIconEnum = PortalPlayPanel.IconName.Eat;
        }
        switch (playIconEnum)
        {
            case PortalPlayPanel.IconName.Eat:
                SetInteractBtnOnClickByIconName(playIconEnum, () => { EdibilitySystemController.Inst.OnSceneNodeFoodBtnClick(this); });
                break;
        }
    }

    public void SetInteractBtnOnClickByIconName(PortalPlayPanel.IconName iconName,Action onClick)
    {
     
        if (PortalPlayPanel.Instance == null || !PortalPlayPanel.Instance.gameObject.activeSelf)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(iconName);
            PortalPlayPanel.Instance.SetTransform(this.transform);
            PortalPlayPanel.Instance.AddButtonClick(()=> { onClick?.Invoke(); });
        }
        else
        {
            PortalPlayPanel.Instance.AddExtraIcon(iconName, onClick);
        }
    }
}
