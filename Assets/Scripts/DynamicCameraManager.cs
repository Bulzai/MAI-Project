using UnityEngine;

public class DynamicCamera2DManager : MonoBehaviour
{
    public Camera MyCam;
    public Transform[] FocusObjects;
    public float speed = 3f;
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 20f;
    public float padding = 0.1f; // Padding around objects in screen space (0-1)
    public Vector3 fallbackPosition = Vector3.zero; // Position to use when no valid targets
    public float fallbackSize = 10f; // Size to use when no valid targets

    void Update()
    {
        // Ensure we're using orthographic projection for 2D
        MyCam.orthographic = true;

        // Filter out null transforms
        var validTransforms = System.Array.FindAll(FocusObjects, t => t != null);

        // If no valid transforms, use fallback position and size
        if (validTransforms.Length == 0)
        {
            MyCam.transform.position = Vector3.Lerp(
                MyCam.transform.position,
                new Vector3(fallbackPosition.x, fallbackPosition.y, MyCam.transform.position.z),
                Time.deltaTime * speed
            );

            MyCam.orthographicSize = Mathf.Lerp(
                MyCam.orthographicSize,
                fallbackSize,
                Time.deltaTime * speed
            );
            return;
        }

        // Calculate bounding box that contains all focus objects
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Transform tr in validTransforms)
        {
            Vector3 pos = tr.position;
            if (pos.x < min.x) min.x = pos.x;
            if (pos.y < min.y) min.y = pos.y;
            if (pos.x > max.x) max.x = pos.x;
            if (pos.y > max.y) max.y = pos.y;
        }

        // Calculate center point
        Vector3 center = (min + max) * 0.5f;
        center.z = MyCam.transform.position.z; // Maintain camera's z position

        // Move camera towards center
        MyCam.transform.position = Vector3.Lerp(
            MyCam.transform.position,
            center,
            Time.deltaTime * speed
        );

        // Calculate required orthographic size
        float width = max.x - min.x;
        float height = max.y - min.y;

        // Calculate required size based on width/height (add padding)
        float sizeBasedOnWidth = (width * 0.5f) / MyCam.aspect * (1f + padding);
        float sizeBasedOnHeight = (height * 0.5f) * (1f + padding);

        float targetOrthoSize = Mathf.Max(sizeBasedOnWidth, sizeBasedOnHeight);
        targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthographicSize, maxOrthographicSize);

        // Smoothly adjust orthographic size
        MyCam.orthographicSize = Mathf.Lerp(
            MyCam.orthographicSize,
            targetOrthoSize,
            Time.deltaTime * speed
        );
    }

    // Optional: Public method to update focus objects list
    public void UpdateFocusObjects(Transform[] newFocusObjects)
    {
        FocusObjects = newFocusObjects;
    }
}