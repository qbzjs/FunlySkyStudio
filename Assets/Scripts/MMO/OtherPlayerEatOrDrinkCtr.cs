/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System;
using UnityEngine;

public class OtherPlayerEatOrDrinkCtr : MonoBehaviour
{
    [HideInInspector]
    public PlayerEatOrDrink curEatOrDrink;
    [HideInInspector]
    public AnimationController animCon;

    private void Awake()
    {
        curEatOrDrink = new PlayerEatOrDrink(this.gameObject);
        animCon = this.GetComponentInChildren<AnimationController>();
        var roleComp = this.GetComponentInChildren<RoleController>();
        curEatOrDrink.animCon = animCon;
        curEatOrDrink.PlayerId = GetComponent<PlayerData>().syncPlayerInfo.uid;
        curEatOrDrink.playerType = PlayerType.other;
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
}
