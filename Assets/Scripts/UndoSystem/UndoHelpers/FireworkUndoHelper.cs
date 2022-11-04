/// /// <summary>
/// Author:wenjia
/// Description:烟花颜色撤销管理
/// Date: 2022-9-2 15:36:23
/// </summary>
using UnityEngine;
public class FireworkUndoData
{
    public int colorType;
    public int colorIndex;
    public Color color;
    public SceneEntity targetEntity;
    public int TextType;
    public int tabId;
    public bool isExpand;
}

public class FireworkUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        FireworkUndoData helpData = record.BeginData as FireworkUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        FireworkUndoData helpData = record.EndData as FireworkUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(FireworkUndoData helpData)
    {

        if (helpData.targetEntity != null && helpData.targetEntity.Get<GameObjectComponent>().bindGo.gameObject != null)
        {
            EditModeController.SetSelect?.Invoke(helpData.targetEntity);
            if (helpData.TextType == (int)TextSelectType.Color)
            {
                FireworkPanel.Instance.SelectColorUndo(helpData.colorType, helpData.color, helpData.tabId, helpData.isExpand);
            }
        }
    }
}
