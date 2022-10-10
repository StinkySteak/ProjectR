using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerHealth : NetworkBehaviour
{
    public ParticleSystem BloodFx;

    [Networked(OnChanged = nameof(OnHealthChanged)), HideInInspector] public int Health { get; set; }
    public int MaxHealth = 100;
    private HealthBarUI healthBarUI;

    public Team Team => PlayerManager.Instance.GetPlayer(Object.InputAuthority).Team;

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

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            healthBarUI = FindObjectOfType<HealthBarUI>();
            Health = MaxHealth;
            healthBarUI.SetMaxHealth(Health);
    }

    public bool CanTakeDamageFrom(PlayerRef _attacker)
    {
        if (Object == null || !Object.IsValid)
            return false;

        var attackerTeam = PlayerManager.Instance.GetPlayer(_attacker).Team;

        return attackerTeam != Team;
    }

    public void AddHealth(int _addition)
    {
        Health += _addition;

        Health = Mathf.Clamp(Health, 0, 100);
        healthBarUI.SetHealth(Health);
    }

    public void ApplyDamage(int _damage, PlayerRef _attacker)
    {
        Health -= _damage;
        healthBarUI.SetHealth(Health);

        if (Health <= 0)
        {
            LevelManager.Instance.OnPlayerDespawned(Object);
        }
    }
}
