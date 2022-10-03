using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float Lifetime = 0.1f;

    public LineRenderer LineRenderer;



    private void Start()
    {
        Invoke(nameof(Destroy), Lifetime);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

}
