using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickablityPanel : BasePanel<PickablityPanel>
{
    public Toggle UnpickableToggle;
    public Toggle PickableToggle;
    [SerializeField]
    private Toggle customToggle;
    [SerializeField]
    private Button edit;
    [HideInInspector]
    public bool IsPickablity;

    private SceneEntity curEntity;

    protected override void Awake()
    {
        base.Awake();
        UnpickableToggle.onValueChanged.AddListener(OnUnpickableClick);
        PickableToggle.onValueChanged.AddListener(OnPickableClick);
        customToggle.onValueChanged.AddListener(onCustomClick);
        edit.onClick.AddListener(onEditClick);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnpickableToggle.onValueChanged.RemoveAllListeners();
        PickableToggle.onValueChanged.RemoveAllListeners();
        customToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnEnable()
    {
        if(curEntity != null)
        {
            bool isPickability = PickabilityManager.Inst.CheckPickability(curEntity);
            UnpickableToggle.isOn = !isPickability;
            PickableToggle.isOn = isPickability;
            edit.gameObject.SetActive(isPickability);
            customToggle.SetIsOnWithoutNotify(isPickability);
            customToggle.graphic.canvasRenderer.SetAlpha(isPickability?1f:0f);
            edit.gameObject.SetActive(isPickability);
        }
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        bool isPickability = PickabilityManager.Inst.CheckPickability(curEntity);

        UnpickableToggle.isOn = !isPickability;
        PickableToggle.isOn = isPickability;
        if (!isPickability)
            customToggle.isOn = isPickability;
    }

    private void OnUnpickableClick(bool isOn)
    {
        if(PickabilityManager.Inst.CheckPickability(curEntity) && isOn)
        {
            PickabilityManager.Inst.RemovePickablityProp(curEntity);
            customToggle.isOn = false;
        }
    }

    private void OnPickableClick(bool isOn)
    {
        var curState = PickableToggle.isOn;
        if (curState)
        {
            if (curEntity.HasComponent<PickablityComponent>())
            {
                return;
            }
        }
        if (IsConstainSpecialComp(curEntity))
        {
            UnpickableToggle.isOn = true;
            PickableToggle.isOn = false;
            return;
        }
        if (isOn)
        {
            bool canSetAttr = PickabilityManager.Inst.CheckCanSetPickability();
            if (canSetAttr)
            {
                PickabilityManager.Inst.AddPickablityProp(curEntity, curEntity.Get<PickablityComponent>().anchors);
            }
            else
            {
                TipPanel.ShowToast(PickabilityManager.MAX_COUNT_TIP);
                UnpickableToggle.isOn = true;
                PickableToggle.isOn = false;
            }
        }
        else
        {
            if (curEntity != null && curEntity.HasComponent<CatchabilityComponent>())
            {
                TipPanel.ShowToast("You can't turn pickpability off while catchability is on");
                UnpickableToggle.isOn = false;
                PickableToggle.isOn = true;
                edit.gameObject.SetActive(true);
                customToggle.SetIsOnWithoutNotify(true);
                return;
            }
        }
    }
    
    private void onCustomClick(bool isOn)
    {
        if(isOn)
        {
            bool isPickability = PickabilityManager.Inst.CheckPickability(curEntity);
            if (!isPickability)
            {
                TipPanel.ShowToast("Please set the object pickable first");
                customToggle.graphic.canvasRenderer.SetAlpha(0f);
                customToggle.SetIsOnWithoutNotify(false);
                return;
            }
            onEditClick();
        }
        else
        {
            PickabilityManager.Inst.SetAnchors(curEntity, Vector3.zero);
        }
        edit.gameObject.SetActive(isOn);
    }
    
    private void onEditClick()
    {
        PickablityAnchorsPanel.Show();
        var pos = PickabilityManager.Inst.GetAnchors(curEntity);
        PickablityAnchorsPanel.Instance.Init(curEntity, pos);
        UIManager.Inst.uiCanvas.gameObject.SetActive(true);
    }

    private bool IsConstainSpecialComp(SceneEntity entity)
    {
        if (entity.HasComponent<FollowableComponent>() && entity.Get<FollowableComponent>().moveType == (int)MoveMode.Follow)
        {
            return true;
        }
        return false;
    }
}
