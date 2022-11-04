using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: Shaocheng
/// Description: 冰方块tiling设置ui
/// Date: 2022-7-23 22:49:44
/// </summary>
public class IceCubePanel : InfoPanel<IceCubePanel>, IUndoRecord
{
    public Button AddTileBtn;
    public Button SubTileBtn;
    private IceCubeBehaviour nBehaviour;
    private SceneEntity curEntity;
    private float tileIncrement = 0.5f;

    public override void OnInitByCreate ()
    {
        base.OnInitByCreate();
        AddListener();
    }

    private void AddListener ()
    {
        AddTileBtn.onClick.AddListener(OnAddTileClick);
        SubTileBtn.onClick.AddListener(OnSubTileClick);
    }

    private void OnAddTileClick ()
    {
        // var beginData = CreateUndoData(MaterialSelectType.Tile);

        var tiling = curEntity.Get<IceCubeComponent>().tile;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        curEntity.Get<IceCubeComponent>().tile = tiling;
        nBehaviour.SetTiling(tiling);

        // var endData = CreateUndoData(MaterialSelectType.Tile);
        // AddRecord(beginData, endData);
    }
    
    private void OnSubTileClick ()
    {
        // var beginData = CreateUndoData(MaterialSelectType.Tile);

        Vector2 tiling = curEntity.Get<IceCubeComponent>().tile;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        curEntity.Get<IceCubeComponent>().tile = tiling;
        nBehaviour.SetTiling(tiling);

        // var endData = CreateUndoData(MaterialSelectType.Tile);

        // AddRecord(beginData, endData);
    }
    
    #region 撤销功能
    public void AddRecord (UndoRecord record)
    {
        // UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord (WaterCubeUndoData beginData, WaterCubeUndoData endData)
    {
        // UndoRecord record = new UndoRecord(UndoHelperName.WaterCubeUndoHelper);
        // record.BeginData = beginData;
        // record.EndData = endData;
        // AddRecord(record);
    }
    private WaterCubeUndoData CreateUndoData (MaterialSelectType undoType)
    {
        // IceCubeComponent compt = curEntity.Get<IceCubeComponent>();
        // WaterCubeUndoData data = new WaterCubeUndoData();
        // data.cfgId = compt.id;
        // data.tiling = compt.tiling;
        // data.velocity = compt.v;
        // data.targetEntity = curEntity;
        // data.undoType = undoType;
        // return data;

        return null;
    }
    
    public void SetEntity (SceneEntity entity)
    {
        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        nBehaviour = entityGo.GetComponent<IceCubeBehaviour>();
    }

    #endregion
}
