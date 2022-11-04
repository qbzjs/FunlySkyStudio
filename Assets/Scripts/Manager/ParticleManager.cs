using System.Collections.Generic;
using UnityEngine;

public class ParticleManager:CInstance<ParticleManager>
{
    public struct EffectData
    {
        public int mEffectKey;
        public int mTargetId;

        public static EffectData Emtpy = new EffectData();
    }

    private Dictionary<int, EffectData> mPlayingEffectData;
    public void Init()
    {
        mPlayingEffectData = new Dictionary<int, EffectData>(128);
    }
    //这个接口功能：处理重复特效的打断问题，新的特效进来，可以打断旧的特效
    public void IncParticleActiveNumber(PooledParticleObjScript effect, string fullPath, GameObject target = null)
    {
        int key = GameUtils.JavaHashCodeIgnoreCase(fullPath);

        if (target != null)
        {
            var enu = mPlayingEffectData.GetEnumerator();
            while (enu.MoveNext())
            {
                if (enu.Current.Value.mEffectKey == key && enu.Current.Value.mTargetId == target.GetInstanceID())
                {
                    ParticleObjPool.Inst.RecycleDelayGameObject(enu.Current.Key);
                    break;
                }
            }
        }
        // 加新的特效
        EffectData data = EffectData.Emtpy;
        data.mEffectKey = key;
        if (target != null)
        {
            data.mTargetId = target.GetInstanceID();
        }
        mPlayingEffectData.Add(effect.mGameObject.GetInstanceID(), data);
    }
    public void DecParticleActiveNumber(PooledParticleObjScript effect)
    {
        int objHash = effect.GetInstanceID();

        if (!mPlayingEffectData.ContainsKey(objHash))
        {
            LoggerUtils.LogError($"DecParticleActiveNumber.mPlayingEffectData not containkey  {effect}");
        }

        mPlayingEffectData.Remove(objHash);
    }
    public void PlayEffect(string name, Transform parent,float lifeTime)
    {
        Vector3 scaling = Vector3.one;
        PooledParticleObjScript particleObject = ParticleObjPool.Inst.GetGameObject(name);
        if (particleObject == null)
            return;
        
        IncParticleActiveNumber(particleObject, name, parent == null ? null : parent.gameObject);
        particleObject.mTransform.parent = parent;
        particleObject.mTransform.localScale = Vector3.one;
        particleObject.mTransform.localPosition = Vector3.zero;
        particleObject.mTransform.localRotation = Quaternion.identity;
        LoggerUtils.Log($"PlayEffect(string name, Transform parent,float lifeTime)  {parent}");
        if (particleObject.mIsInit)
        {
            Play(particleObject, scaling);
        }
        int effectLength = (int)(lifeTime * 1000);
        ParticleObjPool.Inst.RecycleGameObjectDelay(particleObject, effectLength, OnRecycleTickObj);
    }
    public void OnRecycleTickObj(PooledParticleObjScript obj)
    {
        DecParticleActiveNumber(obj);
    }
    public void Play(PooledParticleObjScript gameObj, Vector3 scaling)
    {
        ParticleSystem[] particleSys = gameObj.ParticleSystems;
        if (particleSys == null || particleSys.Length == 0)
            return;
        for (int i = 0; i < particleSys.Length; i++)
        {
            var ps = particleSys[i];
            ParticleSystem.MainModule main = ps.main;
            main.startLifetimeMultiplier *= scaling.y;
            main.startSpeedMultiplier *= scaling.z;
            if (!main.playOnAwake)
            {
                main.playOnAwake = true;
                ps.Play();
            }
        }
    }
}

