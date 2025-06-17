// CursorController.cs
// Attach this to the PlayerCursor prefab (the same GameObject that has PlayerInput).

using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

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

    private GameObject GameWorld;
    private GameObject currentlyHovered;
    private void Awake()
    {
        // this looks up the first PlayerInput in this object or any parent
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
            Debug.LogError("CursorController: no PlayerInput in parents!");
        if (selectableLayer.value == 0)
            selectableLayer = LayerMask.GetMask("Selectable");

        var allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
        foreach (var t in allTransforms)
            if (t.name == "Game")
                GameWorld = t.gameObject;

        if (GameWorld == null)
            Debug.LogError("Could not find a GameObject named 'Game' in the scene!");
    
    
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
        // probably unnecessary
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        // move cursor graphic
        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f)
                        * moveSpeed * Time.deltaTime;
        transform.position += delta;


        // before pick: free movement (unchanged)
        if (!hasPicked)
        {
            HandleHoverHighlight();
            return;
        }

        // during placement: drive highlight on shared TempTilemap
        if (isInPlacementPhase && gridItem != null && !gridItem.Placed)
        {
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
    void LogMyAndParentPositions()
    {
        Debug.Log("printing transforms:");
        Vector2 pos2D = transform.position;
        Debug.Log("transform.position: " + pos2D);
        // grabs this Transform plus _all_ parent Transforms
        var chain = GetComponentsInParent<Transform>();
        foreach (var t in chain)
        {
            Debug.Log($"[{t.gameObject.name}] worldPos = {t.position}, localPos = {t.localPosition}", t.gameObject);
        }
    }
    /*
    void OnDrawGizmos()
    {
        // draws a small green circle at the cursor’s world‐space position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
    */
    void LogEveryCursor()
    {
        Debug.Log("transform: " + transform.position);
        foreach (var cc in FindObjectsOfType<CursorControllerFinal>())
        {
            Debug.Log(
              $"[ALL CURSORS] {cc.gameObject.name} @ world={cc.transform.position}",
              cc.gameObject
            );
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {

        LogEveryCursor();
        Debug.Log("on submit hit");
        // Only act on the “performed” phase (button-down)
        if (!ctx.performed)
            return;

        Vector2 pos2D = transform.position;
        Debug.Log($"[{gameObject.name}] world-pos = {transform.position}");
        Debug.Log($"[{gameObject.name}] world-pos = {transform.position}", this);

        //Debug.Log($"[Cursor] world-pos = {pos2D}   mask = {selectableLayer.value}");

        // flood the area with a tiny circle test
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos2D, 0.1f, selectableLayer);
        if (hits.Length == 0)
        {
            Debug.Log("OverlapCircleAll found ZERO colliders.");
        }
        else
        {
            foreach (var h in hits)
                Debug.Log("    hit → " + h.name + " (layer=" + LayerMask.LayerToName(h.gameObject.layer) + ")");
        }

        // now try the point query too
        var single = Physics2D.OverlapPoint(pos2D, selectableLayer);
        Debug.Log("OverlapPoint     → " + (single ? single.name : "null"));

        // 1) Surprisebox PHASE
        if (!hasPicked)
        {
            Vector2 cursorPos = transform.position;
            Debug.Log("cursor pos: " + cursorPos);
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


                    SurpriseBoxState.Instance.NotifyPlayerPicked(playerInput.playerIndex, itemScript.getOriginalPrefab());



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
                // Optional: change the color of the sprite
                var sprite = gridItem.transform.Find("Sprite");
                if (sprite != null)
                {
                    var sr = sprite.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.color =  Color.white;
                }
                gridItem.transform.SetParent(GameWorld.transform);

                // Tell the GameManager this player is done
                PlaceItemState.Instance.NotifyPlayerPlaced(playerInput.playerIndex);
                hasPicked = false;
                isInPlacementPhase = false;
                gridItem = null;
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
        Debug.Log($"go.activeSelf = {go.activeSelf}");         // Is the GameObject itself active?
        Debug.Log($"go.activeInHierarchy = {go.activeInHierarchy}"); // Is it active in the scene (including parents)?

        go.SetActive(true);

        Collider2D col = go.GetComponent<Collider2D>();
        if (col != null && !col.enabled)
        {
            col.enabled = true;
            Debug.Log("Collider2D was disabled — enabled manually.");
        }

        // Manually re-enable all scripts
        foreach (var script in go.GetComponents<MonoBehaviour>())
        {
            if (!script.enabled)
            {
                script.enabled = true;
                Debug.Log($"Script {script.GetType().Name} was disabled — enabled manually.");
            }
        }

        // show the grid once (GameManager already called ShowGrid())
        // reset highlight tracker
        lastCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        // DIAGNOSTIC REPORT
        MonoBehaviour[] scripts = go.GetComponents<MonoBehaviour>();
        if( col != null )
        {
            Debug.Log("col is null");
        }
        Debug.Log($"[Spawn Debug] Object: {go.name} | Active: {go.activeSelf} | Collider2D: {(col ? col.enabled.ToString() : "null")}");

    }

    private void HandleHoverHighlight()
    {
        Debug.Log("handlehoverhighlight started");
        Vector2 pos2D = transform.position;
        Collider2D hit = Physics2D.OverlapPoint(pos2D, selectableLayer);

        if (hit != null)
        {
            GameObject hitObject = hit.gameObject;

            if (hitObject != currentlyHovered)
            {
                // Turn off previous highlight
                if (currentlyHovered != null)
                    ToggleOutline(currentlyHovered, false);

                // Turn on new highlight
                ToggleOutline(hitObject, true);
                currentlyHovered = hitObject;
            }
        }
        else
        {
            if (currentlyHovered != null)
            {
                ToggleOutline(currentlyHovered, false);
                currentlyHovered = null;
            }
        }
    }
    private void ToggleOutline(GameObject root, bool enable)
    {
        // Find child named "Outline" and activate/deactivate it

        Debug.Log("ToggleOutline started");
        Debug.Log("GameObject to outline:" + root);
        var outline = root.transform.Find("Outline");
        if (outline != null)
        {
            var ol = outline.GetComponent<Outline>();

            outline.gameObject.SetActive(enable);
            Debug.Log("outline enabled");
        }

        // Optional: change the color of the sprite
        var sprite = root.transform.Find("Sprite");
        if (sprite != null)
        {
            var sr = sprite.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = enable ? Color.yellow : Color.white;
        }
    }
    // CursorControllerFinal.cs

    // Make sure these live alongside your other OnXXX(InputAction.CallbackContext) methods:
    public void OnRotateLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isInPlacementPhase && gridItem != null && !gridItem.Placed)
        {
            gridItem.RotateCounterclockwise();
            // Force an immediate redraw of the blue/red highlight
            GridPlacementSystem.Instance.FollowItem(gridItem);
        }
    }

    public void OnRotateRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isInPlacementPhase && gridItem != null && !gridItem.Placed)
        {
            gridItem.RotateClockwise();
            GridPlacementSystem.Instance.FollowItem(gridItem);
        }
    }

}
