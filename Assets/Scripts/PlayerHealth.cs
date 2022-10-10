using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerHealth : NetworkBehaviour
{
    public static PlayerHealth LocalPlayer;

    public ParticleSystem BloodFx;

    [Networked(OnChanged = nameof(OnHealthChanged)), HideInInspector] public int Health { get; set; }
    public int MaxHealth = 100;
    private HealthBarUI healthBarUI;

    static void OnHealthChanged(Changed<PlayerHealth> changed)
    {
        var newHealth = changed.Behaviour.Health;
        changed.LoadOld();
        var oldHealth = changed.Behaviour.Health;

        //  print($"Health is changed from {oldHealth} to {newHealth}");

        if (newHealth < oldHealth)
        {
            //play fx
            changed.Behaviour.BloodFx.Play();
        }
    }

    public Team GetTeam()
    {
        if (PlayerManager.Instance.TryGetPlayerTeam(Object.InputAuthority, out var team))
        {
            return team;
        }

        return Team.Invalid;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            Health = MaxHealth;

        if (Object.HasInputAuthority)
            LocalPlayer = this;
    }

    public bool CanTakeDamageFrom(PlayerRef _attacker)
    {
        if (Object == null || !Object.IsValid)
            return false;

        if (PlayerManager.Instance.TryGetPlayerTeam(_attacker, out var attackerTeam))
        {
            return attackerTeam != GetTeam();
        }
        return false;
    }

    public void AddHealth(int _addition)
    {
        Health += _addition;

        Health = Mathf.Clamp(Health, 0, 100);
    }

    public void ApplyDamage(int _damage, PlayerRef _attacker)
    {
        Health -= _damage;

        if (Health <= 0)
        {
            LevelManager.Instance.OnPlayerDespawned(Object);
        }
    }
}
