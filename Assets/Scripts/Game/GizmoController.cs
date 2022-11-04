using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

public class GizmoController :IUndoRecord
{
    public readonly float minScale = 0.01f;
    private readonly int digitKept = 4;//.0000
    private GizmoBehaviour currentGizmo;

    private ObjectTransformGizmo moveGizmo;
    private ObjectTransformGizmo rotationGizmo;
    public ObjectTransformGizmo scaleGizmo;
    private GameObject current;
    private bool lockState = false;

    private UndoRecord dragUndoRecord = null;
    private GameObject lastGameObj;

    private bool needRecord = true;
    public bool NeedRecord
    {
        get => needRecord;
        set => needRecord = value;
    }

    public void SetTarget(GameObject go)
    {
        if (current != go)
        {
            DisableGizmo();
            current = go;
            SetMoveCtr();
        }
    }

    public void MoveTarget(Vector3 moveVec)
    {
        if(current == null)
            return;

        TransformUndoData beginData = CreateUndoData(current);

        current.transform.position += moveVec;
        (currentGizmo as ObjectTransformGizmo).SetTargetObject(current);

        TransformUndoData endData = CreateUndoData(current);
        AddRecord(beginData,endData);

        NodeTransformController.Inst.OnStepUpdate(current, HandleMode.Move);
    }

    public void RotateTarget(int axis,float angle)
    {
        if (current == null)
            return;

        TransformUndoData beginData = CreateUndoData(current);
        
        Vector3 rotAxis = Mathf.Abs(axis) switch
            {
            0 => current.transform.right,
            1 => current.transform.up,
            2 => current.transform.forward,
            _ => Vector3.up
            };
        current.transform.Rotate(rotAxis, angle, Space.World);
        (currentGizmo as ObjectTransformGizmo).SetTargetObject(current);

        TransformUndoData endData = CreateUndoData(current);
        AddRecord(beginData,endData);

        NodeTransformController.Inst.OnStepUpdate(current, HandleMode.Rotate);
    }

    public void ScaleTarget(Vector3 scale)
    {
        if (current == null)
            return;
            
        TransformUndoData beginData = CreateUndoData(current);

        current.transform.localScale = scale;
        (currentGizmo as ObjectTransformGizmo).SetTargetObject(current);
        TransformUndoData endData = CreateUndoData(current);
        AddRecord(beginData,endData);

        NodeTransformController.Inst.OnStepUpdate(current, HandleMode.Scale);
    }

    public GameObject GetCurrentTarget()
    {
        return current;
    }

    public void SetMoveCtr()
    {
        if(current == null)
            return;
        currentGizmo?.Gizmo.SetEnabled(false);
        var gizmo = CreateMoveGizmo();
        gizmo.SetTargetObject(current);
        currentGizmo = gizmo;

        NodeTransformController.Inst.OnSelectTarget(current);
    }

    public void SetRotateCtr()
    {
        if (current == null)
            return;
        currentGizmo?.Gizmo.SetEnabled(false);
        var gizmo = CreateRotationGizmo();
        gizmo.SetTargetObject(current);
        gizmo.SetTransformSpace(GizmoSpace.Local);
        currentGizmo = gizmo;
    }

    public void SetScaleCtr(Action onCreate, bool norScale = false)
    {
        if (current == null)
            return;
        currentGizmo?.Gizmo.SetEnabled(false);
        var gizmo = CreateScaleGizmo(onCreate, norScale);
        gizmo.SetTargetObject(current);
        currentGizmo = gizmo;
    }


