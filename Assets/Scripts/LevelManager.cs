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
    public int RequiredPlayerToStart = 2;
    public float PreparationTime = 20;
    public float InitialDefendingDuration; // hacker defend duration

    [Networked, HideInInspector] public float DefendingDuration { get; set; }

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

            Debug.LogError("No Team Found: " + _team);
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

    public static event Action OnRender;

    [Networked(OnChanged = nameof(OnGameStateChanged)), HideInInspector] public GameStatus GameStatus { get; set; }

    public static event Action OnSpawned;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            DefendingDuration = InitialDefendingDuration;
        }

        OnSpawned?.Invoke();
    }

    static void OnGameStateChanged(Changed<LevelManager> changed)
    {
        changed.Behaviour.UpdateGame();
    }

    void UpdateGame()
    {
        if (GameStatus.State == State.End)
            PropertyManager.Instance.OnGameEnd(GameStatus.WinningTeam == Player.LocalPlayer.Team);

        if(GameStatus.State == State.Running)
            AudioManager.Instance.PlayOneShot("advance");
    }

    static void OnAdvanceChanged(Changed<LevelManager> changed)
    {
        changed.Behaviour.OnAdvanceChanged();
        changed.Behaviour.UpdateAdvanceProperties();
    }

    void OnAdvanceChanged()
    {
        AudioManager.Instance.PlayOneShot("advance");
    }

    public override void Render()
    {
        OnRender?.Invoke();
    }

    /// <summary>
    /// set new advance!
    /// </summary>
    public void OnPointReached()
    {
        Advance++;
        DefendingDuration += InitialDefendingDuration;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (GameStatus.State == State.Waiting)
        {
            if (PlayerManager.Instance.SpawnedPlayers.Count >= RequiredPlayerToStart)
                StartGame();
        }
        else if (GameStatus.State == State.Running)
        {
            DefendingDuration -= Runner.DeltaTime;

            if (DefendingDuration <= 0)
            {
                if (Cart.Instance.IsContested)
                    return; // do not end the game yet, while player is still on the cart

                EndGame(Team.Hacker);
            }
        }
    }
    public void StartGame()
    {
        GameStatus = new GameStatus() { State = State.Running, WinningTeam = Team.Invalid };

        foreach (var player in PlayerManager.Instance.SpawnedPlayers.Values)
        {
            player.Despawn(false);
        }
    }

    public void EndGame(Team _winningTeam)
    {
        GameStatus = new GameStatus() { State = State.End, WinningTeam = _winningTeam };
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

    public void OnPlayerDespawned(NetworkObject obj, bool _addDeath = true)
    {
        if (obj == null)
            return;

        if (PlayerManager.Instance.TryGetPlayer(obj.InputAuthority, out var player))
        {
            player.State = PlayerState.Despawned;

            if (_addDeath)
                player.Death++;

            Runner.Despawn(obj);
        }
    }

    void OnPlayerLeft(PlayerRef _ref)
    {
        if (!Object.HasStateAuthority)
            return;

        if (PlayerManager.Instance.TryGetPlayerObj(_ref, out var player))
        {
            Runner.Despawn(player.Object);
        }

        if (PlayerManager.Instance.TryGetPlayer(_ref, out var _player))
        {
            Runner.Despawn(_player.Object);
        }
    }


    void SpawnPlayerData(PlayerRef _ref)
    {
        if (!RunnerInstance.NetworkRunner.IsServer)
            return;

        Runner.Spawn(PlayerDataPrefab, Vector3.zero, Quaternion.identity, _ref, (NetworkRunner runner, NetworkObject obj) => InitPlayer(obj));

        void InitPlayer(NetworkObject obj)
        {
            if (obj.TryGetComponent(out Player player))
            {
                PlayerManager.Instance.GetTeamMember(out int ispTeam, out int _hackerTeam);

                var expectedTeam = Team.Invalid;

                if (ispTeam > _hackerTeam)
                    expectedTeam = Team.Hacker;
                else
                    expectedTeam = Team.ISP;

                player.Team = expectedTeam;
            }
        }
    }
    public void SpawnPlayerController(PlayerRef _ref, int _selectedPrimaryWeapon)
    {
        if (!RunnerInstance.NetworkRunner.IsServer)
            return;

        if (PlayerManager.Instance.TryGetPlayerTeam(_ref, out var team))
        {
            var spawn = GetSpawn(team);

            Runner.Spawn(PlayerPrefab, spawn.position, spawn.rotation, _ref, (NetworkRunner runner, NetworkObject obj) => SetWeapon(obj));
        }

        void SetWeapon(NetworkObject obj)
        {
            if (obj.TryGetComponent(out PlayerWeaponManager playerWeapon))
            {
                playerWeapon.Init(_selectedPrimaryWeapon);
            }
        }
    }

    Transform GetSpawn(Team _team)
    {
        foreach (var adv in ActiveAdvance.All)
        {
            if (adv.Team == _team)
            {
                return adv.SpawnPos[UnityEngine.Random.Range(0, adv.SpawnPos.Length - 1)];
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
    Invalid,
    ISP,
    Hacker
}
public enum State
{
    Waiting,
    Running,
    End
}
public struct GameStatus : INetworkStruct
{
    public State State;
    public Team WinningTeam;
}