using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeInAndLife : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;
    private float lifetime;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float blinkDuration = 3;

    private bool collected = false;

    public void Init(float lifetimeSeconds)
    {
        lifetime = lifetimeSeconds;
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        // Start with alpha 0
        Color c = originalColor;
        c.a = 0f;
        sr.color = c;

        StartCoroutine(FadeInThenLife());
    }

    public void MarkAsCollected ()
    {
        collected = true;
    }

    private void MarkAsNotCollected()
    {
        int round = Object.FindAnyObjectByType<RoundController>().currentRound;

        string data = $"{round+1},ExtinguisherCollection,{transform.position}:-999";
        TestingLogger.LogToCSV(data);

        Debug.Log($"Extinguisher Pickup: extinguisher not collected at {transform.position}.");
    }

    private IEnumerator FadeInThenLife()
    {
        // Fade in
        float t = 0f;
        while (t < fadeInDuration)
        {
            float a = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
            t += Time.deltaTime;
            yield return null;
        }
        sr.color = originalColor;

        // Stay visible until blink time
        float waitTime = lifetime - fadeInDuration - blinkDuration;
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        // Blink for last 3s
        float blinkTime = 0f;
        while (blinkTime < blinkDuration)
        {
            float ping = Mathf.PingPong(blinkTime * BlinkSpeed(blinkTime, blinkDuration), 1f);
            float a = Mathf.Lerp(0.1f, 1f, ping);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);

            blinkTime += Time.deltaTime;
            yield return null;
        }

        if (!collected) MarkAsNotCollected();

        Destroy(gameObject);
    }

    private float BlinkSpeed(float time, float blinkDuration)
    {
        return 2f + (time / blinkDuration) * 8f; // accelerate blinking
    }
}
