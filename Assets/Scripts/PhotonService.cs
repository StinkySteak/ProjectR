using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Threading;

/// <summary>
/// Class to manage Fusion Session (Start Game Session, Callbacks)
/// </summary>
public class PhotonService : Singleton<PhotonService>, INetworkRunnerCallbacks
{
    NetworkRunner Runner => RunnerInstance.NetworkRunner;

    public static event Action OnRunnerStart;
    public static event Action OnRunnerStartFailed;

    public static event Action<List<SessionInfo>> OnSessionListUpdated;

    void Start()
    {
        JoinLobby();
    }

    void JoinLobby()
    {
        StartCoroutine(InitializeClient());
    }

    IEnumerator InitializeClient()
    {
        yield return new WaitForSeconds(2);

        if (!RunnerInstance.FreshRunner.LobbyInfo.IsValid)
            RunnerInstance.NetworkRunner.JoinSessionLobby(SessionLobby.ClientServer);

        Runner.AddCallbacks(this);
        Runner.AddCallbacks(CallbackManager.Instance);
    }

    /// <summary>
    /// Call this to start any game (client/Host)
    /// </summary>
    public async void StartSimulation(string _sessionName, GameMode _gamemode, string _region = "")
    {
        if (Runner.IsRunning)
            return;

        OnRunnerStart?.Invoke();

        var result = await Runner.StartGame(new()
        {
            SessionName = _sessionName,
            GameMode = _gamemode,
            PlayerCount = 6,
            Scene = 0,
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),
            Initialized = (NetworkRunner runner) => { Runner.AddCallbacks(LevelManager.Instance); },
            SessionProperties = new Dictionary<string, SessionProperty>() { ["region"] = _region }
        });

        if (!result.Ok)
            OnRunnerStartFailed?.Invoke();
    }
    void OnShutdown()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        JoinLobby();
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
        OnSessionListUpdated?.Invoke(sessionList);
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        OnShutdown();
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
}
