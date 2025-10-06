using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a random item that curves down toward a random landing spot.
/// Only one item exists at a time. Auto-respawns when none exist.
/// Uses trigger pickups (no physics).
/// </summary>
public class SpawnItem : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject[] itemPrefabs;

    [Header("Landing Spots")]
    public Transform[] landingSpots;

    [Header("Spawn Settings")]
    public float spawnTopY = 20f;
    public Vector2 spawnXRange = new Vector2(-15f, 15f);

    [Header("Curved Fall")]
    [Tooltip("Constant vertical speed downward.")]
    public float fallSpeed = 6f;
    [Tooltip("Max horizontal wiggle at the start.")]
    public float curveAmplitude = 1.25f;
    [Tooltip("How quickly the wiggle oscillates (Hz).")]
    public float curveFrequency = 0.6f;
    [Tooltip("Reduce wiggle as it approaches the target (0 = no damping, 1 = fully off at landing).")]
    [Range(0f, 1f)] public float wiggleDamping = 1f;
    [Tooltip("Snap distance to finish exactly on the target (prevents micro-oscillation).")]
    public float snapEpsilon = 0.02f;

    [Header("Auto Respawn")]
    public bool autoRespawn = true;
    public Vector2 respawnDelayRange = new Vector2(5f, 10f);

    private GameObject currentItem;
    private Coroutine autoRespawnRoutine;

    private void OnEnable()
    {
        if (autoRespawn && autoRespawnRoutine == null)
            autoRespawnRoutine = StartCoroutine(AutoRespawnLoop());
    }

    private void OnDisable()
    {
        if (autoRespawnRoutine != null)
        {
            StopCoroutine(autoRespawnRoutine);
            autoRespawnRoutine = null;
        }
    }

    /// <summary>Spawns one random item if none currently exists.</summary>
    public void SpawnOne()
    {
        if (currentItem != null) return;

        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("[SpawnItem] No item prefabs assigned.");
            return;
        }
        if (landingSpots == null || landingSpots.Length == 0)
        {
            Debug.LogWarning("[SpawnItem] No landing spots assigned.");
            return;
        }

        int prefabIndex = Random.Range(0, itemPrefabs.Length);
        int spotIndex = Random.Range(0, landingSpots.Length);

        Vector3 startPos = new Vector3(Random.Range(spawnXRange.x, spawnXRange.y), spawnTopY, 0f);
        Vector3 targetPos = landingSpots[spotIndex].position;

        currentItem = Instantiate(itemPrefabs[prefabIndex], startPos, Quaternion.identity);
        StartCoroutine(GuidedCurvedFall(currentItem, targetPos));
    }

    /// <summary>
    /// Moves toward target with constant downward speed. X follows a target-directed baseline from t=0,
    /// plus a sine wiggle whose amplitude fades out near the target.
    /// </summary>
    private IEnumerator GuidedCurvedFall(GameObject item, Vector3 targetPos)
    {
        Vector3 pos = item.transform.position;
        float startY = pos.y;
        float endY = targetPos.y;
        float totalDrop = Mathf.Max(0.0001f, startY - endY);
        float time = 0f;

        while (item != null && pos.y - endY > snapEpsilon)
        {
            float dt = Time.deltaTime;
            time += dt;

            // Vertical movement: constant downward speed
            pos.y = Mathf.Max(endY, pos.y - fallSpeed * dt);

            // Progress (0 at start, 1 at landing) based on vertical drop
            float p = Mathf.Clamp01((startY - pos.y) / totalDrop);

            // Horizontal baseline aims at target from the start
            float baseX = Mathf.Lerp(item.transform.position.x /* start X at spawn */, targetPos.x, p);

            // Damped wiggle around the baseline (fades with progress)
            float damping = (wiggleDamping <= 0f) ? 1f : Mathf.Lerp(1f, 0f, p * wiggleDamping);
            float wiggle = Mathf.Sin(time * Mathf.PI * 2f * curveFrequency) * curveAmplitude * damping;

            pos.x = baseX + wiggle;

            item.transform.position = pos;
            yield return null;
        }

        // Snap to exact landing spot
        if (item != null) item.transform.position = targetPos;

        // Wait until the item is picked up/destroyed elsewhere
        StartCoroutine(WaitForRemoval(item));
    }

    private IEnumerator WaitForRemoval(GameObject item)
    {
        while (item != null) yield return null;
        currentItem = null;
    }

    private IEnumerator AutoRespawnLoop()
    {
        while (true)
        {
            if (currentItem == null)
            {
                float wait = Mathf.Max(0f, Random.Range(respawnDelayRange.x, respawnDelayRange.y));
                yield return new WaitForSeconds(wait);

                if (currentItem == null) SpawnOne();
            }
            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 a = new Vector3(spawnXRange.x, spawnTopY, 0f);
        Vector3 b = new Vector3(spawnXRange.y, spawnTopY, 0f);
        Gizmos.DrawLine(a, b);

        if (landingSpots != null)
        {
            Gizmos.color = Color.green;
            foreach (var t in landingSpots)
            {
                if (t == null) continue;
                Gizmos.DrawSphere(t.position, 0.15f);
            }
        }
    }
#endif
}
