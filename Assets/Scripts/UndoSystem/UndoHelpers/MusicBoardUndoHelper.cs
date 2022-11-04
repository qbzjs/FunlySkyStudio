/// <summary>
/// Author:zhouzihan
/// Description:音乐班撤销管理
/// Date: #CreateTime#
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MusicBoardUndoData
{
    public int areaId;
    public int colorID;
    public SceneEntity targetEntity;
}
public class MusicBoardUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        MusicBoardUndoData helpData = record.BeginData as MusicBoardUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        MusicBoardUndoData helpData = record.EndData as MusicBoardUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(MusicBoardUndoData helpData)
    {
        LoggerUtils.Log("MusicBoardUndoData areaId:" + helpData.areaId);

        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            MusicBoardPanel.Instance.MusicBoardUndo(helpData.areaId, helpData.colorID);
            //}
        }
    }
}
