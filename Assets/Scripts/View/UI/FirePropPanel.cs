using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author : Tee Li
/// 描述 ： 火焰道具调整面板
/// 时间 ： 2022/8/17 13：58
/// </summary>
public class FirePropPanel : InfoPanel<FirePropPanel>
{
    public Toggle flareTgl;
    public Slider intensitySld;
    public Text intensityTxt;
    public Toggle collisionOnTgl;
    public Toggle collisionOffTgl;

    public Toggle damageTgl;
    public Button damageAddBtn;
    public Button damageReduceBtn;
    public GameObject damageTxtGo;
    public Text damageTxt;
    public Button dmgInputBtn;

    private SceneEntity curEntity;
    private FirePropBehaviour curBehaviour;

    private Vector2 intensityRange = new Vector2(0f, 3f);
    private Vector2 intensitySldRange = new Vector2(0, 100f);
    private Vector2Int dmgRange = new Vector2Int(1,999);

    private Color enableColor = Color.white;
    private Color disableColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);

    private string turnSceneDmgHint = "Fire has been set to be damage source in settings of health point";

    private ScrollRect scrollRect;
    private float upSpeed = 500f;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        scrollRect = GetComponentInChildren<ScrollRect>();
        intensitySld.minValue = intensitySldRange.x;
        intensitySld.maxValue = intensitySldRange.y;
        AddLisitener();
    }

    private void AddLisitener()
    {
        flareTgl.onValueChanged.AddListener(OnFlareChange);
        intensitySld.onValueChanged.AddListener(OnIntensityChange);
        collisionOnTgl.onValueChanged.AddListener(OnCollisionChange);
        damageTgl.onValueChanged.AddListener(OnDoDamageChange);
        damageAddBtn.onClick.AddListener(OnDmgPlus);
        damageReduceBtn.onClick.AddListener(OnDmgMinus);
        dmgInputBtn.onClick.AddListener(OnInputClick);

        
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        curBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponent<FirePropBehaviour>();

        //只设置UI，不执行回调
        SetUiWithoutCallback(curEntity);
    }

    private void SetUiWithoutCallback(SceneEntity entity)
    {
        FirePropComponent comp = entity.Get<FirePropComponent>();

        flareTgl.SetIsOnWithoutNotify(comp.flare > 0);
        float sliderVal = DataUtils.GetProgress(comp.intensity, intensityRange.x, intensityRange.y, intensitySldRange.x, intensitySldRange.y);
        intensitySld.SetValueWithoutNotify(sliderVal);
        intensitySld.interactable = comp.flare > 0;
        intensityTxt.text = Mathf.RoundToInt(sliderVal).ToString();

        EnableSliderColor(comp.flare > 0);

        Toggle turnOnToggle = comp.collision > 0 ? collisionOnTgl : collisionOffTgl;
        turnOnToggle.SetIsOnWithoutNotify(true);

        damageTgl.SetIsOnWithoutNotify(comp.doDamage > 0);
        damageTxt.text = comp.hpDamage.ToString();
        damageTxtGo.SetActive(comp.doDamage > 0);
    }


    private void OnFlareChange(bool isOn)
    {
        curBehaviour.SetFlare(isOn);
        intensitySld.interactable = isOn;
        EnableSliderColor(isOn);

        curEntity.Get<FirePropComponent>().flare = isOn ? 1 : 0;
    }

    private void OnIntensityChange(float val)
    {

        float realVal = DataUtils.GetRealValue((int)val, intensityRange.x, intensityRange.y, intensitySldRange.x, intensitySldRange.y);
        curBehaviour.SetIntensity(realVal);
        curEntity.Get<FirePropComponent>().intensity = realVal;
        intensityTxt.text = Mathf.RoundToInt(val).ToString();

    }

    private void OnCollisionChange(bool isOn)
    {
        curBehaviour.SetCollider(isOn);
        curEntity.Get<FirePropComponent>().collision = isOn ? 1 : 0;
    }

    private void OnDoDamageChange(bool isOn)
    {

        curEntity.Get<FirePropComponent>().doDamage = isOn ? 1 : 0;
        damageTxtGo.SetActive(isOn);

        if (isOn)
        {
            scrollRect.velocity = Vector2.up * upSpeed;

            CheckTurnOnSceneDamageSrc();
        }
    }

    private void OnDmgPlus()
    {
        int dmg = curEntity.Get<FirePropComponent>().hpDamage;
        if (dmg >= dmgRange.y)
        {
            TipPanel.ShowToast("Up to " + dmgRange.y);
            return;
        }
        dmg++;
        curEntity.Get<FirePropComponent>().hpDamage = dmg;
        damageTxt.text = dmg.ToString();
    }

    private void OnDmgMinus()
    {
        int dmg = curEntity.Get<FirePropComponent>().hpDamage;
        if (dmg <= dmgRange.x)
        {
            TipPanel.ShowToast("At least " + dmgRange.x);
            return;
        }
        dmg--;
        curEntity.Get<FirePropComponent>().hpDamage = dmg;
        damageTxt.text = dmg.ToString();
    }

    private void OnInputClick()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = damageTxt.text,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = "Oops! Exceed limit:(",
            defaultText = "",
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, OnDamageEnter);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    private void OnDamageEnter(string damage)
    {
        if (string.IsNullOrEmpty(damage))
        {
            return;
        }

        bool parseDone = int.TryParse(damage, out int dmg);
        if (!parseDone)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }

        if(dmg < dmgRange.x || dmg > dmgRange.y)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }

        curEntity.Get<FirePropComponent>().hpDamage = dmg;
        damageTxt.text = dmg.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    private void EnableSliderColor(bool isOn)
    {
        Image slideBar = intensitySld.transform.Find("Background")?.GetComponent<Image>();
        if (slideBar)
        {
            slideBar.color = isOn ? enableColor : disableColor;
        }
        intensityTxt.color = isOn ? enableColor : disableColor;
    }
    

    //如果场景火焰伤害未开启，则开启
    private void CheckTurnOnSceneDamageSrc()
    {
        if (!SpawnPointPanel.Instance)
        {
            //需要创建
            SpawnPointPanel.Show();
            SpawnPointPanel.Hide();
        }
        bool stateChanged = SpawnPointPanel.Instance.TurnFireToggleOn();
        if (stateChanged)
        {
            TipPanel.ShowToast(turnSceneDmgHint);
        }
    }
}
