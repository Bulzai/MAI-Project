using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)] // run after your movement controller's FixedUpdate
public class SlowDebuff : MonoBehaviour
{
    [SerializeField] private bool affectHorizontal = true;
    [SerializeField] private bool affectVertical = false;

    private Rigidbody2D rb;
    private float activeUntil = 0f;
    private float multiplier = 1f;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    /// <summary>Slow to (multiplier * normal) for 'seconds' seconds.</summary>
    public void SlowFor(float m, float seconds)
    {
        multiplier = Mathf.Clamp01(m);
        activeUntil = Time.time + Mathf.Max(0.01f, seconds);
    }

    private void FixedUpdate()
    {
        if (!rb) return;
        if (Time.time >= activeUntil) return; // not slowed

        var v = rb.velocity;
        if (affectHorizontal) v.x *= multiplier;
        if (affectVertical) v.y *= multiplier;
        rb.velocity = v; // executed AFTER PlayerController writes velocity
    }
}
