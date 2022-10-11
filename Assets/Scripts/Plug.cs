using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;
using TMPro;

public enum MoveType
{
    Idle,
    Forward,
    Backward
}

/// <summary>
/// class for managing cart movement, not to be confused with Cart.cs
/// </summary>
/// 
public class Plug : NetworkBehaviour
{
    public static Plug Instance { get; set; }

    readonly List<KCC> KCCOnTop = new();
    readonly List<KCC> LeavingKCC = new();

    public List<Transform> Waypoints;

    public NetworkRigidbody NetworkRigidbody;

    bool LastTargetReached => LastWaypointCount >= Waypoints.Count - 1;


    [Networked, HideInInspector] private int LastWaypointCount { get; set; }

    public float BaseForwardSpeed;
    public float BaseBackwardSpeed;

    public float MinDistanceToReachPoint = 2;

    public float PlayerPlatformSpeed = 10;

    public bool TryGetDistance(out float distance)
    {
        if (LastTargetReached) // no more target available
        {
            distance = -1;
            return false;
        }

        distance = Vector3.Distance(transform.position, Waypoints[LastWaypointCount + 1].position);
        return true;
    }

    MoveType MovingType { get; set; }
    int PushingPlayer { get; set; }

    Vector3 LastKinematicVelocity { get; set; }


    private void Awake()
    {
        Instance = this;
        MovingType = MoveType.Idle;
    }

    public void SetMove(MoveType _type)
    {
        MovingType = _type;
    }
    public void SetPushingPlayer(int _count)
    {
        PushingPlayer = _count;
    }

    float GetForwardSpeed()
    {
        return PushingPlayer * BaseForwardSpeed;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        LastKinematicVelocity = default;

        switch (MovingType)
        {
            case MoveType.Idle:
                NetworkRigidbody.Rigidbody.velocity = default;

                break;
            case MoveType.Forward:
                MoveForward();
                break;
            case MoveType.Backward:
                MoveBackward();
                break;
            default:
                break;
        }

        CheckIfDistanceIsReached();

        LeavingKCC.Clear();

        foreach (var kcc in KCCOnTop)
        {
            if (kcc.transform.position.y < transform.position.y)
                LeavingKCC.Add(kcc);

            kcc.SetExternalVelocity(LastKinematicVelocity);
        }

        foreach (var kcc in LeavingKCC)
        {
            KCCOnTop.Remove(kcc);
        }


    }

    void CheckIfDistanceIsReached()
    {
        float dist = Vector3.Distance(transform.position, Waypoints[LastWaypointCount + 1].position);

        if (dist <= MinDistanceToReachPoint)
            OnPointReached();
    }

    void OnPointReached()
    {
        LastWaypointCount++;

        if (LastTargetReached)
        {
            LevelManager.Instance.EndGame(Team.ISP);
            return;
        }

        LevelManager.Instance.OnPointReached();
    }

    private void MoveForward()
    {
        if(LastWaypointCount >= Waypoints.Count - 1)
            return;

        var direction = Waypoints[LastWaypointCount + 1].position - transform.position;

        var velocity = direction.normalized * GetForwardSpeed();

        var nextPos = velocity + NetworkRigidbody.Rigidbody.position;

        LastKinematicVelocity = velocity * PlayerPlatformSpeed;

        NetworkRigidbody.Rigidbody.MovePosition(nextPos);
    }

    private void MoveBackward()
    {
        var velocity = Vector3.zero * BaseBackwardSpeed;

        NetworkRigidbody.Rigidbody.velocity = velocity;
    }
    public void OnPlayerCollisionEnter(KCC collision)
    {
        if (collision.transform.position.y > transform.position.y) //is above
        {
            if (KCCOnTop.Contains(collision))
                return;

            KCCOnTop.Add(collision);
        }
    }
    public void OnPlayerCollisionExit(KCC collision)
    {
        if (KCCOnTop.Contains(collision))
        {
            KCCOnTop.Remove(collision);
        }
    }
}
