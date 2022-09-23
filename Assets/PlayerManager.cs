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

    // public Dictionary<PlayerRef, Player> SpawnedPlayerObjects { get; set; } = new(); //PlayerSetup.cs hasnt been made

    /// <summary>
    /// method is called by Player.cs when Spawned OR Despawned
    /// </summary>
    /// <param name="_ref"></param>
    /// <param name="_player"></param>
    public void AddPlayer(PlayerRef _ref,Player _player)
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
            if(pair.Value == _player)
            {
                expectedPair = pair;
                break;
            }
        }

        SpawnedPlayers.Remove(expectedPair.Key);
    }
    //disabled: PlayerSetup.cs hasnt been made

    //public void AddPlayerObj(PlayerRef _ref, PlayerSetup _player)
    //{
    //    if (SpawnedPlayerObjects.TryGetValue(_ref, out PlayerSetup _existedPlayer))
    //    {
    //        if (_player == _existedPlayer)
    //            return;

    //        SpawnedPlayersObject.Remove(_ref);
    //    }

    //    SpawnedPlayerObjects.Add(_ref, _player);
    //}

    public Player GetPlayer(PlayerRef _ref)
    {
        foreach (var pair in SpawnedPlayers)
        {
            if (pair.Key == _ref)
                return pair.Value;
        }

        Debug.LogError($"Player is not valid: {_ref}");
        return null;
    }
}
