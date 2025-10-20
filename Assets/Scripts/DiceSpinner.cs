// DiceSpinner.cs
using System.Collections;
using UnityEngine;

public class DiceSpinner : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinDuration = 1.0f;       
    public float maxSpinSpeed = 2160f;      
    public float minSpinSpeed = 540f;       
    public AnimationCurve easeOut = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float axisWobble = 6f;           // higher = more tumble

    bool spinning = false;

    public void Roll()
    {
        if (!spinning) StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        spinning = true;

        Vector3 axis = Random.onUnitSphere.normalized;
        transform.Rotate(Random.onUnitSphere, Random.Range(0f, 30f), Space.World);

        float t = 0f;
        while (t < spinDuration)
        {
            float k = t / spinDuration;                   
            float target = Mathf.Lerp(maxSpinSpeed, 0f,   // ease speed down
                                      easeOut.Evaluate(k));
            float speed = Mathf.Max(target, minSpinSpeed);

            Vector3 targetAxis = Random.onUnitSphere;
            axis = Vector3.Slerp(axis, targetAxis, axisWobble * Time.deltaTime).normalized;

            transform.Rotate(axis, speed * Time.deltaTime, Space.World);

            t += Time.deltaTime;
            yield return null;
        }

        // snap to a random final orientation so every face is fair
        transform.rotation = Random.rotation;

        spinning = false;
    }
}
