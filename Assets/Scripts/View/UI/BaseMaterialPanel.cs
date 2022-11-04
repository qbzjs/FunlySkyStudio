using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;


public class BaseMaterialPanel : InfoPanel<BaseMaterialPanel>, IUndoRecord
{
    public Transform MatParent;
    public Transform ColorParent;
    public Button AddTileBtn;
    public Button SubTileBtn;
    private GameObject MatPrefab;
    private GameObject ColorPrefab;
    private List<GameObject> allCovers = new List<GameObject>();
    private List<GameObject> allColors = new List<GameObject>();
    private NodeBehaviour nBehaviour;
    private SceneEntity curEntity;
    private Color modelColor;
    private int matId;
    private int colorId;
    private float tileIncrement = 0.1f;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AddTileBtn.onClick.AddListener(OnAddTileClick);
        SubTileBtn.onClick.AddListener(OnSubTileClick);
        var priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        MatPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "BaseMaterialItem");
        ColorPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "BaseMaterialColorItem");
        for (int i = 0; i < GameManager.Inst.matConfigDatas.Count; i++)
        {
            int index = i;
            var matData = GameManager.Inst.matConfigDatas[i];
            var matGo = Instantiate(MatPrefab, MatParent);
            var matImage = matGo.transform.GetChild(0).GetComponent<Image>();
            var matBtn = matGo.GetComponentInChildren<Button>();
            var coverGo = matGo.transform.GetChild(1).gameObject;
            matImage.sprite = priAtlas.GetSprite(matData.iconName);
            matBtn.onClick.AddListener(() => SetMaterial(index));
            allCovers.Add(coverGo);
        }
        
        for (int i = 0; i < AssetLibrary.Inst.colorLib.Size(); i++)
        {
            int index = i;
            var colorGo = Instantiate(ColorPrefab, ColorParent);
            Image img = colorGo.GetComponentInChildren<Image>();
            var colorBtn = colorGo.GetComponentInChildren<Button>();
            img.color = AssetLibrary.Inst.colorLib.Get(i);
            colorBtn.onClick.AddListener(() => SetColor(index));
            var colorm = colorGo.transform.Find("sel").gameObject;
            allColors.Add(colorm);
        }
    }


    private void OnAddTileClick()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);
        var tiling = curEntity.Get<MaterialComponent>().tile;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        curEntity.Get<MaterialComponent>().tile = tiling;

        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
        nBehaviour.SetTiling(tiling);
    }
    public void TileClickUndo(Vector2 tile)
    {
        curEntity.Get<MaterialComponent>().tile = tile;
        nBehaviour.SetTiling(tile);
    }
    private void OnSubTileClick()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);
        var tiling = curEntity.Get<MaterialComponent>().tile;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        curEntity.Get<MaterialComponent>().tile = tiling;
        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
        nBehaviour.SetTiling(tiling);

    }


    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        nBehaviour = entityGo.GetComponent<NodeBehaviour>();
        var matComp = entity.Get<MaterialComponent>();
        modelColor = matComp.color;
        matId = matComp.matId;
        colorId = matComp.colorId;
        var selectIndex = GameManager.Inst.matConfigDatas.FindIndex(x => x.id == matId);
        SetSelect(selectIndex);
        int selIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), AssetLibrary.Inst.colorLib.List);
        SetColorSelect(selIndex);
    }

    private void SetMaterial(int index)
    {
        var beginData = CreateUndoData(MaterialSelectType.Material);
        var matData = GameManager.Inst.matConfigDatas[index];
        SetSelect(index);
        matId = matData.id;
        curEntity.Get<MaterialComponent>().matId = matData.id;
        var endData = CreateUndoData(MaterialSelectType.Material);
        AddRecord(beginData, endData);
        SceneObjectController.SetBaseModelAtr(nBehaviour, matId, modelColor);
    }
    public void SetMaterialUndo(int matid)
    {
        for (int i = 0; i < GameManager.Inst.matConfigDatas.Count; i++)
        {
            if (GameManager.Inst.matConfigDatas[i].id==matid)
            {
                SetSelect(i);
                break;
            }
        }
        matId = matid;
        curEntity.Get<MaterialComponent>().matId = matid;
        SceneObjectController.SetBaseModelAtr(nBehaviour, matId, modelColor);
    }
    private void SetColor(int index)
    {
        var beginData = CreateUndoData(MaterialSelectType.Color);
        modelColor = AssetLibrary.Inst.colorLib.Get(index);
        curEntity.Get<MaterialComponent>().color = modelColor;
        curEntity.Get<MaterialComponent>().colorId = index;
        var endData = CreateUndoData(MaterialSelectType.Color);
        AddRecord(beginData,endData);
        SceneObjectController.SetBaseModelColor(nBehaviour, modelColor);
        SetColorSelect(index);
    }
    public void SetColorUndo(int index)
    {
        modelColor = AssetLibrary.Inst.colorLib.Get(index);
        curEntity.Get<MaterialComponent>().color = modelColor;
        curEntity.Get<MaterialComponent>().colorId = index;
        SceneObjectController.SetBaseModelColor(nBehaviour, modelColor);
        SetColorSelect(index);
    }
    private void SetSelect(int index)
    {
        if (index < 0 || index >= allCovers.Count)
        {
            LoggerUtils.LogError("Mat ID is Error");
            return;
        }
        allCovers.ForEach(x => x.SetActive(false));
        allCovers[index].SetActive(true);
    }

    private void SetColorSelect(int index)
    {
        if (index < 0 || index >= allColors.Count)
        {
            LoggerUtils.LogError("Mat ID is Error");
            return;
        }
        allColors.ForEach(x => x.SetActive(false));
        allColors[index].SetActive(true);
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(BaseMaterialUndoData beginData, BaseMaterialUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.BaseMaterialUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private BaseMaterialUndoData CreateUndoData(MaterialSelectType type )
    {
        MaterialComponent component = curEntity.Get<MaterialComponent>();
        BaseMaterialUndoData data = new BaseMaterialUndoData();
        data.matId = component.matId;
        data.colorId = component.colorId;
        data.tile = component.tile;
        data.targetEntity = curEntity;
        data.baseMaterialType = (int)type;
        return data;
    }
}
