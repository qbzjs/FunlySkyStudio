using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SavingData;

/// <summary>
/// Author:WenJia
/// Description:回血道具控制UI
/// Date: 2022/5/19 14:10:14
/// </summary>


public class BloodPropPanel : WeaponBasePanel<BloodPropPanel>
{
    public Text restoreText;
    public Button InputBtn;
    public Button btnPlus;
    public Button btnSub;
    private int restore = 15;
    private SceneEntity curEntity;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        InputBtn.onClick.AddListener(OnShowKeyBoard);
        btnPlus.onClick.AddListener(OnBtnPlusClick);
        btnSub.onClick.AddListener(OnBtnSubClick);
        restoreText.text = restore.ToString();
    }

    private void OnBtnPlusClick()
    {
        if (restore >= 100)
        {
            TipPanel.ShowToast("Up to {0}", 100);
            return;
        }
        restore += 1;
        curEntity.Get<BloodPropComponent>().restore = restore;
        restoreText.text = restore.ToString();
    }

    private void OnBtnSubClick()
    {
        if (restore <= 1)
        {
            TipPanel.ShowToast("At least {0}", 1);
            return;
        }
        restore -= 1;
        curEntity.Get<BloodPropComponent>().restore = restore;
        restoreText.text = restore.ToString();
    }

    private void OnShowKeyBoard()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = restore.ToString(),
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = "Oops! Exceed limit:(",
            defaultText = "",
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    public void ShowKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int restoreNum;
        if (!int.TryParse(str, out restoreNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (restoreNum > 100 || restoreNum <= 0)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        restore = restoreNum;
        curEntity.Get<BloodPropComponent>().restore = restore;
        restoreText.text = restore.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        RefreshUI();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (curBehav is BloodPropBehaviour)
        {
            //默认占位回血道具
            ChooseItem(string.Empty);
        }
        else if (curBehav is UGCCombBehaviour && entity.HasComponent<BloodPropComponent>())
        {
            //UGC回血道具
            string rId = entity.Get<BloodPropComponent>().rId;
            ChooseItem(rId);
        }
        restore = (int)entity.Get<BloodPropComponent>().restore;
        restoreText.text = restore.ToString();
    }

    /// <summary>
    /// 选择了UGC素材后回调
    /// </summary>
    protected override void ChoosePropCallback(MapInfo mapInfo)
    {
        LoggerUtils.Log($"{WEAPON_LOG} ChoosePropCallback {mapInfo.mapId}");
        var oldPos = curBehav.transform.position;
        var lastPos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
        SecondCachePool.Inst.DestroyEntity(curBehav.gameObject);
        CreateWeaponItemUI(mapInfo);
        CreateUgcPropInScene(mapInfo, lastPos);

        RefreshUI();
    }

    /// <summary>
    /// 选择下发武器Panel的ugcItem
    /// </summary>
    public override void OnWeaponItemClick(UgcWeaponItem weaponItem)
    {
        var oldPos = curBehav.transform.position;
        var lastPos = new Vector3(oldPos.x, oldPos.y, oldPos.z);
        SceneBuilder.Inst.DestroyEntity(curBehav.gameObject);
        CreateUgcPropInScene(weaponItem.mapInfo, lastPos, weaponItem);
        ChooseItem(weaponItem.mapInfo.mapId);
        RefreshUI();
    }

    /// <summary>
    /// 创建UGC武器，分别在从背包选择Ugc和点击panel下发item后触发
    /// </summary>
    protected void CreateUgcPropInScene(MapInfo mapInfo, Vector3 pos, UgcWeaponItem weaponItem = null)
    {
        var resItemDataDic = ResBagManager.Inst.resItemDataDic;
        var rId = "";
        var mapJsonContentStr = "";

        if (resItemDataDic != null && resItemDataDic.ContainsKey(mapInfo.mapId))
        {
            rId = resItemDataDic[mapInfo.mapId].mapInfo.mapId;
            mapJsonContentStr = resItemDataDic[mapInfo.mapId].mapJsonContent;
        }
        else if (weaponItem != null)
        {
            rId = weaponItem.mapInfo.mapId;
            mapJsonContentStr = weaponItem.mapJsonContent;
        }

        var nBehav = BloodPropCreateUtils.Inst.CreateSingleUgcWeapon(pos, mapInfo, rId, mapJsonContentStr);
        if (nBehav)
        {
            AddDataToWeaponManager(nBehav, rId);
            if(PackPanel.Instance != null && PackPanel.Instance.gameObject.activeSelf)
            {
                MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
                return;
            }
        }

        EditModeController.SetSelect?.Invoke(nBehav.entity);
        LoggerUtils.Log($"BloodPropPanel CreateUgcWeaponInScene {mapInfo.mapId}");
    }

    protected override List<string> GetAllUgcWeaponRidList()
    {
        return BloodPropManager.Inst.GetAllUgcWeaponRidList();
    }

    protected override void AddDataToWeaponManager(NodeBaseBehaviour nBehav, string rId)
    {
        BloodPropManager.Inst.AddBloodPropComponent(nBehav, rId);
        BloodPropManager.Inst.AddUgcWeaponItem(rId, nBehav);
    }

    protected override void SetLastChooseWeapon(UgcWeaponItem weaponItem)
    {
        BloodPropManager.Inst.SetLastSelectWeapon(weaponItem);
    }

    protected override void RefreshUI()
    {
        base.RefreshUI();
        var allUsedUgcs = GetAllUgcWeaponRidList();
        if (allUsedUgcs != null)
        {
            goNoPanel.SetActive(allUsedUgcs.Count > 0 == false);
            goHasPanel.SetActive(allUsedUgcs.Count > 0);
        }
    }
}
