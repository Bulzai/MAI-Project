using UnityEditor.SceneManagement;
using UnityEngine;

[DisallowMultipleComponent]
public class HoverHighlight : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float popScale = 1.30f;
    [SerializeField] private float popDuration = 0.10f;
    [SerializeField] private float pulseAmplitude = 0.05f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float lerpSpeed = 8f;
    [SerializeField] private float brightenAmount = 0.4f; // how much closer to white

    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Color originalColor;

    private int hoverCount = 0;
    private float hoverStartTime = 0f;
    private bool animating = false;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (!sr) { enabled = false; return; }
        originalScale = transform.localScale;
        originalColor = sr.color;
    }



    void Update()
    {
        if (!animating) return;

        float dt = Time.deltaTime;

        if (hoverCount > 0)
        {
            float t = Time.time - hoverStartTime;
            float baseTarget = (t < popDuration)
                ? Mathf.Lerp(popScale, hoverScale, t / popDuration)
                : hoverScale;

            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            float targetScale = baseTarget * pulse;

            // Scale
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScale, lerpSpeed * dt);

            // Color brighten
            Color targetColor = Color.Lerp(originalColor, Color.white, brightenAmount);
            sr.color = Color.Lerp(sr.color, targetColor, lerpSpeed * dt);
        }
        else
        {
            // Reset scale & color
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, lerpSpeed * dt);
            sr.color = Color.Lerp(sr.color, originalColor, lerpSpeed * dt);
            // Stop animating when very close
            if ((transform.localScale - originalScale).sqrMagnitude < 0.00001f &&
                ((Vector4)(sr.color - originalColor)).sqrMagnitude < 0.00001f)
            {
                transform.localScale = originalScale;
                sr.color = originalColor;
                animating = false;
            }
        }
    }

    public void AddHover()
    {
        if (!enabled) return;
        hoverCount++;
        if (hoverCount == 1)
        {
            hoverStartTime = Time.time;
            animating = true;
        }
    }

    public void RemoveHover()
    {
        if (!enabled) return;
        if (hoverCount > 0) hoverCount--;
        if (hoverCount == 0) animating = true;
    }

    void OnDisable()
    {
        hoverCount = 0;
        transform.localScale = originalScale;
        if (sr) sr.color = originalColor;
        animating = false;
    }
}
