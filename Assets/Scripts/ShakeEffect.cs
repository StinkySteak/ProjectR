using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    public static ShakeEffect Instance;
    public AnimationCurve Curve;

    float RemainingDuration;
    float ElapsedTime;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        RemainingDuration = 0;
        StopCoroutine(ShakeCo());
    }

    public void Shake(float duration)
    {
        var prevDuration = RemainingDuration;

        RemainingDuration = duration;

        if (prevDuration > 0)
        {
            ElapsedTime = 0;
            return;
        }

        StartCoroutine(ShakeCo());
    }

    IEnumerator ShakeCo()
    {
        Vector3 startPos = transform.localPosition;

        ElapsedTime = 0;

        while (ElapsedTime < RemainingDuration)
        {
            ElapsedTime += Time.deltaTime;

            var strength = Curve.Evaluate(ElapsedTime / RemainingDuration);

            transform.localPosition = startPos + Random.insideUnitSphere * strength;

            yield return null;
        }

        RemainingDuration = 0;
        transform.localPosition = Vector3.zero;
    }
}
