using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: 角色对战状态控制器
/// Date: 2022-04-07 18:08:17
/// </summary>
public enum PlayerType
{
    self,
    other
}

public enum TeamMemberState
{
    Normal = 0,
    Self = 1,
    TeamMate = 2,
    Enem = 3,
}

public class CharBattleControl : MonoBehaviour
{
    private BattleState battleState;
    private float curHP = 100;

    public Action<float> OnUpdateEvent;
    public Action OnDeadEvent;
    public PlayerType playerType;
    private void OnDestroy()
    {
        OnUpdateEvent = null;
        OnDeadEvent = null;
    }

    //初始化血量状态(复位血量/开启显示)
    public void ShowState(string playerId,bool isInit = true)
    {
        SetBloodBarVisiable(true);
        if (isInit)
        {
            curHP = SceneParser.Inst.GetCustomHP(playerId);
            RefreshBloodBarValue(playerId);
        }
        if (PlayModePanel.Instance && GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            PlayModePanel.Instance.ShowFpsPlayerHpPanel(true);
        }
    }
    public void SetHPColor(int memberState)
    {

        var state = GetStateCom();
        if (state != null)
        {
            switch ((TeamMemberState)memberState) {
                case TeamMemberState.Normal:
                case TeamMemberState.Self:
                case TeamMemberState.TeamMate:
                    state.SetColor(TeamColor.Green);
                    break;
                case TeamMemberState.Enem:
                    state.SetColor(TeamColor.Red);
                    break;

            }      
        }
    }
    public void SetHPVisible(bool visible)
    {
        SetBloodBarVisiable(visible);
    }

    //退出血量显示(复位血量/关闭显示)
    public void ExitShowBattle(string playerId)
    {
        curHP = SceneParser.Inst.GetCustomHP(playerId);
        SetBloodBarVisiable(false);
        var state = GetStateCom();
        if (state != null)
        {
            state.ResetValue();
        }
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.ShowFpsPlayerHpPanel(false);
        }
    }
    
    //角色死亡
    public void GetDeath(string playerId)
    {
        curHP = 0;
        RefreshBloodBarValue(playerId);
        OnDeadEvent?.Invoke();
    }

    //更新生命值(从服务器直接获取)
    public void UpdateHpValue(float value,string playerId)
    {
        float hp = Mathf.Max(0, value);
        hp = Mathf.Min(hp,SceneParser.Inst.GetCustomHP(playerId));
        //刷新血量显示
        curHP = hp;
        RefreshBloodBarValue(playerId);
        OnUpdateEvent?.Invoke(value);
    }

    public void AddHp(float addValue,string playerId)
    {
        float addHp = Mathf.Max(0, addValue);
        float hp = curHP + addHp;
        hp = Mathf.Min(hp, SceneParser.Inst.GetCustomHP(playerId));
        //刷新血量显示
        curHP = hp;
        RefreshBloodBarValue(playerId);

        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetBlood(hp);
        }
        OnUpdateEvent?.Invoke(hp);
    }
    public void SubHp(float value,string playerId)
    {
        float hp = curHP - value;
        hp = Mathf.Max(0, hp);
        //刷新血量显示
        curHP = hp;
        RefreshBloodBarValue(playerId);

        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetBlood(hp);
        }
        OnUpdateEvent?.Invoke(hp);
    }
    public void ResetHpValue(string playerId)
    {
        curHP = SceneParser.Inst.GetCustomHP(playerId);
        RefreshBloodBarValue(playerId);

        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetBlood(curHP);
        }
    }

    private BattleState GetStateCom()
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return null;
        }
        if (battleState == null)
        {
            battleState = GetComponentInChildren<BattleState>(true);
        }
        return battleState;
    }
   

    private void RefreshBloodBarValue(string playerId)
    {
        var state = GetStateCom();
        if (state != null)
        {
            state.SetValue(curHP / SceneParser.Inst.GetCustomHP(playerId));
        }
    }

    private void SetBloodBarVisiable(bool isVisiable)
    {
        var state = GetStateCom();
        if (state != null)
        {
            state.SetVisiable(isVisiable);
        }



    }

    public float GetCurHp()
    {
        return curHP;
    }
    public bool GetBloodBarVisiable()
    {
        var state = GetStateCom();
       return state.gameObject.activeSelf ? true : false;
    }
}
