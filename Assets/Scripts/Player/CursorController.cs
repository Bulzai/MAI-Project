using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    public static event Action OnCantPlaceItem;
    public static event Action OnEnableCursor;
    
    [Header("Movement Settings")]
    [SerializeField] private float maxMoveSpeed = 25f;   // Units per second
    [SerializeField] private float stickDeadzone = 0.2f; // Ignore tiny stick inputs

    [Header("Selection Settings")]
    [SerializeField] private LayerMask selectableLayer;
    [SerializeField] private Transform hoverOrigin; // assign in Inspector

    private Vector2 moveInput;

    private GridItem gridItem;
    private Vector3Int lastCell = Vector3Int.one * int.MinValue;
    private GameObject gameWorld;
    private Transform itemsParent;
    private PlayerInput playerInput;

    // Hover state
    private GameObject currentlyHoveredGO;
    private HoverHighlight currentlyHoveredHighlight;
    [Header("Movement Bounds")]
    [SerializeField] private BoxCollider2D surpriseBoxStateBounds;
    [SerializeField] private BoxCollider2D placeItemStateBounds;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        if (!playerInput)
            Debug.LogError("CursorController: no PlayerInput in parents!");

        if (selectableLayer.value == 0)
            selectableLayer = LayerMask.GetMask("Selectable");

        foreach (var t in FindObjectsOfType<Transform>(true))
            if (t.name == "Game") gameWorld = t.gameObject;

        for (int i = 0; i < gameWorld.transform.childCount; i++)
        {
            var child = gameWorld.transform.GetChild(i);
            if (child.name == "ItemsParent")
            {
                Debug.Log("found ItemsParent");
                itemsParent = child;
                break;
            }
        }
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
    
    private void OnEnable()
    {
        OnEnableCursor?.Invoke();
    }

    public void SetBoundsSuprisoeBoxState(BoxCollider2D bounds)
    {
        surpriseBoxStateBounds = bounds;
    }

    public void SetBoundsPlaceItemState(BoxCollider2D bounds)
    {
        placeItemStateBounds = bounds;
    }
    private void HandleMovement()
    {
        Vector2 movement = moveInput * maxMoveSpeed * Time.deltaTime;
        transform.position += (Vector3)movement;

        if (surpriseBoxStateBounds && placeItemStateBounds)
            ClampToColliderBounds();
    }

    private void ClampToColliderBounds()
    {
        if(GameEvents.CurrentState == GameState.PlaceItemState)
        {
            Bounds b = placeItemStateBounds.bounds;
            Vector3 pos = transform.position;

            pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
            pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);

            transform.position = pos;
            return;
        }

        if (GameEvents.CurrentState == GameState.SurpriseBoxState)
        {
            Bounds b = surpriseBoxStateBounds.bounds;
            Vector3 pos = transform.position;

            pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
            pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);

            transform.position = pos;
        }
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
        if (gridItem == null || gridItem.Placed)
        {
            OnCantPlaceItem?.Invoke();
            return;
        }

        if (!gridItem.CanBePlaced() && gridItem.isAttachable == false)       
        {
            OnCantPlaceItem?.Invoke();
            return;
        }
        if (!gridItem.SupportedItemCanBePlaced && gridItem.isAttachable == true)
        {
            OnCantPlaceItem?.Invoke();
            return;
        }
        
        if (gridItem.isAttachable && gridItem.ifAttachableAttachHere != null)
        {
            gridItem.transform.SetParent(itemsParent);
            //gridItem.transform.SetParent(gridItem.ifAttachableAttachHere, true);
        }
        gridItem.Place();

        if (!gridItem.isAttachable) gridItem.transform.SetParent(itemsParent);

        gridItem.gameObject.layer = LayerMask.NameToLayer("Ground/Wall");

        //remove hover script because otherwise weird scaling happens
        HoverHighlight hoverHighlight = gridItem.GetComponent<HoverHighlight>();
        hoverHighlight.RemoveHover();
        Destroy(hoverHighlight);
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
        if (go.TryGetComponent(out Collider2D col))
            col.enabled = true;

        if (go.GetComponent<Animator>())
        {
            go.GetComponent<Animator>().enabled = true;
        }
        // Enable all scripts EXCEPT certain types
        foreach (var script in go.GetComponents<MonoBehaviour>())
        {
            if (script == null) continue;

            // Skip the ones you don't want active during placement
            if (script is ProjectileLauncher || script is RotateSpike)
                continue;

            script.enabled = true;
        }

        var gif = go.transform.Find("Gif");
        if (gif) gif.gameObject.SetActive(false);

        var directionPointer = go.transform.Find("direction_pointer");
        if(directionPointer)
            directionPointer.gameObject.SetActive(true);

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
        Vector2 pos = (Vector2)hoverOrigin.position;

        RaycastHit2D hit = Physics2D.CircleCast(pos, 0.5f, Vector2.zero, 0f, selectableLayer);
        GameObject next = ResolveSelectableRoot(hit.collider);

        if (next == currentlyHoveredGO) return;

        // Remove hover from previous
        if (currentlyHoveredHighlight != null)
            currentlyHoveredHighlight.RemoveHover();

        // Disable scripts on previous hovered object
        if (currentlyHoveredGO != null)
        {
            //TogglePlacementScripts(currentlyHoveredGO, false);
            DisableGif();
        }

        currentlyHoveredGO = next;
        currentlyHoveredHighlight = null;

        // Add hover to new
        if (next != null)
        {
            var hh = next.GetComponent<HoverHighlight>();
            if (!hh) hh = next.AddComponent<HoverHighlight>();
            hh.AddHover();
            currentlyHoveredHighlight = hh;

            //  If it's a BreakableCracker, make it break
            // inside HandleHoverHighlight(), when next != null (new hover target)
            /*var cracker = next.GetComponent<BreakableCracker>();
            if (cracker != null)
            {
                cracker.BreakInstantly(fastReset: true); // uses hoverRespawnDelay
            }*/

            EnableGif();
            //still enable other hover effects
            //TogglePlacementScripts(next, true); 
        }
    }

    private void ClearHoverIfAny()
    {
        if (currentlyHoveredHighlight != null)
        {
            currentlyHoveredHighlight.RemoveHover();         
            currentlyHoveredHighlight = null;
        }

        // Disable scripts again when you leave hover
        if (currentlyHoveredGO != null)
        {
            DisableGif();

            //TogglePlacementScripts(currentlyHoveredGO, false);
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

    private void EnableGif()
    {
        Transform gif = currentlyHoveredGO.transform.Find("Gif");

        if (gif != null)
        {
            gif.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Gif child not found", this);
        }
    }

    private void DisableGif()
    {
        Transform gif = currentlyHoveredGO.transform.Find("Gif");
        Debug.Log("Disable gif");
        if (gif != null)
        {
            gif.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Gif child not found", this);
        }

    }
    private void TogglePlacementScripts(GameObject target, bool enable)
    {
        if (!target) return;

        var launcher = target.GetComponent<ProjectileLauncher>();
        if (launcher) launcher.enabled = enable;

        //var spike = target.GetComponent<RotateSpike>();
        //if (spike) spike.enabled = enable;

        // Optional: log to verify behavior
        // Debug.Log($"{target.name}: {(enable ? "ENABLED" : "DISABLED")} placement scripts");
    }

}
