using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DeadZone : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (other.transform.parent.TryGetComponent(out NetworkObject obj))
        {
            if (obj == null || !obj.gameObject.activeInHierarchy || !obj.IsValid)
                return;

            LevelManager.Instance.OnPlayerDespawned(obj);
        }
    }
}
