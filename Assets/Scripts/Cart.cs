using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// class to manage 
/// </summary>
public class Cart : NetworkBehaviour
{
    public static Cart Instance;

    private void Awake()
    {
        Instance = this;
    }

    Plug Plug;

    readonly public List<PlayerRef> ClosePlayers = new();

    public float DurationToBackward;

    public float OverlapRadius;

    [Networked, HideInInspector] public bool IsPushing { get; set; }
    [Networked, HideInInspector] public bool IsContested { get; set; }
    [Networked, HideInInspector] public int ISPAmount { get; set; }
    [Networked, HideInInspector] public int HackerAmount { get; set; }

    public TickTimer CheckOverlapTimer { get; set; }

    TickTimer StartMovingBackwardTime { get; set; }
    private bool isGameStarted = true;

    public float CheckOverlapInterval = 0.1f;

    public override void Spawned()
    {
        Plug = GetComponent<Plug>();
        isGameStarted = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, OverlapRadius);
    }
    public override void FixedUpdateNetwork()
    {
        // 1. do overlap, and store them every frame(skip if not player tagged)
        //  2. add to closePlayers List
        //  3. check team each player and set as contested if needs to
        //  4. calculate speed

        ProgressCart();
        OverlapClosePlayers();
    }

    void ProgressCart()
    {
        //push cart here
        Plug.SetMove(GetMove());
        Plug.SetPushingPlayer(ISPAmount);
    }

    MoveType GetMove()
    {
        if (IsPushing && !IsContested)
            return MoveType.Forward;

        if (IsContested)
            return MoveType.Idle;

        if (!StartMovingBackwardTime.IsTrueRunning())
        {
            if (isGameStarted == false)
            {
                isGameStarted = false;
                return MoveType.Backward;
            }
        }


        return MoveType.Idle;
    }

    void OverlapClosePlayers()
    {
        if (CheckOverlapTimer.IsTrueRunning())
            return;

        IsContested = false;
        IsPushing = false;
        ClosePlayers.Clear();

        HackerAmount = 0;
        ISPAmount = 0;

        var colliders = Physics.OverlapSphere(transform.position, OverlapRadius, ~LayerMask.GetMask("Player")); // <-- set this to player mask

        foreach (var col in colliders)
        {
            if (!col.CompareTag("Player"))
                continue;

            var playerRef = col.GetComponentInParent<NetworkObject>().InputAuthority;

            if (ClosePlayers.Contains(playerRef)) // 1 player may contain multiple colliders
                continue;

            ClosePlayers.Add(playerRef);

            if (PlayerManager.Instance.TryGetPlayerTeam(playerRef, out Team _team))
            {
                if (_team == Team.ISP)
                {
                    ISPAmount++;
                    IsPushing = true;
                }
                else
                {
                    HackerAmount++;
                }
            }
        }

        if (IsPushing)
        {
            StartMovingBackwardTime = TickTimer.CreateFromSeconds(Runner, DurationToBackward);
        }

        SetContestion();

        CheckOverlapTimer = TickTimer.CreateFromSeconds(Runner, CheckOverlapInterval);
    }
    void SetContestion()
    {
        bool isThereISP = false;

        foreach (var player in ClosePlayers)
        {
            if (PlayerManager.Instance.TryGetPlayerTeam(player, out var team)) // there is a hacker, set it as contestd
            {
                if (team == Team.ISP)
                {
                    isThereISP = true;
                    continue;
                }
                if (isThereISP && team == Team.Hacker) // if hacker & there is ISP
                {
                    IsContested = true;
                    return;
                }
            }
        }
    }
}
