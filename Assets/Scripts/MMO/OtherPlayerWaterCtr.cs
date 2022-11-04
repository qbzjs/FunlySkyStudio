/// <summary>
/// Author:Zhouzihan
/// Description:其他玩家水中控制
/// Date: 2022/3/31 15:46:6
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerWaterCtr
{
    public OtherPlayerCtr ctr;
    public void Init(OtherPlayerCtr otherPlayerCtr)
    {
        ctr = otherPlayerCtr;
    }
    private SwimEffect swimEffect = SwimEffect.idle;
    public void SetSwimEffect()
    {
        if (ctr.m_IsInWater)
        {
            SwimEffect effectName = SwimEffect.idle;
            if (ctr.m_IsMoving)
            {
                if (ctr.m_IsSwimming)
                {
                    effectName = SwimEffect.Swim;
                }
                else
                {
                    effectName = SwimEffect.walk;
                }
            }
            else
            {
                effectName = SwimEffect.idle;
            }
            if (effectName != swimEffect || swimEffectObj == null)
            {
                swimEffect = effectName;
                if (effectName == SwimEffect.Swim)
                {
                    PlaySwimSound();
                }
                else if (effectName == SwimEffect.idle)
                {
                    PlayWaterIdleSound();
                }

                ShowSwimEffect();
            }
        }
        else
        {
            CleanSwimEffect();
        }
    }
    public void PlaySwimSound()
    {
        if (!AKSoundManager.Inst.isOpenFootSound)
            return;
        
        if (ctr!=null&&ctr.m_IsInWater && ctr.m_IsSwimming && swimEffect == SwimEffect.Swim)
        {

            ctr.CancelInvoke("PlaySwimSound");
            AKSoundManager.Inst.PlaySwimSound("play_swim_3p", false, ctr.gameObject);
            var deltaTime = 0.5f;
            ctr.Invoke("PlaySwimSound", deltaTime);
        }

    }
    public void PlayWaterIdleSound()
    {
        if (!AKSoundManager.Inst.isOpenFootSound)
            return;
        if (ctr != null && ctr.m_IsInWater && swimEffect == SwimEffect.idle)
        {
            ctr.CancelInvoke("PlayWaterIdleSound");
            AKSoundManager.Inst.PostEvent("play_underwater_loading_3p", ctr.gameObject);
            var deltaTime = 5f;
            ctr.Invoke("PlayWaterIdleSound", deltaTime);
        }
    }
    private GameObject swimEffectObj;
    public void ShowSwimEffect()
    {
        Transform parent = ctr.transform;
        string path = "";
        Vector3 pos = Vector3.zero;
        switch (swimEffect)
        {
            case SwimEffect.Swim:
                path = "Prefabs/Emotion/Express/swimming";
                parent = ctr.transform.Find("shoe");
                break;
            case SwimEffect.idle:
                path = "Prefabs/Emotion/Express/swimming_idle";
                break;
            case SwimEffect.walk:
                path = "Prefabs/Emotion/Express/swimming_walk";
                break;
        }
        GameObject movePrefab = ResManager.Inst.LoadCharacterRes<GameObject>(path);

        CleanSwimEffect();
        if (movePrefab != null)
        {
            swimEffectObj = UnityEngine.GameObject.Instantiate(movePrefab);
            swimEffectObj.transform.parent = parent;
            swimEffectObj.transform.localRotation = Quaternion.identity;
            swimEffectObj.transform.localPosition = pos;
            swimEffectObj.transform.localScale = Vector3.one;
        }

    }
    public void CleanSwimEffect()
    {
        if (swimEffectObj != null)
        {
            UnityEngine.GameObject.Destroy(swimEffectObj);
            swimEffectObj = null;
        }
    }

}
