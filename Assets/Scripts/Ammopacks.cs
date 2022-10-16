using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Ammopacks : NetworkBehaviour
{
    public int Magazine = 3;


    private void OnTriggerEnter(Collider other)
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
        playerCollect.OnAmmopackCollected(Magazine);

        Runner.Despawn(Object);
    }
}
