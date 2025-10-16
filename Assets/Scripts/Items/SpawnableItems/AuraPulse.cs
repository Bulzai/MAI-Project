using UnityEngine;

public class AuraPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 2f;   // how fast it scales
    [SerializeField] private float pulseAmount = 0.1f; // how strong the pulse (e.g. 0.1 = ±10%)
    [SerializeField] private Vector3 baseScale = Vector3.one;

    private float _t;

    private void OnEnable()
    {
        _t = Random.value * Mathf.PI * 2f; // start at random phase (optional)
        transform.localScale = baseScale;
    }

    private void Update()
    {
        _t += Time.deltaTime * pulseSpeed;
        float scaleOffset = Mathf.Sin(_t) * pulseAmount;
        transform.localScale = baseScale * (1f + scaleOffset);
    }
}
