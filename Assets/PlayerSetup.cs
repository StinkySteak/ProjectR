using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;

public class PlayerSetup : NetworkBehaviour
{
    public PlayerController PlayerController;
    public static PlayerSetup LocalPlayer { get; set; }
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            LocalPlayer = this;
    }
}
