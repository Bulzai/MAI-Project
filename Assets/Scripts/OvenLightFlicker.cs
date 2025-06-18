using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenLightFlicker : MonoBehaviour
{
    public Color colorA = new Color(1f, 1f, 0.5f); // Yellowish
    public Color colorB = new Color(1f, 0.2f, 0.1f); // Reddish
    public float flickerSpeed = 1.0f; // Flicker cycle speed
    public float intensity = 1.0f;

    private Material spriteMaterial;
    private float time;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteMaterial = sr.material;
        }
    }

    void Update()
    {
        if (spriteMaterial != null)
        {
            time += Time.deltaTime * flickerSpeed;
            Color lerpedColor = Color.Lerp(colorA, colorB, Mathf.PingPong(time, 1));
            spriteMaterial.SetColor("_EmissionColor", lerpedColor * intensity);
        }
    }
}
