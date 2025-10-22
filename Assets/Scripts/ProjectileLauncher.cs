using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Refs")]
    public GameObject[] projectilePrefab;       // prefab with ProjectileSimple2D on root
    public Transform muzzle;                  // where to spawn
    public Transform direction;               // child that defines aim (use its +X / right)

    [Header("Firing")]
    public float fireInterval = 1f;           // one shot per second (deterministic)
    public bool fireOnEnable = true;
    public float startDelay = 0f;             // optional offset if you place many shooters

    float _timer;
    bool _started;

    void OnEnable()
    {
        _timer = -Mathf.Max(0f, startDelay);  // wait startDelay, then fire at exact multiples of fireInterval
        _started = fireOnEnable;
    }

    void FixedUpdate()
    {
        if (!_started || projectilePrefab == null || muzzle == null || direction == null) return;

        _timer += Time.fixedDeltaTime;
        while (_timer >= fireInterval)       // catches up deterministically even if a frame hiccups
        {
            _timer -= fireInterval;
            FireOne();
        }
    }

    void FireOne()
    {
        Vector2 dir = direction.right.normalized;        // <- aim defined by child’s +X

        int randomBullet = Random.Range(0, projectilePrefab.Length);
        var go = Instantiate(projectilePrefab[randomBullet], muzzle.position, Quaternion.identity);

        // Optional: ignore self-collision if both have colliders
        var myCol = GetComponent<Collider2D>();
        var projCol = go.GetComponent<Collider2D>();
        if (myCol && projCol) Physics2D.IgnoreCollision(myCol, projCol, true);

        // Launch
        var proj = go.GetComponent<Projectile2D>();
        if (proj == null) { Debug.LogError("Projectile prefab needs ProjectileSimple2D on root."); return; }

        go.transform.right = dir; // for sprite orientation
        proj.Launch(dir);
    }

    void OnDrawGizmosSelected()
    {
        if (muzzle && direction)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(muzzle.position, 0.05f);
            Gizmos.DrawRay(muzzle.position, direction.right.normalized * 2f);
        }
    }
}
