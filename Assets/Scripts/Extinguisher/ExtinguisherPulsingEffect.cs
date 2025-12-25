using UnityEngine;

[DisallowMultipleComponent]
public class ExtinguisherPulsingEffect : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float lerpSpeed = 8f;
    [SerializeField] private float maxScaleMultiplier = 1.2f;

    private Vector3 originalScale;
    private float phaseOffset;

    private void Awake()
    {
        originalScale = transform.localScale;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Sin wave mapped from [-1,1] ? [0,1]
        float t = (Mathf.Sin(Time.time * pulseSpeed + phaseOffset) + 1f) * 0.5f;

        // Interpolate ONLY between original and max scale
        Vector3 targetScale = Vector3.Lerp(
            originalScale,
            originalScale * maxScaleMultiplier,
            t
        );

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            lerpSpeed * dt
        );
    }
}
