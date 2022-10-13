using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerHealth : NetworkBehaviour
{
    PlayerSetup PlayerSetup;
    public static PlayerHealth LocalPlayer;

    public ParticleSystem BloodFx;

    [Networked(OnChanged = nameof(OnHealthChanged)), HideInInspector] public int Health { get; set; }
    public int MaxHealth = 100;
    private HealthBarUI healthBarUI;

    public AudioClip[] OnDamagedSfx;
    AudioClip GetRandomDamageSfx => OnDamagedSfx[Random.Range(0, OnDamagedSfx.Length - 1)];

    static void OnHealthChanged(Changed<PlayerHealth> changed)
    {
        var newHealth = changed.Behaviour.Health;
        changed.LoadOld();
        var oldHealth = changed.Behaviour.Health;

        //  print($"Health is changed from {oldHealth} to {newHealth}");

        if (newHealth < oldHealth)
        {
            //play fx
            changed.Behaviour.PlayerSetup.AudioSource.PlayOneShot(changed.Behaviour.GetRandomDamageSfx);
            changed.Behaviour.BloodFx.Play();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (PlayerSetup == null)
            return;

        PlayerSetup.AudioSource.PlayOneShot(GetRandomDamageSfx);
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
        PlayerSetup = GetComponent<PlayerSetup>();

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
            if (PlayerManager.Instance.TryGetPlayer(_attacker, out var player))
                player.Kill++;

            LevelManager.Instance.OnPlayerDespawned(Object);
        }
    }
}
