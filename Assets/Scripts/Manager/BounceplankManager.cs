/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/29 16:39:2
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using agora_gaming_rtc;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;

using UnityEngine.Android;
public class BounceplankManager : ManagerInstance<BounceplankManager>, IManager
{
    public List<NodeBaseBehaviour> bevs = new List<NodeBaseBehaviour>();
    private const int MaxCount = 999;
    public const string MAX_COUNT_TIP = "Up to 999 Bounceplank can be set.";
    private PlayerBaseControl player;
    public override void Release()
    {
        base.Release();
        Clear();
    }

    public void Init(GameController controller)
    {
        player =controller.playerCom;
    }

    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        var com = newBev.entity.Get<BounceplankComponent>();
        if (com != null)
        {
            var be = newBev as BounceplankBehaviour;
            be.SetMatetial(com.mat);
            AddItem(newBev);
        }
    }

    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<BounceplankBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<BounceplankBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + bevs.Count > MaxCount)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }

        return true;
    }

    public void AddItem(NodeBaseBehaviour b)
    {
        if (!bevs.Contains(b))
        {
            bevs.Add(b);
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.Bounceplank)
        {
            bevs.Remove(behaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.Bounceplank)
        {
            if (!bevs.Contains(behaviour))
            {
                bevs.Add(behaviour);
            }
        }
    }

    public void Clear()
    {
        if (bevs != null)
        {
            bevs.Clear();
        }
    }

  
    public bool IsOverMaxCount()//最大开关数量
    {
        if (bevs.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }

    public void OnBouncePlankJump(string id,string msg)
    {
        if (Player.Id != id)
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
            if (otherCtr != null)
            {
                otherCtr.animCon.PlayBounceplankJump(GetJumpAudioName(GetHeightEnum(msg)));
            }
        }

    }
    public void BouncePlankJump(GameObject obj)
    {
        BounceplankBehaviour behaviour = obj.GetComponentInParent<BounceplankBehaviour>();
        if (behaviour != null)
        {
            if (!IsFaceSide(obj.transform))
            {
                return;
            }
            behaviour.PlayJumpAnim();
            PlayerBaseControl.Inst.BounceplankJump(behaviour);
            PlayBounceplankJump(behaviour.GetHeightEnum());
        }   
    }
    public void PlayBounceplankJump(BounceHeight bounce)
    {
        player.animCon.PlayBounceplankJump(GetJumpAudioName(bounce));
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            CustomData data = new CustomData();
            data.type = (int)ChatCustomType.Bounceplank;
            data.data = bounce.ToString();

            RoomChatData roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Custom,
                data = JsonConvert.SerializeObject(data),
            };
            LoggerUtils.Log("Bounceplank SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }

    }
    public string GetJumpAudioName(BounceHeight height)
    {
        switch (height)
        {
            case BounceHeight.L:
                return "level_one";
            case BounceHeight.M:
                return "level_two";
            case BounceHeight.H:
                return "level_three";
        }
        return "level_one";

    }
    public bool IsFaceSide(Transform targetTrans)
    {
        Vec3 vec3 = player.transform.position - targetTrans.position  ;
        float angle = Vector3.Angle(vec3, targetTrans.up);
        return angle <= 90;
    }
    public BounceHeight GetHeightEnum(string bounce)
    {
        return (BounceHeight)System.Enum.Parse(typeof(BounceHeight), bounce);
    }
   

}
