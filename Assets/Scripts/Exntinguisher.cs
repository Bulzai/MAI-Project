using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exntinguisher : MonoBehaviour
{
    public GameObject extinguisherPrefab;
    private List<Transform> spawnPoints = new List<Transform>();

    public float spawnIntervalSeconds = 5f; // seconds
    public float destroyAfterSeconds = 10f; // seconds
    private int extinguisherCount = 0;

    private void Start()
    {

        foreach(Transform child in transform.GetChild(0))
        {
            spawnPoints.Add(child);
            Debug.Log("added trans position");
        }

        StartCoroutine(SpawnExtinguishers());

        
    }

    private IEnumerator SpawnExtinguishers()
    {
        while (extinguisherCount < 10)
        {
            Debug.Log("Spawning");
            SpawnExtinguisher();
            yield return new WaitForSeconds(spawnIntervalSeconds);
        }
    }

    private void SpawnExtinguisher()
    {
        if (spawnPoints.Count == 0 || extinguisherPrefab == null)
            return;

        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];
        GameObject extinguisher = Instantiate(extinguisherPrefab, spawnPoint.position, extinguisherPrefab.transform.rotation, transform.GetChild(1)); 

    }
}