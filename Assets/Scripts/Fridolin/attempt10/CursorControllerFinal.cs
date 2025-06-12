// CursorController.cs
// Attach this to the PlayerCursor prefab (the same GameObject that has PlayerInput).

using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CursorControllerFinal : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed = 10f;

    [Header("Selection Settings")]
    public LayerMask selectableLayer;
    // = LayerMask.GetMask("Selectable") at runtime, or assign in inspector.

    // Internal state:
    private Vector2 moveInput;
    private bool hasPicked = false;                // Did this player finish selecting?
    private bool isInPlacementPhase = false;       // Toggles behavior after all players have selected
    private GameObject pickedObjectPrefab;         // The prefab that this player picked
    private GameObject attachedInstance = null;    // The runtime instance in placement phase

    // Reference to our own PlayerInput (so we can check playerIndex, etc.)
    public PlayerInput playerInput;


    private bool gridActive = false;
    private GridItem gridItem;
    private Vector3Int lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);


    private void Awake()
    {
        // this looks up the first PlayerInput in this object or any parent
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
            Debug.LogError("CursorController: no PlayerInput in parents!");
        if (selectableLayer.value == 0)
            selectableLayer = LayerMask.GetMask("Selectable");
    }
    /*
    private void Update()
    {
        // 1) Move the cursor around in world space every frame (unless we already "picked" in selection phase).
        if (!hasPicked)
        {
            Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * Time.deltaTime;
            transform.position += delta;
        }
        // 2) In placement phase, if we have an attachedInstance, move both cursor + attachedInstance
        else if (isInPlacementPhase && attachedInstance != null)
        {
            Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * Time.deltaTime;
            transform.position += delta;
            attachedInstance.transform.position = transform.position;
        }
    }*/

    void Update()
    {
        // need this here but not in the old scene for some reason
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        // before pick: free movement (unchanged)
        if (!hasPicked)
        {

            Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f)
                            * moveSpeed * Time.deltaTime;
            transform.position += delta;

            return;
        }

        // during placement: drive highlight on shared TempTilemap
        if (isInPlacementPhase && gridItem != null && !gridItem.Placed)
        {
            // move cursor graphic
            Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f)
                            * moveSpeed * Time.deltaTime;
            transform.position += delta;

            // compute which cell we’re over
            var gps = GridPlacementSystem.Instance;
            Vector3Int cell = gps.gridLayout.WorldToCell(transform.position);


            // clear old highlight
            if (lastCell.x != int.MinValue)
                gps.TempTilemap.SetTile(lastCell, null);

            // set new highlight
            // new:
            gps.TempTilemap.SetTile(cell, gps.highlightTile);
            // or use your own highlightTile if you added one
            lastCell = cell;
            GridPlacementSystem.Instance.FollowItem(gridItem);

        }
    }



    // THIS MUST be CallbackContext, not InputValue
    public void OnMove(InputAction.CallbackContext ctx)
    {
        // ReadVector2 from the context:
        moveInput = ctx.ReadValue<Vector2>();
    }
    /*
    // THIS MUST be CallbackContext, not InputValue
    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        // Only run on the "performed" phase (i.e. button‐down).
        if (!ctx.performed) return;

        if (!hasPicked)
        {
            Vector2 cursorPos = transform.position;
            Collider2D hit = Physics2D.OverlapPoint(cursorPos, selectableLayer);

            if (hit != null)
            {
                SelectableItem itemScript = hit.GetComponent<SelectableItem>();
                if (itemScript != null && itemScript.isAvailable)
                {
                    itemScript.isAvailable = false;
                    hit.gameObject.SetActive(false);

                    pickedObjectPrefab = itemScript.originalPrefab;
                    GameManager.Instance.NotifyPlayerPicked(playerInput.playerIndex, pickedObjectPrefab);

                    hasPicked = true;

                    //GetComponent<SpriteRenderer>().enabled = false;
                    //GetComponent<Collider2D>().enabled = false;
                }
            }
        }
        else if (isInPlacementPhase && attachedInstance != null)
        {
            attachedInstance.transform.SetParent(null);
            attachedInstance = null;

            GameManager.Instance.NotifyPlayerPlaced(playerInput.playerIndex);
            Destroy(gameObject);
        }
    }*/

    public void OnSubmit(InputAction.CallbackContext ctx)
    {

        Debug.Log("on submit hit");
        // Only act on the “performed” phase (button-down)
        if (!ctx.performed)
            return;

        // 1) SELECTION PHASE
        if (!hasPicked)
        {
            Vector2 cursorPos = transform.position;
            Collider2D hit = Physics2D.OverlapPoint(cursorPos, selectableLayer);
            Debug.Log("hit thing: " + hit);
            if (hit != null)
            {
                Debug.Log("hit thing: " + hit);
                SelectableItem itemScript = hit.GetComponent<SelectableItem>();
                if (itemScript != null && itemScript.isAvailable)
                {
                    itemScript.isAvailable = false;
                    hit.gameObject.SetActive(false);

                    pickedObjectPrefab = itemScript.originalPrefab;


                    SurpriseBoxState.Instance.NotifyPlayerPicked(playerInput.playerIndex, pickedObjectPrefab);



                    hasPicked = true;

                    //GetComponent<SpriteRenderer>().enabled = false;
                    //GetComponent<Collider2D>().enabled = false;
                }
            }
        }
        // 2) PLACEMENT PHASE
        else if (isInPlacementPhase && gridItem != null && !gridItem.Placed)
        {
            if (gridItem.CanBePlaced() == true)
            {
                // This stamps the MainTilemap internally and marks Placed = true
                gridItem.Place();

                gridItem.transform.SetParent(null);

                // Tell the GameManager this player is done
                PlaceItemState.Instance.NotifyPlayerPlaced(playerInput.playerIndex);

            }
        }
    }

    /*
    // This will be called by GameManager when the placement phase begins
    public void BeginPlacementPhase(GameObject prefabToAttach, Vector3 startPosition)
    {
        isInPlacementPhase = true;
        hasPicked = true; // Already picked.

        // Re-enable the cursor sprite & collider (in case they were hidden)
        Debug.Log("gonna enable cursor");

        transform.position = startPosition;

        // Instantiate the chosen prefab as a child of this cursor
        attachedInstance = Instantiate(prefabToAttach, transform.position, Quaternion.identity);
        attachedInstance.transform.SetParent(transform);
        attachedInstance.SetActive(true);
        //GetComponent<SpriteRenderer>().enabled = true;
        //GetComponent<Collider2D>().enabled = true;
        lastCell = Vector3Int.zero;  // invalidate
        hasPicked = true;
        isInPlacementPhase = true;
        // optionally move the cursor to its startPosition
        transform.position = startPosition;
    }*/

    // called once per player by GameManager
    public void BeginPlacementPhase(GameObject prefabToAttach, Transform startPosition)
    {
        hasPicked = true;
        isInPlacementPhase = true;

        // move cursor into starting cell

        // spawn the GridItem prefab as a child of this cursor:
        var go = Instantiate(prefabToAttach,
                             transform.position,
                             Quaternion.identity,
                             transform);     // <-- parent = this.transform
        gridItem = go.GetComponent<GridItem>();
        go.SetActive(true);
        // show the grid once (GameManager already called ShowGrid())
        // reset highlight tracker
        lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    }



}
