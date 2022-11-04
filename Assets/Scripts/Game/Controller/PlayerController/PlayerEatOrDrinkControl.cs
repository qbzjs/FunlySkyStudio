/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System;
using UnityEngine;

public class PlayerEatOrDrinkControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public static PlayerEatOrDrinkControl Inst;
    [HideInInspector]
    public static PlayerEatOrDrink curEatOrDrink;
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    [HideInInspector]
    public bool IsEating = false;

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.EatOrDrink, Inst);
        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
        var roleComp = playerBase.transform.GetComponentInChildren<RoleController>(true);

        curEatOrDrink = new PlayerEatOrDrink(playerBase.gameObject);
        curEatOrDrink.animCon = animCon;
        curEatOrDrink.playerType = PlayerType.self;
        curEatOrDrink.PlayerId = GameManager.Inst.ugcUserInfo.uid;
        curEatOrDrink.PickNode = roleComp.GetBandNode((int)BodyNode.PickNode);
        curEatOrDrink.FoodNode = roleComp.GetBandNode((int)BodyNode.FoodNode);
        curEatOrDrink.roleComp = roleComp;
    }

    public void InitFoodData(NodeBaseBehaviour baseBev, GameObject bindGo)
    {
        if (curEatOrDrink != null)
        {
            curEatOrDrink.InitFoodData(baseBev, bindGo);
        }
    }

    public void SetFoodAction(Action onStartHaveMeal, Action onEndHaveMeal)
    {
        if (curEatOrDrink != null)
        {
            curEatOrDrink.SetFoodAction(onStartHaveMeal, onEndHaveMeal);
        }
    }

    public void EatOrDrink()
    {
        if (curEatOrDrink != null)
        {
            curEatOrDrink.EatOrDrink();
        }
    }

    public void DropFood()
    {
        if (curEatOrDrink != null)
        {
            curEatOrDrink.DropFood();
        }
    }
}
