/// <summary>
/// Author:zhouzihan
/// Description:天空盒撤销管理
/// Date: #CreateTime#
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxUndoData
{
    public int skyboxId;
    public SkyboxType skyboxType;
}
public class SkyboxUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        SkyboxUndoData helpData = record.BeginData as SkyboxUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        SkyboxUndoData helpData = record.EndData as SkyboxUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(SkyboxUndoData helpData)
    {
        LoggerUtils.Log("SkyboxUndoData skyboxId:" + helpData.skyboxId);
        EditModeController.SelectSkybox(0);
        SkyboxStylePanel.Instance.SelectSkyUndo(helpData);
    }
}
