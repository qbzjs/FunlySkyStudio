using System.Collections.Generic;
using UnityEngine;

public class FreezeEffectObj
{
    public GameObject mEnterEffectObj;
    public GameObject mExitEffectObj;
    public GameObject mMainEffectObj;
    public GameObject mContinueEffectObj;
    public Animator mAnimator;
    public Dictionary<EAnimType, string> mAnims = new Dictionary<EAnimType, string>() { { EAnimType.Enter, "freez_subject_up" }, { EAnimType.Main, "2" }, { EAnimType.Exit, "freez_subject_end" } };
    public bool mIsPlayerEndEffect = false;
    public enum EAnimType
    {
        Enter,
        Main,
        Exit,
    }
    public void Init(Transform parent)
    {
        CreateMainEffectObj(parent);
        CreateEnterEffectObj(parent);
        CreateExitEffectObj(parent);
        CreateContinueEffectObj(parent);
    }
    public void CreateMainEffectObj(Transform parent)
    {
        GameObject prefab = ResManager.Inst.LoadRes<GameObject>("Effect/freeze/pd/freez_subject");
        mMainEffectObj = UnityEngine.Object.Instantiate(prefab, parent);
        mAnimator = mMainEffectObj.GetComponent<Animator>();
        mMainEffectObj.SetActive(false);
    }
    public void CreateEnterEffectObj(Transform parent)
    {
        GameObject prefab = ResManager.Inst.LoadRes<GameObject>("Effect/freeze/pd/freeze_up");
        mEnterEffectObj = UnityEngine.Object.Instantiate(prefab, parent);
        mEnterEffectObj.SetActive(false);
    }
    public void CreateExitEffectObj(Transform parent)
    {
        GameObject prefab = ResManager.Inst.LoadRes<GameObject>("Effect/freeze/pd/freeze_end");
        mExitEffectObj = UnityEngine.Object.Instantiate(prefab, parent);
        mExitEffectObj.SetActive(false);
    }
    public void CreateContinueEffectObj(Transform parent)
    {
        GameObject prefab = ResManager.Inst.LoadRes<GameObject>("Effect/freeze/pd/freeze_continue");
        mContinueEffectObj = UnityEngine.Object.Instantiate(prefab, parent);
        mContinueEffectObj.SetActive(false);
    }
    public void Play()
    {
        mIsPlayerEndEffect = false;
        mEnterEffectObj.SetActive(true);
        mMainEffectObj.SetActive(true);
        ParticleSystem[] pss = mMainEffectObj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem p in pss)
        {
            p.Play();
        }
        mAnimator.Play(mAnims[EAnimType.Enter]);
    }
    public void Stop()
    {
        mIsPlayerEndEffect = false;
        mMainEffectObj.SetActive(false);
        mExitEffectObj.SetActive(false);
        mEnterEffectObj.SetActive(false);
        mContinueEffectObj.SetActive(false);
    }
    //带有阶段的特效
    public void PlayEndEffect()
    {
        if (mIsPlayerEndEffect)
        {
            return;
        }
        mIsPlayerEndEffect = false;
        mExitEffectObj.SetActive(true);
        mEnterEffectObj.SetActive(false);
        mMainEffectObj.SetActive(false);
        mContinueEffectObj.SetActive(false);
        ParticleSystem[] pss = mExitEffectObj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem p in pss)
        {
            p.Play();
        }
        mAnimator.Play(mAnims[EAnimType.Exit]);
    }
    public void PlayContinueEffect()
    {
        mEnterEffectObj.SetActive(false);
        mContinueEffectObj.SetActive(true);
        ParticleSystem[] pss = mContinueEffectObj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem p in pss)
        {
            p.Play();
        }
    }
}
