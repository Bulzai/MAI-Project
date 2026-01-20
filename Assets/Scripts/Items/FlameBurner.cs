using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlameBurner : MonoBehaviour
{
    [Header("Einstellungen")]
    public float startDelay = 0f;       // Verzögerung beim Spielstart (für asynchrone Fallen)
    public float idleTime = 3f;         // Wie lange die Falle AUS ist
    public float burnTime = 4f;         // Wie lange die Falle BRENNT
    public float endAnimDuration = 1.0f;


    [Header("Referenzen")]
    public Animator animator;           // Referenz zum Animator
    public BoxCollider2D damageCollider;     // Der Trigger-Collider, der den Schaden macht
    public SpriteRenderer spriteRenderer;

    // Interne Variablen
    private bool isActive = false;
    private float damageCooldown = 0.5f; // Damit man nicht jeden Frame Schaden bekommt
    private float lastDamageTime;

    void OnEnable()
    {
        Debug.Log("enabled flame burner");

        if (damageCollider != null)
            damageCollider.enabled = false;

        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (animator != null)
        {
            animator.enabled = true;
            // NEU: Alte Trigger löschen, damit er nicht sofort losfeuert
            animator.ResetTrigger("Ignite");
            animator.ResetTrigger("Extinguish");
            // NEU: Den Animator hart in den Idle-Zustand setzen
            animator.Play("Idle", 0, 0f);
        }

        StopAllCoroutines();
        StartCoroutine(FlameCycle());
    }
    private void OnDisable()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        StopAllCoroutines();
    }
    // Die Haupt-Logikschleife
    IEnumerator FlameCycle()
    {

        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // === PHASE 1: IDLE (Aus) ===
            // Sicherstellen, dass Sprite aus ist
            if (spriteRenderer != null) spriteRenderer.enabled = false;

            yield return new WaitForSeconds(idleTime);

            // === PHASE 2: START (Zünden) ===
            // Jetzt Sprite einschalten, BEVOR die Animation startet -> NEU
            if (spriteRenderer != null) spriteRenderer.enabled = true;

            animator.SetTrigger("Ignite");

            //--------SOUND HIER START ----------

            // Kurz warten bis Flamme groß genug für Schaden
            yield return new WaitForSeconds(0.45f);

            if (damageCollider != null) damageCollider.enabled = true;

            // === PHASE 3: LOOP (Brennen) ===
            yield return new WaitForSeconds(burnTime);

            // === PHASE 4: END (Erlöschen) ===

            if (damageCollider != null) damageCollider.enabled = false;

            animator.SetTrigger("Extinguish");

            // Warten bis die "EndFire" Animation fertig ist
            // WICHTIG: Erst warten, dann Sprite ausmachen!
            yield return new WaitForSeconds(endAnimDuration);

            // Hiernach springt der Loop wieder nach oben zu Phase 1 und schaltet den Renderer aus
        }
    }

  
}