using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletData data;
    private Vector2 direction;
    private Vector2 startPosition;
    private Rigidbody2D rb;

    void Start()
    {
        if (data == null)
            Debug.LogError("Bullet data not set!");
    }

    public void Initialize(Vector2 dir, BulletData bulletData)
    {
        data = bulletData;
        direction = dir.normalized;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Missing Rigidbody2D on bullet!");
            return;
        }

        // Optional gravity (for trajectory effects)
        rb.gravityScale = data.useGravity ? data.gravityScale : 0f;

        // Apply impulse force for physics-based trajectory
        rb.AddForce(direction.normalized * data.launchForce, ForceMode2D.Impulse);

        // Optional: rotate bullet to face movement direction
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, angle);

        // Sprite override
        if (GetComponent<SpriteRenderer>() && data.bulletSprite)
            GetComponent<SpriteRenderer>().sprite = data.bulletSprite;
        Debug.Log("Bullet initialized with speed: " + data.speed);

    }

    void Update()
    {
        //GetComponent<Rigidbody2D>().velocity = direction * data.speed; (used when we dont have AddForce

        if (Vector2.Distance(startPosition, transform.position) > data.maxDistance && gameObject != null)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        ITarget target = collider.GetComponent<ITarget>();
        if (target != null && gameObject != null)
        {
            target.damage();
            Destroy(gameObject);
        }
    }


    /*
     * add if you want bounces
    private void OnCollisionEnter2D(Collision2D collision)
{
    Vector2 inDirection = rb.velocity;
    Vector2 normal = collision.contacts[0].normal;
    Vector2 reflectDirection = Vector2.Reflect(inDirection, normal);

    rb.velocity = reflectDirection * data.speed;

    // Optional: Reduce speed slightly on each bounce
    data.speed *= 0.9f;

    // Optional: Destroy after X bounces
    bounceCount++;
    if (bounceCount > maxBounces) {
        Destroy(gameObject);
    }
} */
}
