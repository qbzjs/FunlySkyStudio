using System;
using System.Collections;
using System.Collections.Generic;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

/// <summary>
/// Author:Mingo-LiZongMing
/// Description:Downtown传送点Bev
/// </summary>


//传送光柱类型：1:downtown-子地图 2:UGC地图-Downtown
public enum TransferType
{
    DowntownTransfer = 1,
    UGCMapTransfer = 2,
}

public enum TransAnimType
{
    Up = 1,
    End = 2,
}

public class BaseTransferBehaviour : NodeBaseBehaviour
{
    private bool _isTransfer = false;
    private BudTimer _transferTimer;
    private Transform _playerTransform;
    private GameObject _transEffectUpPrefab;
    private GameObject _transEffectDownPrefab;
    private GameObject _transEffectObj;

    //传送光柱类型：1:downtown-子地图 2:UGC地图-Downtown
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        _transEffectUpPrefab = ResManager.Inst.LoadRes<GameObject>("Effect/transfer1.0/delivery_ef_UP");
        _transEffectDownPrefab = ResManager.Inst.LoadRes<GameObject>("Effect/transfer1.0/delivery_ef_END");
    }

    public override void OnTrigEnter()
    {
        base.OnTrigEnter();

        if (_isTransfer || !StateManager.Inst.CheckCanTransfer())
            return;
        _isTransfer = true;
        TimerManager.Inst.Stop(_transferTimer);
        LoggerUtils.Log("开始准备传送倒计时！2S");
        DowntownTransferManager.Inst.SendMsgToSever(TransAnimType.Up);
        SetInputLock(true);
        PlayTransferEffect(TransAnimType.Up);
        _transferTimer = TimerManager.Inst.RunOnce("StartTransfer", 2f, () => {
            if (_isTransfer)
            {
                StopTransferEffect();
                StartTransfer();
                _isTransfer = false;
                PlayerBaseControl.Inst.PlayerResetIdle();
            }
            SetInputLock(false);
        });
    }

    public override void OnTrigExit()
    {
        base.OnTrigExit();
        if (_isTransfer)
        {
            _isTransfer = false;
            StopTransferEffect();
            TimerManager.Inst.Stop(_transferTimer);
            _transferTimer = null;
            PlayerBaseControl.Inst.PlayerResetIdle();
            SetInputLock(false);
        }
    }


    public virtual void StartTransfer()
    {

    }

    private void SetInputLock(bool isLock)
    {
        if (isLock)
        {
            PlayerBaseControl.Inst.Move(Vector3.zero);
            InputReceiver.locked = true;
        }
        else
        {
            InputReceiver.locked = false;
        }
    }

    public void PlayTransferEffect(TransAnimType type)
    {
        StopTransferEffect();
        if (_playerTransform == null)
        {
            _playerTransform = PlayerBaseControl.Inst.animCon.transform;
        }

        switch (type)
        {
            case TransAnimType.Up:
                _transEffectObj = GameObject.Instantiate(_transEffectUpPrefab, _playerTransform);
                PlayerBaseControl.Inst.animCon.PlayAnim(null, "portal_up", -1);
                PlayTransferSound("Play_GreatSnowfield_Convey_Start");
                break;
            case TransAnimType.End:
                _transEffectObj = GameObject.Instantiate(_transEffectDownPrefab, _playerTransform);
                PlayerBaseControl.Inst.animCon.PlayAnim(null, "portal_down", -1);
                PlayTransferSound("Play_GreatSnowfield_Convey_End");
                break;
        }
        _transEffectObj.name = "TransEffect";
        _transEffectObj.transform.localPosition = new Vector3(0, -0.85f, 0);
        _transEffectObj.SetActive(true);
    }

    public void StopTransferEffect()
    {
        if (_transEffectObj != null)
        {
            GameObject.Destroy(_transEffectObj);
            _transEffectObj = null;
        }
    }

    public void PlayTransferSound(string eventName)
    {
        if(PlayerBaseControl.Inst != null)
        {
            var playerNode = PlayerBaseControl.Inst.animCon.gameObject;
            AkSoundEngine.PostEvent(eventName, playerNode);
        }
    }

    public void SetPlayerPosAndRot()
    {
        var pos = SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
        var rot = SpawnPointManager.Inst.GetSpawnPoint().transform.localRotation;
        PlayerBaseControl.Inst.SetPlayerPosAndRot(pos, rot);
    }
}
