using System;
using System.Collections.Generic;

/// <summary>
/// TODO:暂时写死在此脚本，待模块互斥的实现
/// UI是否可见的逻辑限定
/// 例如攻击道具UI只有满足一定逻辑条件，才能执行Show/Hide
/// </summary>
public class UIVisibleControlLogic : CInstance<UIVisibleControlLogic>
{
    private Dictionary<string, Func<bool>> _controlLogics = new Dictionary<string, Func<bool>>();

    public override void Release()
    {
        base.Release();
        _controlLogics.Clear();
    }
    
    public Func<bool> GetControlFuncByName(string controlFuncName)
    {
        if (!string.IsNullOrEmpty(controlFuncName) && _controlLogics.ContainsKey(controlFuncName))
        {
            return _controlLogics[controlFuncName];
        }

        return null;
    }

    #region This region was auto created by Bud Tool, DO NOT EDIT IT !!!!!!!!

    public void Init()
    {
        if (_controlLogics == null)
        {
            _controlLogics = new Dictionary<string, Func<bool>>();
        }
        else
        {
            _controlLogics.Clear();
        }
        _controlLogics.Add("AttackWeaponCtrlPanelCanControl", AttackWeaponCtrlPanelCanControl);
        _controlLogics.Add("ShootWeaponCtrlPanelCanControl", ShootWeaponCtrlPanelCanControl);
        _controlLogics.Add("StateEmoPanelCanControl", StateEmoPanelCanControl);
        _controlLogics.Add("StateBaggagePanelCanControl", StateBaggagePanelCanControl);
        _controlLogics.Add("ParachuteCtrlPanelControl", ParachuteCtrlPanelControl);
        _controlLogics.Add("SwordPanelCanControl", SwordPanelCanControl);
        //BudToolUIControlObject_Inject_PanelControlLogic_Init
    }

    #endregion
    

    /////////////////////////////// 手动添加注册方法 /////////////////////////////// 

    private bool AttackWeaponCtrlPanelCanControl()
    {
        if (AttackWeaponCtrlPanel.Instance && PlayerAttackControl.Inst)
        {
            if (PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon == null)
            {
                return false;
            }

            if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
                ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater))
            {
                return false;
            }

            return true;
        }

        return false;
    }
    private bool ShootWeaponCtrlPanelCanControl()
    {
        if (ShootWeaponCtrlPanel.Instance && PlayerShootControl.Inst)
        {
            if (PlayerShootControl.Inst.curShootPlayer.HoldWeapon == null)
            {
                return false;
            }

            if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
                ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private bool StateEmoPanelCanControl()
    {
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            return true;
        }
        return false;
    }

    private bool StateBaggagePanelCanControl()
    {
        if (SceneParser.Inst.GetBaggageSet() == 1)
        {
            return true;
        }
        return false;
    }    
    
    private bool ParachuteCtrlPanelControl()
    {
        if (ParachuteCtrlPanel.Instance && StateManager.IsParachuteGliding)
        {
            return true;
        }
        return false;
    }

    private bool SwordPanelCanControl()
    {
        if (SwordManager.Inst.IsSelfInSword())
        {
            return true;
        }
        return false;
    }

    //BudToolUIControlObject_Inject_PanelControlLogic_ControlFunc

}