using UnityEngine;

public class WaterCubeUndoData
{
    public int cfgId;
    public Vector2 tiling;
    public SceneEntity targetEntity;
    public float velocity;
    public MaterialSelectType undoType;
}
public class WaterCubeUndoHelper : BaseUndoHelper
{
    public override void Undo (UndoRecord record)
    {
        base.Undo(record);

        WaterCubeUndoData helpData = record.BeginData as WaterCubeUndoData;
        ExcuteData(helpData);
    }
    public override void Redo (UndoRecord record)
    {
        base.Redo(record);
        WaterCubeUndoData helpData = record.EndData as WaterCubeUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData (WaterCubeUndoData helpData)
    {
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.undoType == MaterialSelectType.Tile)
            {
                WaterMaterialPanel.Instance.SetTiling(helpData.tiling);
            }
            else if (helpData.undoType == MaterialSelectType.Velocity)
            {
                WaterMaterialPanel.Instance.SetVelocity(helpData.velocity);
            }
            else if (helpData.undoType == MaterialSelectType.Material)
            {
                WaterMaterialPanel.Instance.SetMaterialWithCfgId(helpData.cfgId);
            }
        }
    }
}
