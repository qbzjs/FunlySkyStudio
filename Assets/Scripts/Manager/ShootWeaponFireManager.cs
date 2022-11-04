/// <summary>
/// Author:Mingo-LiZongMing
/// Description:控制射击道具开火的行为
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWeaponFireManager : MonoManager<ShootWeaponFireManager>
{
    private List<PlayerMeleeShoot> curShootingPlayers = new List<PlayerMeleeShoot>();

    private void Update()
    {
        if (curShootingPlayers != null && curShootingPlayers.Count > 0)
        {
            for (int i = 0; i < curShootingPlayers.Count; i++)
            {
                var playerMelee = curShootingPlayers[i];
                var playerAnimator = playerMelee.playerAnimator;
                if (playerMelee != null && playerAnimator.isActiveAndEnabled)
                {
                    playerMelee.OnPointerDown();
                }
            }
        }
    }

    public void AddPlayerInShootingList(PlayerMeleeShoot playerMelee)
    {
        if (playerMelee == null)
        {
            return;
        }
        if (!curShootingPlayers.Contains(playerMelee))
        {
            playerMelee.fireRateTimer = 0;
            playerMelee.Attack();
            playerMelee.SendBulletCalibrate();
            curShootingPlayers.Add(playerMelee);
        }
    }

    public void RemovePlayerInShootingList(PlayerMeleeShoot playerMelee)
    {
        if (playerMelee == null)
        {
            return;
        }
        if (curShootingPlayers.Contains(playerMelee))
        {
            playerMelee.OnPointerUp();
            curShootingPlayers.Remove(playerMelee);
        }
    }

    public void OnRest()
    {
        curShootingPlayers.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
