using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TarodevController;

[DefaultExecutionOrder(100)]
public class PlayerItemHandler : MonoBehaviour
{
    public static event Action OnAuraExpires;
    public static event Action OnAuraPickedUp;
    public static event Action OnRepelAuraActivated;
    public static event Action OnRepelAuraDeactivated;
    public static event Action OnOtherPlayerSlowed;
    public static event Action OnSpeedAuraActivated;
    public static event Action OnDamageAuraActivated;
    public static event Action OnConfusionAuraActivated;

    private Coroutine _activeSlowCo, _activeRepelCo, _activeDamageCo, _activeConfusionCo, _confusionBlinkCo;

    [Header("Slow Aura")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float auraRadius = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float slowPercent = 0.5f;
    [SerializeField] private float slowSeconds = 2f;
    [SerializeField] private float auraDuration = 10f;
    [SerializeField] private GameObject slowAuraVisual;

    [Header("Repel Aura")]
    [SerializeField] private float repelRadius = 2.5f;
    [SerializeField] private float repelSeconds = 5f;
    [SerializeField] private float repelKickSpeed = 14f;
    [SerializeField] private GameObject repelAuraVisual;
    private bool repelActive;

    [Header("Speed Aura (SELF ONLY)")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float speedDuration = 4f;

    [Header("Damage Aura")]
    [SerializeField] private float damageAuraRadius = 2.6f;
    [SerializeField] private int damagePerTick = 2;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float damageAuraDuration = 6f;
    [SerializeField] private GameObject damageAuraVisual;

    [Header("Confusion Aura")]
    [SerializeField] private float confusionAuraRadius = 2.5f;
    [SerializeField] private float confusionDuration = 3f; // Wie lange das OPFER verwirrt ist
    [SerializeField] private float confusionAuraDuration = 8f; // Wie lange die AURA aktiv bleibt
    [SerializeField] private GameObject confusionAuraVisual;
    [SerializeField] private ParticleSystem confusionEffect; // Das Teilchen-System aus deinem Snippet
    // ---------------- BLINK TUNING ----------------
    [Header("Aura Blink (End Warning)")]
    [Tooltip("Start blinking when remaining time <= this.")]
    [SerializeField] private float blinkStartSeconds = 2.0f;

    [Tooltip("Slowest blink interval at the beginning of the warning window.")]
    [SerializeField] private float blinkMaxInterval = 0.35f;

    [Tooltip("Fastest blink interval right before ending.")]
    [SerializeField] private float blinkMinInterval = 0.05f;

    [Tooltip("Controls how aggressively blink accelerates near the end (0->1).")]
    [SerializeField]
    private AnimationCurve blinkCurve =
        new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 0.1f),
            new Keyframe(1f, 1f)
        );

    // optional: keep handles so reset can stop them
    private Coroutine _slowBlinkCo, _repelBlinkCo, _damageBlinkCo;

    private void OnEnable() => GameEvents.OnMainGameStateExited += ResetAuras;

    private void OnDisable()
    {
        if (!repelActive) return;         

        repelActive = false;
        OnRepelAuraDeactivated?.Invoke();
        GameEvents.OnMainGameStateExited -= ResetAuras;

    }
    private void OnDestroy()
    {
        if (!repelActive) return;          

        repelActive = false;
        OnRepelAuraDeactivated?.Invoke();
    }

    public void ApplyItem(PickUpItem.ItemType itemType)
    {
        OnAuraPickedUp?.Invoke();
        switch (itemType)
        {
            case PickUpItem.ItemType.Slow:
                // Stoppe die alte, falls sie läuft
                if (_activeSlowCo != null) StopCoroutine(_activeSlowCo);
                _activeSlowCo = StartCoroutine(ApplySlowAura());
                break;

            case PickUpItem.ItemType.Repel:
                if (_activeRepelCo != null) StopCoroutine(_activeRepelCo);
                _activeRepelCo = StartCoroutine(EnableRepelAura());
                break;

            case PickUpItem.ItemType.Speed:
                var selfBuff = GetComponent<SlowDebuff>();
                if (selfBuff)
                {
                    // SlowDebuff sollte idealerweise intern das Refreshing handhaben
                    selfBuff.ApplySpeedModifier(speedMultiplier, speedDuration);
                    OnSpeedAuraActivated?.Invoke();
                }
                break;

            case PickUpItem.ItemType.Damage:
                if (_activeDamageCo != null) StopCoroutine(_activeDamageCo);
                _activeDamageCo = StartCoroutine(ApplyDamageAura());
                break;
            case PickUpItem.ItemType.Confusion:
                if (_activeConfusionCo != null) StopCoroutine(_activeConfusionCo);
                _activeConfusionCo = StartCoroutine(ApplyConfusionAura());
                break;
        }
    }

