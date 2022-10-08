using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;

public class PlayerCollision : NetworkBehaviour
{
    public KCC KCC;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
            return;

        KCC.OnCollisionEnter += KCC_OnCollisionEnter;
        KCC.OnCollisionExit += KCC_OnCollisionExit;
    }

    private void KCC_OnCollisionExit(KCC arg1, KCCCollision arg2)
    {
        if (arg2.Collider == null)
            return;

        if (!arg2.Collider.CompareTag("Cart"))
            return;

        if (arg2.Collider.TryGetComponent(out Plug plug))
        {
            plug.OnPlayerCollisionExit(KCC);
        }
    }

    private void KCC_OnCollisionEnter(KCC arg1, KCCCollision arg2)
    {
        if (arg2.Collider == null)
            return;

        if (!arg2.Collider.CompareTag("Cart"))
            return;

        if (arg2.Collider.TryGetComponent(out Plug plug))
        {
            print("plug");
            plug.OnPlayerCollisionEnter(KCC);
        }
    }
}
