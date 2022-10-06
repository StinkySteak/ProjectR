using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;

[OrderAfter(typeof(Player))]
public class PlayerSetup : NetworkBehaviour
{
    public PlayerController PlayerController;
    public static PlayerSetup LocalPlayer { get; set; }

    public GameObject[] RemoteVisual;
    public GameObject[] LocalVisual;

    public MeshRenderer Renderer;

    public Material Material_Enemy;

    Player PlayerData = null;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            LocalPlayer = this;

        PlayerData = PlayerManager.Instance.GetPlayer(Object.InputAuthority);

        PlayerData.OnPlayerInitialized += OnPlayerInitilized;

        if (Object.HasStateAuthority)
        {
            PlayerData.State = PlayerState.Spawned;
        }

        OnPlayerInitilized();
    }
    void OnPlayerInitilized()
    {
        if (!PlayerData.IsInitialized)
            return;

        if (Player.LocalPlayer == null)
            return;

        print($"PlayerData is null {PlayerData == null} Player.LocalPlayer is null {Player.LocalPlayer == null}");

        if (PlayerData.Team != Player.LocalPlayer.Team)
            Renderer.material = Material_Enemy;

        var hasInputAuth = Object.HasInputAuthority;

        foreach (var visual in RemoteVisual)
            visual.SetActive(!hasInputAuth);

        foreach (var visual in LocalVisual)
            visual.SetActive(hasInputAuth);
    }
}
