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

    const int RespawnDelay = 5;
    [Networked,HideInInspector] public TickTimer RespawnTimer { get; set; }
    [Networked, HideInInspector] bool IsRespawning { get; set; } = false;

    static void OnPlayerStateChanged(Changed<Player> changed)
    {
        if (!changed.Behaviour.HasInputAuthority)
            return;

        PropertyManager.Instance.UpdateProperty(changed.Behaviour.State == PlayerState.Spawned);

        if (changed.Behaviour.State == PlayerState.Spawned)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if(State == PlayerState.Despawned && !IsRespawning)
        {
            RespawnTimer = TickTimer.CreateFromSeconds(Runner, RespawnDelay);
            IsRespawning = true;
        }

        if(!RespawnTimer.IsTrueRunning() && State == PlayerState.Despawned && IsRespawning)
        {
            RequestRespawn(Object.InputAuthority);
        }
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
            RPC_SetupPlayer(PlayerData.Instance.Nickname);
        }

        if (Object.HasStateAuthority)
            State = PlayerState.Despawned;

        PlayerManager.Instance.AddPlayer(Object.InputAuthority, this);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    void RPC_SetupPlayer(string _nickname)
    {
        Nickname = _nickname;
        IsInitialized = true;
    }

    void RequestRespawn(PlayerRef _ref)
    {
        if (PlayerManager.Instance.TryGetPlayer(_ref, out var player))
        {
            if (player.State == PlayerState.Spawned)
                return;

            if (!player.IsInitialized)
                return;

            var selectedWeaponIndex = Team == Team.ISP ? 0 : 1;

            LevelManager.Instance.SpawnPlayerController(_ref, selectedWeaponIndex);
            IsRespawning = false;
        }
    }
    public void RequestDespawn()
    {
        RPC_RequestDespawn();
    }
    public void Despawn(bool addDeath)
    {
        if (PlayerManager.Instance.TryGetPlayerObj(Object.InputAuthority, out var player))
        {
            LevelManager.Instance.OnPlayerDespawned(player.Object, addDeath);
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestDespawn()
    {
        Despawn(true);
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
