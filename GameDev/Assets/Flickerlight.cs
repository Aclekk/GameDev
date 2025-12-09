using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FlickerLight : MonoBehaviour
{
    public Light lamp;
    public float minTime = 0.05f;
    public float maxTime = 0.2f;

    void Start()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            lamp.enabled = !lamp.enabled; // toggle nyala/mati
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        }
    }
}

