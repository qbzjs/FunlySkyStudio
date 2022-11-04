using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using BudEngine.NetEngine;

/// <summary>
/// Author: 熊昭
/// Description: 陷阱盒道具行为功能类
/// Date: 2021-11-26 14:50:28
/// </summary>
public class TrapBoxBehaviour : NodeBaseBehaviour
{
    private GameObject boxGO;
    private MeshRenderer bRenderer;
    private Color[] oldColor;

    private MeshRenderer tRenderer;
    private TextMeshPro textMesh;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        boxGO = transform.GetChild(0).gameObject;
        bRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        tRenderer = transform.GetChild(1).GetComponent<MeshRenderer>();
        if (textMesh == null)
        {
            textMesh = this.GetComponentInChildren<TextMeshPro>(true);
        }
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void OnReset()
    {
        base.OnReset();
        textMesh.text = "";
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public void RefreshShowId()
    {
        var tComp = entity.Get<TrapBoxComponent>();
        textMesh.text = tComp.tId.ToString();
        SetTextVisiable(tComp.rePos == (int)TrapBoxTrans.CustomSpawn);
    }

    public void SetBoxVisiable(bool state)
    {
        bRenderer.enabled = state;
    }

    public void SetTextVisiable(bool state)
    {
        tRenderer.enabled = state;
    }

    private void OnChangeMode(GameMode mode)
    {
        var tComp = entity.Get<TrapBoxComponent>();
        if (mode == GameMode.Edit)
        {
            bRenderer.enabled = true;
            SetTextVisiable(tComp.rePos == (int)TrapBoxTrans.CustomSpawn);
        }
        else
        {
            bRenderer.enabled = false;
            SetTextVisiable(false);
        }
    }

    public override void OnTrigEnter()
    {
        base.OnTrigEnter();

        //在游戏等待区域，不能交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart ||PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            return;
        }
        //without trigger protected
        if (ReferManager.Inst.isRefer)
        {
            return;
        }

        if (StateManager.IsParachuteUsing)
        {
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
        //打断钓鱼
        if (StateManager.IsFishing)
        {
            FishingManager.Inst.ForceStopFishing();
        }
        //本地预测展示
        TrapSpawnManager.Inst.HandleLocalShow(this);
        var tComp = entity.Get<TrapBoxComponent>();
        if(tComp.rePos == (int)TrapBoxTrans.NoTrans)
        {
            TouchTrap();
        }
        else
        {
            BlackPanel.Show();
            BlackPanel.Instance.PlayTransitionAnimAct(TouchTrap);
        }
    }

    private void TouchTrap()
    {
        var tComp = entity.Get<TrapBoxComponent>();
        int tId = tComp.tId;
        int rePos = tComp.rePos;

        int reTex = tComp.reTex;
        string text = tComp.text;
        bool isDefault = reTex == 0 || string.IsNullOrEmpty(text);

        
        TouchTrapToast(text, isDefault);
        // TouchTrapChat(text, isDefault);
        if(tComp.rePos != (int)TrapBoxTrans.NoTrans)
        {
            TouchTrapTransport(tId, rePos);
        }

         //TODO:暂时修改陷阱盒和磁力板不受伤
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if(TrapSpawnManager.Inst.IsCanHurt() == true)
            {
                TrapSpawnManager.Inst.SendRequest(this);
            }
        }   
    }

    private void TouchTrapTransport(int tId, int rePos)
    {
        MessageHelper.Broadcast(MessageName.PosMove, true);
        if (rePos == (int)TrapBoxTrans.MapSpawn) //回到出生点
        {
            SteeringWheelBehaviour sw = null;
            if (PlayerDriveControl.Inst)
            {
                sw = PlayerDriveControl.Inst.steeringWheel;
            }
            if (sw)
            {
                var trs = SpawnPointManager.Inst.GetSpawnPoint().transform;
                sw.carRgb.transform.SetPositionAndRotation(trs.localPosition, trs.localRotation);
                SteeringWheelManager.Inst.ResetPlayerLookSteering();
            }
            else
            {
                PlayerBaseControl.Inst.SetPosToSpawnPoint();
            }
            LoggerUtils.Log("TouchTrap -- SetPosToBornPoint");
        }
        else if(rePos == (int)TrapBoxTrans.CustomSpawn) //自定义传送点
        {
            var point = TrapSpawnManager.Inst.GetPointGo(tId);
            if (point == null)
            {
                return;
            }
            if (point.Get<GameObjectComponent>().bindGo != null)
            {
                Transform pointTransform = point.Get<GameObjectComponent>().bindGo.transform;

                SteeringWheelBehaviour sw = null;
                if (PlayerDriveControl.Inst)
                {
                    sw = PlayerDriveControl.Inst.steeringWheel;
                }
                if (sw)
                {
                    sw.carRgb.transform.SetPositionAndRotation(pointTransform.position, pointTransform.rotation);
                    SteeringWheelManager.Inst.ResetPlayerLookSteering();
                }
                else
                {
                    PlayerBaseControl.Inst.SetPosToNewPoint(pointTransform.position, pointTransform.rotation);
                }
                LoggerUtils.Log("TouchTrap -- SetPosToCustomPoint");
            }
        }
        PlayerBaseControl.Inst.ResetUpwardVec();
    }

    private void TouchTrapToast(string text, bool isDefault)
    {
        string defText = LocalizationConManager.Inst.GetLocalizedText("Oops! You triggered a trap!");
        string toast = isDefault ? defText : text;
        TipPanel.ShowToast("{0}", toast);
    }

    private void TouchTrapChat(string text, bool isDefault)
    {
        if (PlayerBaseControl.Inst.curGameMode == GameMode.Guest)
        {
            string content = isDefault ? "" : text;
            RoomChatData roomchatdata = new RoomChatData()
            {
                //msgType = (int)RecChatType.HitTrap,
                data = content
            };
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomchatdata));
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, boxGO, ref oldColor);
    }
}
