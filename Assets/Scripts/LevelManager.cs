using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

/// <summary>
/// Class for game loop, mechanics
/// </summary>
public class LevelManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static LevelManager Instance { get; set; }

    public Transform SpawnPosition;

    [Header("Prefabs")]
    public NetworkObject PlayerDataPrefab;
    public NetworkObject PlayerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void OnPlayerDespawned(NetworkObject obj)
    {
        var player = PlayerManager.Instance.GetPlayer(obj.InputAuthority);

        player.State = PlayerState.Despawned;

        Runner.Despawn(obj);
    }

   

    void SpawnPlayerData(PlayerRef _ref)
    {
        if (!RunnerInstance.NetworkRunner.IsServer)
            return;

        Runner.Spawn(PlayerDataPrefab, Vector3.zero, Quaternion.identity, _ref);
    }
    public void SpawnPlayerController(PlayerRef _ref)
    {
        if (!RunnerInstance.NetworkRunner.IsServer)
            return;

        Runner.Spawn(PlayerPrefab, SpawnPosition.position, Quaternion.identity, _ref);
    }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        SpawnPlayerData(player);
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
}

public enum Team
{
    ISP,
    Hacker
}