/// <summary>
/// Author:zhouzihan
/// Description:颜色材质撤销管理
/// Date: #CreateTime#
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TerrainMaterialUndoData
{
    public int matId;
    public string ugcMatUrl;
    public string ugcMatMapId;
    public int colorId;
    public int colorType;
    public string colorStr;
    public int baseMaterialType;
    public int tabId;
    public bool isExpand;
}
public class TerrainMaterialUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        TerrainMaterialUndoData helpData = record.BeginData as TerrainMaterialUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        TerrainMaterialUndoData helpData = record.EndData as TerrainMaterialUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(TerrainMaterialUndoData helpData)
    {
        LoggerUtils.Log("TerrainMaterialUndoData colorId:" + helpData.colorId);
        LoggerUtils.Log("TerrainMaterialUndoData matId:" + helpData.matId);
        EditModeController.SelectGround(0);
        if (helpData.baseMaterialType == (int)MaterialSelectType.Color)
        {
            TerrainMaterialPanel.Instance.SetColorUndo(helpData.colorId,helpData.colorType,helpData.colorStr,helpData.tabId,helpData.isExpand);
        }
        else if (helpData.baseMaterialType == (int)MaterialSelectType.Material)
        {
            TerrainMaterialPanel.Instance.SetMaterialUndo(helpData.matId, helpData.ugcMatUrl, helpData.ugcMatMapId, helpData.tabId,helpData.isExpand);
        }
       
    }
}
