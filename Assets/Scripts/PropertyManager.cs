using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyManager : SceneSingleton<PropertyManager>
{
    public GameObject PlayerCamera;
    public GameObject SceneCamera;

    public int SelectedPrimaryWeaponIndex;

    private void Start()
    {
        LevelManager.OnSpawned += OnGameJoin;
    }

    private void OnDestroy()
    {
        LevelManager.OnSpawned -= OnGameJoin;
    }

    void OnGameJoin()
    {
        MainMusic.Instance.StartPlay();
    }

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
    public void Suicide()
    {
        Player.LocalPlayer.RequestDespawn();
    }
    public void SetWeapon(int _index)
    {
        SelectedPrimaryWeaponIndex = _index;
    }
    public void OnGameEnd(bool _won)
    {
        InGameHUD.Instance.EndGamePanel.SetActive(true);
        InGameHUD.Instance.EndGameText.text = _won ? "VICTORY" : "DEFEAT";

        if (Player.LocalPlayer.Team == Team.ISP)
            InGameHUD.Instance.EndGameTextDesc.text = _won ? "Internet connected successfuly!\n +10% Salary!" : "Connection terminated\n boss will mad at you!";
        else if (Player.LocalPlayer.Team == Team.Hacker)
            InGameHUD.Instance.EndGameTextDesc.text = _won ? "Connection Terminated successfuly!\n +1 Fiverr Gig Completed" : "Failed! \n-1 Respect";

        AudioManager.Instance.PlayOneShot(_won ? "win" : "lose");

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
