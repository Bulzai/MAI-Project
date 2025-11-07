using System.Collections;
using UnityEngine;
using System; // for Action if needed

/// <summary>
/// Spawns a random item that curves down toward a random landing spot.
/// Only one item exists at a time. Auto-respawns when none exist.
/// Only active during MainGameState.
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
    public float fallSpeed = 6f;
    public float curveAmplitude = 1.25f;
    public float curveFrequency = 0.6f;
    [Range(0f, 1f)] public float wiggleDamping = 1f;
    public float snapEpsilon = 0.02f;

    [Header("Auto Respawn")]
    public bool autoRespawn = true;
    public Vector2 respawnDelayRange = new Vector2(5f, 10f);

    private GameObject currentItem;
    private Coroutine autoRespawnRoutine;
    private bool isActive;

    private void OnEnable()
    {
        // Subscribe to your custom events
        GameEvents.OnMainGameStateEntered += HandleMainGameStateEntered;
        GameEvents.OnMainGameStateExited += HandleMainGameStateExited;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent leaks
        GameEvents.OnMainGameStateEntered -= HandleMainGameStateEntered;
        GameEvents.OnMainGameStateExited -= HandleMainGameStateExited;

        StopSpawning();
    }

    private void HandleMainGameStateEntered()
    {
        isActive = true;

        if (autoRespawn && autoRespawnRoutine == null)
            autoRespawnRoutine = StartCoroutine(AutoRespawnLoop());
    }

    private void HandleMainGameStateExited()
    {
        isActive = false;
        StopSpawning();
    }

    private void StopSpawning()
    {
        if (autoRespawnRoutine != null)
        {
            StopCoroutine(autoRespawnRoutine);
            autoRespawnRoutine = null;
        }

        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }
    }

    /// <summary>Spawns one random item if none currently exists.</summary>
    public void SpawnOne()
    {
        if (!isActive) return; // only during MainGameState
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

        int prefabIndex = UnityEngine.Random.Range(0, itemPrefabs.Length);
        int spotIndex = UnityEngine.Random.Range(0, landingSpots.Length);

        Vector3 startPos = new Vector3(UnityEngine.Random.Range(spawnXRange.x, spawnXRange.y), spawnTopY, 0f);
        Vector3 targetPos = landingSpots[spotIndex].position;

        currentItem = Instantiate(itemPrefabs[prefabIndex], startPos, Quaternion.identity);
        StartCoroutine(GuidedCurvedFall(currentItem, targetPos));
    }

    private IEnumerator GuidedCurvedFall(GameObject item, Vector3 targetPos)
    {
        Vector3 pos = item.transform.position;
        float startY = pos.y;
        float endY = targetPos.y;
        float totalDrop = Mathf.Max(0.0001f, startY - endY);
        float time = 0f;
        float startX = pos.x;

        while (item != null && pos.y - endY > snapEpsilon)
        {
            float dt = Time.deltaTime;
            time += dt;

            // Vertical movement
            pos.y = Mathf.Max(endY, pos.y - fallSpeed * dt);

            float p = Mathf.Clamp01((startY - pos.y) / totalDrop);
            float baseX = Mathf.Lerp(startX, targetPos.x, p);
            float damping = Mathf.Lerp(1f, 0f, p * wiggleDamping);
            float wiggle = Mathf.Sin(time * Mathf.PI * 2f * curveFrequency) * curveAmplitude * damping;

            pos.x = baseX + wiggle;
            item.transform.position = pos;

            yield return null;
        }

        if (item != null) item.transform.position = targetPos;
        StartCoroutine(WaitForRemoval(item));
    }

    private IEnumerator WaitForRemoval(GameObject item)
    {
        while (item != null) yield return null;
        currentItem = null;
    }

    private IEnumerator AutoRespawnLoop()
    {
        while (isActive)
        {
            if (currentItem == null)
            {
                float wait = Mathf.Max(0f, UnityEngine.Random.Range(respawnDelayRange.x, respawnDelayRange.y));
                yield return new WaitForSeconds(wait);

                if (isActive && currentItem == null)
                    SpawnOne();
            }
            yield return null;
        }
        autoRespawnRoutine = null;
    }

}
