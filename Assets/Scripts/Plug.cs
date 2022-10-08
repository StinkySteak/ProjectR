using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;

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
    public List<KCC> KCCOnTop;

    public NetworkRigidbody NetworkRigidbody;

    [SerializeField] private PathingConfig path;
    List<Transform> Waypoints;
    private int LastWaypointCount;

    public float BaseForwardSpeed;
    public float BaseBackwardSpeed;

    public float MinDistanceToReachPoint = 2;

    public float PlayerPlatformSpeed = 10;

  //  [Tooltip("Used for if CollisionExit is not called, check distance instead")]
  //  public float RemoveCollisionMaxDistance = 2;

    List<KCC> LeavingKCC = new();

    MoveType MovingType { get; set; }
    int PushingPlayer { get; set; }

    Vector3 LastKinematicVelocity { get; set; }


    private void Awake()
    {
        Waypoints = path.GetPathPoints();
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

            print($"KCCOnTop: {KCCOnTop.Count} LastKinematicVelocity: {LastKinematicVelocity}");
            kcc.SetExternalVelocity(LastKinematicVelocity);
            print(kcc.FixedData.DynamicVelocity);
        }

        foreach (var kcc in LeavingKCC)
        {
            KCCOnTop.Remove(kcc);
        }
    }

    void CheckIfDistanceIsReached()
    {
        float dist = Vector3.Distance(transform.position, Waypoints[LastWaypointCount + 1].position);

        if (dist <= 2)
        {
            OnPointReached();
        }
    }

    void OnPointReached()
    {
        print("OnPointReached!");
        LevelManager.Instance.OnPointReached();
        LastWaypointCount++;
    }

    private void MoveForward()
    {
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
