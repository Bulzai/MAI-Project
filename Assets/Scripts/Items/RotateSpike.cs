using UnityEngine;

public class RotateSpike : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 180f; // degrees per second

    [Header("Pivot Point")]
    [Tooltip("Empty child object placed at the desired rotation center (e.g., handle base).")]
    public Transform pivotPoint;

    private bool shouldRotate = false;

    private void Awake()
    {
        // Auto-find a child named "PivotPoint" if none assigned
        if (pivotPoint == null)
            pivotPoint = transform.Find("PivotPoint");
    }

    private void OnEnable() => shouldRotate = true;
    private void OnDisable() => shouldRotate = false;

    private void Update()
    {
        if (!shouldRotate || pivotPoint == null) return;

        // Rotate THIS object (the parent with the sprite)
        // around the pivotPoint's position
        transform.RotateAround(pivotPoint.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