    public void ShowXZAxis(bool isEnabled = true)
    {
        var setting = RTGizmosEngine.Get.RotationGizmoLookAndFeel3D;
        setting.SetAxisVisible(0, isEnabled);
        setting.SetAxisVisible(2, isEnabled);
    }
    public void ShowScaleXZAxis(bool isEnabled = true)
    {
        var setting = RTGizmosEngine.Get.ScaleGizmoLookAndFeel3D;
        setting.SetPositiveSliderVisible(1, isEnabled);
        setting.SetPositiveCapVisible(1, isEnabled);
    }
    public void ShowScaleYAxis(bool isEnabled = true)
    {
        var setting = RTGizmosEngine.Get.ScaleGizmoLookAndFeel3D;
        setting.SetPositiveSliderVisible(0, isEnabled);
        setting.SetPositiveCapVisible(0, isEnabled);
        setting.SetPositiveSliderVisible(2, isEnabled);
        setting.SetPositiveCapVisible(2, isEnabled);
    }
    public void ShowYAxis(bool isEnabled = true)
    {
        var setting = RTGizmosEngine.Get.ScaleGizmoLookAndFeel3D;
        setting.SetPositiveSliderVisible(0, isEnabled);
        setting.SetPositiveCapVisible(0, isEnabled);
        setting.SetPositiveSliderVisible(1, !isEnabled);
        setting.SetPositiveCapVisible(1, !isEnabled);
        setting.SetPositiveSliderVisible(2, isEnabled);
        setting.SetPositiveCapVisible(2, isEnabled);
    }
    
    public void SetZMoveStatus(bool status)
    {
        var setting = RTGizmosEngine.Get.MoveGizmoLookAndFeel3D;
        setting.SetPositiveSliderVisible(2,status);
        setting.SetPositiveCapVisible(2,status);
        setting.SetDblSliderVisible(PlaneId.YZ,status);
        setting.SetDblSliderVisible(PlaneId.ZX,status);
    }

    public void DisableGizmo()
    {
        currentGizmo?.Gizmo.SetEnabled(false);
        NodeTransformController.Inst.OnDisSelectTarget(current);
        current = null;
    }


    public ObjectTransformGizmo CreateMoveGizmo()
    {
        if (moveGizmo == null)
        {
            moveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
            LockInputOnMove(moveGizmo.Gizmo, HandleMode.Move);
            Snap(moveGizmo.Gizmo);
        }
        moveGizmo.Gizmo.SetEnabled(true);
        return moveGizmo;
    }

    public ObjectTransformGizmo CreateRotationGizmo()
    {
        if (rotationGizmo == null)
        {
            rotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
            LockInputOnMove(rotationGizmo.Gizmo, HandleMode.Rotate);
            Snap(rotationGizmo.Gizmo);
        }
        rotationGizmo.Gizmo.SetEnabled(true);
        return rotationGizmo;
    }

    public ObjectTransformGizmo CreateScaleGizmo(Action onCreate, bool norScale)
    {
        if (scaleGizmo == null)
        {
            scaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
            LockInputOnMove(scaleGizmo.Gizmo, HandleMode.Scale);
            UseMinScale(scaleGizmo.Gizmo);
            Snap(scaleGizmo.Gizmo);
            onCreate?.Invoke();
            //AttachOnScaleUpdate(scaleGizmo.Gizmo);
            if (norScale)
            {
                UseNorScale(scaleGizmo.Gizmo);
            }
        }
        scaleGizmo.Gizmo.SetEnabled(true);
        return scaleGizmo;
    }


    void LockInputOnMove(Gizmo gizmo, HandleMode handleMode)
    {
        gizmo.PreDragBeginAttempt += (Gizmo giz, int handle) =>
        {
            if (!current) {
                return;
            }
            LoggerUtils.Log("拖动PreDragBeginAttempt");
            moveGizmo?.SetCanAffectPosition(!lockState);
            scaleGizmo?.SetCanAffectScale(!lockState);
            rotationGizmo?.SetCanAffectRotation(!lockState);
            if(lockState)
                TipPanel.ShowToast("Unlock it to edit");
        };
        gizmo.PostDragBegin += (Gizmo giz, int handle) =>
        {
            InputReceiver.locked = true;
            
            OnTargetDrag("PostDragBegin",current);
        };
        gizmo.PostDragEnd += (Gizmo giz, int handle) =>
        {
            InputReceiver.locked = false;
            OnTargetDrag("PostDragEnd",current);
        };
        gizmo.PostDragUpdate += (Gizmo giz, int handle) =>
        {
            NodeTransformController.Inst.OnDragUpdate(current, handleMode);
        };
    }

