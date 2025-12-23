using UnityEngine;

[DisallowMultipleComponent]
public class ExtinguisherPulsingEffect : MonoBehaviour
{
    private float baseScale = 1.15f;
    private float pulseAmplitude = 0.6f;
    private float pulseSpeed = 2f;
    private float lerpSpeed = 8f;

    private Vector3 originalScale;
    private Color originalColor;

    float phaseOffset;   // different per instance

    void Awake()
    {
        originalScale = transform.localScale;

        // random start time/phase
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 0..1..0 pulse
        float t = (Mathf.Sin(Time.time * pulseSpeed + phaseOffset) + 1f) * 0.5f; // 0..1

        // scale factor: always >= 1
        float targetScaleFactor = baseScale + t * pulseAmplitude; // min = baseScale, max = baseScale + pulseAmplitude

        Vector3 targetScale = originalScale * targetScaleFactor;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, lerpSpeed * dt);
    }
}