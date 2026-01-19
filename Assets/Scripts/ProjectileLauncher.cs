using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public static event System.Action OnFireCanon;

    [Header("Refs")]
    public GameObject[] projectilePrefab;
    public Transform muzzle;
    public Transform direction;

    [Header("Firing")]
    public float fireInterval = 0.2f;      // Zeit zwischen den 5 Projektilen
    public float burstPause = 2f;         // Pause nach den 5 Projektilen
    public int shotsPerBurst = 5;         // Wie viele Projektile pro Salve
    public bool fireOnEnable = true;
    public float startDelay = 0f;

    float _timer;
    bool _started;
    int _shotsFired;                      // Zähler für die aktuelle Salve

    void OnEnable()
    {
        _timer = -Mathf.Max(0f, startDelay);
        _started = fireOnEnable;
        _shotsFired = 0;
    }

    void FixedUpdate()
    {
        if (!_started || projectilePrefab == null || muzzle == null || direction == null) return;

        _timer += Time.fixedDeltaTime;

        // Wir prüfen, welches Intervall gerade gilt
        float currentTargetInterval = (_shotsFired >= shotsPerBurst) ? burstPause : fireInterval;

        while (_timer >= currentTargetInterval)
        {
            _timer -= currentTargetInterval;

            if (_shotsFired >= shotsPerBurst)
            {
                // Die Pause ist vorbei, wir setzen den Zähler zurück
                _shotsFired = 0;
                // Nach der Pause feuern wir direkt das erste Projektil der neuen Salve
                FireOne();
                _shotsFired++;
                // Wichtig: Das Intervall für den nächsten Durchlauf der Schleife aktualisieren
                currentTargetInterval = fireInterval;
            }
            else
            {
                FireOne();
                _shotsFired++;

                // Wenn wir gerade den letzten Schuss gefeuert haben, 
                // muss das nächste Intervall die Pause sein
                if (_shotsFired >= shotsPerBurst)
                {
                    currentTargetInterval = burstPause;
                }
            }
        }
    }

    void FireOne()
    {
        Vector2 dir = direction.right.normalized;

        if (transform.localScale.x < 0f)
        {
            dir.y = -dir.y;
        }

        int randomBullet = Random.Range(0, projectilePrefab.Length);
        var go = Instantiate(projectilePrefab[randomBullet], muzzle.position, Quaternion.identity);

        var myCol = GetComponent<Collider2D>();
        var projCol = go.GetComponent<Collider2D>();
        if (myCol && projCol) Physics2D.IgnoreCollision(myCol, projCol, true);

        var proj = go.GetComponent<Projectile2D>();
        if (proj == null) { Debug.LogError("Projectile prefab needs Projectile2D on root."); return; }

        go.transform.right = dir;
        proj.Launch(dir);

        if (name.Contains("Effect_Shooter"))
        {
            OnFireCanon?.Invoke();
        }
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