using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HealthIndicator : SceneSingleton<HealthIndicator>
{
    public Volume Volume;
    private void Start()
    {
        LevelManager.OnRender += LevelManager_OnRender;
    }

    private void LevelManager_OnRender()
    {
        if (PlayerHealth.LocalPlayer == null || !PlayerHealth.LocalPlayer.gameObject.activeInHierarchy)
        {
            Volume.weight = 0;
            return;
        }

        var value = (float)PlayerHealth.LocalPlayer.Health / PlayerHealth.LocalPlayer.MaxHealth;

        Volume.weight = 1 - value;
    }
}
