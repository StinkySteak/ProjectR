using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyManager : SceneSingleton<PropertyManager>
{
    public GameObject PlayerCamera;
    public GameObject SceneCamera;

    public int SelectedPrimaryWeaponIndex;

    /// <summary>
    /// update property in the scene, such as UI, visual
    /// </summary>
    /// <param name="_isSpawned"></param>
    public void UpdateProperty(bool _isSpawned)
    {
        PlayerCamera.SetActive(_isSpawned);
        SceneCamera.SetActive(!_isSpawned);

        InGameHUD.Instance.DespawnedPanel.SetActive(!_isSpawned);
        InGameHUD.Instance.SpawnedPanel.SetActive(_isSpawned);
    }

    public void SpawnPlayer()
    {
        Player.LocalPlayer.RequestRespawn();
    }
    public void SetWeapon(int _index)
    {
        SelectedPrimaryWeaponIndex = _index;
    }
}
