using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExtinguisherPickUp : MonoBehaviour
{
    private ExtingSpawner spawner;
    public static event Action OnMilkCollected;
    public void Init(ExtingSpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponent<PlayerHealthSystem>();
        if (health == null) return;

        health.Extinguish();
        OnMilkCollected?.Invoke();

        var fadeLife = GetComponent<FadeInAndLife>();
        if (fadeLife != null) fadeLife.MarkAsCollected();

        // log extinguisher collection
        string playerName = "Unknown";
        PlayerInput pi = collision.GetComponentInParent<PlayerInput>();
        if (pi != null) playerName = pi.gameObject.name;
        else Debug.Log("LOG EXTINGUISHER COLLECITON: PLAYER NAME CANNNOT BE FOUND");

        float timeToCollect = Time.time - ExtingSpawner.ExtinguisherSpawnTime;
        int round = UnityEngine.Object.FindAnyObjectByType<RoundController>().currentRound;

        string data = $"{round+1},ExtinguisherCollection,{playerName},{transform.position}:{timeToCollect}";
        TestingLogger.LogToCSV(data);
        Debug.Log($"Extinguisher Pickup: collected at position {transform.position} in {timeToCollect}");

        Destroy(gameObject);

        // tell the loop: spawn the next one immediately
        spawner?.RequestAdvance();
    }
}
