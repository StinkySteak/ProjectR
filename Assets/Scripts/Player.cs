using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

public enum PlayerState
{
    Spawned,
    Despawned
}

public class Player : NetworkBehaviour
{


    [Networked, HideInInspector] public string Nickname { get; set; }
    [Networked, HideInInspector] public int Kill { get; set; }
    [Networked, HideInInspector] public int Death { get; set; }
    [Networked, HideInInspector] public Team Team { get; set; } = Team.Invalid;

    /// <summary>
    /// made for triggering PropertyManager
    /// </summary>
    [Networked(OnChanged = nameof(OnPlayerStateChanged)), HideInInspector] public PlayerState State { get; set; } = PlayerState.Despawned;

    public static Player LocalPlayer { get; set; }

    [Networked(OnChanged = nameof(OnInitialized)), HideInInspector] public bool IsInitialized { get; set; }
    public event Action OnPlayerInitialized;

    static void OnPlayerStateChanged(Changed<Player> changed)
    {
        if (!changed.Behaviour.HasInputAuthority)
            return;

        PropertyManager.Instance.UpdateProperty(changed.Behaviour.State == PlayerState.Spawned);

        if (changed.Behaviour.State == PlayerState.Spawned)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    static void OnInitialized(Changed<Player> changed)
    {
        changed.Behaviour.OnPlayerInitialized?.Invoke();
        print($"Player Spawned: {changed.Behaviour.Nickname}");
    }
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            PropertyManager.Instance.UpdateProperty(false);
            LocalPlayer = this;
            RPC_SetupPlayer(PlayerData.Instance.Nickname, PlayerData.Instance.Team);
        }

        if (Object.HasStateAuthority)
            State = PlayerState.Despawned;

        PlayerManager.Instance.AddPlayer(Object.InputAuthority, this);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    void RPC_SetupPlayer(string _nickname, Team _team)
    {
        Nickname = _nickname;
      //  Team = _team;

        IsInitialized = true;
    }
    public void RequestRespawn()
    {
        RPC_RequestRespawn(Runner.LocalPlayer, PropertyManager.Instance.SelectedPrimaryWeaponIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestRespawn(PlayerRef _ref, int _selectedPrimaryWeapon)
    {
        if (PlayerManager.Instance.TryGetPlayer(_ref, out var player))
        {
            if (player.State == PlayerState.Spawned)
                return;

            LevelManager.Instance.SpawnPlayerController(_ref, _selectedPrimaryWeapon);
        }
    }
    public void RequestDespawn()
    {
        RPC_RequestDespawn();
    }
    public void Despawn()
    {
        if (PlayerManager.Instance.TryGetPlayerObj(Object.InputAuthority, out var player))
        {
            LevelManager.Instance.OnPlayerDespawned(player.Object);
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestDespawn()
    {
        Despawn();
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (hasState)
        {
            PlayerManager.Instance.RemovePlayer(Object.InputAuthority, this);
            return;
        }

        PlayerManager.Instance.RemovePlayer(this);
    }
}
