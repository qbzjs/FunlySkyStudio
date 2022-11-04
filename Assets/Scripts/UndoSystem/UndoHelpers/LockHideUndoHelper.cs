/// <summary>
/// Author:zhouzihan
/// Description:隐藏锁定撤销管理
/// Date: #CreateTime#
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LockHideUndoData
{
    public bool isLock;
    public List<SceneEntity> hideList;
    public bool activeSelf;
    public Transform targetNode;
    public int LockHideType;
}
public class LockHideUndoHelper : BaseUndoHelper
{
    public override void Undo(UndoRecord record)
    {
        base.Undo(record);

        LockHideUndoData helpData = record.BeginData as LockHideUndoData;
        ExcuteData(helpData);
    }

    public override void Redo(UndoRecord record)
    {
        base.Redo(record);
        LockHideUndoData helpData = record.EndData as LockHideUndoData;
        ExcuteData(helpData);
    }

    private void ExcuteData(LockHideUndoData helpData)
    {
        switch (helpData.LockHideType)
        {
            case (int)LockHideType.Hide:
                Transform targetNode = helpData.targetNode;
                if (targetNode != null && targetNode.gameObject != null)
                {
                    LockHideManager.Inst.HideCurPropUndo(helpData.activeSelf, helpData.targetNode.gameObject);
                }
                break;
            case (int)LockHideType.Show:
                Debug.Log(helpData.LockHideType);
                LockHideManager.Inst.ShowHideUndo(helpData);
                break;
            case (int)LockHideType.Lock:
                targetNode = helpData.targetNode;
                if (targetNode != null && targetNode.gameObject != null)
                {
                    var nodeBehaviour = targetNode.GetComponent<NodeBaseBehaviour>();
                    if (nodeBehaviour != null && nodeBehaviour.entity != null)
                    {
                        EditModeController.SetSelect?.Invoke(nodeBehaviour.entity);
                        ModelHandlePanel.Instance.SetCurLockStateUndo(helpData.isLock);
                        if (!helpData.isLock)
                        {
                            TipPanel.ShowToast("undo locking");
                        }
                        else
                        {
                            TipPanel.ShowToast("redo locking");
                        }
                        
                    }
                }
                    break;
        }

        
    }
}
