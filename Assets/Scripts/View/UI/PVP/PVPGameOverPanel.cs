using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class PVPGameOverPanel : BasePVPGamePanel
{
    public enum  GameOverStateEnum
    {
        Win,
        Loss,
        TimeUp
    }
    
    [SerializeField] private GameObject YouWinToast;
    [SerializeField] private GameObject YouLoseToast;
    [SerializeField] private GameObject TimeOutToast;
    public Action GameOverAction;
    [SerializeField] private ParticleSystem Particle;

    public override void Enter(PVPGameConnectEnum connect)
    {
    }

    public void SetWinner(GameOverStateEnum  state)
    {
        SetWinnerPanel(state);
        PVPWaitAreaManager.Inst.IsPVPGameStart = false;
        Invoke("GameOver", 1.7f);
    }

    public void SetWinnerPanel(GameOverStateEnum state)
    {
        if (PVPWaitAreaManager.Inst.IsPVPGameStart)
        {
            YouWinToast.SetActive(state == GameOverStateEnum.Win);
            YouLoseToast.SetActive(state == GameOverStateEnum.Loss);
            TimeOutToast.SetActive(state == GameOverStateEnum.TimeUp);
            MessageHelper.Broadcast(MessageName.OnPVPResult, state);
        }
    }

    public void GameOver()
    {
        GameOverAction?.Invoke();
    }

    public override void Leave()
    {
        YouWinToast.SetActive(false);
        YouLoseToast.SetActive(false);
        TimeOutToast.SetActive(false);
    }
    
    // private void YouWin()
    // {
    //     AKSoundManager.Inst.PostEvent("Play_You_Win", PlayerControlManager.Inst.playerEmoji.gameObject);
    //     CoroutineManager.Inst.StartCoroutine(DoubleJump());
    //     CoroutineManager.Inst.StartCoroutine(ShowParticle());
    // }
    //
    // private void YouLose()
    // {
    //     AKSoundManager.Inst.PostEvent("Play_Game_Over", PlayerControlManager.Inst.playerEmoji.gameObject);
    //     PlayerControlManager.Inst.playerEmoji.PlayMove(13);
    // }
    //
    // private void TimeOut()
    // {
    //     AKSoundManager.Inst.PostEvent("Play_Time_Out", PlayerControlManager.Inst.playerEmoji.gameObject);
    //     // PlayerControlManager.Inst.playerEmoji.PlayMove(13);
    // }
    //
    // private IEnumerator DoubleJump()
    // {
    //     PlayerControlManager.Inst.playerEmoji.PlayMove(22);
    //     yield return new WaitForSeconds(1.2f);
    //     PlayerControlManager.Inst.playerEmoji.PlayMove(22);
    //     yield return new WaitForSeconds(1.2f);
    //
    // }
    //
    // private IEnumerator ShowParticle()
    // {
    //     var p = Instantiate(Particle);
    //     var trs = PlayerControlManager.Inst.playerEmoji.transform;
    //     var pos = trs.position;
    //     var rot = trs.eulerAngles;
    //     var ptrs = p.transform;
    //     ptrs.localScale = Vector3.one * 3;
    //     var ppos = trs.position;
    //     ppos.y += 4;
    //     ptrs.position = ppos;
    //     var prot = ptrs.eulerAngles;
    //     prot.y = rot.y;
    //     ptrs.eulerAngles = prot;
    //     p.gameObject.SetActive(true);
    //     p.Play();
    //     yield return new WaitForSeconds(3f);
    //     Destroy(p.gameObject);
    // }
    //
    // private Dictionary<BasePanel, bool> pvpStateList;
    // private bool needSwitch = true;
    // private void SwitchUI()
    // {
    //     if (!needSwitch)
    //     {
    //         needSwitch = true;
    //         return;
    //     }
    //     PlayModePanel.Instance.PVPUIswitch();
    //     if (pvpStateList == null)
    //     {
    //         pvpStateList = new Dictionary<BasePanel, bool>();
    //         var pobj = GameObject.Find("Canvas/Panel");
    //         BasePanel[] ui = new BasePanel[] { };
    //         if (pobj) ui = pobj.transform.GetComponentsInChildren<BasePanel>();
    //         foreach (var x in ui)
    //         {
    //             if (x != null)
    //             {
    //                 if (x is TipPanel ||
    //                     x is PortalPlayPanel ||
    //                     x is PlayModePanel ||
    //                     x is VideoFullPanel ||
    //                     x is StorePanel ||
    //                     x is PVPWinConditionGamePlayPanel ||
    //                     x is PVPSurvivalGamePlayPanel) continue;
    //                 pvpStateList.Add(x,x.gameObject.activeSelf);
    //                 x.gameObject.SetActive(false);
    //             }
    //         }
    //     }
    //     else
    //     {
    //         foreach (var kv in pvpStateList)
    //         {
    //             if (!kv.Value) continue;
    //             kv.Key.gameObject.SetActive(kv.Value);
    //         }
    //         pvpStateList = null;
    //     }
    // }
}
