using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerCollect : NetworkBehaviour
{
    PlayerSetup PlayerSetup;

    public override void Spawned()
    {
        PlayerSetup = GetComponent<PlayerSetup>();
    }

    public void OnHealthpackCollected(int _healAmount)
    {
        PlayerSetup.PlayerHealth.AddHealth(_healAmount);
    }
    public void OnAmmopackCollected(int _ammoAmount)
    {
        PlayerSetup.WeaponManager.AddAmmo(_ammoAmount);
    }
}
