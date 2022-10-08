using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public Material Material;

    static Color32 MinColor = new(255, 255, 0, 80);
    static Color32 MaxColor = new(0, 255, 255, 80);
    static float ColorLerpSpeed = 3;

    public Collider Collider;

    public void Update()
    {
        float t = (Mathf.Sin(Time.time * ColorLerpSpeed) + 1) / 2.0f;

        Material.color = Color.Lerp(MinColor, MaxColor, t);
    }
}
