using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
    [Networked, HideInInspector] public string Nickname { get; set; }
    [Networked, HideInInspector] public int Kill { get; set; }
    [Networked, HideInInspector] public int Death { get; set; }
    [Networked, HideInInspector] public Team Team { get; set; }

    public override void Spawned()
    {
        PlayerManager.Instance.AddPlayer(Object.InputAuthority, this);

        print($"Player Spawned: {Nickname}");
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
