using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Healthpacks : NetworkBehaviour
{
    public int HealAmount = 100;

    private void OnTriggerStay(Collider other)
    {
        if (Object == null)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (!other.CompareTag("Player"))
            return;

        OnCollect(other.transform.parent.GetComponent<PlayerCollect>());
    }

    void OnCollect(PlayerCollect playerCollect)
    {
        playerCollect.OnHealthpackCollected(HealAmount);

        Runner.Despawn(Object);
    }
}
