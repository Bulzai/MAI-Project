using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TarodevController;

[DefaultExecutionOrder(100)]
public class PlayerItemHandler : MonoBehaviour
{
    [Header("Slow Aura")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float auraRadius = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float slowPercent = 0.5f;
    [SerializeField] private float slowSeconds = 2f;
    [SerializeField] private float auraDuration = 10f;
    [SerializeField] private GameObject slowAuraVisual;

    [Header("Repel Aura")]
    [SerializeField] private float repelSeconds = 5f;
    [SerializeField] private float repelKickSpeed = 14f;
    [SerializeField] private GameObject repelAuraVisual;
    private bool repelActive;

    [Header("Speed Aura (SELF ONLY)")]
    [SerializeField] private float speedMultiplier = 1.5f; // >1 speeds up
    [SerializeField] private float speedDuration = 4f;     // seconds

    // ---------- Damage Aura ----------
    [Header("Damage Aura")]
    [SerializeField] private float damageAuraRadius = 2.6f;
    [SerializeField] private int damagePerTick = 2;
    [SerializeField] private float tickInterval = 1f;     // seconds between ticks
    [SerializeField] private float damageAuraDuration = 6f; // total active time
    [SerializeField] private GameObject damageAuraVisual;


    private void OnEnable()
    {
        GameEvents.OnMainGameStateExited += ResetAuras;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateExited -= ResetAuras;
    }
    public void ApplyItem(PickUpItem.ItemType itemType)
    {
        switch (itemType)
        {
            case PickUpItem.ItemType.Slow:
                StartCoroutine(ApplySlowAura());
                break;

            case PickUpItem.ItemType.Repel:
                StartCoroutine(EnableRepelAura());
                break;

            case PickUpItem.ItemType.Speed:
                // Self-only: apply to THIS player only
                var selfBuff = GetComponent<SlowDebuff>();
                if (selfBuff) selfBuff.ApplySpeedModifier(speedMultiplier, speedDuration);
                break;

            case PickUpItem.ItemType.Damage: // <--- add to your enum
                StartCoroutine(ApplyDamageAura());
                break;

            default:
                Debug.LogWarning("Unknown item type: " + itemType);
                break;
        }
    }

    // ---------- Slow aura ----------
    private IEnumerator ApplySlowAura()
    {

        slowAuraVisual.SetActive(true);
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
            }
            t -= Time.deltaTime;
            yield return null;
        }
        slowAuraVisual.SetActive(false);

    }

    // ---------- Repel aura ----------
    private IEnumerator EnableRepelAura()
    {
        repelAuraVisual.SetActive(true);

        repelActive = true;
        yield return new WaitForSeconds(repelSeconds);
        repelActive = false;

        repelAuraVisual.SetActive(false);

    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!repelActive) return;
        if (!col.collider || !col.collider.CompareTag(playerTag)) return;
        if (col.gameObject == gameObject) return;

        var otherCtrl = col.collider.GetComponentInParent<PlayerController>();
        if (!otherCtrl) return;

        Vector2 dir = (otherCtrl.transform.position - transform.position).normalized;
        otherCtrl.AddImpulse(dir * repelKickSpeed); // instant Δv via your controller hook
    }

    // ---------- Damage aura (simple periodic OverlapCircle) ----------
    private IEnumerator ApplyDamageAura()
    {

        damageAuraVisual.SetActive(true);
        float remaining = damageAuraDuration;

        while (remaining > 0f)
        {
            DoDamageTick();          // OverlapCircleAll -> apply damage to all players inside
            remaining -= tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
        damageAuraVisual.SetActive(false);

    }

    private void DoDamageTick()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageAuraRadius);
        Debug.Log("Damage Aura Tick, hits: " + hits.Length);
        // Deduplicate by player root (handles multi-collider rigs)
        HashSet<Transform> uniquePlayers = new HashSet<Transform>();

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col || col.gameObject == gameObject) continue;
            if (!col.CompareTag(playerTag)) continue;

            Transform root = col.transform;
            if (!uniquePlayers.Add(root)) continue;

            var health = root.GetComponent<PlayerHealthSystem>();
            if (health != null)
            {
                // Adjust signature if your health method differs
                health.TakeDamage(damagePerTick, true);
            }
        }
    }

    void ResetAuras()
    {
        damageAuraVisual.SetActive(false);
        slowAuraVisual.SetActive(false);
        repelAuraVisual.SetActive(false);
        repelActive = false;    }
}
