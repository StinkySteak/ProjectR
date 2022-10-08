using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerWeaponManager : NetworkBehaviour
{
    public PlayerSetup PlayerSetup;

    public Weapon[] WeaponList;

    public Weapon ActiveWeapon;

    public override void Spawned()
    {
        PlayerSetup = GetComponent<PlayerSetup>();
    }

    public override void FixedUpdateNetwork()
    {
        if(Runner.TryGetInputForPlayer(Object.InputAuthority,out PlayerInput input))
        {
            if (input.Buttons.IsSet(ActionButton.Fire))
                InputFire();

            if (input.Buttons.IsSet(ActionButton.Reload))
                InputReload();
        }
    }

    public void AddAmmo(int _amount)
    {
        ActiveWeapon.TotalAmmo += _amount;
    }


    public void InputFire()
    {
        ActiveWeapon.InputFire();
    }

    public void InputAltFire()
    {
        ActiveWeapon.InputFire();
    }

    public void InputReload()
    {
        ActiveWeapon.InputReload();
    }
}