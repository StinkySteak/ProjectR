using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Class for game loop, mechanics
/// </summary>
public class LevelManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public int RequiredPlayerToStart = 2;
    public float PreparationTime = 20;


    [Serializable]
    public class Advancement
    {
        public TeamAdvance ISP;
        public TeamAdvance Hacker;

        public GameObject[] UniversalPropertyToEnable;
        public GameObject[] UniversalPropertyToDisable;

        public TeamAdvance[] All => new TeamAdvance[] { ISP, Hacker };

        public TeamAdvance GetMyTeamAdvance(Team _team)
        {
            foreach (var adv in All)
            {
                if (adv.Team == _team)
                    return adv;
            }

            Debug.LogError("No Team Found");
            return null;
        }
    }

    [Serializable]
    public class TeamAdvance
    {
        public Team Team;
        public Transform[] SpawnPos;
        public Barrier SpawnBarrier;
        public GameObject[] PropertyToEnable;
        public GameObject[] PropertyToDisable;
    }

    public static LevelManager Instance { get; set; }


    [Space]
    public Advancement[] Advancements;

    [Header("Prefabs")]
    public NetworkObject PlayerDataPrefab;
    public NetworkObject PlayerPrefab;

    [Networked(OnChanged = nameof(OnAdvanceChanged))] public int Advance { get; set; }

    static void OnAdvanceChanged(Changed<LevelManager> changed)
    {
        changed.Behaviour.UpdateAdvanceProperties();
    }

    /// <summary>
    /// set new advance!
    /// </summary>
    public void OnPointReached()
    {
        Advance++;
    }

    void UpdateAdvanceProperties()
    {
        foreach (var player in PlayerManager.Instance.SpawnedPlayerObjects)
            player.Value.ManageColliders();

        foreach (var adv in Advancements)
        {
            foreach (var prop in adv.All)
            {
                bool isMyTeam = prop.Team == Player.LocalPlayer.Team;

                if (!isMyTeam)
                    continue;

                foreach (var item in prop.PropertyToEnable)
                    item.SetActive(true);

                foreach (var item in prop.PropertyToDisable)
                    item.SetActive(false);
            }

            foreach (var prop in adv.UniversalPropertyToEnable)
            {
                prop.gameObject.SetActive(true);
            }
            foreach (var prop in adv.UniversalPropertyToDisable)
            {
                prop.gameObject.SetActive(false);
            }
        }
    }

    public Advancement ActiveAdvance => Advancements[Advance];

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

    void OnPlayerLeft(PlayerRef _ref)
    {
        if (!Object.HasStateAuthority)
            return;

        if (PlayerManager.Instance.TryGetPlayerObj(_ref, out var player))
            Runner.Despawn(player.GetComponent<NetworkObject>());

        Runner.Despawn(PlayerManager.Instance.GetPlayer(_ref).Object);
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

        Runner.Spawn(PlayerPrefab, GetSpawnPos(PlayerManager.Instance.GetPlayerTeam(_ref)), Quaternion.identity, _ref);
    }

    Vector3 GetSpawnPos(Team _team)
    {
        foreach (var adv in ActiveAdvance.All)
        {
            if (adv.Team == _team)
            {
                return adv.SpawnPos[UnityEngine.Random.Range(0, adv.SpawnPos.Length - 1)].position;
            }
        }

        Debug.LogError("No Matching Advance Team");
        return default;
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
        OnPlayerLeft(player);
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