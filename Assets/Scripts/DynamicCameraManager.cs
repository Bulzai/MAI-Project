using UnityEngine;

public class DynamicCamera2DManager : MonoBehaviour
{
    public Camera DynamicCamera;
    public float speed = 1f;
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 20f;
    public float padding = 0.1f; // Padding around objects in screen space (0-1)
    public Vector3 fallbackPosition = Vector3.zero; // Position to use when no valid targets
    public float fallbackSize = 10f; // Size to use when no valid targets
    private bool dynamicEnabled = false;
    
    
    private Transform[] FocusObjects;
    
    
    private void OnEnable()
    {
        GameEvents.OnPlayerSelectionStateEntered += SetCameraPlayerSelectionStatePosition;
        GameEvents.OnSurpriseBoxStateEntered += SetCameraSurpriseBoxStatePosition;
        GameEvents.OnPlaceItemStateEntered += SetCameraPlaceItemStatePosition;
        GameEvents.OnMainGameStateEntered +=  EnableDynamicCamera2D;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSelectionStateEntered -= SetCameraPlayerSelectionStatePosition;
        GameEvents.OnSurpriseBoxStateEntered -= SetCameraSurpriseBoxStatePosition;
        GameEvents.OnPlaceItemStateEntered -= SetCameraPlaceItemStatePosition;
        GameEvents.OnMainGameStateEntered -=  EnableDynamicCamera2D;
    }
    
    void Start()
    {
        if (DynamicCamera == null)
            DynamicCamera = Camera.main; // Auto-assigns the main camera [note: because the D&D didn't work in the Inspector of this script]
        DynamicCamera.orthographic = true;

    }

    void Update()
    {
       if (!dynamicEnabled) return;
        UpdateFocusObjects();

        // Filter out null transforms
        var validTransforms = System.Array.FindAll(FocusObjects, t => t != null);

        // If no valid transforms, use fallback position and size
        if (validTransforms.Length == 0)
        {
            DynamicCamera.transform.position = Vector3.Lerp(
                DynamicCamera.transform.position,
                new Vector3(fallbackPosition.x, fallbackPosition.y, DynamicCamera.transform.position.z),
                Time.deltaTime * speed
            );

            DynamicCamera.orthographicSize = Mathf.Lerp(
                DynamicCamera.orthographicSize,
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
        center.z = DynamicCamera.transform.position.z;

        // Move camera towards center
        DynamicCamera.transform.position = Vector3.Lerp(
            DynamicCamera.transform.position,
            center,
            Time.deltaTime * speed
        );

        // Calculate required orthographic size
        float width = max.x - min.x;
        float height = max.y - min.y;

        // Calculate required size based on width/height (add padding)
        float sizeBasedOnWidth = (width * 0.5f) / DynamicCamera.aspect * (1f + padding);
        float sizeBasedOnHeight = (height * 0.5f) * (1f + padding);

        float targetOrthoSize = Mathf.Max(sizeBasedOnWidth, sizeBasedOnHeight);
        targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthographicSize, maxOrthographicSize);

        // Smoothly adjust orthographic size
        DynamicCamera.orthographicSize = Mathf.Lerp(
            DynamicCamera.orthographicSize,
            targetOrthoSize,
            Time.deltaTime * speed
        );
    }

    void UpdateFocusObjects()
    {
        // Get the Players dynamically and not manually into the Inspector
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        FocusObjects = System.Array.ConvertAll(playerObjects, p => p.transform);
    }


    private void SetCameraPlayerSelectionStatePosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 15;
        DynamicCamera.transform.position = new Vector3(-4.2f, -8, -30);
    }

    private void SetCameraSurpriseBoxStatePosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 25;
        DynamicCamera.transform.position = new Vector3(0f, 0, -30);
    }

    private void SetCameraPlaceItemStatePosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 21.94311f;
        DynamicCamera.transform.position = new Vector3(0f, 6.789f, -30);
    }
    private void EnableDynamicCamera2D()
    {
        dynamicEnabled = true;
    }
}
