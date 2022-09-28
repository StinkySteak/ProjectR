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
    static NetworkRunner Runner { get; set; }

    NetworkRunner GetRunner()
    {
        if (Runner == null)
        {
            Runner = gameObject.AddComponent<NetworkRunner>();
        }

        if (!Runner.TryGetComponent(out NetworkEvents events))
            Runner.AddComponent<NetworkEvents>();

        return Runner;
    }
}
