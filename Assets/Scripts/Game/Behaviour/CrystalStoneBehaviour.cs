using BudEngine.NetEngine;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: pzkunn
/// Description: 冰晶宝石行为类
/// Date: 2022/10/21 13:15:36
/// </summary>
public class CrystalStoneBehaviour : NodeBaseBehaviour
{
    private bool canCollect;
    private Vector3 defPos = new Vector3(0, 0.3f, 0);
    private Vector3 defRot = new Vector3(-90, 0, 0);
    private Vector3 defSca = Vector3.one;

    public Animator animator;
    public GameObject effectGO;
    public GameObject glowGO;
    public GameObject glowEffect;
    public MeshRenderer cRenderer;
    public Material defMat;
    public Material repMat;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        //创建初始化
        canCollect = false;
        effectGO.SetActive(false);
        animator.enabled = false;
        animator.SetBool("isCollect", false);
    }

    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            canCollect = false;
            animator.enabled = false;
            //还原模型位置
            animator.transform.localScale = defSca;
            animator.transform.localPosition = defPos;
            animator.transform.localRotation = Quaternion.Euler(defRot);
            //停止循环音效
            AKSoundManager.Inst.StopCrystalStoneLoop(gameObject);
        }
        else
        {
            canCollect = true;
            animator.enabled = true;
            //复原动画
            PlayCollectAnim(false);
            //播放循环音效
            AKSoundManager.Inst.PlayCrystalStoneLoop(gameObject);
        }
        effectGO.SetActive(false);
    }

    //替换材质颜色, 游玩模式GetItems之后, 已收集过需要替换
    public void ChangeIceGemMaterial(IceGemMatType type)
    {
        cRenderer.material = type == IceGemMatType.Dark ? repMat : defMat;
        glowGO.SetActive(type == IceGemMatType.Bright);
        glowEffect.SetActive(type == IceGemMatType.Bright);
    }

    public override void OnTrigEnter()
    {
        base.OnTrigEnter();
        //without trigger protected
        if (ReferManager.Inst.isRefer || !canCollect)
        {
            return;
        }
        //被冻结时候不能收集宝石
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        //收集宝石
        SetCollect();
        canCollect = false;
    }

    private void SetCollect()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && GlobalFieldController.IsDowntownEnter)
        {
            SendCollectRequest();
        }
        else
        {
            OnCollectSuccess();
        }
    }

    public void OnCollectSuccess()
    {
        //人物动画
        CrystalStoneManager.Inst.OnPlayerCollectAnim();
        //宝石动画
        PlayCollectAnim(true);
        //收集音效
        AKSoundManager.Inst.StopCrystalStoneLoop(gameObject);
        AKSoundManager.Inst.PlayCrystalStonePickUp(gameObject);
    }

    private void PlayCollectAnim(bool state)
    {
        animator.SetBool("isCollect", state);
    }

    public void ActiveCollectEffect()
    {
        effectGO.SetActive(true);
    }

    private void SendCollectRequest()
    {
        IceGemSendData iceGemData = new IceGemSendData();
        iceGemData.playerId = Player.Id;
        iceGemData.mapId = GameManager.Inst.gameMapInfo.mapId;
        iceGemData.subMapId = GlobalFieldController.CurMapInfo.mapId;
        iceGemData.itemId = entity.Get<GameObjectComponent>().uid;

        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.IceGem,
            data = JsonConvert.SerializeObject(iceGemData),
        };
        LoggerUtils.Log("IceGem SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
}
