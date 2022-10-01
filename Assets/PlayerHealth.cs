using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerHealth : NetworkBehaviour
{
    [Networked, HideInInspector] public int Health { get; set; }
    public int MaxHealth = 100;

    public Team Team => PlayerManager.Instance.GetPlayer(Object.InputAuthority).Team;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            Health = MaxHealth;
    }

    public bool CanTakeDamageFrom(PlayerRef _attacker)
    {
        var attackerTeam = PlayerManager.Instance.GetPlayer(_attacker).Team;

        return attackerTeam != Team;
    }

    public void ApplyDamage(int _damage, PlayerRef _attacker)
    {
        Health -= _damage;

        if (Health <= 0)
        {

        }
    }
}
