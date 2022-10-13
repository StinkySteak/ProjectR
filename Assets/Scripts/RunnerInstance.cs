using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;

/// <summary>
/// Used for to prevent NULL runner & re-establish new runner
/// </summary>
public class RunnerInstance : SceneSingleton<RunnerInstance>
{
    /// <summary>
    /// Main Active Runner
    /// </summary>
    public static NetworkRunner NetworkRunner => Instance.GetRunner();
    /// <summary>
    /// ONLY USE THIS FOR JOINING SESSION LOBBY
    /// </summary>
    /// <returns></returns>
    public static NetworkRunner FreshRunner => Instance.GetFreshRunner();
    public NetworkRunner InstanceFreshRunner => GetFreshRunner();
    static NetworkRunner Runner { get; set; }
    public static RunnerInstance GetSafeInstance => SafeInstance();
    static RunnerInstance SafeInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("Runner Instance");

            Instance = go.AddComponent<RunnerInstance>();
        }

        return Instance;
    }


    NetworkRunner GetFreshRunner()
    {
        Destroy(Runner);

        if (!transform.TryGetComponent(out NetworkRunner runner))
        {
            Runner = gameObject.AddComponent<NetworkRunner>();
        }

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
