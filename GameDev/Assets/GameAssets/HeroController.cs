using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotSpeed = 150f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        // maju / mundur mengikuti forward hero
        transform.Translate(transform.forward * v * moveSpeed * Time.deltaTime, Space.World);

        // rotate di sumbu Y
        transform.Rotate(0f, h * rotSpeed * Time.deltaTime, 0f);
    }
}
