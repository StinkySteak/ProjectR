using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class PlayerWeaponManager : NetworkBehaviour
{
    public PlayerSetup PlayerSetup;

    public Weapon[] WeaponList;

    [Networked(OnChanged = nameof(OnActiveWeaponChanged)), HideInInspector] int ActiveWeaponIndex { get; set; }

    public Weapon ActiveWeapon => WeaponList[ActiveWeaponIndex];

    static void OnActiveWeaponChanged(Changed<PlayerWeaponManager> changed)
    {
        changed.Behaviour.SetWeaponVisual();
    }


    /// <summary>
    /// turning on/off weapon visual
    /// </summary>
    void SetWeaponVisual ()
    {
        int weaponIdex = 0;

        foreach (var weapon in WeaponList)
        {
            if(weaponIdex == ActiveWeaponIndex) { weapon.gameObject.SetActive(true); Debug.Log("Current Weapon " + weaponIdex); }
            else { weapon.gameObject.SetActive(false); }
            weaponIdex++;
        }
    }

    public override void Spawned()
    {
        PlayerSetup = GetComponent<PlayerSetup>();
    }

    public override void FixedUpdateNetwork()
    {

        int previousWeapon = ActiveWeaponIndex;

        if (Runner.TryGetInputForPlayer(Object.InputAuthority, out PlayerInput input))
        {
            if (input.Buttons.IsSet(ActionButton.Fire))
                InputFire();

            if (input.Buttons.IsSet(ActionButton.Reload))
                InputReload();

            ProcessWeaponKeyInput(input);

        }

        if(previousWeapon != ActiveWeaponIndex)
        {
            SetWeaponVisual();
        }



    }

    private void ProcessWeaponKeyInput(PlayerInput input)
    {
        if(input.Buttons.IsSet(ActionButton.weaponSwitch1)) { ActiveWeaponIndex = 0; Debug.Log("Weapon 1"); }
        else if(input.Buttons.IsSet(ActionButton.weaponSwitch2)) { ActiveWeaponIndex = 1; Debug.Log("Weapon 2"); }
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
