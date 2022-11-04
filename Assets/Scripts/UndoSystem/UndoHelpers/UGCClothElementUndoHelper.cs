using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ElementUndoData
{
    public Vector2 postion;
    public Vector3 eulerAngles;
    public Vector2 sizeDelta;
    public RectTransform targetNode;
    public string color;
    public int transformType;
    public int colorType;
    public GameObject selectPart;
    public Texture tex;
    public string url;
}
public class UGCClothElementUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        ElementUndoData helpData = record.BeginData as ElementUndoData;
        ExcuteData(helpData);
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        ElementUndoData helpData = record.EndData as ElementUndoData;
        ExcuteData(helpData);
    }

    private void ExcuteData(ElementUndoData helpData)
    {
        if (helpData != null)
        {
           var interActor = TransformInteractorController.Inst.interActor;
            if (helpData.selectPart != MainUGCResPanel.Inst.curSelectPart)
            {
                MainUGCResPanel.Inst.ChangeParts(helpData.selectPart);
            }
            if (helpData.transformType == (int)UGCElementType.Trans)
            {
                interActor.SetTransUndo(helpData.targetNode, helpData.postion, helpData.eulerAngles, helpData.sizeDelta);
                UGCClothesPhotoManager.Inst.SetTextureUndo(helpData.targetNode, helpData.tex, helpData.url);
            }
            else if (helpData.transformType == (int)UGCElementType.Color)
            {
                UGCClothesTextManager.Inst.SetColorUndo(helpData.targetNode, DataUtils.DeSerializeColor(helpData.color));
            }
            interActor.SetUndoRedoAct?.Invoke();
        }
    }
}
