using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ShootWeaponCtrlPanel : BasePanel<ShootWeaponCtrlPanel>
{
    public Button btnShoot;
    public Image frontSight;
    public Transform ctrlPanel;
    public float fireRate = 0.2f;
    public float timer = -1;

    public static bool isShooting;

    private float tmp_CurTime = 0.5f;
    private float tmp_LastTime = 0f;

    private Vector3 recoilIncremental = new Vector3(0.1f, 0.1f, 0.1f);
    private Vector3 recoilMax = new Vector3(1.5f, 1.5f, 1.5f);

    [SerializeField] private EventTrigger trigger;
    [SerializeField] private Image magazineClip;

    private int curBullet;
    private int curCapacity;
    private ShootWeaponComponent curShootComp;
    private Action OnStartReload;
    private Action OnReloadComplete;
    private Color translucent = new Color(1, 1, 1, 0.5f);

    private int maximum = 1;
    private bool autoReload = true;
    private float autoReloadSpendTime = 3f;
    private float autoReloadTime = 0;
    public bool reloading = false;
    private Action reloadingACT;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        ShootWeaponManager.Inst.HideWeaponCtrPanel = Hide;
        ShootWeaponManager.Inst.ShowWeaponCtrPanel = ShowShootCtrPanel;
        OnStartReload = PlayerShootControl.Inst.OnStartReload;
        OnReloadComplete = PlayerShootControl.Inst.OnReloadComplete;

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { OnPointerDrag((PointerEventData)data); });
        trigger.triggers.Add(entry);

    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        if (PlayerShootControl.Inst != null &&
            PlayerShootControl.Inst.curShootPlayer != null &&
            PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
        {
            var baseBev = PlayerShootControl.Inst.curShootPlayer.HoldWeapon.weaponBehaviour;
            var entity = baseBev.entity;
            curShootComp = entity.Get<ShootWeaponComponent>();
            curBullet = curShootComp.curBullet;
            var hasCap = curShootComp.hasCap;
            curCapacity = (hasCap == (int)CapState.HasCap) ? curShootComp.capacity : 0;
            InitMagazineClip(curBullet, curCapacity);
        }
    }

    private void OnDisable()
    {
        timer = -1;
        OnPointerUp();
    }

    private void ShowShootCtrPanel()
    {
        if (PlayerShootControl.Inst != null &&
            PlayerShootControl.Inst.curShootPlayer != null &&
            PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
        {
            Show();
        }
    }

    public void CheckShowHide()
    {
        if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
            ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater)||
            StateManager.IsOnLadder || StateManager.IsOnSeesaw || StateManager.IsOnSwing
            ||StateManager.IsOnSlide)
        {
            Hide();
        }
    }

    private void OnShootBtnClick()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        tmp_CurTime = Time.time;
        if (tmp_CurTime - tmp_LastTime >= fireRate)
        {
            tmp_LastTime = tmp_CurTime;
            timer = 0;
            FrontSightZoomOut();
            curBullet = curShootComp.curBullet;
            SetMagazineClip(curBullet);
        }
    }

    private void FrontSightZoomOut()
    {
        var tempScale = frontSight.transform.localScale + recoilIncremental;
        var newScale = tempScale.x > recoilMax.x ? recoilMax : tempScale;
        frontSight.transform.DOKill();
        frontSight.transform.DOScale(newScale, 0.1f).SetEase(Ease.Linear);
    }
    private void FrontSightZoomIn()
    {
        var difference = frontSight.transform.localScale - Vector3.one;
        var zoomInTime = difference.x / recoilIncremental.x * 0.1f;
        frontSight.transform.DOKill();
        frontSight.transform.DOScale(Vector3.one, zoomInTime).SetEase(Ease.Linear);
    }

    public void OnPointerDown()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (reloading)
        {
            return;
        }
        PlayerShootControl.Inst.OnPointerDown();
        OnShootBtnClick();
        timer = 0;
        isShooting = true;
    }

    public void OnPointerUp()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        timer = -1;
        FrontSightZoomIn();
        PlayerShootControl.Inst.OnPointerUp();
        isShooting = false;
    }
    
    public void OnPointerDrag(PointerEventData data)
    {
    }

    private void Update()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (timer >= 0)
        {
            timer += Time.deltaTime;
        }
        if(timer >= fireRate)
        {
            OnShootBtnClick();
        }
        MagazineUpdate();
    }

    public void InitMagazineClip(int count, int max)
    {
        maximum = max;
        magazineClip.fillAmount = max > 0 ? 1 : 0;
        btnShoot.interactable = true;
        btnShoot.image.color = Color.white;
        reloading = false;
        SetMagazineClip(count, max);
    }

    public void SetMagazineClip(int count)
    {
        SetMagazineClip(count, maximum);
    }
    
    public void SetMagazineClip(int count, int max)
    {
        if (max <= 0 || reloading)
        {
            return;
        }
        magazineClip.fillAmount = (float)count/max;
        if (autoReload && count <= 0)
        {
            Reloading(null);
        }
    }

    public void Reloading(Action act)
    {
        reloadingACT = act == null ? reloadingACT : act;
        magazineClip.fillAmount = 0;
        reloadingACT = act;
        autoReloadTime = 0;
        btnShoot.interactable = false;
        btnShoot.image.color = translucent;
        OnPointerUp();
        OnStartReload?.Invoke();
        reloading = true;
    }

    public void MagazineUpdate()
    {
        if (!reloading)
        {
            return;
        }
        autoReloadTime += Time.deltaTime;
        var v = Mathf.Lerp(0, 1, autoReloadTime / autoReloadSpendTime);
        magazineClip.fillAmount = v;
        if (autoReloadTime >= autoReloadSpendTime)
        {
            reloading = false;
            btnShoot.interactable = true;
            btnShoot.image.color = Color.white;
            OnReloadComplete?.Invoke();
            reloadingACT?.Invoke();
        }
    }

    public void SetCtrlPanelVisible(bool isVisible)
    {
        ctrlPanel.gameObject.SetActive(isVisible);
    }
}
