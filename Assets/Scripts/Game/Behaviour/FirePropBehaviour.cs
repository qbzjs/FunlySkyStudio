using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using BudEngine.NetEngine;
using HLODSystem;
using UnityEngine;

public class FirePropBehaviour : BaseHLODBehaviour
{
    private Light pointLight;
    private Collider collid;
    private ParticleSystem particle;

    private Color defaultCol = Color.white;
    private Color highlightCol = new Color(1f, 0.5f, 0.5f);


    public override void SetUp()
    {
        InitRes();
        FirePropComponent comp = entity.Get<FirePropComponent>();
        SetFlare(comp.flare > 0);
        SetIntensity(comp.intensity);
        SetCollider(comp.collision > 0);
        SetLightRange(comp.lightRange);
    }
    private void InitRes()
    {
        pointLight = assetObj.GetComponentInChildren<Light>();
        var boxCol = assetObj.transform.Find("BoxCollider");
        if(boxCol != null)
        {
            collid = boxCol.GetComponent<Collider>();
        }
        var fire = assetObj.transform.Find("fire");
        if(fire != null)
        {
            fire = fire.Find("subject");
            if(fire != null)
            {
                fire = fire.Find("fire");
                if(fire != null)
                {
                    particle = fire.GetComponent<ParticleSystem>();
                }
            }
        }
    }
    
    public void SetCollider(bool isOn)
    {
        collid.enabled = isOn;
    }

    public void SetIntensity(float inten)
    {
        pointLight.intensity = inten;
    }

    public void SetFlare(bool isOn)
    {
        pointLight.enabled = isOn;
    }
    public override void OnTrigEnter()
    {
        LoggerUtils.Log($"FirePropBehaviour  OnTrigEnter");
        //在游戏等待区域，不能交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart || PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            return;
        }
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        if (!FirePropManager.Inst.IsCanHurt())
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            FirePropComponent compt = entity.Get<FirePropComponent>();
            bool isCanAnimation = compt.doDamage == 1 
                && FirePropManager.Inst.IsOpenDamage() 
                && !PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move);
            FirePropManager.Inst.EditorBurn(Player.Id, compt.hpDamage, compt.collision == 1, isCanAnimation);
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (FirePropManager.Inst.IsCanHurt())
            {
                FirePropManager.Inst.ReqFireDamge(entity);
            }
        }
    }

    public void SetLightRange(float rg)
    {
        if (pointLight)
        {
            pointLight.range = rg;
        }      
    }

    public void PlaySound()
    {
        AKSoundManager.Inst.PlayAttackSound("", "Play_Fire_Flame_Loop", "", gameObject);
    }

    public void StopSound()
    {
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Fire_Flame_Loop", "", gameObject);
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        if (particle)
        {
            var mainModule = particle.main;
            mainModule.startColor = isHigh ? highlightCol : defaultCol;
        }
    }

    private void OnEnable()
    {
        if(GlobalFieldController.CurGameMode == GameMode.Play
            || GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            PlaySound();
        }
    }

    private void OnDisable()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Play
            || GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            StopSound();
        }
    }
}
