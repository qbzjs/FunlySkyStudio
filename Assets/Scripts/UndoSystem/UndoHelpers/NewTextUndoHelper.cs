/// /// <summary>
/// Author:LiShuZhan
/// Description:新版3d文字撤销管理
/// Date: 2022-5-27 17:44:22
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NewTextUndoData
{ 
    public int colorType;
    public Color color;
    public SceneEntity targetEntity;
    public string text;
    public int TextType;
    public int tabId;
    public bool isExpand;
}

/// <summary>
/// 修改此脚本时要考虑旧版3d文字TextUndoHelper
/// </summary>
public class NewTextUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        NewTextUndoData helpData = record.BeginData as NewTextUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        NewTextUndoData helpData = record.EndData as NewTextUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(NewTextUndoData helpData)
    {
  
        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.TextType == (int)TextSelectType.Color)
            {
                NewDTextPanel.Instance.SelectColorUndo(helpData.colorType,helpData.color,helpData.tabId,helpData.isExpand);
            }
            else if (helpData.TextType == (int)TextSelectType.Text)
            {
                NewDTextPanel.Instance.TextUndo(helpData.text,helpData.tabId,helpData.isExpand);
            }
        }
    }
}
