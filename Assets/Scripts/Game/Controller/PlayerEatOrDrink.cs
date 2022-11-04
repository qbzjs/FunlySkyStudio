/// <summary>
/// Author:Mingo-LiZongMing
/// Description:玩家食用行为控制
/// Date: 2022-7-25 19:43:08
/// </summary>
using System;
using System.Collections;
using UnityEngine;

public class PlayerEatOrDrink
{
    private string eatName = "eating";
    private string drinkName = "drinking";
    private string hiccupName = "hiccup";
    private string massagebelly = "massagebelly";
    private Coroutine curCountine;

    private NodeBaseBehaviour curFoodBev;
    private GameObject curFoodObj;
    private ParticleSystem effectPs;

    public AnimationController animCon;
    public RoleController roleComp;
    public Transform PickNode;
    public Transform FoodNode;
    public GameObject CurPlayer;
    public string PlayerId;

    public Action OnStartHaveMealAct;
    public Action OnEndHaveMealAct;

    private float eatLength = 3f;
    private float hiccupLength = 1.5f;
    private float massagebellyLength = 1f;

    public PlayerType playerType = PlayerType.self;

    public PlayerEatOrDrink(GameObject player)
    {
        CurPlayer = player;
    }

    public void InitFoodData(NodeBaseBehaviour baseBev, GameObject bindGo)
    {
        curFoodBev = baseBev;
        curFoodObj = bindGo;
    }

    public void SetFoodAction(Action onStartHaveMeal, Action onEndHaveMeal)
    {
        OnStartHaveMealAct = onStartHaveMeal;
        OnEndHaveMealAct = onEndHaveMeal;
    }

    public void EatOrDrink()
    {
        if(curFoodBev != null)
        {
            animCon.RleasePrefab();
            animCon.CancelLastEmo();
            OnStartHaveMeal();
            var entity = curFoodBev.entity;
            var foodComp = entity.Get<EdibilityComponent>();
            foodComp.eatState = EateState.HasEated;
            var foodMode = foodComp.Mode;
            var stateName = (foodMode == EdibilityMode.Eat) ? eatName : drinkName;
            PickNode.gameObject.SetActive(false);
            PlayAudio(stateName);
            PlayAnim(stateName);
            if(foodMode == EdibilityMode.Eat)
            {
                PlayEffect();
            }
        }
    }

    private void OnStartHaveMeal()
    {
        if (curFoodObj != null)
        {
            PickNode.gameObject.SetActive(false);
            OnStartHaveMealAct?.Invoke();
            curFoodObj.transform.SetParent(FoodNode);
            curFoodObj.transform.localPosition = new Vector3(0, 0, 0);
            curFoodObj.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    private void PlayAnim(string stateName)
    {
        if (playerType == PlayerType.self)
        {
            CoroutineManager.Inst.StartCoroutine(PlayerBaseControl.Inst.SetPlayerRoleActive(0, true, stateName));
        }
        roleComp.SetFacialDefaultPos();
        animCon.PlayAnim(null, stateName, 1);
        animCon.PlayAnim(onInterruptAction, stateName, -1);
        curCountine = CoroutineManager.Inst.StartCoroutine(DelayRunEatEffectCallback(stateName));
    }

    private IEnumerator DelayRunEatEffectCallback(string stateName)
    {
        yield return new WaitForSeconds(eatLength);
        StopEffect();
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        OnEndHaveMealAct?.Invoke();
        animCon.SetOnAnimChangeAct(null);
        PickNode.gameObject.SetActive(true);
        yield return null;
        var nextStateName = (stateName == eatName) ? massagebelly : hiccupName;
        PlayAudio(nextStateName);
        animCon.PlayAnim(null, nextStateName, -1);
        animCon.PlayAnim(onInterruptAction, nextStateName, 1);
        var SecondLength = (stateName == eatName) ? hiccupLength : massagebellyLength;
        yield return new WaitForSeconds(SecondLength);
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        roleComp.SetCustomDefaultPos();
        if (playerType == PlayerType.self)
        {
            animCon.isEating = false;
            PlayerEatOrDrinkControl.Inst.IsEating = false;
        }
    }

    private void onInterruptAction()
    {
        LoggerUtils.Log("PlayerEatOrDrink onInterruptAction!!");
        StopEffect();
        ForceStopEatMusic();
        CoroutineManager.Inst.StopCoroutine(curCountine);
        OnEndHaveMealAct?.Invoke();
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
    }

    public void PlayAudio(string name)
    {
        string AudioName = "";
        switch (name)
        {
            case "eating":
                AudioName = "Eat_Thing";
                break;
            case "drinking":
                AudioName = "Drink_Thing";
                break;
            case "hiccup":
                AudioName = "Hiccup";
                break;
            case "massagebelly":
                AudioName = "Touchbelly";
                break;

        }
        AKSoundManager.Inst.PlayAttackSound(AudioName, "Play_Eat_Drink", "Eat_Drink", CurPlayer);
    }

    public void DropFood()
    {
        ForceStopEatMusic();
        StopEffect();
        CoroutineManager.Inst.StopCoroutine(curCountine);
        if (curFoodObj != null)
        {
            GameObject.Destroy(curFoodObj);
        }
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        OnEndHaveMealAct?.Invoke();
        animCon.SetOnAnimChangeAct(null);
        PickNode.gameObject.SetActive(true);
    }

    private void ForceStopEatMusic()
    {
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Eat_Drink", "Eat_Drink", CurPlayer);
    }

    private void PlayEffect()
    {
        if (effectPs == null)
        {
            GameObject deathEffect = ResManager.Inst.LoadRes<GameObject>("Effect/eat/eat");
            var effect = GameObject.Instantiate(deathEffect, roleComp.transform);
            effectPs = effect.GetComponentInChildren<ParticleSystem>();
        }
        effectPs.gameObject.SetActive(true);
        effectPs.Play();
    }

    private void StopEffect()
    {
        if (effectPs != null)
        {
            effectPs.gameObject.SetActive(false);
            effectPs.Pause();
        }
    }
}
