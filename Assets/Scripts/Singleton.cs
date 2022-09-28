using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// base singleton pattern class
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; set; }

    public virtual void Awake()
    {
        if(Instance == null)
        {
            Instance = GetComponent<T>();
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }
}
