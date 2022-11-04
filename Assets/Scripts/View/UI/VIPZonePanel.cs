using System;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class VIPZonePanel : InfoPanel<VIPZonePanel>
{
    private SceneEntity entity;
    private VIPZoneBehaviour vipZoneBehaviour;
    private VIPZoneComponent vIPZoneComponent;
    public UgcPropChooseItem dcChoose;
    public UgcPropChooseItem doorChoose;
    public UgcPropChooseItem checkChoose;
    public UgcPropChooseItem doorEffectChoose;
    private Transform noDcPart;
    private Transform hasDcPart;
    private Transform customBtnTrans;
    private Toggle customToggle;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitView();
        InitPropChooseItems();
    }

    private void InitPropChooseItems()
    {
        dcChoose.OnInitByCreate();
        dcChoose.allUsedUgcs.AddRange(VIPZoneManager.Inst.GetUseUgcs(VIPComponentType.PassDC));
        dcChoose.destroyBehav = false;
        dcChoose.chooseType = UgcPropChooseItem.CHOOSE_TYPE_DC;
        dcChoose.createItemObjWhenClick = false;
        dcChoose.OnItemClickNotCreateCallBack += DCChooseCallBack;
        
        doorChoose.OnInitByCreate();
        doorChoose.AddSpecialItems(GetDoorDefaultItems());
        doorChoose.allUsedUgcs.AddRange(VIPZoneManager.Inst.GetUseUgcs(VIPComponentType.Door));
        doorChoose.destroyBehav = false;
        doorChoose.selectCreateObj = false;
        doorChoose.AfterUgcCreateFinishCallback += DoorCreated;
        
        checkChoose.OnInitByCreate();
        checkChoose.AddSpecialItems(GetCheckDefaultItems());
        checkChoose.allUsedUgcs.AddRange(VIPZoneManager.Inst.GetUseUgcs(VIPComponentType.Check));
        checkChoose.destroyBehav = false;
        checkChoose.selectCreateObj = false;
        checkChoose.AfterUgcCreateFinishCallback += CheckCreated;
        
        doorEffectChoose.OnInitByCreate();
        doorEffectChoose.AddSpecialItems(GetDoorEffectDefaultItems());
        doorEffectChoose.destroyBehav = false;
        doorEffectChoose.selectCreateObj = false;
    }

    private List<UgcPropChooseItem.SpecialItem> GetDoorDefaultItems()
    {
        List<UgcPropChooseItem.SpecialItem> defaultItems = new List<UgcPropChooseItem.SpecialItem>();
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_DEFAULT_ICON_PATH_1,
            key = VIPZoneConstant.DOOR_ID_1,
            onClick = ClickDoorDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_DEFAULT_ICON_PATH_2,
            key = VIPZoneConstant.DOOR_ID_2,
            onClick = ClickDoorDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_DEFAULT_ICON_PATH_3,
            key = VIPZoneConstant.DOOR_ID_3,
            onClick = ClickDoorDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_DEFAULT_ICON_PATH_5,
            key = VIPZoneConstant.DOOR_ID_5,
            onClick = ClickDoorDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_DEFAULT_ICON_PATH_4,
            key = VIPZoneConstant.DOOR_ID_4,
            onClick = ClickDoorDefault
        } );
        return defaultItems;
    }

    private List<UgcPropChooseItem.SpecialItem> GetCheckDefaultItems()
    {
        List<UgcPropChooseItem.SpecialItem> defaultItems = new List<UgcPropChooseItem.SpecialItem>();
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.CHECK_DEFAULT_ICON_PATH_1,
            key = VIPZoneConstant.CHECK_ID_1,
            onClick = ClickCheckDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.CHECK_DEFAULT_ICON_PATH_2,
            key = VIPZoneConstant.CHECK_ID_2,
            onClick = ClickCheckDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.CHECK_DEFAULT_ICON_PATH_3,
            key = VIPZoneConstant.CHECK_ID_3,
            onClick = ClickCheckDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.CHECK_DEFAULT_ICON_PATH_5,
            key = VIPZoneConstant.CHECK_ID_5,
            onClick = ClickCheckDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.CHECK_DEFAULT_ICON_PATH_4,
            key = VIPZoneConstant.CHECK_ID_4,
            onClick = ClickCheckDefault
        } );
        return defaultItems;
    }

    private List<UgcPropChooseItem.SpecialItem> GetDoorEffectDefaultItems()
    {
        List<UgcPropChooseItem.SpecialItem> defaultItems = new List<UgcPropChooseItem.SpecialItem>();
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_EFFECT_DEFAULT_ICON_PATH_1,
            key = ((int)GameResType.VIPDoorEffect).ToString(),
            onClick = ClickDoorEffectDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_EFFECT_DEFAULT_ICON_PATH_2,
            key = ((int)GameResType.VIPDoorEffect2).ToString(),
            onClick = ClickDoorEffectDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_EFFECT_DEFAULT_ICON_PATH_3,
            key = ((int)GameResType.VIPDoorEffect3).ToString(),
            onClick = ClickDoorEffectDefault
        } );
        defaultItems.Add(new UgcPropChooseItem.SpecialItem
        {
            iconPath = VIPZoneConstant.DOOR_EFFECT_DEFAULT_ICON_PATH_4,
            key = ((int)GameResType.VIPDoorEffect4).ToString(),
            onClick = ClickDoorEffectDefault
        } );
        return defaultItems;
    }

    private void ClickDoorDefault(string key)
    {
        doorChoose.ChooseItem(key);
        NodeBaseBehaviour doorBehaviour = vipZoneBehaviour.ChangeDoorDefault(key);
        doorBehaviour.entity.Get<VIPDoorComponent>().id = key;
        doorChoose.SetEntity(doorBehaviour);
    }

    private void ClickCheckDefault(string key)
    {
        checkChoose.ChooseItem(key);
        NodeBaseBehaviour checkBehaviour = vipZoneBehaviour.ChangeCheckDefault(key);
        checkChoose.SetEntity(checkBehaviour);
    }

    private void ClickDoorEffectDefault(string key)
    {
        doorEffectChoose.ChooseItem(key);
        NodeBaseBehaviour doorEffectBehaviour = vipZoneBehaviour.ChangeDoorEffectDefault(key);
        doorEffectBehaviour.entity.Get<VIPDoorEffectComponent>().id = key;
        doorEffectChoose.SetEntity(doorEffectBehaviour);
    }

    private void CheckCreated(NodeBaseBehaviour checkUgc, string id)
    {
        checkUgc.entity.Get<VIPCheckComponent>().id = id;
        var old = checkChoose.curBehav;
        checkUgc.transform.SetParent(old.transform.parent);
        VIPCheckBoundControl vipCheckBoundControl = checkUgc.GetComponent<VIPCheckBoundControl>();
        if (vipCheckBoundControl == null)
        {
            vipCheckBoundControl = checkUgc.gameObject.AddComponent<VIPCheckBoundControl>();
        }
        //这里的删除可能有延迟，直接创一个新的
        vipCheckBoundControl.InitEffect(false);
        vipCheckBoundControl.UpdateEffectShow();
        
        if (checkUgc.gameObject.GetComponent<SpawnPointConstrainer>() == null)
        {
            SpawnPointConstrainer spawnPointConstrainer = checkUgc.gameObject.AddComponent<SpawnPointConstrainer>();
            spawnPointConstrainer.minHeight = 0;
        }
        SceneBuilder.Inst.DestroyEntity(old.gameObject);
        checkChoose.SetEntity(checkUgc);
        checkChoose.ChooseItem(id);
    }

    private void DoorCreated(NodeBaseBehaviour doorUgc, string id)
    {
        doorUgc.entity.Get<VIPDoorComponent>().id = id;
        doorUgc.transform.SetParent(doorChoose.curBehav.transform.parent);
        SceneBuilder.Inst.DestroyEntity(doorChoose.curBehav.gameObject);
        doorChoose.SetEntity(doorUgc);
        doorChoose.ChooseItem(id);
    }

    private void DCChooseCallBack(MapInfo mapInfo)
    {
        string mapId = mapInfo.mapId;
        vIPZoneComponent.passId = mapId;
        if (mapInfo.dcInfo != null)
        {
            vIPZoneComponent.dcItemId = mapInfo.dcInfo.itemId;
        }
        dcChoose.ChooseItem(mapId);
        ParsePropWithTipsManager.Inst.DestroyGameObj();
        RefreshUI();
    }

    private void RefreshUI()
    {
        bool hasPassId = vIPZoneComponent.passId != null;
        noDcPart.gameObject.SetActive(!hasPassId);
        hasDcPart.gameObject.SetActive(hasPassId);
        if (hasPassId)
        {
            SyncViewShow();
        }
    }

    private void InitView()
    {
        noDcPart = GameUtils.FindChildByName(transform, "NoDCPart");
        hasDcPart = GameUtils.FindChildByName(transform, "HasDCPart");
        GameUtils.FindChildByName(transform,"closeBtnNo").GetComponent<Button>().onClick.AddListener(CloseClick);
        GameUtils.FindChildByName(transform,"closeBtnHas").GetComponent<Button>().onClick.AddListener(CloseClick);
        GameUtils.FindChildByName(transform,"AddButtonNo").GetComponent<Button>().onClick.AddListener(AddClick);
        customToggle = GameUtils.FindChildByName(transform,"CustomToggle").GetComponent<Toggle>();
        customToggle.onValueChanged.AddListener(OnCustomToggleChange);
        customBtnTrans = GameUtils.FindChildByName(transform,"CustomBtn");
        customBtnTrans.GetComponent<Button>().onClick.AddListener(EditClick);
    }

    private void OnCustomToggleChange(bool status)
    {
        customBtnTrans.gameObject.SetActive(status);
        vIPZoneComponent.isEdit = status ? 1 : 0;
        if (status)
        {
            EditClick();
        }
        else
        {
            vipZoneBehaviour.ResetComponentData();
        }
    }

    private void AddClick()
    {
        dcChoose?.OpenUgcBagpackAndChoose();
    }

    private void CloseClick()
    {
        var opanel = UIManager.Inst.uiCanvas.GetComponentsInChildren<IPanelOpposable>(true);
        for (int i = 0; i < opanel.Length; i++)
        {
            opanel[i].SetGlobalHide(true);
        }
        gameObject.SetActive(false);
    }

    public void SetEntity(SceneEntity entity)
    {
        this.entity = entity;
        vipZoneBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponent<VIPZoneBehaviour>();
        vIPZoneComponent = entity.Get<VIPZoneComponent>();
        dcChoose.SetEntity(vipZoneBehaviour);
        InitItemsEntities();
        RefreshUI();
        SyncViewShow();
    }

    private void SyncViewShow()
    {
        if (vIPZoneComponent.passId == null)
        {
            return;
        }
        dcChoose.ChooseItem(vIPZoneComponent.passId);
        VIPZoneManager.Inst.FindComponentsInChild(vipZoneBehaviour.gameObject,
            (behaviour, c) =>
            {
                if (c is VIPDoorComponent doorC)
                {
                    doorChoose.ChooseItem(doorC.id);
                }
            },
            (behaviour, c) =>
            {
                if (c is VIPDoorEffectComponent doorEffectC)
                {
                    doorEffectChoose.ChooseItem(doorEffectC.id);
                }
            },
            (behaviour,c) => { 
                if (c is VIPCheckComponent checkC)
                {
                    checkChoose.ChooseItem(checkC.id);
                }
            });
        //编辑状态恢复
        bool isEdit = vIPZoneComponent.isEdit == 1;
        customToggle.SetIsOnWithoutNotify(isEdit);
        customBtnTrans.gameObject.SetActive(isEdit);
    }

    private void InitItemsEntities()
    {
        VIPZoneManager.Inst.FindComponentsInChild(vipZoneBehaviour.gameObject, 
            (behaviour,c) => { doorChoose.SetEntity(behaviour);},
            (behaviour,c) => { doorEffectChoose.SetEntity(behaviour);},
            (behaviour,c) => { checkChoose.SetEntity(behaviour);});
    }

    private void EditClick()
    {
        SeparatePartEditPanel.Show();
        VIPZoneManager.Inst.HideOtherVIPZone(vipZoneBehaviour);
        SaveComponentData();
        AdjustInEdit();
        //先默认选中区域
        VIPAreaBehaviour area = entity.Get<GameObjectComponent>().bindGo.GetComponentInChildren<VIPAreaBehaviour>();
        SeparatePartEditPanel.Instance.SetUp(area.entity);
        SeparatePartEditPanel.Instance.SetEntities(new SceneEntity[]{entity},SeparatePartEditPanel.ENTITY_TYPE_VIP_ZONE);
        SeparatePartEditPanel.Instance.GetGizmoController().ShowXZAxis(false);
        SeparatePartEditPanel.Instance.SetTitle("Adjust the geometry of the VIP Zone as you want");
        Action sure = () =>
        {
            EditModeController.SetSelect(entity);
            VIPZoneManager.Inst.ShowAllVIPZone();
            ResumeFromEdit();
        };
        Action cancel = () =>
        {
            EditModeController.SetSelect(entity);
            VIPZoneManager.Inst.ShowAllVIPZone();
            ResumeFromEdit();
            ResumeComponentData();
        };
        SeparatePartEditPanel.Instance.SureBtnClickAct = sure;
        SeparatePartEditPanel.Instance.CancelBtnClickAct = cancel;
        SeparatePartEditPanel.Instance.SetClickAction(SelectVIPComponent);
        SeparatePartEditPanel.Instance.OnMoveSelect = OnEditPanelMoveSelect;
        SeparatePartEditPanel.Instance.OnRotateSelect = OnEditPanelRotateSelect;
        SeparatePartEditPanel.Instance.OnScaleSelect = OnEditPanelScaleSelect;
    }

    private void OnEditPanelMoveSelect()
    {
        SceneEntity entity = SeparatePartEditPanel.Instance.GetCurSelectEntity();
        if (entity == null)
        {
            return;
        }
        SeparatePartEditPanel.Instance.GetGizmoController().SetZMoveStatus(true);
        var behav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (behav.entity.HasComponent<VIPDoorComponent>())
        {
            VIPDoorWrapBehaviour doorWrap = behav.GetComponentInParent<VIPDoorWrapBehaviour>();
            SeparatePartEditPanel.Instance.SetCurSelectEntity(doorWrap.entity);
            SeparatePartEditPanel.Instance.GetGizmoController().SetTarget(doorWrap.gameObject);
            SeparatePartEditPanel.Instance.GetGizmoController().SetMoveCtr();
            SeparatePartEditPanel.Instance.GetGizmoController().SetZMoveStatus(false);
        }
    }

    private void OnEditPanelRotateSelect()
    {
        SceneEntity entity = SeparatePartEditPanel.Instance.GetCurSelectEntity();
        if (entity == null)
        {
            return;
        }
        var behav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (behav is VIPDoorWrapBehaviour)
        {
            NodeBaseBehaviour door = VIPZoneManager.Inst.FindDoor(behav);
            SeparatePartEditPanel.Instance.SetCurSelectEntity(door.entity);
            SeparatePartEditPanel.Instance.GetGizmoController().SetTarget(door.gameObject);
            SeparatePartEditPanel.Instance.GetGizmoController().SetRotateCtr();
        }
    }

    private void OnEditPanelScaleSelect()
    {
        SceneEntity entity = SeparatePartEditPanel.Instance.GetCurSelectEntity();
        if (entity == null)
        {
            return;
        }
        var behav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (behav is VIPDoorWrapBehaviour)
        {
            NodeBaseBehaviour door = VIPZoneManager.Inst.FindDoor(behav);
            SeparatePartEditPanel.Instance.SetCurSelectEntity(door.entity);
            SeparatePartEditPanel.Instance.GetGizmoController().SetTarget(door.gameObject);
            SeparatePartEditPanel.Instance.GetGizmoController().SetScaleCtr(null);
        }
    }

    private void ResumeFromEdit()
    {
        ResumeHierarchy();
        RemoveScaleKeeper();
    }

    private void RemoveScaleKeeper()
    {
        Transform door = vipZoneBehaviour.FindDoor();
        Destroy(door.GetComponent<DoorScaleKeeper>());
    }

    private void ResumeHierarchy()
    {
        Transform door = vipZoneBehaviour.FindDoor();
        Transform doorWrap = vipZoneBehaviour.FindDoorWrap();
        Transform area = vipZoneBehaviour.FindArea();
        Transform effect = vipZoneBehaviour.FindDoorEffect();
        doorWrap.SetParent(area.parent);
        door.SetParent(area.parent);
        effect.SetParent(area.parent);
    }

    private void AdjustInEdit()
    {
        AdjustHierarchy();
        AddScaleKeeper();
        SetScaleType();
    }

    private void SetScaleType()
    {
        VIPZoneManager.Inst.FindComponentsInChild(vipZoneBehaviour.gameObject, 
            (be,c) => { be.norScale = true; }, null,
            (be,c) => { be.norScale = true; });
    }

    private void AddScaleKeeper()
    {
        Transform doorWrap = vipZoneBehaviour.FindDoorWrap();
        DoorScaleKeeper keeper = doorWrap.gameObject.GetComponent<DoorScaleKeeper>();
        if (keeper == null)
        {
            keeper = doorWrap.gameObject.AddComponent<DoorScaleKeeper>();
        }
        keeper.StashScale();
    }

    private void AdjustHierarchy()
    {
        Transform door = vipZoneBehaviour.FindDoor();
        Transform doorWrap = vipZoneBehaviour.FindDoorWrap();
        Transform area = vipZoneBehaviour.FindArea();
        Transform effect = vipZoneBehaviour.FindDoorEffect();
        doorWrap.SetParent(area);
        door.SetParent(doorWrap);
        effect.SetParent(door);
    }

    private void ResumeComponentData()
    {
        vipZoneBehaviour.ResumeComponentData();
    }

    private void SaveComponentData()
    {
        vipZoneBehaviour.SaveComponentData();
    }

    public void SelectVIPComponent(Ray ray,float maxDis)
    {
        bool isHit = Physics.Raycast(ray, out RaycastHit hit, 2 * maxDis);
        if (isHit)
        {
            GameObject go = hit.collider.gameObject;
            NodeBaseBehaviour nodeBehav = go.GetComponent<NodeBaseBehaviour>();
            if (nodeBehav == null)
                nodeBehav = FindFirst(go.transform);
            if (nodeBehav == null)
            {
                SeparatePartEditPanel.Instance.Select(null);
                return;
            }
            if (VIPZoneManager.Inst.IsCanSelectComponent(nodeBehav.entity))
            {
                SeparatePartEditPanel.Instance.Select(nodeBehav.entity);
                var gizmoController = SeparatePartEditPanel.Instance.GetGizmoController();
                if (nodeBehav.entity.HasComponent<VIPDoorComponent>())
                {
                    gizmoController.SetMoveTransformLocal();
                }
                else
                {
                    gizmoController.SetMoveTransformGlobal();
                }
                if (nodeBehav is VIPAreaBehaviour)
                {
                    gizmoController.ShowXZAxis(false);
                }
                else
                {
                    gizmoController.ShowXZAxis(true);
                }
            }
            else
            {
                SeparatePartEditPanel.Instance.Select(null);
            }
        }
    }

    private NodeBaseBehaviour FindFirst(Transform t)
    {
        Transform parent = t.parent;
        if (parent == null)
        {
            return null;
        }
        NodeBaseBehaviour behavInParent = parent.GetComponent<NodeBaseBehaviour>();
        if (behavInParent != null)
        {
            return behavInParent;
        }

        return FindFirst(t.parent);
    }
    
}