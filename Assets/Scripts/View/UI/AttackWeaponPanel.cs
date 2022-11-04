using System.Collections.Generic;
using SavingData;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:攻击道具控制UI
/// Date: 2022-4-14 17:44:22
/// </summary>
public class AttackWeaponPanel : WeaponBasePanel<AttackWeaponPanel>
{
    public Button btnPlusAttack;
    public Button btnSubAttack;
    public Button InputAttackBtn;
    public Text AttackText;
    private float damage = 20;
    public Button btnPlusHits;
    public Button btnSubHits;
    public Button InputHitsBtn;
    public Toggle ToggleHitsPermission;
    public Text HitsText;
    public GameObject HitsAdjust;
    public ScrollRect scrollRect;
    public Text HitsTitle;
    private float hits = 20;
    private const float defaultHits = 20;
    private const int MAX_VALUE = 999;
    //当前选择操作的攻击武器
    private AttackWeaponComponent curAttackWeapon;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnPlusAttack.onClick.AddListener(() =>
        {
            OnChangeAttack(true);
        });
        btnSubAttack.onClick.AddListener(() =>
        {
            OnChangeAttack(false);
        });

        InputAttackBtn.onClick.AddListener(() =>
        {
            OnShowKeyBoard(true);
        });
        InputHitsBtn.onClick.AddListener(() =>
        {
            OnShowKeyBoard(false);
        });
        btnPlusHits.onClick.AddListener(() =>
        {
            OnChangeHits(true);
        });

        btnSubHits.onClick.AddListener(() =>
        {
            OnChangeHits(false);
        });
        ToggleHitsPermission.onValueChanged.AddListener(OnToggleClick);

        UpdateWeaponInfo();
    }

    public void SetEntity(SceneEntity entity)
    {
        RefreshUI();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (curBehav is AttackWeaponDefaultBehaviour)
        {
            //默认占位武器道具
            ChooseItem(string.Empty);
        }
        else if (curBehav is UGCCombBehaviour && entity.HasComponent<AttackWeaponComponent>())
        {
            //UGC武器道具
            curAttackWeapon = entity.Get<AttackWeaponComponent>();
            string rId = curAttackWeapon.rId;
            ChooseItem(rId);
        }
        UpdateWeaponInfo();
    }
    
    protected override List<string> GetAllUgcWeaponRidList()
    {
        return AttackWeaponManager.Inst.GetAllUgcWeaponRidList();
    }

    protected override void AddDataToWeaponManager(NodeBaseBehaviour nBehav,string rId)
    {
        AttackWeaponManager.Inst.AddWeaponComponent(nBehav, rId);
        AttackWeaponManager.Inst.AddUgcWeaponItem(rId, nBehav);
    }

    protected override void SetLastChooseWeapon(UgcWeaponItem weaponItem)
    {
        AttackWeaponManager.Inst.SetLastSelectWeapon(weaponItem);
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

    private void UpdateWeaponInfo()
    {
        if (curAttackWeapon != null)
        {
            damage = curAttackWeapon.damage;
            hits = curAttackWeapon.hits;
            ToggleHitsPermission.isOn = curAttackWeapon.openDurability == 1;
        }
        //兼容之前没有耐力值的攻击道具
        if (hits <= 0)
        {
            hits = defaultHits;
        }
        AttackText.text = damage.ToString();
        HitsText.text = hits.ToString();
        HitsAdjust.SetActive(ToggleHitsPermission.isOn);
    }

    private void SetWeaponInfo()
    {
        if (curAttackWeapon != null)
        {
            curAttackWeapon.damage = damage;
            curAttackWeapon.hits = hits;
            var openDurability = ToggleHitsPermission.isOn ? 1 : 0;
            curAttackWeapon.openDurability = openDurability;
        }
    }

    private void ResetPropDurability()
    {
        hits = defaultHits;
        HitsText.text = hits.ToString();
        SetWeaponInfo();
    }

    private void OnChangeAttack(bool isAdd)
    {
        if (isAdd)
        {
            if (damage >= MAX_VALUE)
            {
                TipPanel.ShowToast("Up to {0}", 999);
                return;
            }
            damage += 1;
        }
        else
        {
            if (damage <= 1)
            {
                TipPanel.ShowToast("At least {0}", 1);
                return;
            }
            damage -= 1;
        }
        AttackText.text = damage.ToString();
        SetWeaponInfo();
    }

    private void OnToggleClick(bool isToggle)
    {
        HitsAdjust.SetActive(isToggle);
        if (isToggle)
        {
            //列表滑动到底部，显示自定义血量按钮
            scrollRect.verticalNormalizedPosition = 0;
        }
        SetWeaponInfo();
        if (isToggle == false)
        {
            //重置耐力值
            ResetPropDurability();
        }
    }


    private void OnChangeHits(bool isAdd)
    {
        if (isAdd)
        {
            if (hits >= MAX_VALUE)
            {
                TipPanel.ShowToast("Up to {0}", 999);
                return;
            }
            hits += 1;
        }
        else
        {
            if (hits <= 1)
            {
                TipPanel.ShowToast("At least {0}", 1);
                return;
            }
            hits -= 1;
        }
        HitsText.text = hits.ToString();
        SetWeaponInfo();
    }

    private void OnShowKeyBoard(bool isAttack)
    {
        string placeHolderStr = damage.ToString();
        if (!isAttack)
        {
            placeHolderStr = hits.ToString();
        }
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = placeHolderStr,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = "Oops! Exceed limit:(",
            defaultText = "",
            returnKeyType = (int)ReturnType.Return
        };
        if (isAttack)
        {
            MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowAttackInput);
        }
        else
        {
            MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowHitsInput);
        }

        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    public void ShowAttackInput(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int num;
        if (!int.TryParse(str, out num))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (num > MAX_VALUE || num <= 0)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        damage = num;
        AttackText.text = damage.ToString();
        SetWeaponInfo();

        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    public void ShowHitsInput(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int num;
        if (!int.TryParse(str, out num))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (num > MAX_VALUE || num <= 0)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        hits = num;
        HitsText.text = hits.ToString();
        SetWeaponInfo();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

}