    // ---------- Slow aura ----------
    private IEnumerator ApplySlowAura()
    {
        slowAuraVisual.SetActive(true);
        OnOtherPlayerSlowed?.Invoke();

        // start blink coroutine (it will wait until the last blinkStartSeconds)
        StopBlink(ref _slowBlinkCo);
        _slowBlinkCo = StartCoroutine(BlinkVisual(
            slowAuraVisual,
            totalDuration: auraDuration,
            blinkStartAtSeconds: blinkStartSeconds,
            minInterval: blinkMinInterval,
            maxInterval: blinkMaxInterval,
            curve: blinkCurve
        ));

        float t = auraDuration;
        while (t > 0f)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, auraRadius);
            foreach (var h in hits)
            {
                if (!h || h.gameObject == gameObject) continue;
                if (!h.CompareTag(playerTag)) continue;

                var otherRb = h.attachedRigidbody ?? h.GetComponentInParent<Rigidbody2D>();
                if (!otherRb) continue;

                var slow = otherRb.GetComponent<SlowDebuff>();
                if (slow) slow.ApplySpeedModifier(slowPercent, slowSeconds);
                OnOtherPlayerSlowed?.Invoke();
            }

            t -= Time.deltaTime;
            yield return null;
        }

        StopBlink(ref _slowBlinkCo);
        slowAuraVisual.SetActive(false);
    }

    // ---------- Repel aura ----------
    private IEnumerator EnableRepelAura()
    {
        repelAuraVisual.SetActive(true);
        OnRepelAuraActivated?.Invoke();
        StopBlink(ref _repelBlinkCo);
        _repelBlinkCo = StartCoroutine(BlinkVisual(
            repelAuraVisual,
            totalDuration: repelSeconds,
            blinkStartAtSeconds: blinkStartSeconds,
            minInterval: blinkMinInterval,
            maxInterval: blinkMaxInterval,
            curve: blinkCurve
        ));

        float t = repelSeconds;
        while (t > 0f)
        {
            // Suche alle im Radius
            var hits = Physics2D.OverlapCircleAll(transform.position, repelRadius);
            foreach (var h in hits)
            {
                if (!h || h.gameObject == gameObject) continue;
                if (!h.CompareTag(playerTag)) continue;

                var otherCtrl = h.GetComponentInParent<PlayerController>();
                if (otherCtrl)
                {
                    Vector2 dir = (otherCtrl.transform.position - transform.position).normalized;
                    otherCtrl.AddImpulse(dir * repelKickSpeed);
                }
            }
            t -= Time.deltaTime;
            yield return null;
        }

        repelAuraVisual.SetActive(false);
        OnRepelAuraDeactivated?.Invoke();
    }

    /*private void OnCollisionEnter2D(Collision2D col)
    {
        if (!repelActive) return;
        if (!col.collider || !col.collider.CompareTag(playerTag)) return;
        if (col.gameObject == gameObject) return;

        var otherCtrl = col.collider.GetComponentInParent<PlayerController>();
        if (!otherCtrl) return;

        Vector2 dir = (otherCtrl.transform.position - transform.position).normalized;
        otherCtrl.AddImpulse(dir * repelKickSpeed);
    }*/

    // ---------- Damage aura ----------
    private IEnumerator ApplyDamageAura()
    {
        OnDamageAuraActivated?.Invoke();
        damageAuraVisual.SetActive(true);

        StopBlink(ref _damageBlinkCo);
        _damageBlinkCo = StartCoroutine(BlinkVisual(
            damageAuraVisual,
            totalDuration: damageAuraDuration,
            blinkStartAtSeconds: blinkStartSeconds,
            minInterval: blinkMinInterval,
            maxInterval: blinkMaxInterval,
            curve: blinkCurve
        ));

        float remaining = damageAuraDuration;
        while (remaining > 0f)
        {
            DoDamageTick();
            remaining -= tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }

        StopBlink(ref _damageBlinkCo);
        damageAuraVisual.SetActive(false);
    }
    private IEnumerator ApplyConfusionAura()
    {
        OnConfusionAuraActivated?.Invoke();
        confusionAuraVisual.SetActive(true);

        // Blink-Warnung starten
        StopBlink(ref _confusionBlinkCo);
        _confusionBlinkCo = StartCoroutine(BlinkVisual(
            confusionAuraVisual,
            totalDuration: confusionAuraDuration,
            blinkStartAtSeconds: blinkStartSeconds,
            minInterval: blinkMinInterval,
            maxInterval: blinkMaxInterval,
            curve: blinkCurve
        ));

        float remaining = confusionAuraDuration;
        while (remaining > 0f)
        {
            // Suche nach Spielern im Radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, confusionAuraRadius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                if (!hit.CompareTag(playerTag)) continue;

                // HealthSystem suchen (wie in deinem Snippet)
                var healthSystem = hit.GetComponent<PlayerHealthSystem>();
                if (healthSystem != null)
                {
                    // Effekt nur abspielen, wenn der Spieler nicht schon verwirrt ist (optional)
                    // oder einfach bei jedem Tick triggern:
                    if (confusionEffect != null && !confusionEffect.isPlaying)
                        confusionEffect.Play();

                    healthSystem.ApplyConfusion(confusionDuration);
                }
            }

            remaining -= Time.deltaTime;
            yield return null; // Prüft jeden Frame
        }

        StopBlink(ref _confusionBlinkCo);
        confusionAuraVisual.SetActive(false);
    }
    private void DoDamageTick()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageAuraRadius);

        HashSet<Transform> uniquePlayers = new HashSet<Transform>();
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col || col.gameObject == gameObject) continue;
            if (!col.CompareTag(playerTag)) continue;

            Transform root = col.transform;
            if (!uniquePlayers.Add(root)) continue;

            var health = root.GetComponent<PlayerHealthSystem>();
            if (health != null) health.TakeDamage(damagePerTick, true);
        }
    }

    // ---------------- BLINK CORE ----------------
    private IEnumerator BlinkVisual(
        GameObject visual,
        float totalDuration,
        float blinkStartAtSeconds,
        float minInterval,
        float maxInterval,
        AnimationCurve curve
    )
    {
        if (!visual) yield break;

        // If duration is short, clamp start window so it still blinks.
        blinkStartAtSeconds = Mathf.Clamp(blinkStartAtSeconds, 0f, totalDuration);
        float timeToWait = totalDuration - blinkStartAtSeconds;

        // Wait until we're inside the warning window
        float wait = timeToWait;
        while (wait > 0f)
        {
            wait -= Time.deltaTime;
            yield return null;
        }

        // Now blink from remaining=blinkStartAtSeconds down to 0
        float remaining = blinkStartAtSeconds;

        // Ensure it starts visible
        visual.SetActive(true);

        while (remaining > 0f)
        {
            // progress: 0 at start of warning, 1 at end
            float p = 1f - (remaining / blinkStartAtSeconds);
            float shaped = Mathf.Clamp01(curve != null ? curve.Evaluate(p) : p);

            // shaped=0 -> maxInterval, shaped=1 -> minInterval
            float interval = Mathf.Lerp(maxInterval, minInterval, shaped);

            // toggle
            visual.SetActive(!visual.activeSelf);

            // wait interval while reducing remaining
            float dt = interval;
            while (dt > 0f)
            {
                float step = Time.deltaTime;
                dt -= step;
                remaining -= step;
                if (remaining <= 0f) break;
                yield return null;
            }
        }

        OnAuraExpires?.Invoke();
        // End state: keep on (so the final "off" doesn't look like it vanished early)
        visual.SetActive(true);
    }

    private void StopBlink(ref Coroutine co)
    {
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }
    }

    void ResetAuras()
    {
        if (_activeSlowCo != null) StopCoroutine(_activeSlowCo);
        if (_activeRepelCo != null) StopCoroutine(_activeRepelCo);
        if (_activeDamageCo != null) StopCoroutine(_activeDamageCo);
        if (_activeConfusionCo != null) StopCoroutine(_activeConfusionCo);

        _activeSlowCo = null;
        _activeRepelCo = null;
        _activeDamageCo = null;
        _activeConfusionCo = null;

        StopBlink(ref _slowBlinkCo);
        StopBlink(ref _repelBlinkCo);
        StopBlink(ref _damageBlinkCo);
        StopBlink(ref _confusionBlinkCo);

        if (damageAuraVisual) damageAuraVisual.SetActive(false);
        if (slowAuraVisual) slowAuraVisual.SetActive(false);
        if (repelAuraVisual) repelAuraVisual.SetActive(false);
        if (confusionAuraVisual) confusionAuraVisual.SetActive(false);

        repelActive = false;
    }
}
