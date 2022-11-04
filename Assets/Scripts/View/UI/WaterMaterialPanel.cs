using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using SavingData;

public class WaterMaterialPanel : InfoPanel<WaterMaterialPanel>, IUndoRecord
{
    public Transform MatParent;
    private GameObject MatPrefab;
    public Button AddTileBtn;
    public Button SubTileBtn;
    public Button AddVelocityBtn;
    public Button SubVelocityBtn;
    private List<GameObject> allCovers = new List<GameObject>();
    private int cfgId;
    private WaterCubeBehaviour nBehaviour;
    private SceneEntity curEntity;
    private float tileIncrement = 0.1f;
    private float velocityIncrement = 0.1f;

    public override void OnInitByCreate ()
    {
        base.OnInitByCreate();
        AddListener();
        GenMaterialItemList();
    }

    private void GenMaterialItemList ()
    {
        var priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        MatPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "WaterCubeMaterialItem");
        for (int i = 0; i < GameManager.Inst.waterCubeDatas.Count; i++)
        {
            int index = i;
            var matData = GameManager.Inst.waterCubeDatas[i];
            var matGo = Instantiate(MatPrefab, MatParent);
            var matImage = matGo.transform.Find("Image").GetComponent<Image>();
            var matBtn = matGo.GetComponentInChildren<Button>();
            var coverGo = matGo.transform.Find("Select").gameObject;
            matImage.sprite = priAtlas.GetSprite(matData.iconName);
            matBtn.onClick.AddListener(() => SetMaterial(index));
            allCovers.Add(coverGo);
        }
    }

    private void AddListener ()
    {
        AddTileBtn.onClick.AddListener(OnAddTileClick);
        SubTileBtn.onClick.AddListener(OnSubTileClick);
        AddVelocityBtn.onClick.AddListener(OnAddVelocityClick);
        SubVelocityBtn.onClick.AddListener(OnSubVelocityClick);
    }

    private void OnAddTileClick ()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);

        Vector2 tiling = curEntity.Get<WaterComponent>().tiling;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        curEntity.Get<WaterComponent>().tiling = tiling;
        nBehaviour.SetTiling(tiling);

        var endData = CreateUndoData(MaterialSelectType.Tile);

        AddRecord(beginData, endData);
    }
    private void OnSubTileClick ()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);

        var tiling = curEntity.Get<WaterComponent>().tiling;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        curEntity.Get<WaterComponent>().tiling = tiling;
        nBehaviour.SetTiling(tiling);

        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
    }
    private void OnAddVelocityClick ()
    {
        var beginData = CreateUndoData(MaterialSelectType.Velocity);
        //
        float velocity = curEntity.Get<WaterComponent>().v;
        velocity += velocityIncrement;
        curEntity.Get<WaterComponent>().v = velocity;
        nBehaviour.SetVelocity(velocity);
        //
        var endData = CreateUndoData(MaterialSelectType.Velocity);
        AddRecord(beginData, endData);
    }
    private void OnSubVelocityClick ()
    {
        var beginData = CreateUndoData(MaterialSelectType.Velocity);
        //
        float velocity = curEntity.Get<WaterComponent>().v;
        velocity -= velocityIncrement;
        velocity = Mathf.Clamp(velocity, 0, velocity);
        curEntity.Get<WaterComponent>().v = velocity;
        nBehaviour.SetVelocity(velocity);
        //
        var endData = CreateUndoData(MaterialSelectType.Velocity);
        AddRecord(beginData, endData);
    }
    public void SetEntity (SceneEntity entity)
    {
        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        nBehaviour = entityGo.GetComponent<WaterCubeBehaviour>();
        var matComp = entity.Get<WaterComponent>();
        cfgId = matComp.id;
        var selectIndex = GameManager.Inst.waterCubeDatas.FindIndex(x => x.id == cfgId);
        SetSelect(selectIndex);
    }
    private void SetMaterial (int index)
    {
        var beginData = CreateUndoData(MaterialSelectType.Material);

        var matData = GameManager.Inst.waterCubeDatas[index];
        SetSelect(index);
        cfgId = matData.id;
        curEntity.Get<WaterComponent>().id = matData.id;
        //
        SetMaterialWithCfg(matData);
        //
        var endData = CreateUndoData(MaterialSelectType.Material);
        AddRecord(beginData, endData);
    }
    private void SetMaterialWithCfg (WaterCubeData cfg)
    {
        Color color = DataUtils.DeSerializeColorByHex(cfg.surfaceDiffuse);
        color.a = ColorInt2Float(cfg.surfaceDiffuseAlpha);
        nBehaviour.SetSurfaceDiffuse(color);

        color = DataUtils.DeSerializeColorByHex(cfg.surfaceEmission);
        color.a = ColorInt2Float(cfg.surfaceEmissionAlpha);
        nBehaviour.SetSurfaceEmission(color);

        color = DataUtils.DeSerializeColorByHex(cfg.edgeAlbedo);
        color.a = ColorInt2Float(cfg.edgeAlbedoAlpha);
        nBehaviour.SetEdgeAlbedo(color);
    }
    private float ColorInt2Float (int i)
    {
        return ((float)i) / 255;
    }
    private void SetSelect (int index)
    {
        if (index < 0 || index >= allCovers.Count)
        {
            LoggerUtils.LogError("Mat ID is Error");
            return;
        }
        allCovers.ForEach(x => x.SetActive(false));
        allCovers[index].SetActive(true);
    }
    #region 撤销功能
    public void AddRecord (UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord (WaterCubeUndoData beginData, WaterCubeUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.WaterCubeUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }
    private WaterCubeUndoData CreateUndoData (MaterialSelectType undoType)
    {
        WaterComponent compt = curEntity.Get<WaterComponent>();
        WaterCubeUndoData data = new WaterCubeUndoData();
        data.cfgId = compt.id;
        data.tiling = compt.tiling;
        data.velocity = compt.v;
        data.targetEntity = curEntity;
        data.undoType = undoType;
        return data;
    }
    public void SetTiling (Vector2 tiling)
    {
        curEntity.Get<WaterComponent>().tiling = tiling;
        nBehaviour.SetTiling(tiling);
    }
    public void SetVelocity (float v)
    {
        curEntity.Get<WaterComponent>().v = v;
        nBehaviour.SetVelocity(v);
    }
    public void SetMaterialWithCfgId (int cfgId)
    {
        for (int i = 0; i < GameManager.Inst.waterCubeDatas.Count; i++)
        {
            if (GameManager.Inst.waterCubeDatas[i].id == cfgId)
            {
                this.cfgId = cfgId;
                curEntity.Get<WaterComponent>().id = cfgId;
                SetSelect(i);
                WaterCubeData matData = GameManager.Inst.waterCubeDatas[i];
                SetMaterialWithCfg(matData);
                break;
            }
        }
    }
    #endregion
}
