using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float Lifetime = 0.1f;

    float LifetimeCounter;

    public LineRenderer LineRenderer;

    private void Start()
    {
        Invoke(nameof(Destroy), Lifetime);

        //  LifetimeCounter = Lifetime;
    }

    private void Update()
    {
        LifetimeCounter += Time.deltaTime;

        var percentage = LifetimeCounter / Lifetime;

        var a = 255 - (byte)(255 * percentage);

        var color = new Color32(255, 255, 0, (byte)a);

        var newMat = LineRenderer.material;
        newMat.color = color;
        newMat.SetColor("_EmissionColor", color);

        LineRenderer.material = newMat;
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

}
