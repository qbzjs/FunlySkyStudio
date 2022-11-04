using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeesawPanel : UgcChoosePanel<SeesawPanel>
{
    private SceneEntity entity;
    private SeesawBehaviour seesawBehaviour;
    private SeesawComponent seesawComponent;
    private Transform leftRightChoose;
    private Transform leftRightChooseParent;
    private ToggleGroup tgLeftRightChoose;
    private Toggle leftSeat;
    private Toggle rightSeat;
    private Transform seatContent;
    private Toggle symmetryToggle;
    
    private float tileIncrement = 0.1f;

    private bool IsChooseLeftSeat => leftSeat.isOn;
    private bool IsSymmetry => symmetryToggle.isOn;
    private bool symmetrySign;
    private Button customSeatPosBtn;
    private Button customSitPointBtn;
    private bool customSitPointChangeIgnore;
    private bool customSeatPosChangeIgnore;
    private bool symmetryChangeIgnore;
    private Toggle customSitPointToggle;
    private Toggle customSeatPosToggle;
    private Vector3 leftSeatPos;
    private Quaternion leftSeatRot;
    private Vector3 leftSeatScale;
    private Vector3 rightSeatPos;
    private Quaternion rightSeatRot;
    private Vector3 rightSeatScale;

    enum PanelShowType
    {
        Material = 0,
        Color = 1,
        Setting = 2,
    };
    
    public override void SetEntity(SceneEntity entity)
    {
        this.entity = entity;
        seesawBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponentInChildren<SeesawBehaviour>();
        seesawComponent = entity.Get<SeesawComponent>();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        
        SeesawComponent comp = entity.Get<SeesawComponent>();
        Color modelColor = DataUtils.DeSerializeColor(comp.color);
        colorStr = DataUtils.ColorToString(modelColor);

        var matData = GetMatDataById(comp.mat);
        var matSelectIndex = matDatas.FindIndex(x => x.id == comp.mat);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
        SetMatSelect(matSelectIndex);
        var colorSelectIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), colorDatas.List);
        SetEntitySelectColor(colorSelectIndex, comp.color);
        SetSettingData();
    }

    private void SetSettingData()
    {
//        SetSymmetryToggleValue(seesawComponent.symmetry == 1);
        symmetryToggle.isOn = seesawComponent.symmetry == 1;
        bool panelChooseLeft = seesawComponent.panelChooseSeatIndex == 0;
        leftSeat.isOn = panelChooseLeft;
        rightSeat.isOn = !panelChooseLeft;
        SyncLeftRightViewShow(panelChooseLeft);
    }

    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        tab.AddClickListener(OnMaterialClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
        tab = tabGroup.GetTab((int)PanelShowType.Setting);
        tab.AddClickListener(OnSettingClick);
    }

    private void OnSettingClick()
    {
    }

    private void OnMaterialClick()
    {
        var matData = GetMatDataById(seesawComponent.mat);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
    }
    private void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }
    
    public override void GetMatDatas()
    {
        matDatas.Clear();
        matDatas.AddRange(GameManager.Inst.matConfigDatas);
        // 屏蔽透明材质
        HideSpecialMat(GameConsts.TRANSPARENT_MAT_ID);
        //TODO: FEAT 发光材质
        //屏蔽发光材质
        // HideSpecialMat(GameConsts.EMISSION_MAT_ID);
    }
    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        CreatePanel();
        GetMatDatas();
        GetColorDatas();
        CreateColorItems();
        CreateMatItems();
        UpdateCustomizePanel();
        AddCommonListener();
        AddMatTypeListener();
        InitSettingViews();
        InitTilingViews();
        InitDefaultSeatItem();
    }

    private void InitDefaultSeatItem()
    {
        string iconPath = "Texture/Seesaw/seat";
        CreateItemUISpecial(iconPath, () =>
        {
            ChangeSeatOrigin();
        });
    }

    private void ChangeSeatOrigin()
    {
        ChooseItem(SeesawManager.SEAT_DEFAULT);
        if (IsSymmetry)
        {
            seesawBehaviour.ChangeSeatOrigin(true,IsSymmetry);
            seesawBehaviour.ChangeSeatOrigin(false,IsSymmetry);
        }
        else
        {
            if (IsChooseLeftSeat)
            {
                seesawBehaviour.ChangeSeatOrigin(true,IsSymmetry);
            }
            else
            {
                seesawBehaviour.ChangeSeatOrigin(false,IsSymmetry);
            }
        }
    }

    private void InitTilingViews()
    {
        Button addTile = GameUtils.FindChildByName(transform,"AddButton").GetComponent<Button>();
        addTile.onClick.AddListener(AddTile);
        Button subTile = GameUtils.FindChildByName(transform,"SubButton").GetComponent<Button>();
        subTile.onClick.AddListener(SubTile);
    }

    private void InitSettingViews()
    {
        symmetryToggle = GameUtils.FindChildByName(transform,"SymmetryToggle").GetComponent<Toggle>();
        symmetryToggle.onValueChanged.AddListener(SymmetryChange);
        leftRightChoose = GameUtils.FindChildByName(transform, "LeftRightChoose");
        leftRightChooseParent = GameUtils.FindChildByName(transform, "LeftRightParent");
        tgLeftRightChoose = leftRightChoose.GetComponent<ToggleGroup>();
        leftSeat = GameUtils.FindChildByName(transform, "LeftSeat").GetComponent<Toggle>();
        rightSeat = GameUtils.FindChildByName(transform, "RightSeat").GetComponent<Toggle>();
        leftSeat.onValueChanged.AddListener(SeatChooseChange);
        rightSeat.onValueChanged.AddListener(SeatChooseChange);
        seatContent = GameUtils.FindChildByName(transform, "SeatContent");
        customSeatPosToggle = GameUtils.FindChildByName(transform, "CustomSeatPositionToggle").GetComponent<Toggle>();
        customSeatPosToggle.onValueChanged.AddListener(OnCustomSeatPositionChange);
        customSeatPosBtn = GameUtils.FindChildByName(transform, "CustomSeatPositionBtn").GetComponent<Button>();
        customSeatPosBtn.onClick.AddListener(CustomSeatPosition);
        customSitPointToggle = GameUtils.FindChildByName(transform, "CustomSitPointToggle").GetComponent<Toggle>();
        customSitPointToggle.onValueChanged.AddListener(OnCustomSitPointChange);
        customSitPointBtn = GameUtils.FindChildByName(transform, "CustomSitPointBtn").GetComponent<Button>();
        customSitPointBtn.onClick.AddListener(CustomSitPoint);
    }

    private void CustomSeatPosition()
    {
        bool left = ShouldOperateLeft();
        SceneEntity seatEntity;
        if (left)
        {
            seatEntity = seesawBehaviour.FindSeat(0).GetComponent<NodeBaseBehaviour>().entity;
            seesawComponent.setLeftSeatPos = 1;
            if (IsSymmetry)
            {
                seesawComponent.setRightSeatPos = 1;
            }
        }
        else
        {
            seatEntity = seesawBehaviour.FindSeat(1).GetComponent<NodeBaseBehaviour>().entity;
            seesawComponent.setRightSeatPos = 1;
        }
        SeparatePartEditPanel.Show();
        SaveSeatParams();
        SeparatePartEditPanel.Instance.SetUp(seatEntity);
        SeparatePartEditPanel.Instance.SetEntities(new SceneEntity[]{entity},SeparatePartEditPanel.ENTITY_TYPE_UGC_COMBINE);
        SeparatePartEditPanel.Instance.SetTitle("Adjust the seat position.");
        Action sure = () =>
        {
            EditModeController.SetSelect(entity);
        };
        Action cancel = () =>
        {
            ResumeSeatParams();
            EditModeController.SetSelect(entity);
        };
        SeparatePartEditPanel.Instance.SureBtnClickAct = sure;
        SeparatePartEditPanel.Instance.CancelBtnClickAct = cancel;
        SeparatePartEditPanel.Instance.SetClickAction(null);
    }

    private void ResumeSeatParams()
    {
        Transform leftSeat = seesawBehaviour.FindSeat(0);
        leftSeat.transform.position = leftSeatPos;
        leftSeat.transform.rotation = leftSeatRot;
        leftSeat.transform.localScale = leftSeatScale;
        Transform rightSeat = seesawBehaviour.FindSeat(1);
        rightSeat.transform.position = rightSeatPos;
        rightSeat.transform.rotation = rightSeatRot;
        rightSeat.transform.localScale = rightSeatScale;
    }

    private void SaveSeatParams()
    {
        Transform leftSeat = seesawBehaviour.FindSeat(0);
        leftSeatPos = leftSeat.transform.position;
        leftSeatRot = leftSeat.transform.rotation;
        leftSeatScale = leftSeat.transform.localScale;
        Transform rightSeat = seesawBehaviour.FindSeat(1);
        rightSeatPos = rightSeat.transform.position;
        rightSeatRot = rightSeat.transform.rotation;
        rightSeatScale = rightSeat.transform.localScale;
    }

    private NodeBaseBehaviour FindFirstNodeBaseInParent(Transform t)
    {
        if (t == null)
        {
            return null;
        }
        NodeBaseBehaviour nodeBehav = t.GetComponent<NodeBaseBehaviour>();
        if (nodeBehav == null)
        {
            return FindFirstNodeBaseInParent(t.parent);
        }

        return nodeBehav;
    }

    private void CustomSitPoint()
    {
        OnEditSitPosClick("Players will sit at the sitting point.","adjustSitPoint",entity,
            (pos,entity) => { SaveSeatPos(pos); },
            (pos,entity) =>
            {
            });
    }

    private void OnCustomSeatPositionChange(bool status)
    {
        if (customSeatPosChangeIgnore)
        {
            return;
        }
        customSeatPosBtn.gameObject.SetActive(status);
        if (status)
        {
            CustomSeatPosition();
        }
        else
        {
            ResetSeatPosition();
        }
    }

    private void ResetSeatPosition()
    {
        bool left = ShouldOperateLeft();
        if (left)
        {
            seesawBehaviour.ResetLeftSeat();
            seesawComponent.setLeftSeatPos = 0;
            if (IsSymmetry)
            {
                seesawComponent.setRightSeatPos = 0;
            }
        }
        else
        {
            seesawBehaviour.ResetRightSeat();
            seesawComponent.setRightSeatPos = 0;
        }
    }

    private void OnCustomSitPointChange(bool status)
    {
        if (customSitPointChangeIgnore)
        {
            return;
        }
        customSitPointBtn.gameObject.SetActive(status);
        if (status)
        {
            CustomSitPoint();
        }
        else
        {
            ResetSitPoint();
        }
    }

    private void ResetSitPoint()
    {
        if (IsChooseLeftSeat)
        {
            seesawComponent.setLeftSitPoint = 0;
        }
        else
        {
            seesawComponent.setRightSitPoint = 0;
        }
    }

    private void SaveSeatPos(Vector3 pos)
    {
        if (IsSymmetry)
        {
            seesawComponent.setLeftSitPoint = 1;
            seesawComponent.leftSitPoint = pos;
            //右边是对称位置
            seesawComponent.setRightSitPoint = 1;
            Vector3 symmetryPos = new Vector3(-pos.x,pos.y,pos.z);
            seesawComponent.rightSitPoint = symmetryPos;
        }
        else
        {
            if (IsChooseLeftSeat)
            {
                seesawComponent.setLeftSitPoint = 1;
                seesawComponent.leftSitPoint = pos;
            }
            else
            {
                seesawComponent.setRightSitPoint = 1;
                seesawComponent.rightSitPoint = pos;
            }
        }
    }

    private void SeatChooseChange(bool choose)
    {
        if (!choose)
        {
            return;
        }

        Toggle activeToggle = tgLeftRightChoose.GetFirstActiveToggle();
        if (activeToggle == null)
        {
            return;
        }

        if (activeToggle == leftSeat)
        {
            SyncLeftRightViewShow(true);
            seesawComponent.panelChooseSeatIndex = 0;
        }else if (activeToggle == rightSeat)
        {
            SyncLeftRightViewShow(false);
            seesawComponent.panelChooseSeatIndex = 1;
        }
    }

    private void SyncLeftRightViewShow(bool left)
    {
        if (left)
        {
            ChooseItem(seesawBehaviour.GetLeftSeatUgcId());
        }
        else
        {
            ChooseItem(seesawBehaviour.GetRightSeatUgcId());
        }

        bool customSeatPosSet = left ? seesawComponent.setLeftSeatPos == 1 : seesawComponent.setRightSeatPos == 1;
        customSeatPosBtn.gameObject.SetActive(customSeatPosSet);
        SetCustomSeatToggleValue(customSeatPosSet);
        
        bool customSitPointSet = false;
        if (left)
        {
            customSitPointSet = seesawComponent.setLeftSitPoint == 1;
        }
        else
        {
            customSitPointSet = seesawComponent.setRightSitPoint == 1;
        }
        customSitPointBtn.gameObject.SetActive(customSitPointSet);
        SetCustomSitToggleValue(customSitPointSet);
    }

    private void SetCustomSeatToggleValue(bool customSet)
    {
        customSeatPosChangeIgnore = true;
        customSeatPosToggle.isOn = customSet;
        customSeatPosChangeIgnore = false;
    }

    private void SetCustomSitToggleValue(bool customSet)
    {
        customSitPointChangeIgnore = true;
        customSitPointToggle.isOn = customSet;
        customSitPointChangeIgnore = false;
    }

    private void SetSymmetryToggleValue(bool value)
    {
        symmetryChangeIgnore = true;
        symmetryToggle.isOn = value;
        symmetryChangeIgnore = false;
    }

    private void OnEditSitPosClick(string title, string nodeName, SceneEntity entity, Action<Vector3, SceneEntity> SureBtnClickAct, Action<Vector3, SceneEntity> CancelBtnClickAct)
    {
        CommonEditAnchorsPanel.Show();
        CommonEditAnchorsPanel.Instance.SetTitle(title);
        CommonEditAnchorsPanel.Instance.Init(entity, GetSitPos());
        CommonEditAnchorsPanel.Instance.SetNodeName(nodeName);
        CommonEditAnchorsPanel.Instance.SureBtnClickAct = (pointPos) => { SureBtnClickAct(pointPos, entity); };
        CommonEditAnchorsPanel.Instance.CancelBtnClickAct = (pointPos) => { CancelBtnClickAct(pointPos, entity); };
    }

    private Vector3 GetSitPos()
    {
        bool left = ShouldOperateLeft();
        if (left)
        {
            return seesawBehaviour.GetLeftSitPos();
        }
        return seesawBehaviour.GetRightSitPos();
    }

    private bool ShouldOperateLeft()
    {
        if (IsSymmetry)
        {
            return true;
        }

        if (IsChooseLeftSeat)
        {
            return true;
        }

        return false;
    }

    private void SymmetryChange(bool status)
    {
        if (symmetryChangeIgnore)
        {
            return;
        }
        //这个要放前面，先隐藏了的话就触发不了回调了
        if (status)
        {
            leftSeat.isOn = true;
            seesawComponent.panelChooseSeatIndex = 0;
        }

        leftRightChooseParent.gameObject.SetActive(!status);
        if (status)
        {
            SyncRightSeatToLeft();
            SyncRightSitPointToLeft();
            seesawComponent.setRightSeatPos = seesawComponent.setLeftSeatPos;
        }
        SymmetrySeat symmetrySeat = curBehav.GetComponentInChildren<SymmetrySeat>();
        if (symmetrySeat == null)
        {
            seesawBehaviour.InitSymmetry();
            symmetrySeat = curBehav.GetComponentInChildren<SymmetrySeat>();
        }
        if (status)
        {
            symmetrySeat.SetActive(true);
            seesawComponent.symmetry = 1;
        }
        else
        {
            symmetrySeat.SetActive(false);
            seesawComponent.symmetry = 0;
        }
    }

    private void SyncRightSitPointToLeft()
    {
        seesawComponent.setRightSitPoint = seesawComponent.setLeftSitPoint;
        Vec3 leftPos = seesawComponent.leftSitPoint;
        if (leftPos == null)
        {
            seesawComponent.rightSitPoint = null;
        }
        else
        {
            Vector3 symmetryPos = new Vector3(-leftPos.x,leftPos.y,leftPos.z);
            seesawComponent.rightSitPoint = symmetryPos;
        }
    }

    private void SyncRightSeatToLeft()
    {
        string leftSeatUgcId = seesawBehaviour.GetLeftSeatUgcId();
        if (leftSeatUgcId == seesawBehaviour.GetRightSeatUgcId())
        {
            return;
        }

        if (leftSeatUgcId == SeesawManager.SEAT_DEFAULT)
        {
            seesawBehaviour.ChangeSeatOrigin(false,IsSymmetry);
        }
        else
        {
            foreach (var item in UgcChooseItems.Values)
            {
                if (item.mapInfo == null)
                {
                    continue;
                }
                if (item.mapInfo.mapId == leftSeatUgcId)
                {
                    symmetrySign = true;
                    OnUgcChooseItemClick(item);
                    break;
                }
            }
        }
    }

    private void CreatePanel()
    {
        tabGroup = transform.GetComponentInChildren<TabSelectGroup>();
        tabGroup.isExpand = true;//默认展开
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        matNormalScroll = tab.normalPanel.GetComponentInChildren<ScrollRect>();
        matExpandScroll = tab.expandPanel.GetComponentInChildren<ScrollRect>();
        matTypeGroup = tab.expandPanel.GetComponentInChildren<ToggleGroup>();
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        colorNormalScroll = tab.normalPanel.GetComponentInChildren<ScrollRect>();
        ScrollRect rect = tab.expandPanel.GetComponentInChildren<ScrollRect>();
        colorExpandScroll = rect.GetComponentInChildren<ScrollRect>();
        Button[] customizeBtns = tab.expandPanel.GetComponentsInChildren<Button>();
        delectBtn = customizeBtns[0];
        confirmBtn = customizeBtns[1];
        GridLayoutGroup[] grids = tab.expandPanel.GetComponentsInChildren<GridLayoutGroup>();
        GridLayoutGroup colorCustomizeGrid = grids[1];
        for (int i = 0; i < colorCustomizeGrid.transform.childCount; i++)
        {
            customizeItems.Add(colorCustomizeGrid.transform.GetChild(i).gameObject);
        }
    }


    public override void SetColor(int index, ColorSelectType type)
    {
        SeesawSetColorUndoData beginData = CreateSetColorUndoData();
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        SeesawSetColorUndoData endData = CreateSetColorUndoData();
        AddToRecord(beginData, endData);
    }
    
    public override void SetColor(Color color)
    {
        seesawComponent.color = DataUtils.ColorToString(color);
        seesawBehaviour.SetColor(color);
    }

    public override void SetMaterial(int index)
    {
        SeesawSetMatUndoData beginData = CreateSetMatUndoData();
        int id = matDatas[index].id;
        seesawComponent.mat = id;
        seesawBehaviour.SetMat(id);
        SeesawSetMatUndoData endData = CreateSetMatUndoData();
        AddToRecord(beginData, endData);
    }
    
    public void SetMatUndo(SeesawSetMatUndoData matUndoData)
    {
        for (int i = 0; i < matDatas.Count; i++)
        {
            if (matDatas[i].id == matUndoData.mat)
            {
                SetMatSelect(i);
                break;
            }
        }
        var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == matUndoData.mat);
        seesawComponent.mat = matUndoData.mat;
        seesawBehaviour.SetMat(matUndoData.mat);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(matUndoData.tabId), matUndoData.isExpand);
        RefreshScrollPanel(ShowType.Material, matUndoData.isExpand, matData);
    }
    
    public void SetTileUndo(SeesawChangeTilingUndoData tileData)
    {
        var tiling = tileData.tiling;
        seesawComponent.tiling = tiling;
        seesawBehaviour.SetTiling(tiling);
    }
    
    public void SetColorUndo(SeesawSetColorUndoData colorUndoData)
    {
        Color color = DataUtils.DeSerializeColor(colorUndoData.color);
        SetSelectColor(color,(ColorSelectType)colorUndoData.colorType);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(colorUndoData.tabId), colorUndoData.isExpand);
        RefreshScrollPanel(ShowType.Color, colorUndoData.isExpand);
    }

    private void AddToRecord(object beginData, object endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.SeesawUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }

    private SeesawSetMatUndoData CreateSetMatUndoData()
    {
        SeesawSetMatUndoData data = new SeesawSetMatUndoData();
        data.mat = seesawComponent.mat;
        data.tabId = (int) PanelShowType.Material;
        data.isExpand = tabGroup.isExpand;
        return data;
    }

    private SeesawChangeTilingUndoData CreateChangeTilingUndoData()
    {
        SeesawChangeTilingUndoData data = new SeesawChangeTilingUndoData();
        data.tiling = seesawComponent.tiling;
        return data;
    }

    private SeesawSetColorUndoData CreateSetColorUndoData()
    {
        SeesawSetColorUndoData data = new SeesawSetColorUndoData();
        data.color = seesawComponent.color;
        data.colorType = (int)colorType;
        data.tabId = (int) PanelShowType.Color;
        data.isExpand = tabGroup.isExpand;
        return data;
    }

    public void AddTile()
    {
        SeesawChangeTilingUndoData beginData = CreateChangeTilingUndoData();
        Vector2 tiling = seesawComponent.tiling;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        seesawComponent.tiling = tiling;
        seesawBehaviour.SetTiling(tiling);
        SeesawChangeTilingUndoData endData = CreateChangeTilingUndoData();
        AddToRecord(beginData,endData);
    }

    public void SubTile()
    {
        SeesawChangeTilingUndoData beginData = CreateChangeTilingUndoData();
        Vector2 tiling = seesawComponent.tiling;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        seesawComponent.tiling = tiling;
        seesawBehaviour.SetTiling(tiling);
        SeesawChangeTilingUndoData endData = CreateChangeTilingUndoData();
        AddToRecord(beginData,endData);
    }
    
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand);
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }
    
    #region  UGC素材选择处理

    /// <summary>
    /// 点击Item，切换UGC素材，用新素材来展现道具
    /// </summary>
    public override void OnUgcChooseItemClick(UgcChooseItem fireworkItem)
    {
        base.OnUgcChooseItemClick(fireworkItem);
    }

    protected override List<string> GetAllUgcRidList()
    {
        return SeesawManager.Inst.GetAllUGC();
    }

    protected override void AfterUgcCreateFinish(NodeBaseBehaviour nBehav, string rId)
    {
        if (symmetrySign)
        {
            seesawBehaviour.ChangeSeat(nBehav,1,rId,IsSymmetry);
            return;
        }
        ChooseItem(rId);
        SeesawManager.Inst.SaveRid(rId);
        if (IsSymmetry)
        {
            seesawBehaviour.ChangeSeat(nBehav,0,rId,IsSymmetry);
        }
        else
        {
            seesawBehaviour.ChangeSeat(nBehav,IsChooseLeftSeat ? 0 : 1,rId,IsSymmetry);
        }
    }

    //设置最后选择的ugc
    public override void SetLastChooseUgcItem(UgcChooseItem fireworkItem)
    {
        if (symmetrySign)
        {
            symmetrySign = false;
            return;
        }
        if (IsSymmetry)
        {
            symmetrySign = true;
            OnUgcChooseItemClick(lastChooseItem);
        }
    }
    
    protected override bool DestroySelf()
    {
        return false;
    }

    protected override bool SelectUgc()
    {
        return false;
    }

    #endregion
    
    
    /// <summary>
    /// 创建非UGC的特殊Item
    /// </summary>
    /// <param name="IconPath">图标路径</param>
    /// <param name="ClickAction">点击后的行为</param>
    protected void CreateItemUISpecial(string IconPath, Action ClickAction)
    {
        var itemPrefab = ResManager.Inst.LoadResNoCache<GameObject>("Prefabs/UI/Panel/WeaponItem");
        var item = Instantiate(itemPrefab, content, true);
        item.transform.SetAsFirstSibling();
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        var ugcCover = item.transform.Find("ResCover").GetComponent<RawImage>();
        ugcCover.texture = ResManager.Inst.LoadRes<Texture>(IconPath);
        var selectBg = item.transform.Find("SelectBg").gameObject;
        var btnCover = ugcCover.GetComponent<Button>();
        ugcCover.gameObject.SetActive(true);

        selectBg.gameObject.SetActive(false);
        btnCover.onClick.AddListener(() =>
        {
            ClickAction?.Invoke();
        });
        
        var newItem = new UgcChooseItem()
        {
            itemObj = item,
            resCover = ugcCover,
            selectBg = selectBg
        };
        UgcChooseItems.Add(SeesawManager.SEAT_DEFAULT, newItem);
    }
}