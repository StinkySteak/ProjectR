using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

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
            Runner = gameObject.AddComponent<NetworkRunner>();

        return Runner;
    }
}
