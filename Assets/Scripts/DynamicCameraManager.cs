using UnityEngine;

public class DynamicCamera2DManager : MonoBehaviour
{
    [Header("References")]
    public Camera DynamicCamera;
    [SerializeField] private Transform player;
    [SerializeField] private Transform milkContainer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float fastMoveMultiplier = 2f;
    [SerializeField] private float distanceThreshold = 5f;

    [Header("Zoom Limits")]
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 21f;

    [Header("Padding")]
    [Range(0f, 1f)]
    public float padding = 0.1f;

    [Header("Fallback")]
    public Vector3 fallbackPosition = Vector3.zero;
    public float fallbackSize = 21f;

    private bool dynamicEnabled = false;
    private Transform[] FocusObjects;
    private Vector3 lastCenter;

    private void Awake()
    {
        GameEvents.OnPlayerSelectionStateEntered += SetCameraPlayerSelectionStatePosition;
        GameEvents.OnSurpriseBoxStateEntered += SetCameraSurpriseBoxStatePosition;
        GameEvents.OnPlaceItemStateEntered += SetCameraPlaceItemStatePosition;
        PlaceItemState.CountDownFinished += EnableDynamicCamera2D;
        PlaceItemState.CountDownStarted += SetCameraDefaultPosition;
        GameEvents.OnScoreStateEntered += SetCameraDefaultPosition;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerSelectionStateEntered -= SetCameraPlayerSelectionStatePosition;
        GameEvents.OnSurpriseBoxStateEntered -= SetCameraSurpriseBoxStatePosition;
        GameEvents.OnPlaceItemStateEntered -= SetCameraPlaceItemStatePosition;
        PlaceItemState.CountDownFinished -= EnableDynamicCamera2D;
        PlaceItemState.CountDownStarted -= SetCameraDefaultPosition;
        GameEvents.OnScoreStateEntered -= SetCameraDefaultPosition;
    }

    void Start()
    {
        if (DynamicCamera == null)
            DynamicCamera = Camera.main;

        DynamicCamera.orthographic = true;
    }

    void Update()
    {
        if (!dynamicEnabled) return;

        UpdateFocusObjects();

        if (FocusObjects.Length == 0)
        {
            // fallback
            DynamicCamera.transform.position = Vector3.Lerp(
                DynamicCamera.transform.position,
                new Vector3(fallbackPosition.x, fallbackPosition.y, DynamicCamera.transform.position.z),
                Time.deltaTime * moveSpeed
            );

            DynamicCamera.orthographicSize = Mathf.Lerp(
                DynamicCamera.orthographicSize,
                fallbackSize,
                Time.deltaTime * zoomSpeed
            );

            return;
        }

        // --- Calculate bounds ---
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Transform tr in FocusObjects)
        {
            Vector3 pos = tr.position;

            if (pos.x < min.x) min.x = pos.x;
            if (pos.y < min.y) min.y = pos.y;
            if (pos.x > max.x) max.x = pos.x;
            if (pos.y > max.y) max.y = pos.y;
        }

        // --- Center ---
        Vector3 center = (min + max) * 0.5f;
        center.z = DynamicCamera.transform.position.z;

        // --- Dynamic speed boost ---
        float distance = Vector3.Distance(lastCenter, center);
        float dynamicMoveSpeed = distance > distanceThreshold
            ? moveSpeed * fastMoveMultiplier
            : moveSpeed;

        // --- Move camera ---
        DynamicCamera.transform.position = Vector3.Lerp(
            DynamicCamera.transform.position,
            center,
            Time.deltaTime * dynamicMoveSpeed
        );

        lastCenter = center;

        // --- Zoom calculation ---
        float width = max.x - min.x;
        float height = max.y - min.y;

        float sizeBasedOnWidth = (width * 0.5f) / DynamicCamera.aspect * (1f + padding);
        float sizeBasedOnHeight = (height * 0.5f) * (1f + padding);

        float targetOrthoSize = Mathf.Max(sizeBasedOnWidth, sizeBasedOnHeight);
        targetOrthoSize = Mathf.Clamp(targetOrthoSize, minOrthographicSize, maxOrthographicSize);

        // --- Apply zoom ---
        DynamicCamera.orthographicSize = Mathf.Lerp(
            DynamicCamera.orthographicSize,
            targetOrthoSize,
            Time.deltaTime * zoomSpeed
        );
    }

    void UpdateFocusObjects()
    {
        int maxCount = 1 + (milkContainer != null ? milkContainer.childCount : 0);
        Transform[] temp = new Transform[maxCount];

        int index = 0;

        // Player
        if (player != null)
            temp[index++] = player;

        // Active milk cartons
        if (milkContainer != null)
        {
            for (int i = 0; i < milkContainer.childCount; i++)
            {
                Transform child = milkContainer.GetChild(i);

                if (child.gameObject.activeInHierarchy)
                    temp[index++] = child;
            }
        }

        FocusObjects = new Transform[index];
        System.Array.Copy(temp, FocusObjects, index);
    }

    // --- State Camera Positions ---

    private void SetCameraPlayerSelectionStatePosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 15;
        DynamicCamera.transform.position = new Vector3(-4.2f, -8, -30);
    }

    private void SetCameraSurpriseBoxStatePosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 13;
        DynamicCamera.transform.position = new Vector3(0f, 1.86f, -30);
    }

    private void SetCameraPlaceItemStatePosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 22f;
        DynamicCamera.transform.position = new Vector3(0f, 5.93f, -30f);
    }

    private void SetCameraDefaultPosition()
    {
        dynamicEnabled = false;
        DynamicCamera.orthographicSize = 21f;
        DynamicCamera.transform.position = new Vector3(0, 8, -30);
    }

    private void EnableDynamicCamera2D()
    {
        dynamicEnabled = true;
    }
}