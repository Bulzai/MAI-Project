using UnityEngine;



[RequireComponent(typeof(Rigidbody2D))]
public class Projectile2D : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 8f;           // launcher sets velocity using this
    public bool faceVelocity = true;

    [Header("Life / Hit")]
    public float lifetime = 5f;
    public bool destroyOnAnyHit = true;
    public int damage = 1;
    public float knockBackStrength = 2;
    public LayerMask hitMask = ~0;

    Rigidbody2D _rb;
    float _t;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // deterministic straight shot => no gravity, continuous detection is nice for fast shots
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Launch(Vector2 dir)
    {
        _rb.velocity = dir * speed;
        if (faceVelocity && _rb.velocity.sqrMagnitude > 0.0001f)
            transform.right = _rb.velocity.normalized;
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= lifetime) Destroy(gameObject);

        if (faceVelocity && _rb.velocity.sqrMagnitude > 0.0001f)
            transform.right = -_rb.velocity.normalized;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {
                player.TakeDamage(damage, true);

                // Direction from projectile -> player
                Vector2 dir = (other.transform.position - transform.position);
                if (dir.sqrMagnitude > 0.0001f) dir.Normalize();


                player.Knockback(dir, knockBackStrength);
            }
            else
            {
                Debug.Log("No PlayerHealthSystem Script");
            }

            // then destroy projectile if you like
            Destroy(gameObject);
        }
        Destroy(gameObject);

    }
    // In your projectile's OnTriggerEnter2D:
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {
                player.TakeDamage(damage, true);

                // Direction from projectile -> player
                Vector2 dir = (other.transform.position - transform.position);
                if (dir.sqrMagnitude > 0.0001f) dir.Normalize();


                player.Knockback(dir, knockBackStrength);
            }
            else
            {
                Debug.Log("No PlayerHealthSystem Script");
            }

            // then destroy projectile if you like
            Destroy(gameObject);
        }
    }

}