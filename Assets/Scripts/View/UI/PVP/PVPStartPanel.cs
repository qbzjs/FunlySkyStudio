using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PVPStartPanel : BasePVPGamePanel
{
    public GameObject GameTime;
    public Text GameTimeText;
    public Action StartEndAction;
    public int durationTime;

    public override void Enter(PVPGameConnectEnum connect)
    {
        PVPWaitAreaManager.Inst.IsPVPGameStart = true;
        PVPWaitAreaManager.Inst.SetMeshAndBoxVisible(false, true);
        if (connect == PVPGameConnectEnum.Normal)
        {
            BlackPanel.Show();
            BlackPanel.Instance.PlayTransitionAnimAct(StartGame);
        }
        else
        {
            StartGameByReconnect();
        }
    }

    private void StartGame()
    {
        if(!gameObject.activeInHierarchy)
            return;
        
        PVPWaitAreaManager.Inst.OnReset();
        PlayerManager.Inst.ReturnSpawnPoint();
        SceneSystem.Inst.StopSystem();
        SceneSystem.Inst.StartSystem();
        PlayerManager.Inst.StartShowPlayerState();
        PlayerBaseControl.Inst.SetEndFlyPlayerPos();
        GameTime.SetActive(true);
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
    }

    private void StartGameByReconnect()
    {
        PlayerManager.Inst.StartShowPlayerState();
        GameTime.SetActive(true);
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
    }


    private IEnumerator GameTimeCount()
    {
        while (durationTime >= 0)
        {
            GameTimeText.text = durationTime.ToString();
            if (durationTime < 100)
            {
                GameTimeText.text = "0" + durationTime.ToString();
            }
            if (durationTime < 10)
            {
                GameTimeText.text = "00" + durationTime.ToString();
            }
            if (durationTime <= 0)//对局时间到
            {
                StartEndAction?.Invoke();
            }
            durationTime--;
            yield return new WaitForSeconds(1);
        }
    }

    public void StartCountDown()
    {
        StartCoroutine("GameTimeCount");
    }

    public void StartCountDown(string afterGameTime)
    {
        OnResetTime(afterGameTime);
        StopCoroutine("GameTimeCount");
        StartCoroutine("GameTimeCount");
    }

    public void OnResetTime(string afterGameTime)
    {
        if (int.TryParse(afterGameTime, out int mileDurationTime))
        {
            var gameTime = GlobalFieldController.pvpData.gameTime;
            durationTime = gameTime - mileDurationTime / 1000;
        }
    }

    public override void Leave()
    {
        // PVPWaitAreaManager.Inst.OnReset();
        // PlayerManager.Inst.ReturnPVPWaitArea();
        // SceneSystem.Inst.StopSystem();
        // SceneSystem.Inst.StartSystem();
        // PlayerBaseControl.Inst.SetEndFlyPlayerPos();
        BlackPanel.Instance.ForceKillTransformAnim();
        StopCoroutine("GameTimeCount");
        GameTime.SetActive(false);
    }
}