using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Fusion.Sockets;

public class PlayerWeaponManager : NetworkBehaviour
{
    public static PlayerWeaponManager LocalPlayer;

    public PlayerSetup PlayerSetup;

    public Weapon[] PrimaryWeapon;
    public GameObject[] PrimaryWeaponVisual;
    [Space]
    public Weapon SecondaryWeapon;
    public GameObject SecondaryWeaponVisual;

    public GameObject[] AllWeaponVisual => new GameObject[] { PrimaryWeaponVisual[0], PrimaryWeaponVisual[1], SecondaryWeaponVisual };

    /// <summary>
    /// This was set by player on respawn, (loadout)
    /// </summary>
    [Networked] public int SelectedPrimaryWeaponIndex { get; set; }

    /// <summary>
    /// 0, 1 => Primary Weapon
    /// <para></para>
    /// 2    => Secondary Weapon
    /// </summary>
    [Networked(OnChanged = nameof(OnActiveWeaponChanged)), HideInInspector] int ActiveWeaponIndex { get; set; }

    public Weapon ActiveWeapon;

    public void Init(int _selectedWeaponIndex)
    {
        SelectedPrimaryWeaponIndex = _selectedWeaponIndex;
        ActiveWeaponIndex = SelectedPrimaryWeaponIndex;
    }


    static void OnActiveWeaponChanged(Changed<PlayerWeaponManager> changed)
    {
        changed.Behaviour.SetWeaponVisual();
    }

    /// <summary>
    /// turning on/off weapon visual
    /// </summary>
    void SetWeaponVisual()
    {
        foreach (var visual in AllWeaponVisual)
            visual.SetActive(false);

        if (ActiveWeaponIndex == 2) // is secondary
        {
            SecondaryWeaponVisual.SetActive(true);
            return;
        }

        //   print(ActiveWeaponIndex);

        PrimaryWeaponVisual[ActiveWeaponIndex].SetActive(true);

        if (Object.HasStateAuthority) // updating the ActiveWeapon class to for the non State Auth
            return;

        SetWeapon();
    }

    /// <summary>
    /// Set ActiveWeapon Class
    /// </summary>
    void SetWeapon()
    {
        if (ActiveWeaponIndex == 2) // is secondary
        {
            ActiveWeapon = SecondaryWeapon;
            return;
        }

        ActiveWeapon = PrimaryWeapon[ActiveWeaponIndex];
    }

    public override void Spawned()
    {
        PlayerSetup = GetComponent<PlayerSetup>();

        if (Object.HasInputAuthority)
            LocalPlayer = this;

        SetWeapon();
        SetWeaponVisual();
    }

    public void OnInput(PlayerInput input)
    {
        if (input.Buttons.IsSet(ActionButton.Fire))
            InputFire();

        if (input.Buttons.IsSet(ActionButton.Reload))
            InputReload();

        ProcessWeaponKeyInput(input);
    }
    private void ProcessWeaponKeyInput(PlayerInput input)
    {
        if (input.Buttons.IsSet(ActionButton.weaponSwitch1)) { ActiveWeaponIndex = SelectedPrimaryWeaponIndex; Debug.Log("Weapon 1"); }
        else if (input.Buttons.IsSet(ActionButton.weaponSwitch2)) { ActiveWeaponIndex = 2; Debug.Log("Weapon 2"); }

        SetWeapon();
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
