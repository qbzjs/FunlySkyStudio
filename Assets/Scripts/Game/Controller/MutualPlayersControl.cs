using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:玩家牵手状态下两个玩家的控制脚本(目前仅牵手使用)
/// Date: 2022/2/15 12:07:42
/// </summary>

public class MutualPlayersControl : MonoBehaviour
{
    public GameObject playerObj;
    public GameObject playerTrigger;

    // 牵手前自己的角色控制器属性记录(便于放手后恢复)
    private float beforeRadius;
    private Vector3 beforeCenter;
    private float beforeSlopeLimit;
    private float beforeStepOffset;
    private float beforeSkinWidth;
    private float beforeMinMoveDistance;
    private float beforeHeight;

    // 牵手前自己的碰撞器属性记录(便于放手后恢复)
    private float beforeColliderRadius;
    private Vector3 beforeColliderCenter;

    public CharacterController playerController;
    private GameObject otherPlayerNode;
    private Transform PlayersNode;
    private GameObject otherPlayer = null;
    private Transform playerModel;

    private CapsuleCollider playerCollider;

    //牵手跑时跟随者玩家的位置
    private Vector3 otherPlayerRunPos = new Vector3(0.37f, 0, -0.6f);
    // 牵手跑时玩家胶囊体的中心点位置
    private Vector3 playerContrllerRunCenter = new Vector3(0, 0.15f, -0.5f);
    private Vector3 playerColliderRunCenter = new Vector3(0, 0, -0.5f);
    // 牵手静止站立时玩家胶囊体的中心点位置
    private Vector3 playerContrllerIdleCenter = new Vector3(0.5f, 0.15f, 0);
    private Vector3 playerColliderIdleCenter = new Vector3(0.35f, 0, 0);

    /**
    * 设置牵手的跟随者/被牵者
    */
    public void SetOtherPlayer(GameObject gameObject)
    {
        otherPlayer = gameObject;
    }

    /**
    * 玩家开始牵手，主从跟随处理
    */
    public void StartMutual()
    {
        otherPlayerNode = GameObject.Find("OtherPlayerNode");

        playerObj = GameObject.Find("PlayerNode").gameObject.transform.Find("Player").gameObject;
        playerObj.transform.rotation = new Quaternion(0, 0, 0, 0);

        playerController = playerObj.GetComponent<CharacterController>();

        var player = PlayerMutualControl.Inst;
        if (player == null)
        {
            LoggerUtils.Log("MutualPlayersControl.StartMutual player is null !!!");
            return;
        }
        var otherPlayerCtrl = otherPlayer.GetComponent<OtherPlayerCtr>();
        otherPlayerCtrl.animCon = otherPlayer.GetComponent<AnimationController>();
        player.playerFollowerCtrl = otherPlayerCtrl;
        otherPlayerCtrl.isAvoidFrame = true;
        otherPlayerCtrl.animCon.RecStopLoop();

        beforeRadius = playerController.radius;
        beforeCenter = playerController.center;
        beforeSlopeLimit = playerController.slopeLimit;
        beforeStepOffset = playerController.stepOffset;
        beforeSkinWidth = playerController.skinWidth;
        beforeMinMoveDistance = playerController.minMoveDistance;
        beforeHeight = playerController.height;

        playerTrigger = playerObj.transform.Find("PTrigger").gameObject;
        playerCollider = playerTrigger.GetComponent<CapsuleCollider>();
        beforeColliderCenter = playerCollider.center;
        beforeColliderRadius = playerCollider.radius;
        playerCollider.radius = 1.1f;
        playerCollider.center = playerColliderIdleCenter;

        PlayersNode = playerObj.transform.Find("PlayersNode");
        if (PlayersNode == null)
        {
            PlayersNode = new GameObject("PlayersNode").transform;
            PlayersNode.parent = playerObj.transform;
            PlayersNode.localPosition = new Vector3(0, -0.95f, 0);
        }
        PlayersNode.gameObject.SetActive(true);
        playerModel = playerObj.transform.Find("Player").transform;
        otherPlayer.transform.parent = playerModel;
        otherPlayer.transform.localPosition = new Vector3(0.5f, 0, 0);
        otherPlayer.transform.localScale = Vector3.one;
        otherPlayer.transform.localRotation = Quaternion.identity;
        playerModel.parent = PlayersNode.transform;
        playerModel.localPosition = Vector3.zero;
        playerModel.rotation = new Quaternion(0, 0, 0, 0);

        InitCharacterController(playerController, 1, playerContrllerIdleCenter);
    }

    /**
    * 玩家牵手跑(一前一后)
    */
    public void PlayerRun()
    {
        if (!otherPlayer)
        {
            return;
        }
        otherPlayer.transform.localPosition = otherPlayerRunPos;
        playerController.center = playerContrllerRunCenter;
        playerCollider.center = playerColliderRunCenter;
    }

    /**
    * 玩家牵手站立静止(并排站)
    */
    public void PlayerIdle()
    {
        if (!otherPlayer)
        {
            return;
        }
        float x1 = Mathf.Lerp(otherPlayer.transform.localPosition.x, 0.5f, 0.5f);
        otherPlayer.transform.localPosition = new Vector3(x1, 0, 0);
        playerController.center = playerContrllerIdleCenter;
        playerCollider.center = playerColliderIdleCenter;
    }

    private void Update()
    {
        if (PlayerBaseControl.Inst.isMoving)
        {
            PlayerRun();
        }
        else
        {
            PlayerIdle();
        }
    }

    /**
    * 玩家放手后，恢复玩家自由状态
    */
    public void EndMutual()
    {
        playerCollider.radius = beforeColliderRadius;
        playerCollider.center = beforeColliderCenter;
        InitCharacterController(playerController, beforeRadius, beforeCenter);
        playerTrigger.transform.parent = playerObj.transform;
        if (otherPlayerNode && otherPlayer)
        {
            otherPlayer.transform.SetParent(otherPlayerNode.transform, true);
            var otherPlayerCtrl = otherPlayer.GetComponent<OtherPlayerCtr>();
            //牵手跟随者恢复帧同步
            otherPlayerCtrl.isAvoidFrame = false;
            // 恢复牵手跟随者的状态
            otherPlayerCtrl.SetPlayerAnimParam(false, false);
            otherPlayer = null;
        }
        playerModel.parent = playerObj.transform;
        playerObj.transform.parent = GameObject.Find("PlayerNode").transform;

        if (PlayersNode)
        {
            PlayersNode.gameObject.SetActive(false);
        }
    }

    private void InitCharacterController(CharacterController controller, float radius, Vector3 center)
    {
        controller.slopeLimit = beforeSlopeLimit;
        controller.stepOffset = beforeStepOffset;
        controller.skinWidth = radius * 0.1f;
        controller.minMoveDistance = beforeMinMoveDistance;
        controller.radius = radius;
        controller.center = center;
        controller.height = beforeHeight;
    }
}
