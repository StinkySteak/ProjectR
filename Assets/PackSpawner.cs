using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PackSpawner : NetworkBehaviour
{
    public float SpawnInterval = 10;

    public Vector3 SpawnOffset;
    public NetworkObject PackPrefab;

    NetworkObject SpawnedPack { get; set; }

    [Networked] TickTimer SpawnTimer { get; set; }

    bool IsPackExist => SpawnedPack != null && SpawnedPack.gameObject != null && SpawnedPack.IsValid;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (IsPackExist) // no need to spawn another one
            return;

        if (SpawnTimer.IsTrueRunning())
            return;

        SpawnPack();
    }

    void SpawnPack()
    {
        SpawnTimer = TickTimer.CreateFromSeconds(Runner, SpawnInterval);

        SpawnedPack = Runner.Spawn(PackPrefab, transform.position + SpawnOffset, Quaternion.identity);
    }

    public override void Render()
    {
        //show UI status here
    }
}
