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

        PlayerInput playerInput = collision.GetComponentInParent<PlayerInput>();

        if (playerInput != null && GameMetricsLogger.Instance != null)
        {
            GameMetricsLogger.Instance.RegisterMilkCollected(playerInput.playerIndex);
        }

        health.Extinguish();
        OnMilkCollected?.Invoke();

        Destroy(gameObject);

        spawner?.RequestAdvance();
    }
}