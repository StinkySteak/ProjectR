using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

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

    [Networked, HideInInspector] public float InAccuracy { get; set; }

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
        public int Tick;

        public static FireData Create(bool _isHit, Vector3 _pos)
        {
            return new FireData() { IsHit = _isHit, LastHitPos = _pos, Tick = RunnerInstance.NetworkRunner.Simulation.Tick };
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

    int LastShotTick;
    bool BulletTrailSpawned;

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!hasState)
            return;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

    bool IsFirstShot()
    {
        return !BulletTrailSpawned;
    }

    static void OnFire(Changed<Weapon> changed)
    {
        if (!changed.Behaviour.IsFirstShot())
            return;


        changed.Behaviour.BulletTrailSpawned = true;
        changed.Behaviour.RemoteMuzzleFlashFx.Play();
        changed.Behaviour.MuzzleFlashFx.Play();

        changed.Behaviour.PlayAudio();

        changed.Behaviour.SpawnBulletTracer();
        changed.Behaviour.SpawnBulletImpact();

        changed.Behaviour.BulletTrailSpawned = true;

        if (changed.Behaviour.Object.HasInputAuthority)
            changed.Behaviour.ShakeCamera();
    }

    void PlayAudio()
    {
        AudioSource.pitch = 1 + PitchCount;
        AudioSource.PlayOneShot(AudioClip);
    }
    void ShakeCamera()
    {
        print("Shake");
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
            PitchCount -= PitchDecreasePerTick * Runner.DeltaTime;

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
            InAccuracy -= InAccuracyDecrease * Runner.DeltaTime;

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
        IsReloading = false;

        var remainingBullet = CurrentBullet;

        if (TotalAmmo >= BulletPerMagazine)
        {
            CurrentBullet = BulletPerMagazine;

            TotalAmmo = (remainingBullet + TotalAmmo) - BulletPerMagazine;
            return;
        }

        CurrentBullet = TotalAmmo;

        TotalAmmo = 0;
    }

    public void InputFire()
    {
        if (CurrentBullet <= 0 || FireTimer.IsTrueRunning() || IsReloading)
            return;

        Fire();
    }
    void Fire()
    {
        var AbleToFire = Runner.Simulation.Tick > LastShotTick + FireRate * Runner.Simulation.Config.TickRate;

        //   print($"IsNotPredicted: {AbleToFire} => {Runner.Simulation.Tick} > {LastShotTick} + {FireRate * Runner.Simulation.Config.TickRate}");

        //   if (!Runner.tick)
        //       return;

        //   if (!AbleToFire)
        //       return;

        FireTimer = TickTimer.CreateFromSeconds(Runner, FireRate);

        //   print(InAccuracy);

        var direction = ProjectilePoint.forward;

        InAccuracy += InAccuracyIncreasePerShot;

        if (InAccuracy >= MaxInAccuracy)
            InAccuracy = MaxInAccuracy;

        var x = Random.Range(-InAccuracy, InAccuracy);
        var y = Random.Range(-InAccuracy, InAccuracy);
        var z = Random.Range(-InAccuracy, InAccuracy);

        var random = new Vector3(x, y, z);

        if (Runner.LagCompensation.Raycast(ProjectilePoint.position, direction + random, MaxDistance, Object.InputAuthority, out var hit, HitMask, HitOptions.IncludePhysX))
        {
            LastFireData = FireData.Create(true, hit.Point);

            if (hit.GameObject.CompareTag("Player"))
            {
                if (hit.GameObject.transform.parent.TryGetComponent(out PlayerHealth health))
                {
                    if (health.CanTakeDamageFrom(Object.InputAuthority))
                        health.ApplyDamage(BaseDamage, Object.InputAuthority);
                }
            }
        }
        else
        {
            var hitPos = ProjectilePoint.position + ((ProjectilePoint.forward * MaxDistance) + (direction + random * MaxDistance));

            LastFireData = FireData.Create(false, hitPos);
        }


        CurrentBullet--;



        LastShotTick = Runner.Simulation.Tick;

        if (!Runner.IsResimulation)
        {
            PitchCount += IncreasePitchPerShot;
            //  print($"{Runner.IsResimulation} {Runner.IsForward} {Runner.IsFirstTick} {Runner.TicksExecuted}");

            BulletTrailSpawned = false;
            //  print("BulletTrailSpawned : " + BulletTrailSpawned);
        }

    }
}

public static class NetworkUtil
{
    public static bool IsTrueRunning(this TickTimer _timer)
    {
        return !_timer.ExpiredOrNotRunning(RunnerInstance.NetworkRunner);
    }
}