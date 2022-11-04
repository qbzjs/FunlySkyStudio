/// <summary>
/// Author:LiShuZhan
/// Description:射击道具控制UI
/// Date: 2022-4-24 18:22:22
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootWeaponPanel : WeaponBasePanel<ShootWeaponPanel>
{
    public Button bPBtn;
    public Image isSelectBP;
    public GameObject bulletPrefab;
    public Button editBtn;
    public Text BulletPointText;
    public Toggle capacityToggle;
    public GameObject setCapacityPanel;
    public Text txtCapacityCount;
    public Button subCapBtn;
    public Button addCapBtn;
    public Text txtDamage;
    public Button addDmgBtn;
    public Button subDmgBtn;
    public GameObject buttonParent;
    public GameObject buttonPrefab;
    public Button inputCapBtn;
    public Button inputDmgBtn;

    public RectTransform PVPLayScollRect;

    private SceneEntity curEntity;
    private const int MaxDamage = 999;
    private const int MinDamage = 1;
    private const int DefaultCap = 30;
    private const int MaxCap = 999;
    private const int MinCap = 1;
    private List<CommonButtonItem> FireRateBtns;

    private Dictionary<int, string> FireRateDic = new Dictionary<int, string>()
    {
         { 0, "Slow"},
         { 1, "Medium"},
         { 2, "Fast"},
    };

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        bPBtn.onClick.AddListener(OnBulletPointBtnClick);
        editBtn.onClick.AddListener(OnEditClick);
        capacityToggle.onValueChanged.AddListener(OnCapacityToggleChange);
        subCapBtn.onClick.AddListener(() => { OnCapacityCountChange(false); });
        addCapBtn.onClick.AddListener(() => { OnCapacityCountChange(true); });
        subDmgBtn.onClick.AddListener(() => { OnDamageChange(false); });
        addDmgBtn.onClick.AddListener(() => { OnDamageChange(true); });
        inputCapBtn.onClick.AddListener(OnShowCapKeyBoard);
        inputDmgBtn.onClick.AddListener(OnShowDmgKeyBoard);
        InitFireRateBtns();
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        RefreshUI();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (curBehav is ShootWeaponDefaultBehaviour)
        {
            //默认占位武器道具
            ChooseItem(string.Empty);
        }
        else if (curBehav is UGCCombBehaviour && entity.HasComponent<ShootWeaponComponent>())
        {
            //UGC武器道具
            string rId = entity.Get<ShootWeaponComponent>().rId;
            ChooseItem(rId);
        }

        if (entity.HasComponent<ShootWeaponComponent>())
        {
            var comp = curEntity.Get<ShootWeaponComponent>();
            var isSelect = comp.isCustomPoint == (int)CustomPointState.Off ? false : true;
            SetBulletPointState(isSelect);
            SetBulletPointColor(comp);
            InitPanelByComp(comp);
        }
    }

    protected override List<string> GetAllUgcWeaponRidList()
    {
        return ShootWeaponManager.Inst.GetAllUgcWeaponRidList();
    }

    protected override void AddDataToWeaponManager(NodeBaseBehaviour nBehav, string rId)
    {
        ShootWeaponManager.Inst.AddWeaponComponent(nBehav, rId);
        ShootWeaponManager.Inst.AddUgcWeaponItem(rId, nBehav);
    }

    protected override void SetLastChooseWeapon(UgcWeaponItem weaponItem)
    {
        ShootWeaponManager.Inst.SetLastSelectWeapon(weaponItem);
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

    #region BulletShotPoint
    private void OnBulletPointBtnClick()
    {
        if (!curEntity.HasComponent<ShootWeaponComponent>())
        {
            return;
        }
        var comp = curEntity.Get<ShootWeaponComponent>();
        if (comp.rId == ShootWeaponManager.DEFAULT_MODEL)
        {
            TipPanel.ShowToast("Please set the object first");
            return;
        }
        if(comp.isCustomPoint == (int)CustomPointState.Off)
        {
            SetBulletPointState(true);
            comp.isCustomPoint = (int)CustomPointState.On;
            OnEditClick();
        }
        else
        {
            SetBulletPointState(false);
            comp.isCustomPoint = (int)CustomPointState.Off;
            ShootWeaponManager.Inst.SetAnchors(curEntity, Vector3.zero);
        }
    }

    private void OnEditClick()
    {
        BulletAnchorsPanel.Show();
        var pos = ShootWeaponManager.Inst.GetAnchors(curEntity);
        BulletAnchorsPanel.Instance.Init(curEntity, pos);
    }

    private void SetBulletPointState(bool isSelect)
    {
        isSelectBP.gameObject.SetActive(isSelect);
        editBtn.gameObject.SetActive(isSelect);
    }

    private void SetBulletPointColor(ShootWeaponComponent comp)
    {
        float value = comp.rId == ShootWeaponManager.DEFAULT_MODEL ? 0.6f : 1f;
        BulletPointText.canvasRenderer.SetAlpha(value);
        bPBtn.image.canvasRenderer.SetAlpha(value);
    }
    #endregion

    private void InitPanelByComp(ShootWeaponComponent comp)
    {
        InitCapacityData(comp);
        InitAttackDamageData(comp);
        InitFireRateData(comp);
    }

    private void InitAttackDamageData(ShootWeaponComponent comp)
    {
        var damage = comp.damage;
        txtDamage.text = damage.ToString();
    }

    private void InitCapacityData(ShootWeaponComponent comp)
    {
        bool hasCap = comp.hasCap == (int)CapState.HasCap;
        capacityToggle.isOn = hasCap;
        OnCapacityToggleChange(hasCap);
    }

    private void InitFireRateBtns()
    {
         FireRateBtns = new List<CommonButtonItem>();
         for (int i = 0; i < FireRateDic.Count; i++) {
            var item = Instantiate(buttonPrefab);
            item.transform.SetParent(buttonParent.transform);
            CommonButtonItem itemScript = null;
            itemScript = item.GetComponent<CommonButtonItem>();
            itemScript.Init();
            FireRateBtns.Add(itemScript);
            itemScript.SetText(FireRateDic[i]);
            int index = i;
            itemScript.AddClick(() => OnFireRateBtnsClick(index));
            var rectComp = itemScript.GetComponent<RectTransform>();
            rectComp.localScale = Vector3.one;
            rectComp.anchoredPosition3D = new Vector3(rectComp.anchoredPosition3D.x, rectComp.anchoredPosition3D.y, 0);
        }
    }

    private void InitFireRateData(ShootWeaponComponent comp)
    {
        OnFireRateBtnsClick(comp.fireRate);
    }

    private void OnFireRateBtnsClick(int fireRate)
    {
        FireRateBtns.ForEach(x =>
        {
            x.SetSelectState(false);
        });
        FireRateBtns[fireRate].SetSelectState(true);
        var shootComp = curEntity.Get<ShootWeaponComponent>();
        shootComp.fireRate = fireRate;
    }

    private void SetCapacityText(int capCount)
    {
        txtCapacityCount.text = capCount.ToString();
    }

    private void OnCapacityToggleChange(bool value)
    {
        setCapacityPanel.SetActive(value);
        var shootComp = curEntity.Get<ShootWeaponComponent>();
        if (value)
        {
            var hasCap = shootComp.hasCap;
            var curCap = (hasCap == (int)CapState.HasCap) ? shootComp.capacity : DefaultCap;
            shootComp.capacity = curCap;
            shootComp.curBullet = shootComp.capacity;
            shootComp.hasCap = (int)CapState.HasCap;
            SetCapacityText(curCap);
        }
        else
        {
            shootComp.hasCap = (int)CapState.NoCap;
        }
        UpdateSwitchPanel();
    }

    private void OnCapacityCountChange(bool isAdd)
    {
        if (curEntity != null && curEntity.HasComponent<ShootWeaponComponent>())
        {
            var shootComp = curEntity.Get<ShootWeaponComponent>();
            var curapacity = shootComp.capacity;
            if (isAdd)
            {
                if (curapacity < MaxCap)
                {
                    shootComp.capacity++;
                }
                else{
                    TipPanel.ShowToast("Up to {0}", 999);
                }
            }
            else
            {
                if (curapacity > MinCap)
                {
                    shootComp.capacity--;
                }
                else
                {
                    TipPanel.ShowToast("At least {0}", 1);
                }
            }
            shootComp.curBullet = shootComp.capacity;
            SetCapacityText(shootComp.capacity);
        }
    }

    private void OnDamageChange(bool isAdd)
    {
        if(curEntity != null && curEntity.HasComponent<ShootWeaponComponent>())
        {
            var shootComp = curEntity.Get<ShootWeaponComponent>();
            var curDamage = shootComp.damage;
            if (isAdd)
            {
                if (curDamage < MaxDamage)
                {
                    shootComp.damage++;
                }
                else
                {
                    TipPanel.ShowToast("Up to {0}", 999);
                }
            }
            else
            {
                if (curDamage > MinDamage)
                {
                    shootComp.damage--;
                }
                else
                {
                    TipPanel.ShowToast("At least {0}", 1);
                }
            }
            InitAttackDamageData(shootComp);
        }
    }

    private void UpdateSwitchPanel()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(PVPLayScollRect);
    }

    private void OnShowCapKeyBoard()
    {
        if (curEntity != null && curEntity.HasComponent<ShootWeaponComponent>())
        {
            var shootComp = curEntity.Get<ShootWeaponComponent>();
            var curCapacity = shootComp.capacity;

            KeyBoardInfo keyBoardInfo = new KeyBoardInfo
            {
                type = 0,
                placeHolder = curCapacity.ToString(),
                inputMode = 1,
                maxLength = 200,
                inputFlag = 0,
                lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
                defaultText = "",
                returnKeyType = (int)ReturnType.Return
            };
            MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowCapKeyBoard);
            MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
        }
    }

    private void ShowCapKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int pointNum;
        if (!int.TryParse(str, out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if(pointNum > 999)
        {
            TipPanel.ShowToast("Up to {0}", 999);
            return;
        }
        if(pointNum < 1)
        {
            TipPanel.ShowToast("At least {0}", 1);
            return;
        }
        var shootComp = curEntity.Get<ShootWeaponComponent>();
        shootComp.capacity = pointNum;
        shootComp.curBullet = shootComp.capacity;
        SetCapacityText(shootComp.capacity);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    private void OnShowDmgKeyBoard()
    {
        if (curEntity != null && curEntity.HasComponent<ShootWeaponComponent>())
        {
            var shootComp = curEntity.Get<ShootWeaponComponent>();
            var curDmage = shootComp.damage;

            KeyBoardInfo keyBoardInfo = new KeyBoardInfo
            {
                type = 0,
                placeHolder = curDmage.ToString(),
                inputMode = 1,
                maxLength = 200,
                inputFlag = 0,
                lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
                defaultText = "",
                returnKeyType = (int)ReturnType.Return
            };
            MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowDmgKeyBoard);
            MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
        }
    }

    private void ShowDmgKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int pointNum;
        if (!int.TryParse(str, out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (pointNum > 999)
        {
            TipPanel.ShowToast("Up to {0}", 999);
            return;
        }
        if (pointNum < 1)
        {
            TipPanel.ShowToast("At least {0}", 1);
            return;
        }
        var shootComp = curEntity.Get<ShootWeaponComponent>();
        shootComp.damage = pointNum;
        InitAttackDamageData(shootComp);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }
}
