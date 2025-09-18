using System.Collections;
using UnityEngine;
using TarodevController; // <-- so we can call PlayerController.AddImpulse

public class PlayerItemHandler : MonoBehaviour
{
    [Header("Slow Aura")]
    [SerializeField] private float auraRadius = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float slowPercent = 0.5f;
    [SerializeField] private float slowSeconds = 2f;
    [SerializeField] private float auraDuration = 10f;

    [Header("Repel Aura")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float repelSeconds = 5f;       // aura time after pickup
    [SerializeField] private float repelKickSpeed = 14f;    // Δv applied to the other player
    [SerializeField] private bool horizontalOnly = false;   // set true if you want no vertical knock

    private bool repelActive;

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

        }
    }

    // -------- Slow aura (unchanged pattern) --------
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

                var slow = otherRb.GetComponent<SlowDebuff>(); // your tiny slow script
                if (slow) slow.SlowFor(slowPercent, slowSeconds);
            }

            t -= Time.deltaTime;
            yield return null;
        }
    }

    // -------- Repel aura (timed buff) --------
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
        if (horizontalOnly) dir = new Vector2(Mathf.Sign(dir.x == 0 ? 1 : dir.x), 0f).normalized;

        otherCtrl.AddImpulse(dir * repelKickSpeed); // instant Δv applied INSIDE their controller tick
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
