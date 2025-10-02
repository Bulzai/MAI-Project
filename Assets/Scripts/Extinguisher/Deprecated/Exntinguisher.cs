using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguisherSpawner : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [SerializeField] private GameObject extinguisherPrefab;
    [SerializeField] private Transform spawnPointsParent; // child 0 in your original
    [SerializeField] private Transform extinguishersContainer; // child 1

    [Header("Spawn Settings")]
    [SerializeField] private float spawnIntervalSeconds = 5f;
    [SerializeField] private float destroyAfterSeconds = 10f;
    [SerializeField] private int maxExtinguishers = 10;

    private readonly List<Transform> _spawnPoints = new List<Transform>();
    private Coroutine _spawnRoutine;
    private int _spawnedCount;

    private void Awake()
    {
        // cache all the spawn points
        foreach (Transform t in spawnPointsParent)
            _spawnPoints.Add(t);
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += BeginSpawning;
        GameEvents.OnMainGameStateExited  += StopSpawning;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= BeginSpawning;
        GameEvents.OnMainGameStateExited  -= StopSpawning;
    }

    private void BeginSpawning()
    {
        // reset counters & start coroutine
        _spawnedCount = 0;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnExtinguishers());
    }

    private void StopSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    private IEnumerator SpawnExtinguishers()
    {
        while (_spawnedCount < maxExtinguishers)
        {
            SpawnOne();
            _spawnedCount++;
            yield return new WaitForSeconds(spawnIntervalSeconds);
        }
    }

    private void SpawnOne()
    {
        if (_spawnPoints.Count == 0 || extinguisherPrefab == null) 
            return;

        int idx = Random.Range(0, _spawnPoints.Count);
        var point = _spawnPoints[idx];

        var go = Instantiate(
            extinguisherPrefab, 
            point.position, 
            extinguisherPrefab.transform.rotation,
            extinguishersContainer
        );

        Destroy(go, destroyAfterSeconds);
    }
}
