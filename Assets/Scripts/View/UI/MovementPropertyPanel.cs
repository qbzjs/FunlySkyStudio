using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class MovementPropertyPanel : MonoBehaviour
{
    public Transform SpeedParent;
    public Text PointNumText;
    public Button SubButton;
    public Button AddButton;
    public Toggle TurnAroundToggle;
    public Toggle anchorToggle;
    public Toggle followToggle;
    public Transform ancTypePanel;
    //为了兼容旧版保存的数据，不更改原有数组的顺序和数量
    private string[] speedNames = { "Still", "Slow", "Medium", "Fast" };
    private List<CommonButtonItem> speedScripts = new List<CommonButtonItem>();
    private SceneEntity curEntity;
    private int sIndex;
    private int pointCount;
    private EventTrigger trigger;
    private PropertySwitchPanel switchPanel;
    private PropertyCollectiblesPanel collectiblesPanel;
    private PropertySensorBoxPanel sensorBoxPanel;
    public Toggle activeToggle;
    public Toggle inActiveToggle;
    public GameObject[] Panels;
    public List<GameObject> hidePanel;

    public void Init()
    {
        trigger = GetComponentInChildren<EventTrigger>();
        EventTrigger.Entry onSelect = new EventTrigger.Entry();
        onSelect.eventID = EventTriggerType.PointerClick;
        onSelect.callback.AddListener(Select);
        trigger.triggers.Add(onSelect);
        SubButton.onClick.AddListener(OnSubPoint);
        AddButton.onClick.AddListener(OnAddPoint);
        TurnAroundToggle.onValueChanged.AddListener(OnTurnAround);
        anchorToggle.onValueChanged.AddListener(OnAnchorMode);
        followToggle.onValueChanged.AddListener(OnFollowMode);
        var proItem = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "PropertiesButtonItem");
        for (int i = 0; i < speedNames.Length; i++)
        {
            int index = i;
            var rItem = GameObject.Instantiate(proItem, SpeedParent);
            var rScript = rItem.AddComponent<CommonButtonItem>();
            rScript.Init();
            rScript.SetText(speedNames[i]);
            rScript.AddClick(() => OnMoveBySpeed(index));
            //为了兼容旧版保存的数据，index 为0表示静止，且不显示静止选项
            rScript.gameObject.SetActive(index != 0);
            speedScripts.Add(rScript);
        }


        activeToggle.onValueChanged.AddListener(OnToggleActive);
        inActiveToggle.onValueChanged.AddListener(OnToggleInactive);
        switchPanel = Panels[0].GetComponent<PropertySwitchPanel>();
        switchPanel.CtrlType = SwitchControlType.MOVEMENT_CONTROL;
        switchPanel.Init();
        collectiblesPanel = Panels[1].GetComponent<PropertyCollectiblesPanel>();
        collectiblesPanel.CtrlType = CollectControlType.MOVEMENT_CONTROL;
        collectiblesPanel.Init();
        sensorBoxPanel = Panels[2].GetComponent<PropertySensorBoxPanel>();
        sensorBoxPanel.CtrlType = PropControlType.MOVEMENT_CONTROL;
        sensorBoxPanel.Init();
    }

    private void OnToggleActive(bool isOn)
    {
        if (isOn)
        {
            var comp = curEntity.Get<MovementComponent>();
            comp.moveState = 0;
            comp.tempMoveState = comp.moveState;
        }
    }

    private void OnToggleInactive(bool isOn)
    {
        if (isOn)
        {
            var comp = curEntity.Get<MovementComponent>();
            comp.moveState = 1;
            comp.tempMoveState = comp.moveState;
        }
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var comp = curEntity.Get<MovementComponent>();
        comp.tempMoveState = comp.moveState;
        activeToggle.isOn = comp.moveState == 0;
        inActiveToggle.isOn = comp.moveState == 1;
        InitMoveType();
        OnMoveBySpeed(comp.speedId);
        pointCount = comp.pathPoints == null ? 0 : comp.pathPoints.Count;
        PointNumText.text = pointCount.ToString();
        TurnAroundToggle.isOn = comp.turnAround == 1;
        SubButton.interactable = pointCount > 0;
        AddButton.interactable = pointCount < 99;
        switchPanel.SetEntity(entity);
        collectiblesPanel.SetEntity(entity);
        //SetFollowModePro(entity);
        sensorBoxPanel.SetEntity(entity);
    }

    private void OnMoveBySpeed(int index)
    {
        speedScripts[sIndex].SetSelectState(false);
        sIndex = index;
        speedScripts[sIndex].SetSelectState(true);
        curEntity.Get<MovementComponent>().speedId = index;
        //为了兼容旧版保存的数据，index=0 物体静止，且选中慢速选项
        if (index == 0)
        {
            activeToggle.isOn = false;
            inActiveToggle.isOn = true;
            sIndex = 1;
            speedScripts[1].SetSelectState(true);
            curEntity.Get<MovementComponent>().speedId = 1;
            curEntity.Get<MovementComponent>().moveState = 1;
            curEntity.Get<MovementComponent>().tempMoveState = 1;
        }
    }

    private void OnSubPoint()
    {
        var moveComp = curEntity.Get<MovementComponent>();
        SubButton.interactable = moveComp.pathPoints.Count > 0;
        if (moveComp.pathPoints != null && moveComp.pathPoints.Count > 0)
        {
            pointCount = moveComp.pathPoints.Count - 1;
            moveComp.pathPoints.RemoveAt(pointCount);
            MovePathManager.Inst.SubMovePoint();
            PointNumText.text = pointCount.ToString();
            AddButton.interactable = true;
        }
    }

    private void OnAddPoint()
    {
        AddButton.interactable = pointCount < 99;
        if (pointCount >= 99)
        {
            TipPanel.ShowToast("Exceed limit:(");
            return;
        }
        SubButton.interactable = true;
        var moveComp = curEntity.Get<MovementComponent>();
        if (moveComp.pathPoints == null)
        {
            moveComp.pathPoints = new List<Vector3>();
        }
        var pos = CameraUtils.Inst.GetCreatePosition();
        moveComp.pathPoints.Add(pos);
        pointCount = moveComp.pathPoints.Count;
        MovePathManager.Inst.AddMovePoint(pos, moveComp.pathPoints.Count);
        PointNumText.text = pointCount.ToString();

    }

    private void OnTurnAround(bool isOn)
    {
        curEntity.Get<MovementComponent>().turnAround = isOn ? 1 : 0;
    }

    private void Select(BaseEventData data)
    {
        string str = PointNumText.text.Trim();
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = str,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }
    public void ShowKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        //记录玩家输入的数量
        int pointNum;
        //判断如果输入不是整数则跳过并且弹tips
        if(!int.TryParse(str,out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        //如果输入的不是0-99的数字弹tips
        if (pointNum > 99|| pointNum < 0)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        GeneratePoint(pointNum);
        //把结果传给UI文字
        PointNumText.text = int.Parse(str).ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    public void GeneratePoint(int pointNum)
    {
        //获取现有的移动点
        var moveComp = curEntity.Get<MovementComponent>();
        //用玩家输入的移动点减去现有的
        pointNum -= moveComp.pathPoints.Count;
        //如果大于零则说明输入的比现有的多，循环调用添加方法
        if (pointNum > 0)
        {
            for (int i = 0; i < pointNum; i++)
            {
                OnAddPoint();
            }
        }
        //如果小于零则说明输入的比现有的少，循环调用减少方法
        else if (pointNum < 0)
        {
            for (int i = 0; i < Mathf.Abs(pointNum); i++)
            {
                OnSubPoint();
            }
        }
    }

    public void SetFollowModePro(SceneEntity entity)
    {
        if (entity.HasComponent<FollowableComponent>())
        {
            followToggle.gameObject.SetActive(true);
        }
        else
        {
            followToggle.gameObject.SetActive(false);
        }
    }

    private void OnAnchorMode(bool isOn)
    {
        if (!curEntity.HasComponent<FollowableComponent>())
        {
            SetClickPanelState(true);
            return;
        }
        if (isOn)
        {
            SetClickPanelState(true);
            var gComp = curEntity.Get<GameObjectComponent>();
            curEntity.Get<MovementComponent>();
            curEntity.Remove<FollowableComponent>();
            FollowModeManager.Inst.DistoryFollowBox(gComp.bindGo);
        }
        ModelPropertyPanel.Instance.RefreshAttributePanel();
    }

    private void OnFollowMode(bool isOn)
    {
        if (isOn)
        {
            if (curEntity.HasComponent<CatchabilityComponent>())
            {
                anchorToggle.isOn = true;
                TipPanel.ShowToast("You can't set Movement Mode to Follow while Catchability is on");
                return;
            }

            SetClickPanelState(false);
            var comp = curEntity.Get<MovementComponent>();
            MovePathManager.Inst.ReleaseAllPoints();
            comp.pathPoints.Clear();
            pointCount = comp.pathPoints.Count;
            PointNumText.text = pointCount.ToString();
            curEntity.Get<FollowableComponent>();
            if (curEntity.HasComponent<PickablityComponent>())
            {
                TipPanel.ShowToast("Pickablity has been switched off");
            }
            if (PickabilityManager.Inst.CheckPickability(curEntity))
            {
                PickabilityManager.Inst.RemovePickablityProp(curEntity);
            }

            if (curEntity.HasComponent<EdibilityComponent>())
            {
                TipPanel.ShowToast("Edibility has been switched off");
                EdibilityManager.Inst.RemoveEdibilityProp(curEntity);
            }
            ModelPropertyPanel.Instance.RefreshAttributePanel();

            var gComp = curEntity.Get<GameObjectComponent>();
            if (FollowModeManager.Inst.GetFolowBoxGo(gComp.uid)) 
            {
                return;
            }
            CreateFollowBox(gComp.bindGo);
        }
    }

    private void CreateFollowBox(GameObject target)
    {
        curEntity.Get<FollowableComponent>().moveType = (int)MoveMode.Follow;
        var behav = target.GetComponent<NodeBaseBehaviour>();
        FollowModeManager.Inst.BuildFollowBox(behav);
    }

    private void SetClickPanelState(bool active)
    {
        ancTypePanel.gameObject.SetActive(active);
        var tempColor = active == true ? Color.white : new Color(0.58f, 0.58f, 0.58f, 1f);
        foreach (var panel in hidePanel)
        {
            var comps = panel.GetComponentsInChildren<MaskableGraphic>(true);
            foreach (var comp in comps)
            {
                comp.color = tempColor;
            }
        }
    }

    private void SetFollowToggle(bool active)
    {
        anchorToggle.isOn = !active;
        followToggle.isOn = active;
        SetClickPanelState(!active);
    }

    private void InitMoveType()
    {
        SetClickPanelState(true);
        if (!FollowModeManager.Inst.IsContainSpecialEntity(curEntity))
        {
            followToggle.gameObject.SetActive(false);

        }
        else if (curEntity.HasComponent<BloodPropComponent>())
        {
            followToggle.gameObject.SetActive(false);
        }
        else if (curEntity.HasComponent<FireworkComponent>())
        {
            followToggle.gameObject.SetActive(false);
        }
        else if (curEntity.HasComponent<FreezePropsComponent>())
        {
            followToggle.gameObject.SetActive(false);
        }
        else
        {
            followToggle.gameObject.SetActive(true);
            var isFollow = curEntity.HasComponent<FollowableComponent>();
            SetFollowToggle(isFollow);
        }
    }
}
