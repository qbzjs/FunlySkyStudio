/// <summary>
/// Author:Mingo-LiZongMing
/// Description:锁定和隐藏的控制器
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockHideManager : ManagerInstance<LockHideManager>, IManager,IUndoRecord,IPVPManager
{
    public GizmoController gController;
    //HideMode
    public List<SceneEntity> hideList = new List<SceneEntity>();
    //LockMode
    public List<int> lockList = new List<int>();

    #region Lock Func
    public bool GetCurLockState()
    {
        var curTarget = gController.GetCurrentTarget();
        if (curTarget == null)
        {
            return false;
        }
        var nodeBehav = curTarget.GetComponent<NodeBaseBehaviour>();
        var entity = nodeBehav.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        bool lockState = lockList.Contains(uid) ? true : false;
        return lockState;
    }

    public void SetCurLockState(bool isLock)
    {
        var curTarget = gController.GetCurrentTarget();
        if (curTarget == null)
        {
            return;
        }
        gController.SetLockState(isLock);

        var nodeBehav = curTarget.GetComponent<NodeBaseBehaviour>();
        var entity = nodeBehav.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        RefreshLockList(uid, isLock);
    }
    public void RefreshLockList(int eId ,bool isLock)
    {
        if (isLock)
        {
            if (!lockList.Contains(eId))
            {
                lockList.Add(eId);
            }
        }
        else
        {
            if (lockList.Contains(eId))
            {
                lockList.Remove(eId);
            }
        }
    }
    #endregion

    #region Hide Func

    public void HideCurProp()
    {

        var curTarget = gController.GetCurrentTarget();
        if (curTarget == null)
        {
            return;
        }
        LockHideUndoData begin =CreateUndoData(LockHideType.Hide);
        var nodeBehav = curTarget.GetComponent<NodeBaseBehaviour>();
        var entity = nodeBehav.entity;
        var curObj = nodeBehav.gameObject;
        if (!hideList.Contains(entity))
        {
            hideList.Add(entity);
        }
        curObj.SetActive(false);
        FollowModeManager.Inst.SetFollowBoxVisable(nodeBehav, false);

        LockHideUndoData end = CreateUndoData(LockHideType.Hide);
        AddRecord(begin, end);
        DisableAllPanel();
        CheckHidePanelVisable();
        TipPanel.ShowToast("Hidden only in edit mode, press the upper right button [Show] to display.");
    }
    public void HideCurPropUndo(bool isActive,GameObject gameObject)
    {
        var nodeBehav = gameObject.GetComponent<NodeBaseBehaviour>();
        var entity = nodeBehav.entity;
        var curObj = nodeBehav.gameObject;
        if (isActive)
        {
            if (hideList.Contains(entity))
            {
                hideList.Remove(entity);
            }
        }
        else
        {
            if (!hideList.Contains(entity))
            {
                hideList.Add(entity);

            }
            if (gController.GetCurrentTarget() == curObj)
            {
                DisableAllPanel();
            }
        }
        curObj.SetActive(isActive);
        CheckHidePanelVisable();
    }

    public void CheckHidePanelVisable()
    {
        if(hideList.Count > 0) {
            ShowHidePropPanel.Show();
        }
        else
        {
            ShowHidePropPanel.Hide();
        }
    }

    private void SetHidePropVisable(bool isActive) {
        foreach (var entity in hideList)
        {
            var entityGo = entity.Get<GameObjectComponent>().bindGo;
            if(entityGo == null)
            {
                continue;
            }
            var nodeBehav = entityGo.GetComponent<NodeBaseBehaviour>();
            var nodeObj = nodeBehav.gameObject;
            FollowModeManager.Inst.SetFollowBoxVisable(nodeBehav, isActive);
            switch (GlobalFieldController.CurGameMode)
            {
                case GameMode.Edit:
                    nodeObj.SetActive(isActive);
                    break;
                case GameMode.Play:
                    if (!entity.HasComponent<ShowHideComponent>())
                    {
                        nodeObj.SetActive(isActive);
                    }
                    BloodPropManager.Inst.SetDefaultModeShow(false);
                    AttackWeaponManager.Inst.SetDefaultModeShow(false);
                    ShootWeaponManager.Inst.SetDefaultModeShow(false);
                    FireworkManager.Inst.SetDefaultModeShow(false);
                    FreezePropsManager.Inst.SetDefaultModeShow(false);
                    break;
            }
        }
    }

    public void ClearHideList()
    {
        SetHidePropVisable(true);
        hideList.Clear();
        DisableAllPanel();
        CheckHidePanelVisable();
    }
    public void ShowHideUndo(LockHideUndoData data)
    {

        if (data.activeSelf)
        {
            SetHidePropVisable(true);
            hideList.Clear();
        }
        else
        {
            bool isHidePanel = false;
            var obj = gController.GetCurrentTarget();
            NodeBaseBehaviour nodeBehav = null;
            SceneEntity curEntity = null;
            if (obj == null)
            {
                isHidePanel = true;
            }
            else
            {
                nodeBehav = obj.GetComponent<NodeBaseBehaviour>();
                curEntity = nodeBehav.entity;
            }
          
            foreach (var entity in data.hideList)
            {
                if (entity != null)
                {
                    var entityGo = entity.Get<GameObjectComponent>().bindGo;
                    if (entityGo != null)
                    {
                        entityGo.SetActive(false);
                        if (!hideList.Contains(entity))
                        {
                            hideList.Add(entity);
                        }
                    }
                }
                if (entity == curEntity && curEntity!=null)
                {
                    isHidePanel = true;
                }
            }
            if (isHidePanel)
            {
                DisableAllPanel();
            }
        }
        CheckHidePanelVisable();
    }
    public void EnterPlayMode()
    {
        SetHidePropVisable(true);
    }

    public void EnterEditMode()
    {
        CheckHidePanelVisable();
        SetHidePropVisable(false);
    }

    public void OnReset()
    {
        SetHidePropVisable(true);
    }
    #endregion

    public bool IsHidedEntity(SceneEntity entity)
    {
        if(entity == null) return false;
        if(hideList == null || hideList.Count == 0) return false;
        return hideList.Contains(entity);
    }

    private void DisableAllPanel()
    {
        gController.DisableGizmo();
        UIManager.Inst.CloseAllDialog();
        SceneGizmoPanel.Show();
        BasePrimitivePanel.Show();
        if(GlobalFieldController.CurSceneType == SCENE_TYPE.MAP_SCENE || GlobalFieldController.CurSceneType == SCENE_TYPE.MYSPACE_SCENE)
        {
            GameEditModePanel.Show();
        }
        else if(GlobalFieldController.CurSceneType == SCENE_TYPE.ResMAP_SCENE)
        {
            PropEditModePanel.Show();
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {

    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {

    }

    public void Clear()
    {
        hideList.Clear();
        lockList.Clear();
    }
    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void AddRecord(LockHideUndoData beginData, LockHideUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.LockHideUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private LockHideUndoData CreateUndoData(LockHideType type)
    {
        LockHideUndoData undoData = new LockHideUndoData();
        undoData.LockHideType = (int)type;
        GameObject obj = gController.GetCurrentTarget();
        undoData.activeSelf = obj.activeSelf;
        undoData.targetNode = obj.transform;
        return undoData;
    }
}
