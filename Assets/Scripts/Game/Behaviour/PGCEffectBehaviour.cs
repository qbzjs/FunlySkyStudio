using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:PGC 特效 Behavior：设置颜色
/// Date: 2022/10/25 14:29:16
/// </summary>
public class PGCEffectBehaviour : BaseHLODBehaviour
{
    private static MaterialPropertyBlock mpb;
    public string ChooseColor = string.Empty;

    public override void SetUp()
    {
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        var comp = entity.Get<PGCEffectComponent>();
        SetColor(DataUtils.DeSerializeColor(comp.effectColor));
    }

    private void OnEnable()
    {
        if (GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            PlaySound(true);
        }
    }

    private void OnDisable()
    {
        if (GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            PlaySound(false);
        }
    }

    public void UpdateAssetObj(GameObject gameObject, int id)
    {
        gameObject.transform.SetParent(this.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        assetObj = gameObject;
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.modId = id;
    }

    /// <summary>
    /// 设置颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        var effectRenderers = gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (var effectRender in effectRenderers)
        {
            effectRender.GetPropertyBlock(mpb);
            color.a = effectRender.material.color.a;
            mpb.SetColor("_Color", color);
            effectRender.SetPropertyBlock(mpb);
        }
    }

    public void PlaySound(bool isPlay)
    {
        if(entity == null)
        {
            return;
        }
        var gameComp = entity.Get<GameObjectComponent>();
        var id = gameComp.modId;
        var effectConfig = PGCEffectManager.Inst.GetPGCEffectConfigData(id);
        var playSound = entity.Get<PGCEffectComponent>().playSound;
        if (effectConfig.useSound == 0)
        {
            return;
        }

        if (!isPlay)
        {
            PlayEffectSound(effectConfig.stopName, gameObject);
        }
        else
        {
            if (playSound == 1)
            {
                PlayEffectSound(effectConfig.soundName, gameObject);
            }
        }
    }

    public void PlayEffectSound(string name, GameObject gameObject)
    {
        AKSoundManager.Inst.SetSwitch("", "", gameObject);
        AKSoundManager.Inst.PostEvent(name, gameObject);
    }

    public void PlayParticleEffect()
    {
        var particles = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
        {
            var particle = particles[i];
            particle.Play();
        }
    }
}
