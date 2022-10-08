using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class PlayerCamera : SimulationBehaviour
{
    public static PlayerCamera LocalPlayer { get; set; }

    public Camera Camera;
    public ShakeEffect ShakeEffect;

    public void Awake()
    {
        LocalPlayer = this;
    }
}
