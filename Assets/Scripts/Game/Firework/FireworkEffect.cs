using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;
using static UnityEngine.ParticleSystem;

/// <summary>
/// Author:Meimei-LiMei
/// Description:烟花粒子特效播放逻辑
/// Date: 2022/7/26 14:32:33
public class FireworkEffect : MonoBehaviour
{
    public GameObject BurstEffect;
    public GameObject ShotLight;
    private ParticleSystem[] BurstEffectParticles;
    private ParticleSystem[] ShotLightParticles;
    private const float fireTime = 1.8f;
    private float playtime = 0;
    private Vector3 targetPos;
    FireworkComponent comp;
    public Action PlayCallBack;
    public bool lightIsPlay = false;//发射光线是否播放
    private bool burstIsPlay = false;
    private Burst lowburst = new ParticleSystem.Burst(0.0f, 125);
    private Burst meduimburst = new ParticleSystem.Burst(0.0f, 125);
    private Burst heightburst = new ParticleSystem.Burst(0.0f, 255);
    private MinMaxCurve lowcurve = new MinMaxCurve(4, 12);
    private MinMaxCurve meduimcurve = new MinMaxCurve(8, 16);
    private MinMaxCurve heightcurve = new MinMaxCurve(16, 32);

    private Dictionary<int, FireworkHeight> fireworkHeightDics = new Dictionary<int, FireworkHeight>()
    {
        {3,FireworkHeight.Low},
        {7,FireworkHeight.Medium},
        {12,FireworkHeight.Heigh}
    };
    public void Awake()
    {
        BurstEffectParticles = BurstEffect.GetComponentsInChildren<ParticleSystem>();
        ShotLightParticles = ShotLight.GetComponentsInChildren<ParticleSystem>();
    }
    private void Update()
    {
        if (lightIsPlay == true)
        {
            ShotLight.transform.localPosition = Vector3.MoveTowards(ShotLight.transform.localPosition, targetPos, 10 * Time.deltaTime);
        }
        if (Vector3.Distance(ShotLight.transform.localPosition, targetPos) < 0.01f)
        {
            lightIsPlay = false;
            ShotLightPlayCallBack();
        }
        if (burstIsPlay == true)
        {
            playtime += Time.deltaTime;
            if (playtime >= fireTime)
            {
                DestroyFirework();
                playtime = 0;
            }
        }
    }
    /// <summary>
    /// 初始化烟花，同时播放烟花
    /// </summary>
    /// <param name="component"></param>
    /// <param name="callback"></param>
    /// <param name="parentPos"></param>
    public void InitFireworkEffect(FireworkComponent component, Action callback, Transform parentPos)
    {
        comp = component;
        this.PlayCallBack = callback;
        targetPos = new Vector3(0, comp.fireworkHeight, 0);
        this.gameObject.SetActive(true);
        this.transform.SetParent(FireworkPool.Inst.FireworkNode);
        Vector3 firePos = parentPos.TransformPoint(comp.anchorsPos);
        this.transform.position = firePos;
        this.transform.rotation = parentPos.rotation;
        this.transform.localScale = Vector3.one;
        PlayLightEffect();
    }
    /// <summary>
    /// 播放光线发射效果
    /// </summary>
    private void PlayLightEffect()
    {
        StopPlayFirework(BurstEffectParticles);
        lightIsPlay = true;
        ShotLight.SetActive(true);
        ShotLight.transform.localPosition = Vector3.zero;
        Color color = DataUtils.DeSerializeColor(comp.fireworkcolor);
        TrailRenderer tr = ShotLight.transform.GetComponentInChildren<TrailRenderer>();
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
             new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
             new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
         );
        tr.colorGradient = gradient;
        foreach (var particle in ShotLightParticles)
        {
            particle.Play();
            var mainModule = particle.main;
            mainModule.startColor = color;
        }
        AKSoundManager.Inst.PostEvent("Play_Fireworks_Rise", ShotLight);
    }
    private void ShotLightPlayCallBack()
    {
        StopPlayFirework(ShotLightParticles);
        ShotLight.SetActive(false);
        ShotLight.transform.localPosition = Vector3.zero;
        PlayFireEffect();
    }
    /// <summary>
    /// 播放烟花
    /// </summary>
    private void PlayFireEffect()
    {
        AKSoundManager.Inst.PostEvent("Play_Fireworks_Bloom", BurstEffect);
        BurstEffect.transform.localPosition = targetPos;
        SetEffectparticle(fireworkHeightDics[comp.fireworkHeight]);
        burstIsPlay = true;
        foreach (var particle in BurstEffectParticles)
        {
            particle.Play();
        }
    }
    /// <summary>
    /// 动态设置烟花粒子特效颜色、数量
    /// </summary>
    /// <param name="fireworkHeight"></param>
    private void SetEffectparticle(FireworkHeight fireworkHeight)
    {
        Color color = DataUtils.DeSerializeColor(comp.fireworkcolor);
        foreach (var particle in BurstEffectParticles)
        {
            var mainModule = particle.main;
            mainModule.startColor = color;
        }
        var emission = BurstEffectParticles[0].emission;
        var main = BurstEffectParticles[0].main;
        emission.enabled = true;
        switch (fireworkHeight)
        {
            case FireworkHeight.Low:
                emission.SetBurst(0, lowburst);
                main.maxParticles = 50;
                main.startSpeed = lowcurve;
                break;
            case FireworkHeight.Medium:
                emission.SetBurst(0, meduimburst);
                main.maxParticles = 200;
                main.startSpeed = meduimcurve;
                break;
            case FireworkHeight.Heigh:
                emission.SetBurst(0, heightburst);
                main.maxParticles = 300;
                main.startSpeed = heightcurve;
                break;
        }
    }
    /// <summary>
    /// 停止烟花播放
    /// </summary>
    /// <param name="particleSystems"></param>
    public void StopPlayFirework(ParticleSystem[] particleSystems)
    {
        foreach (var particle in particleSystems)
        {
            particle.Stop();
        }
    }
    public void DestroyFirework()
    {
        this.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        StopPlayFirework(ShotLightParticles);
        StopPlayFirework(BurstEffectParticles);
        PlayCallBack?.Invoke();
        ShotLight.SetActive(false);
        lightIsPlay = false;
        burstIsPlay = false;
        FireworkPool.Inst.AddEffectToPool(this.gameObject);
    }
    private void OnDestroy()
    {
        FireworkPool.Inst.RemoveGameobject(this.gameObject);
    }
}
