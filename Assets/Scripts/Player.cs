using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

public class Player : NetworkBehaviour
{
    [Networked, HideInInspector] public string Nickname { get; set; }
    [Networked, HideInInspector] public int Kill { get; set; }
    [Networked, HideInInspector] public int Death { get; set; }
    [Networked, HideInInspector] public Team Team { get; set; }

    public static Player LocalPlayer { get; set; }

    [Networked(OnChanged = nameof(OnInitialized)), HideInInspector] public bool IsInitialized { get; set; }
    public event Action OnPlayerInitialized;

    static void OnInitialized(Changed<Player> changed)
    {
        changed.Behaviour.OnPlayerInitialized?.Invoke();
        print($"Player Spawned: {changed.Behaviour.Nickname}");
    }
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            print("LocalPlayer is Initialized");
            LocalPlayer = this;
            RPC_SetupPlayer(PlayerData.Instance.Nickname, PlayerData.Instance.Team);
        }

        PlayerManager.Instance.AddPlayer(Object.InputAuthority, this);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    void RPC_SetupPlayer(string _nickname, Team _team)
    {
        Nickname = _nickname;
        Team = _team;

        IsInitialized = true;
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
