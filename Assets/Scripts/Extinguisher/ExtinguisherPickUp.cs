using UnityEngine;

public class ExtinguisherPickUp : MonoBehaviour
{
    private ExtingSpawner spawner;

    public void Init(ExtingSpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponent<PlayerHealthSystem>();
        if (health == null) return;

        health.Extinguish();
        Destroy(gameObject);

        // tell the loop: spawn the next one immediately
        spawner?.RequestAdvance();
    }
}
