using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxMoveSpeed = 25f;   // Units per second
    [SerializeField] private float stickDeadzone = 0.2f; // Ignore tiny stick inputs

    [Header("Selection Settings")]
    [SerializeField] private LayerMask selectableLayer;

    private Vector2 moveInput;

    private GridItem gridItem;
    private Vector3Int lastCell = Vector3Int.one * int.MinValue;
    private GameObject gameWorld;

    private PlayerInput playerInput;

    // Hover state
    private GameObject currentlyHoveredGO;
    private HoverHighlight currentlyHoveredHighlight;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        if (!playerInput)
            Debug.LogError("CursorController: no PlayerInput in parents!");

        if (selectableLayer.value == 0)
            selectableLayer = LayerMask.GetMask("Selectable");

        foreach (var t in FindObjectsOfType<Transform>(true))
            if (t.name == "Game") gameWorld = t.gameObject;

        if (!gameWorld)
            Debug.LogError("Could not find a GameObject named 'Game' in the scene!");
    }

    private void Update()
    {
        HandleMovement();

        switch (GameEvents.CurrentState)
        {
            case GameState.SurpriseBoxState:
                HandleHoverHighlight();
                break;

            case GameState.PlaceItemState:
                ClearHoverIfAny();
                if (gridItem != null && !gridItem.Placed)
                    HandlePlacementMovement();
                break;

            default:
                ClearHoverIfAny();
                break;
        }
    }

    // --- Movement ---
    private void HandleMovement()
    {
        Vector2 movement = moveInput * maxMoveSpeed * Time.deltaTime;
        transform.position += (Vector3)movement;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();

        if (moveInput.magnitude < stickDeadzone)
            moveInput = Vector2.zero;

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    // --- Submit actions ---
    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        switch (GameEvents.CurrentState)
        {
            case GameState.SurpriseBoxState:
                TryPickObject();
                break;

            case GameState.PlaceItemState:
                TryPlaceObject();
                break;
        }
    }

    private void TryPickObject()
    {
        if (!currentlyHoveredGO) return;

        var item = currentlyHoveredGO.GetComponent<SelectableItem>();
        if (item == null || !item.isAvailable) return;

        item.isAvailable = false;

        if (currentlyHoveredHighlight)
            currentlyHoveredHighlight.RemoveHover();

        currentlyHoveredGO.SetActive(false);

        SurpriseBoxState.Instance.NotifyPlayerPicked(
            playerInput.playerIndex,
            item.getOriginalPrefab()
        );

        currentlyHoveredGO = null;
        currentlyHoveredHighlight = null;
    }

    private void TryPlaceObject()
    {
        if (gridItem == null || gridItem.Placed) return;

        if (!gridItem.CanBePlaced() && gridItem.requiresSupport == false) return;

        if (!gridItem.SupportedItemCanBePlaced && gridItem.requiresSupport == true) return;

        if (gridItem.requiresSupport && gridItem.ifAttachableAttachHere != null)
        {
            Debug.Log("griditem iffattachable inside: " + gridItem.ifAttachableAttachHere);

            gridItem.transform.SetParent(gridItem.ifAttachableAttachHere, true);
        }
        gridItem.Place();
        Debug.Log("griditem iffattachable: " + gridItem.ifAttachableAttachHere);

        if (!gridItem.requiresSupport) gridItem.transform.SetParent(gameWorld.transform);

        PlaceItemState.Instance.NotifyPlayerPlaced(playerInput.playerIndex);
        gridItem = null;
    }

    public void OnSkipPlacement(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (GameEvents.CurrentState != GameState.PlaceItemState) return;

        if (gridItem != null)
        {
            Destroy(gridItem.gameObject);
            gridItem = null;
        }

        PlaceItemState.Instance.NotifyPlayerPlaced(playerInput.playerIndex);
        gameObject.SetActive(false);
    }

    // --- Placement ---
    public void BeginPlacementPhase(GameObject prefabToAttach, Transform startPosition)
    {
        var go = Instantiate(prefabToAttach, transform.position, Quaternion.identity, transform);
        gridItem = go.GetComponent<GridItem>();

        if (!gridItem)
            Debug.LogError("Spawned object missing GridItem component!");

        go.SetActive(true);
        if (go.TryGetComponent(out Collider2D col)) col.enabled = true;

        foreach (var script in go.GetComponents<MonoBehaviour>())
            script.enabled = true;

        lastCell = Vector3Int.one * int.MinValue;
    }
    /*
    private void HandlePlacementMovement()
    {
        var gps = GridPlacementSystem.Instance;

        Vector3Int cell = gps.gridLayout.WorldToCell(transform.position);
        Vector3 snappedWorld = gps.gridLayout.CellToWorld(cell);


        gridItem.transform.position = snappedWorld;

        if (lastCell.x != int.MinValue)
            gps.TempTilemap.SetTile(lastCell, null);

        gps.FollowItem(gridItem);
        lastCell = cell;
    }
    */

    private void HandlePlacementMovement()
    {
        var gps = GridPlacementSystem.Instance;

        Vector3Int cell = gps.gridLayout.WorldToCell(transform.position);
        Vector3 snappedWorld = gps.gridLayout.CellToWorld(cell) + gps.gridLayout.cellSize / 2f;

        // ✅ Apply item-specific placement offset
        Vector3 finalPosition = snappedWorld + gridItem.placementOffset;

        gridItem.transform.position = finalPosition;

        if (lastCell.x != int.MinValue)
            gps.TempTilemap.SetTile(lastCell, null);

        gps.FollowItem(gridItem);
        lastCell = cell;
    }


    // --- Hover highlight ---
    private void HandleHoverHighlight()
    {
        Vector2 pos = transform.position;

        RaycastHit2D hit = Physics2D.CircleCast(pos, 0.5f, Vector2.zero, 0f, selectableLayer);
        GameObject next = ResolveSelectableRoot(hit.collider);

        if (next == currentlyHoveredGO) return;

        // Remove hover from previous
        if (currentlyHoveredHighlight != null)
            currentlyHoveredHighlight.RemoveHover();

        currentlyHoveredGO = next;
        currentlyHoveredHighlight = null;

        // Add hover to new
        if (next != null)
        {
            var hh = next.GetComponent<HoverHighlight>();
            if (!hh) hh = next.AddComponent<HoverHighlight>();
            hh.AddHover();
            currentlyHoveredHighlight = hh;
        }
    }

    private void ClearHoverIfAny()
    {
        if (currentlyHoveredHighlight != null)
        {
            currentlyHoveredHighlight.RemoveHover();
            currentlyHoveredHighlight = null;
            currentlyHoveredGO = null;
        }
    }

    private GameObject ResolveSelectableRoot(Collider2D col)
    {
        if (!col) return null;

        // Prefer parent SelectableItem
        var si = col.GetComponentInParent<SelectableItem>();
        if (si) return si.gameObject;

        // Then parent GridItem
        var gi = col.GetComponentInParent<GridItem>();
        if (gi) return gi.gameObject;

        // Fallback: collider object if it's on selectable layer
        if (((1 << col.gameObject.layer) & selectableLayer.value) != 0)
            return col.gameObject;

        return null;
    }

    // --- Rotation ---
    public void OnRotateLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (GameEvents.CurrentState == GameState.PlaceItemState && gridItem != null && !gridItem.Placed)
        {
            gridItem.RotateCounterclockwise();
            GridPlacementSystem.Instance.FollowItem(gridItem);
        }
    }

    public void OnRotateRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (GameEvents.CurrentState == GameState.PlaceItemState && gridItem != null && !gridItem.Placed)
        {
            gridItem.RotateClockwise();
            GridPlacementSystem.Instance.FollowItem(gridItem);
        }
    }
}
