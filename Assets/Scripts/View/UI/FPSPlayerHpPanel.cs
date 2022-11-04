/// <summary>
/// Author:Mingo-LiZongMing
/// Description:第一人称下的玩家生命值显示面板
/// Date: 2022-5-17 17:44:22
/// </summary>
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FPSPlayerHpPanel : BasePanel<FPSPlayerHpPanel>
{
    public Text txtUserName;
    public RawImage profile;
    public Image hpBar;
    public GameObject hpPanel;

    private float maxHP = 100;
    private float maxBar;
    
    [SerializeField] private CanvasGroup hitCG;
    [SerializeField] private Image hitImage;
    private bool isHit = false;
    private Color hitRed = Color.red;
    private float[] hitTime = new[] {0, 0.1f, 0.7f, 0.2f};
    private float[] hitAlpha = new[] {0f, 1f, 1f, 0f};
    private float[] hitAlphaRed = new[] {1f, 0f, 0f, 1f};
    private float Redpercent = 0.2f;
    private bool isRed = false;

    private float hurtTime = 0;
    private int hurtIndex = 1;
    private string userName, portraitUrl;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        maxBar = hpBar.rectTransform.sizeDelta.x;
        InitHitAction();
        userName = GetPlayerUserName();
        portraitUrl = GameManager.Inst.ugcUserInfo.portraitUrl;
        UpdatePlayerInfo();
    }

    public void UpdatePlayerInfo()
    {
        userName = GetPlayerUserName();
        portraitUrl = GameManager.Inst.ugcUserInfo.portraitUrl;
        txtUserName.text = userName;
        if (string.IsNullOrEmpty(portraitUrl))
        {
            return;
        }
        CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTexture2D(portraitUrl,
        (tex) =>
        {
            profile.texture = tex;
        },
        (error) =>
        {
            LoggerUtils.Log("LoadTextureError");
        }));
        InitHitAction();
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        maxHP = SceneParser.Inst.GetCustomHP(GameManager.Inst.ugcUserInfo.uid);
        var selfCon = PlayerBaseControl.Inst.GetComponentInChildren<CharBattleControl>(true);
        if (selfCon == null)
        {
            selfCon = PlayerBaseControl.Inst.gameObject.AddComponent<CharBattleControl>();
        }
        var curHP = selfCon.GetCurHp();
        SetBlood(curHP);
        if (string.IsNullOrEmpty(userName))
        {
            UpdatePlayerInfo();
        }
    }

    public void SetBlood(float value)
    {
        var percent = value / maxHP;
        SetValue(percent);
    }

    public void SetValue(float percent)
    {
        if (!isRed && percent <= Redpercent)
        {
            isHit = false;
            hitImage.color = hitRed;
            hitCG.alpha = 1;
        }

        if (isRed && percent > Redpercent)
        {
            hitImage.color = Color.white;
            hitCG.alpha = hitCG.alpha == 1 ? 0 : hitCG.alpha;
        }
        isRed = percent <= Redpercent;
        
        Vector2 barTar = hpBar.rectTransform.sizeDelta;
        barTar.x = percent * maxBar;
        Tween tweener2 = DOTween.To(() => hpBar.rectTransform.sizeDelta, x => hpBar.rectTransform.sizeDelta = x, barTar, 0.6f).SetEase(Ease.OutQuart);
        var isActive = PlayModePanel.Instance && PlayModePanel.Instance.isTps;
        hitImage.gameObject.SetActive(!isActive);
    }
    
    public void Hit()
    {
        if (isRed)
        {
            return;
        }
        isHit = true;
        hitCG.alpha = isRed?1:0;
        hurtTime = 0;
        hurtIndex = 1;
        hitImage.color = isRed ? hitRed : Color.white;
    }
    
    private void HurtUpdate()
    {
        if (!isHit)
        {
            return;
        }

        var ha = isRed ? hitAlphaRed : hitAlpha;
        var t = hitTime[hurtIndex];
        var v = Mathf.Lerp(ha[hurtIndex-1], ha[hurtIndex], hurtTime/t);
        hurtTime += Time.deltaTime;
        hitCG.alpha = v;
        
        if (hurtTime >= t)
        {
            hurtIndex++;
            hurtTime -= t;
        }

        if (hurtIndex >= hitTime.Length)
        {
            isHit = false;
            hitCG.alpha = isRed?1:0;
        }
    }

    private void InitHitAction()
    {
        ShootWeaponManager.Inst.OnHitUiEffect = Hit;
        AttackWeaponManager.Inst.OnHitUiEffect = Hit;
    }

    private void Update()
    {
        HurtUpdate();
    }

    private void OnDisable()
    {
        isHit = false;
        hitCG.alpha = 0;
    }

    private string GetPlayerUserName()
    {
        var userName = GameManager.Inst.ugcUserInfo.userName;
        if (userName.Length > 10)
        {
            var subUserName = userName.Substring(0, 9) + "...";
            return subUserName;
        }
        else
        {
            return userName;
        }
    }

    public void SetHpPanelVisible(bool isVisible)
    {
        hpPanel.gameObject.SetActive(isVisible);
    }
}
