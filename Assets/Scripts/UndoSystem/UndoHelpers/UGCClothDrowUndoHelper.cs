/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/6/13 20:51:4
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGCClothDrawUndoData
{
    public Dictionary<Vector2Int,Color> drawGridPairs;
    public GameObject selectPart;
   
}
public class UGCClothDrawUndoHelper : BaseUndoHelper
{
    private MainUGCResPanel mainClothesPanel;
    private MainUGCResPanel MainClothesPanel
    {
        get
        {
            if (mainClothesPanel == null)
            {
                mainClothesPanel = GameObject.Find("GameStart").GetComponent<UGCClothesGameStart>().mainPanel;
            }
            return mainClothesPanel;
        }
    }
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        UGCClothDrawUndoData helpData = record.BeginData as UGCClothDrawUndoData;
        ExcuteData(helpData);
    }
    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        UGCClothDrawUndoData helpData = record.EndData as UGCClothDrawUndoData;
        ExcuteData(helpData);
    }
    private void ExcuteData(UGCClothDrawUndoData helpData)
    {
        LoggerUtils.Log("UGCClothDrowUndoData drowGrid:" + helpData.drawGridPairs.Count);
        LoggerUtils.Log("UGCClothDrowUndoData selectPart:" + helpData.selectPart.name);
        MainClothesPanel.OnUndo(helpData);
    }
    
}
