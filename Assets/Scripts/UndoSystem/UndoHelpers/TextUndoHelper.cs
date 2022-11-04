/// <summary>
/// Author:zhouzihan
/// Description:文字撤销管理
/// Date: #CreateTime#
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TextUndoData
{
    public int colorId;
    public SceneEntity targetEntity;
    public string text;
    public int TextType;
}

public class TextUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        TextUndoData helpData = record.BeginData as TextUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        TextUndoData helpData = record.EndData as TextUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(TextUndoData helpData)
    {
        LoggerUtils.Log("TextUndoData colorId:" + helpData.colorId);


        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.TextType == (int)TextSelectType.Color)
            {
                DTextPanel.Instance.SelectColotUndo(helpData.colorId);
            }
            else if (helpData.TextType == (int)TextSelectType.Text)
            {
                DTextPanel.Instance.TextUndo(helpData.text);
            }
            //else if (helpData.baseMaterialType == (int)MaterialSelectType.Tile)
            //{
            //    BaseMaterialPanel.Instance.TileClickUndo(helpData.tile);
            //}
        }
    }
}
