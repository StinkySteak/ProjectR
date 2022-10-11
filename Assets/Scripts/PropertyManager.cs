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

        InGameHUD.Instance.UniversalPanel.SetActive(true);
    }

    public void SpawnPlayer()
    {
        Player.LocalPlayer.RequestRespawn();
    }
    public void SetWeapon(int _index)
    {
        SelectedPrimaryWeaponIndex = _index;
    }
    public void OnGameEnd(bool _won)
    {
        InGameHUD.Instance.EndGamePanel.SetActive(true);
        InGameHUD.Instance.EndGameText.text = _won ? "VICTORY" : "DEFEAT";

        StartCoroutine(Shutdown());
    }
    IEnumerator Shutdown()
    {
        yield return new WaitForSeconds(6);
        RunnerInstance.NetworkRunner.Shutdown();
    }
    public void ExitGame()
    {
        RunnerInstance.NetworkRunner.Shutdown();
    }
}
