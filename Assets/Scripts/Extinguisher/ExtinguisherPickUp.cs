using System;
using UnityEngine;

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

        Destroy(gameObject);

        // tell the loop: spawn the next one immediately
        spawner?.RequestAdvance();
    }
}
