using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEditor.Timeline;

public class Weapon : NetworkBehaviour
{
    public PlayerWeaponManager PlayerWeaponManager;

    public ParticleSystem MuzzleFlashFx;
    public ParticleSystem RemoteMuzzleFlashFx;
    public GameObject BulletTracer;
    public GameObject BulletImpact;

    [Space]

    public int BulletPerMagazine = 30;
    [Networked] public int CurrentBullet { get; set; } = 30;
    [Networked] public int TotalAmmo { get; set; } = 90;

    public float ShakeDuration;

    public float FireRate = 0.15f;

    public int BaseDamage = 30;

    public float MaxInAccuracy = 0.1f;
    public float MinInAccuracy = 0.01f;

    public float InAccuracyIncreasePerShot;

    public float InAccuracyDecrease;

    float InAccuracy;

    public float ReloadDuration = 0.8f;
    [Networked] TickTimer ReloadTimer { get; set; }
    [Networked] TickTimer FireTimer { get; set; }

    [Networked(OnChanged = nameof(OnStartReloading))] public bool IsReloading { get; set; }

    [Networked(OnChanged = nameof(OnFire))] FireData LastFireData { get; set; }

    public AudioSource AudioSource => PlayerWeaponManager.PlayerSetup.AudioSource;
    public AudioClip AudioClip;

    public struct FireData : INetworkStruct
    {
        public Vector3 LastHitPos;
        public bool IsHit;

        public static FireData Create(bool _isHit, Vector3 _pos)
        {
            return new FireData() { IsHit = _isHit, LastHitPos = _pos };
        }
    }

    public float IncreasePitchPerShot;
    public float PitchDecreasePerTick;

    float PitchCount = 0;

    public float ReloadTime { get; set; }

    [Space]

    public LayerMask HitMask;

    public Transform ProjectilePoint;
    public Transform BulletTracerPoint;

    public float MaxDistance = 50;

    readonly List<LagCompensatedHit> Hits = new();

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
        if (!changed.Behaviour.FireTimer.IsRunning)
            return;

        changed.Behaviour.RemoteMuzzleFlashFx.Play();
        changed.Behaviour.MuzzleFlashFx.Play();

        changed.Behaviour.PlayAudio();

        changed.Behaviour.SpawnBulletTracer();
        changed.Behaviour.SpawnBulletImpact();

        if (changed.Behaviour.Object.HasInputAuthority)
        {
            changed.Behaviour.ShakeCamera();
        }
    }
    void PlayAudio()
    {
        AudioSource.pitch = 1 + PitchCount;
        AudioSource.PlayOneShot(AudioClip);
    }
    void ShakeCamera()
    {
        ShakeEffect.Instance.Shake(ShakeDuration);
    }

    void SpawnBulletImpact()
    {
        if (!LastFireData.IsHit) return;

        var obj = Instantiate(BulletImpact, LastFireData.LastHitPos, Quaternion.identity);

        Destroy(obj, 2f);
    }

    void SpawnBulletTracer()
    {
        var obj = Instantiate(BulletTracer, BulletTracerPoint.transform.position, BulletTracerPoint.transform.localRotation);

        if (obj.TryGetComponent(out BulletTracer tracer))
        {
            tracer.LineRenderer.SetPosition(0, BulletTracerPoint.position);
            tracer.LineRenderer.SetPosition(1, LastFireData.LastHitPos);
        }

        Destroy(obj, 2f);
    }

    public override void Render()
    {
        if (!FireTimer.IsTrueRunning())
            PitchCount -= PitchDecreasePerTick;

        PitchCount = Mathf.Clamp(PitchCount, 0, 2);
    }

    static void OnStartReloading(Changed<Weapon> changed)
    {
        //play animation & sfx
    }

    public override void FixedUpdateNetwork()
    {
        // decrease InAccuracy if player is not shooting for a while
        if (!FireTimer.IsTrueRunning())
            InAccuracy -= InAccuracyDecrease;

        InAccuracy = Mathf.Clamp(InAccuracy, MinInAccuracy, MaxInAccuracy);

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
        var direction = ProjectilePoint.forward;

        InAccuracy += InAccuracyIncreasePerShot;

        if (InAccuracy >= MaxInAccuracy)
            InAccuracy = MaxInAccuracy;

        var x = Random.Range(-InAccuracy, InAccuracy);
        var y = Random.Range(-InAccuracy, InAccuracy);
        var z = Random.Range(-InAccuracy, InAccuracy);

        var random = new Vector3(x, y, z);

        var hitCount = Runner.LagCompensation.RaycastAll(ProjectilePoint.position, direction + random, MaxDistance, player: Object.InputAuthority, Hits, HitMask, true, HitOptions.IncludePhysX);

        var hitPos = ProjectilePoint.position + ((ProjectilePoint.forward * MaxDistance) + (direction + random * MaxDistance));


        foreach (var hit in Hits)
        {
            hitPos = hit.Point;

            LastFireData = FireData.Create(true, hitPos);

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

        print(InAccuracy);

        if (hitCount <= 0)
            LastFireData = FireData.Create(false, hitPos);

        FireTimer = TickTimer.CreateFromSeconds(Runner, FireRate);
        CurrentBullet--;

        PitchCount += IncreasePitchPerShot;
    }
}

public static class NetworkUtil
{
    public static bool IsTrueRunning(this TickTimer _timer)
    {
        return !_timer.ExpiredOrNotRunning(RunnerInstance.NetworkRunner);
    }
}