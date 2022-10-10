using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    /// <summary>
    /// Track Active players (Basic Player data: nickname) in the session, not to be confused with PlayerSetup (player object in the game)
    /// </summary>
    public Dictionary<PlayerRef, Player> SpawnedPlayers { get; set; } = new();

    public Dictionary<PlayerRef, PlayerSetup> SpawnedPlayerObjects { get; set; } = new(); //PlayerSetup.cs hasnt been made

    /// <summary>
    /// method is called by Player.cs when Spawned OR Despawned
    /// </summary>
    /// <param name="_ref"></param>
    /// <param name="_player"></param>
    public void AddPlayer(PlayerRef _ref, Player _player)
    {
        if (SpawnedPlayers.TryGetValue(_ref, out Player _existedPlayer))
        {
            if (_player == _existedPlayer)
                return;

            SpawnedPlayers.Remove(_ref);
        }

        SpawnedPlayers.Add(_ref, _player);
    }
    public void RemovePlayer(PlayerRef _ref, Player _player)
    {
        SpawnedPlayers.Remove(_ref);
    }
    public void RemovePlayer(Player _player)
    {
        KeyValuePair<PlayerRef, Player> expectedPair = new();

        foreach (var pair in SpawnedPlayers)
        {
            if (pair.Value == _player)
            {
                expectedPair = pair;
                break;
            }
        }

        SpawnedPlayers.Remove(expectedPair.Key);
    }


    public void AddPlayerObj(PlayerRef _ref, PlayerSetup _player)
    {
        if (SpawnedPlayerObjects.ContainsKey(_ref))
            return;

        SpawnedPlayerObjects.Add(_ref, _player);
    }

    public bool TryGetPlayerObj(PlayerRef _ref, out PlayerSetup player)
    {
        if (SpawnedPlayerObjects.TryGetValue(_ref, out PlayerSetup _existedPlayer))
        {
            player = _existedPlayer;
            return true;
        }

        player = null;
        return false;
    }

    public bool TryGetPlayer(PlayerRef _ref, out Player _player)
    {
        foreach (var pair in SpawnedPlayers)
        {
            if (pair.Key == _ref)
            {
                _player = pair.Value;
                return true;
            }
        }

        _player = null;
        Debug.LogError($"Player is not valid: {_ref}");
        return false;
    }
    public bool TryGetPlayerTeam(PlayerRef _ref, out Team _team)
    {
        if(TryGetPlayer(_ref,out var _player))
        {
            _team = _player.Team;
            return true;
        }

        _team = Team.Invalid;
        return false;
    }
}
