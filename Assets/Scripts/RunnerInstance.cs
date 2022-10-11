using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;

/// <summary>
/// Used for to prevent NULL runner & re-establish new runner
/// </summary>
public class RunnerInstance : Singleton<RunnerInstance>
{
    /// <summary>
    /// Main Active Runner
    /// </summary>
    public static NetworkRunner NetworkRunner => Instance.GetRunner();
    public static NetworkRunner FreshRunner => Instance.GetFreshRunner();
    static NetworkRunner Runner { get; set; }

    /// <summary>
    /// ONLY USE THIS FOR JOINING SESSION LOBBY
    /// </summary>
    /// <returns></returns>
    NetworkRunner GetFreshRunner()
    {
        Destroy(Runner);

        Runner = transform.AddComponent<NetworkRunner>();

        print(Runner == null);

        return Runner;
    }

    NetworkRunner GetRunner()
    {
        if (!TryGetComponent(out NetworkRunner runner))
        {
            Runner = gameObject.AddComponent<NetworkRunner>();
        }

        if (!Runner.TryGetComponent(out NetworkEvents events))
            Runner.AddComponent<NetworkEvents>();

        return Runner;
    }
}
