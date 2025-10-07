using UnityEngine;

[DefaultExecutionOrder(100)] // run after controller writes rb.velocity
[DisallowMultipleComponent]
public class SpeedDebuff : MonoBehaviour
{
    [SerializeField] private bool affectHorizontal = true;
    [SerializeField] private bool affectVertical = false;

    private Rigidbody2D rb;
    private float activeUntil = 0f;
    private float multiplier = 1f;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    /// <summary>
    /// Multiplies velocity by 'm' for 'seconds'.
    /// m < 1 = slow, m > 1 = speed-up. Calling again restarts the timer.
    /// </summary>
    public void ApplySpeedModifier(float m, float seconds)
    {
        multiplier = m;
        activeUntil = Time.time + Mathf.Max(0.01f, seconds);
    }

    private void FixedUpdate()
    {
        if (!rb) return;
        if (Time.time >= activeUntil || Mathf.Approximately(multiplier, 1f)) return;

        var v = rb.velocity;
        if (affectHorizontal) v.x *= multiplier;
        if (affectVertical) v.y *= multiplier;
        rb.velocity = v;
    }
}
