using UnityEngine;

public class CrossbowShooter : MonoBehaviour
{
    [Header("Arrow Settings")]
    [Tooltip("Prefab of the arrow. Must have Rigidbody2D, Collider2D, ArrowBehavior.")]
    public GameObject arrowPrefab;

    [Tooltip("Point from which the arrow spawns; its forward (right) is firing direction.")]
    public Transform firePoint;

    [Tooltip("Seconds between shots")]
    public float shootInterval = 1f;

    [Tooltip("Speed applied to the arrow's Rigidbody2D")]
    public float arrowSpeed = 10f;

    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= shootInterval)
        {
            Shoot();
            _timer = 0f;
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null || firePoint == null) return;

        // Instantiate arrow
        GameObject arrowGo = Instantiate(
            arrowPrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Give it velocity in its right-facing direction
        var rb = arrowGo.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = firePoint.right * arrowSpeed;
    }
}