    void Snap(Gizmo gizmo)
    {
        gizmo.PostDragEnd += (Gizmo giz, int handle) =>
        {
            if (!current) return;
            Snap(current.transform, digitKept);
        };
    }

    void UseMinScale(Gizmo gizmo)
    {
        gizmo.PostDragUpdate += (Gizmo giz, int handle) =>
        {
            if (!current) return;

            Vector3 ret = current.transform.localScale;
            if (ret.x < minScale)
            {
                ret.x = minScale;
            }
            if (ret.y < minScale)
            {
                ret.y = minScale;
            }
            if (ret.z < minScale)
            {
                ret.z = minScale;
            }
            current.transform.localScale = ret;
        };
    }
    
    void UseNorScale(Gizmo gizmo)
    {
        gizmo.PostDragUpdate += (Gizmo giz, int handle) =>
        {
            if (!current) return;

            Vector3 ret = current.transform.localScale;
            float ns = ret.x;
            if (ret.x == ret.y) ns = ret.z;
            else if (ret.x == ret.z) ns = ret.y;
            else if (ret.y == ret.z) ns = ret.x;
            current.transform.localScale = new Vector3(ns, ns, ns);
        };
    }
  

    public static void Snap(Transform target, int fract)
    {

        Vector3 mov = target.localPosition;
        Vector3 rot = target.localRotation.eulerAngles;
        Vector3 sca = target.localScale;

        mov.x = (float)Math.Round(mov.x, fract);
        mov.y = (float)Math.Round(mov.y, fract);
        mov.z = (float)Math.Round(mov.z, fract);

        rot.x = (float)Math.Round(rot.x, fract);
        rot.y = (float)Math.Round(rot.y, fract);
        rot.z = (float)Math.Round(rot.z, fract);

        sca.x = (float)Math.Round(sca.x, fract);
        sca.y = (float)Math.Round(sca.y, fract);
        sca.z = (float)Math.Round(sca.z, fract);

        target.localPosition = mov;
        target.localRotation = Quaternion.Euler(rot);
        target.localScale = sca;
    }

    public void SetLockState(bool isLock)
    {
        lockState = isLock;
    }
    public bool GetLockState()
    {
        return lockState;
    }
    private void OnTargetDrag(string dragType,GameObject target){
        if(dragType == "PostDragBegin"){
            var undoData = CreateUndoData(target);
            dragUndoRecord = new UndoRecord(UndoHelperName.TransformUndoHelper);
            dragUndoRecord.BeginData = undoData;
            lastGameObj = target;
        }else if(dragType == "PostDragEnd"){
            if(dragUndoRecord != null && lastGameObj == target && lockState == false){
                var undoData = CreateUndoData(target);
                dragUndoRecord.EndData = undoData;
                AddRecord(dragUndoRecord);
            }
            lastGameObj = null;
            dragUndoRecord = null;
        }        
    }

    public void AddRecord(UndoRecord record)
    {
        if (!NeedRecord) return;
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void AddRecord(TransformUndoData beginData,TransformUndoData endData)
    {
        if (!NeedRecord) return;
        UndoRecord record = new UndoRecord(UndoHelperName.TransformUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private TransformUndoData CreateUndoData(GameObject target)
    {
        TransformUndoData undoData = new TransformUndoData();
        undoData.postion = current.transform.localPosition;
        undoData.eulerAngles = current.transform.localEulerAngles;
        undoData.scale = current.transform.localScale;
        undoData.targetNode = target.transform;
        if(currentGizmo == moveGizmo){
            undoData.transformType = (int)HandleMode.Move;
        }else if(currentGizmo == scaleGizmo){
            undoData.transformType = (int)HandleMode.Scale;
        }else if(currentGizmo == rotationGizmo){
            undoData.transformType = (int)HandleMode.Rotate;
        }
        return undoData;
    }
    
    public void SetMoveTransformLocal()
    {
        moveGizmo.SetTransformSpace(GizmoSpace.Local);
    }

    public void SetMoveTransformGlobal()
    {
        moveGizmo.SetTransformSpace(GizmoSpace.Global);
    }
}
