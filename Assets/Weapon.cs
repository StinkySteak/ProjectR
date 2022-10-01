using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Weapon : NetworkBehaviour
{
    public int BulletPerMagazine = 30;
    [Networked] public int CurrentBullet { get; set; } = 30;
    [Networked] public int TotalAmmo { get; set; } = 90;

    public float FireRate = 0.15f;

    public int BaseDamage = 30;

    public float ReloadDuration = 0.8f;
    TickTimer ReloadTimer { get; set; }
    [Networked(OnChanged = nameof(OnFire))] TickTimer FireTimer { get; set; }

    [Networked(OnChanged = nameof(OnStartReloading))] public bool IsReloading { get; set; }

    public float ReloadTime { get; set; }

    [Space]

    public LayerMask HitMask;

    public Transform ProjectilePoint;

    public float MaxDistance = 50;

    List<LagCompensatedHit> Hits = new();

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!hasState)
            return;

        if (Object.HasInputAuthority)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

    static void OnFire(Changed<Weapon> changed)
    {
        //play fire sfx & muzzle flash
    }

    static void OnStartReloading(Changed<Weapon> changed)
    {
        //play animation & sfx
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentBullet <= 0 && !IsReloading)
            InputReload();

        if (IsReloading && !ReloadTimer.IsTrueRunning())
            Reload();
    }

    public void InputReload()
    {
        if (IsReloading || CurrentBullet >= BulletPerMagazine || TotalAmmo <= 0)
            return;

        StartReload();
    }

    void StartReload()
    {
        IsReloading = true;
        ReloadTimer = TickTimer.CreateFromSeconds(Runner, ReloadDuration);
    }

    void Reload()
    {
        var remainingBullet = CurrentBullet;

        CurrentBullet = BulletPerMagazine;

        TotalAmmo = (remainingBullet + TotalAmmo) - BulletPerMagazine;

        IsReloading = false;
    }

    public void InputFire()
    {
        if (CurrentBullet <= 0 || FireTimer.IsTrueRunning() || IsReloading)
            return;

        Fire();
    }
    void Fire()
    {
        var hitCount = Runner.LagCompensation.RaycastAll(ProjectilePoint.position, ProjectilePoint.forward, MaxDistance, player: Object.InputAuthority, Hits, HitMask, true, HitOptions.IncludePhysX);

        print($"Fire: {hitCount} from {Object.InputAuthority}");

        foreach (var hit in Hits)
        {
            if (!hit.GameObject.CompareTag("Player"))
                continue;

            if (hit.GameObject.transform.parent.TryGetComponent(out PlayerHealth health))
            {
                if (health.CanTakeDamageFrom(Object.InputAuthority))
                {
                    health.ApplyDamage(BaseDamage, Object.InputAuthority);
                }
            }
        }

        FireTimer = TickTimer.CreateFromSeconds(Runner, FireRate);
        CurrentBullet--;
    }
}

public static class NetworkUtil
{
    public static bool IsTrueRunning(this TickTimer _timer)
    {
        return !_timer.ExpiredOrNotRunning(RunnerInstance.NetworkRunner);
    }
}