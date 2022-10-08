using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

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
        foreach (var weapon in WeaponList)
            weapon.gameObject.SetActive(false);

        ActiveWeapon.gameObject.SetActive(true);
    }

    public override void Spawned()
    {
        PlayerSetup = GetComponent<PlayerSetup>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer(Object.InputAuthority, out PlayerInput input))
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
