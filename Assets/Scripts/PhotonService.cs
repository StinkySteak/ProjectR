using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

/// <summary>
/// Class to manage Fusion Session (Start Game Session, Callbacks)
/// </summary>
public class PhotonService : Singleton<PhotonService>, INetworkRunnerCallbacks
{
    NetworkRunner Runner => RunnerInstance.NetworkRunner;

    public void InitializeClient()
    {
        Runner.AddCallbacks(this);
        Runner.AddCallbacks(CallbackManager.Instance);
    }

    public void StartAuto()
    {
        StartSimulation("session", GameMode.AutoHostOrClient);
    }

    /// <summary>
    /// Call this to start any game (client/Host)
    /// </summary>
    public void StartSimulation(string _sessionName, GameMode _gamemode)
    {
        Runner.StartGame(new()
        {
            SessionName = _sessionName,
            GameMode = _gamemode,
            PlayerCount = 16,
            Scene = 0,
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),
            Initialized = (NetworkRunner runner) => { Runner.AddCallbacks(LevelManager.Instance); }
        });
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
