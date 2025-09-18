using System.Collections;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [Header("Slow Aura")]
    [SerializeField] private float auraRadius = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float slowPercent = 0.5f; // 0.5 = 50% speed
    [SerializeField] private float slowSeconds = 2f;   // per-target duration
    [SerializeField] private float auraDuration = 10f;  // how long we keep detecting targets

    public void ApplyItem(PickUpItem.ItemType itemType)
    {
        if (itemType == PickUpItem.ItemType.Slow)
            StartCoroutine(ApplySlowAura());
    }

    private IEnumerator ApplySlowAura()
    {
        float t = auraDuration;
        while (t > 0f)
        {
            DetectAndSlowOthers();
            t -= Time.deltaTime;
            yield return null;
        }
    }

    private void DetectAndSlowOthers()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, auraRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h || h.gameObject == gameObject) continue;
            if (!h.CompareTag("Player")) continue;

            // get the Rigidbody2D reliably even if collider is on a child
            var otherRb = h.attachedRigidbody ?? h.GetComponentInParent<Rigidbody2D>();
            if (!otherRb) continue;

            var slow = otherRb.GetComponent<SlowDebuff>();
            if (!slow) continue; // make sure PlayerSlow is on every player prefab

            slow.SlowFor(slowPercent, slowSeconds); // exact-duration slow
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
