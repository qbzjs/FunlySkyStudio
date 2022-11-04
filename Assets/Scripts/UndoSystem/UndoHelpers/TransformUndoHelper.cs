using UnityEngine;
/// <summary>
/// Author:JayWill
/// Description:Undo/Redo实现物体旋转、缩放、平移
/// </summary>
public class TransformUndoData
{
    public Vector3 postion;
    public Vector3 eulerAngles;
    public Vector3 scale;
    public Transform targetNode;
    public int transformType;
}
public class TransformUndoHelper:BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        TransformUndoData helpData = record.BeginData as TransformUndoData;
        ExcuteData(helpData);
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        TransformUndoData helpData = record.EndData as TransformUndoData;
        ExcuteData(helpData);
    }

    private void ExcuteData(TransformUndoData helpData)
    {
        LoggerUtils.Log("TransformUndoHelper postion:"+helpData.postion);
        LoggerUtils.Log("TransformUndoHelper eulerAngles:"+helpData.eulerAngles);
        LoggerUtils.Log("TransformUndoHelper scale:"+helpData.scale);
        LoggerUtils.Log("TransformUndoHelper transformType:"+helpData.transformType);
        
        Transform targetNode = helpData.targetNode;
        if(targetNode !=null && targetNode.gameObject != null){
            targetNode.localPosition = helpData.postion;
            targetNode.localEulerAngles = helpData.eulerAngles;
            targetNode.localScale = helpData.scale;

            var nodeBehaviour = targetNode.GetComponent<NodeBaseBehaviour>();
            if(nodeBehaviour != null && nodeBehaviour.entity != null){
                EditModeController.SetSelect?.Invoke(nodeBehaviour.entity);
            }
            if(helpData.transformType == (int)HandleMode.Move){
                ModelHandlePanel.Instance.OnMoveClick();
            }else if(helpData.transformType == (int)HandleMode.Scale){
                ModelHandlePanel.Instance.OnScaleClick();
            }else if(helpData.transformType == (int)HandleMode.Rotate){
                ModelHandlePanel.Instance.OnRotateClick();
            }
            NodeTransformController.Inst.OnUndoRedo(targetNode.gameObject, (HandleMode)helpData.transformType);
        }
    }
}