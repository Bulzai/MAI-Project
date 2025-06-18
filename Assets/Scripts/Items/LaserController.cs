using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Time in seconds for the laser to fade in fully.")]
    public float fadeInDuration = 1.0f;
    [Tooltip("Time in seconds for the laser to stay fully visible before fading out.")]
    public float activeDuration = 1.0f;
    [Tooltip("Time in seconds for the laser to fade out fully.")]
    public float fadeOutDuration = 1.0f;
    [Tooltip("Time in seconds the laser is fully invisible and inactive before restarting.")]
    public float cooldownDuration = 1.0f;

    [Tooltip("Blink speed during fade out.")]
    public float blinkInterval = 0.1f;


    [Tooltip("Laser Damage")]
    public int laserDamage = 50; 
    private SpriteRenderer[] spriteRenderers;
    private BoxCollider2D boxCollider;
    private bool isActive = false;

    private void Start()
    {
        // Cache all SpriteRenderers on this object and its children
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        // Ensure BoxCollider2D is set as a trigger for 2D collision detection
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;

    }
    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += StartLaser;
        GameEvents.OnMainGameStateExited += StopLaser;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= StartLaser;
        GameEvents.OnMainGameStateExited -= StopLaser;
    }


    private void StartLaser()
    {
        GetComponent<LaserController>().enabled = true;
        // Start invisible and disable collider
        SetAlpha(0f);
        boxCollider.enabled = false;
        // Start the repeating fade-in/out sequence when enabled
        StartCoroutine(FadeLoop());
    }
    private void StopLaser()
    {
        SetAlpha(1f);
        StopCoroutine(FadeLoop());
    }
    private IEnumerator FadeLoop()
    {
        while (true)
        {
            // Fade in
            yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

            // Activate laser
            isActive = true;
            boxCollider.enabled = true;

            // Wait while fully visible
            yield return new WaitForSeconds(activeDuration);

         

            // Fade out with blinking
            yield return StartCoroutine(BlinkFadeOut(fadeOutDuration));


            // Deactivate laser
            isActive = false;
            boxCollider.enabled = false;

            // Fully invisible during cooldown
            SetAlpha(0f);
            yield return new WaitForSeconds(cooldownDuration);
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(endAlpha);
    }

    private IEnumerator BlinkFadeOut(float duration)
    {
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            visible = !visible;
            SetAlpha(visible ? 1f : 0f);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {
                Debug.Log("Player hit by Laser." + laserDamage + " damage took");
                player.TakeDamage(laserDamage, true);
            }
            else
            {
                Debug.Log("No PlayerHealthSystem Script");
            }
        }
    }
}
