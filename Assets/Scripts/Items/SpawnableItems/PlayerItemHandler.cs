using System.Collections;
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

    [Header("Repel Aura")]
    [SerializeField] private float repelSeconds = 5f;
    [SerializeField] private float repelKickSpeed = 14f;
    private bool repelActive;

    [Header("Speed Aura (SELF ONLY)")]
    [SerializeField] private float speedMultiplier = 1.5f; // >1 speeds up
    [SerializeField] private float speedDuration = 4f;     // seconds

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
                // ⇩ Self-only: apply to THIS player only
                var selfBuff = GetComponent<SpeedDebuff>();
                if (selfBuff) selfBuff.ApplySpeedModifier(speedMultiplier, speedDuration);
                break;

            default:
                Debug.LogWarning("Unknown item type: " + itemType);
                break;
        }
    }

    // ---------- Slow aura (same as before) ----------
    private IEnumerator ApplySlowAura()
    {
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

                var slow = otherRb.GetComponent<SpeedDebuff>();
                if (slow) slow.ApplySpeedModifier(slowPercent, slowSeconds);
            }
            t -= Time.deltaTime;
            yield return null;
        }
    }

    // ---------- Repel aura (unchanged minimal) ----------
    private IEnumerator EnableRepelAura()
    {
        repelActive = true;
        yield return new WaitForSeconds(repelSeconds);
        repelActive = false;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
