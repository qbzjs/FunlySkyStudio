using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:LiShuZhan
/// Description:人物参照模式，可以在编辑模式里使用人物
/// Date: 2022.03.02
/// </summary>
public class ReferPanel : BasePanel<ReferPanel>
{
    public Button referBtn;
    public JoyStick joyStick;
    public Sprite onReferSprite;
    public Sprite enterReferSprite;

    const int MODEL_LAYER = 7;
    const int PLAYER_LAYER = 9;
    const int SHOTEXCLUDE_LAYER = 6;
    const int SPECIALMODEL_LAYER = 10;
    const int TOUCH_LAYER = 12;
    const int PVPAREA_LAYER = 17;
    const int ICECUBE_LAYER = 23;
    int[] layerGroup = { MODEL_LAYER, SHOTEXCLUDE_LAYER, SPECIALMODEL_LAYER, TOUCH_LAYER , PVPAREA_LAYER , ICECUBE_LAYER };
    public Transform editCam;
    public PlayerBaseControl playerCom;
    private SceneEditModeController editController;
    public bool isFarst;
    private Quaternion editComRot;
    private Vector3 playerComPos;
    private Quaternion playerComRot;
    private Vector3 airWallBorder = new Vector3(10, 10, 10);

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        referBtn.onClick.AddListener(OnReferClick);
        playerCom = GameObject.Find("GameStart").GetComponent<GameController>().playerCom;
        editController = GameObject.Find("GameStart").GetComponent<GameController>().editController;
        editCam = GameObject.Find("EditCamera").transform;
        SetPlayerPosAndRot();
        isFarst = false;
    }


    private void LateUpdate()
    {
        if (ReferManager.Inst.isRefer && !ReferManager.Inst.isHafeRefer)
        {
            var editCam = GameObject.Find("EditCamera").transform;
            if (editCam != null)
            {
                editCam.position = new Vector3(playerCom.transform.position.x, playerCom.transform.position.y - 0.95f, playerCom.transform.position.z);
            }
        }
    }

    public void OnReferMode()//进入参照模式
    {
        CloseUselessInfo();
        InputReceiver.locked = false;
        referBtn.image.sprite = onReferSprite;
        joyStick.gameObject.SetActive(true);
        editController.SetEditHandler();
        SetPlayerPosAndRot();
        
        playerCom.gameObject.SetActive(false);
     
        ReferManager.Inst.CheckSpawnPos(playerCom.transform);
        playerCom.transform.rotation = Quaternion.Euler(playerComRot.eulerAngles.x, editComRot.eulerAngles.y, playerComRot.eulerAngles.z);
        playerCom.gameObject.SetActive(true);
    }

    public void EnterReferMode()//离开参照模式，但是还显示玩家幽灵
    {
        InputReceiver.locked = false;
        referBtn.image.sprite = enterReferSprite;
        joyStick.gameObject.SetActive(false);
        SceneBuilder.Inst.SpawnPoint.SetActive(true);
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(true);
    }


    public void OnReferClick()
    {
        if (ReferManager.Inst.isRefer)
        {
            OnEnterReferInvoke();
        }
        else
        {
            OnReferInvoke();
        }
    }

    public void SetIgnoreLayer(bool isIgnore)
    {
        foreach (var item in layerGroup)
        {
            Physics.IgnoreLayerCollision(item, PLAYER_LAYER, isIgnore);
        }
    }

    public void OnEnterReferInvoke()
    {
        SetIgnoreLayer(false);
        playerCom.transform.gameObject.SetActive(false);
        ReferManager.Inst.isRefer = false;
        ReferManager.Inst.isHafeRefer = false;
        EnterReferMode();
    }

    public void OnReferInvoke()
    {
        if (BeyondAirWall()) { return; }
        //if (isFarst)
        //{
        //    playerCom.playerAnim.gameObject.SetActive(true);
        //}
        //isFarst = false;
        if (playerCom.isChanged)
        {
            SetReferPlayer();
        }
        else
        {
            isFarst = true;
            playerCom.WaitForShow();
            playerCom.InitUserInfo();
        }
    }

    private void SetPlayerPosAndRot()
    {
        editComRot = editCam.rotation;
        playerComPos = playerCom.transform.position;
        playerComRot = playerCom.transform.rotation;
    }

    private void CloseUselessInfo()
    {
        if (playerCom)
        {
            InputReceiver.locked = true;
            playerCom.Move(Vector3.zero);
            playerCom.isFastRun = false;
            joyStick.JoystickReset();
            InputReceiver.locked = false;
        }
    }

    private bool BeyondAirWall()
    {
        var airWall = GameObject.Find("Scene").transform.Find("Terrain").Find("AirWall");
        var boundbox = DataUtils.CalculateBoundingBox(airWall);
        boundbox.size -= airWallBorder;
        if (!boundbox.Contains(editCam.position))
        {
            TipPanel.ShowToast("You have reached the end:)");
            return true;
        }
        return false;
    }

    public void SetReferPlayer()
    {
        playerCom.transform.gameObject.SetActive(true);
        playerCom.playerAnim.gameObject.SetActive(true);
        ReferManager.Inst.isRefer = true;
        ReferManager.Inst.isHafeRefer = false;
        SetIgnoreLayer(true);
        OnReferMode();
    }
    //当保存地图时，隐藏参照模式
    public void OnSaveMapCloseRefer()
    {
        playerCom.transform.gameObject.SetActive(false);
        ReferManager.Inst.isReferPlay = true;
        Hide();
    }
    //在panel被删除时重置玩家层级碰撞检测
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SetIgnoreLayer(false);
    }
}
