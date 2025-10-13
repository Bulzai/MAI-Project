using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{

    [Header("Attachment Settings")]
    public bool requiresSupport = false;               // true for icy/sticky surfaces
    public Transform ifAttachableAttachHere = null;
    public bool SupportedItemCanBePlaced { get; set; }

    [Header("Placement Settings")]
    public Vector3 placementOffset = Vector3.zero;

    [Header("Forbidden Sign Settings")]
    public Vector3 forbiddenSignOffset = Vector3.zero;  // local positional adjustment
    public float forbiddenSignScale = 1f;               // scale multiplier

    public bool Placed { get; private set; }
    //public BoundsInt area;
    // Start is called before the first frame update
    //private Vector3Int _originalSize;      // to remember your neutral W×H
    //public Vector3Int originalPositionHorizontal;
    //public Vector3Int originalPositionVertical;

    //public Vector3Int adjustPosition;
    private bool ItemIsHorizontal = true;

    public FacingDirection currentFacingDirection;
    //public int length = 1; // 1 or 2
    private List<Vector3Int> occupiedCells = new List<Vector3Int>();


    void Awake()
    {
        //_originalSize = area.size;
        //adjustPosition = originalPositionHorizontal;


    }

    private void UpdateOccupiedCells()
    {
        occupiedCells.Clear();

        Collider2D col = GetComponentInChildren<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning($"[GridItem] No collider found on {name}.");
            return;
        }

        var grid = GridPlacementSystem.Instance.gridLayout;
        Bounds bounds = col.bounds;
        float step = grid.cellSize.x * 0.25f; // smaller step = more reliable coverage

        // Expand slightly to avoid missing border points
        bounds.Expand(0.01f);

        for (float x = bounds.min.x; x <= bounds.max.x; x += step)
        {
            for (float y = bounds.min.y; y <= bounds.max.y; y += step)
            {
                Vector2 p = new Vector2(x, y);
                if (col.OverlapPoint(p))
                {
                    Vector3Int cell = grid.WorldToCell(p);
                    if (!occupiedCells.Contains(cell))
                        occupiedCells.Add(cell);
                }
            }
        }
    }
    public bool CanBePlaced()
    {
        UpdateOccupiedCells();

        foreach (var cell in occupiedCells)
        {
            if (!GridPlacementSystem.Instance.CanTakeCell(cell))
                return false;
        }

        return true;
    }

    public void Place()
    {
        UpdateOccupiedCells();

        foreach (var cell in occupiedCells)
        {
            GridPlacementSystem.Instance.TakeCell(cell);
        }

        Placed = true;
    }


    public List<Vector3Int> GetOccupiedCells()
    {
        UpdateOccupiedCells();
        return occupiedCells;
    }


    /*
    public bool CanBePlaced()
    {
        Vector3Int positionInt = GridPlacementSystem.Instance.gridLayout.LocalToCell(transform.position) - adjustPosition; // adjust to center the item correctly;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if (GridPlacementSystem.Instance.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }

    public void Place()
    {
        if (!requiresSupport)
        {
        Vector3Int positionInt = GridPlacementSystem.Instance.gridLayout.WorldToCell(transform.position) - adjustPosition; // adjust to center the item correctly;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        GridPlacementSystem.Instance.TakeArea(areaTemp);
        }
    }*/
    /*
    public void RotateClockwise()
    {
        // Rotate visually
        transform.Rotate(0f, 0f, -90f);

        // Swap area dimensions (width <-> height)
        area.size = new Vector3Int(area.size.y, area.size.x, area.size.z);

        // Flip between horizontal / vertical
        ItemIsHorizontal = !ItemIsHorizontal;
        adjustPosition = ItemIsHorizontal ? originalPositionHorizontal : originalPositionVertical;

        // Update facing direction (clockwise)
        switch (currentFacingDirection)
        {
            case FacingDirection.Up:
                currentFacingDirection = FacingDirection.Right;
                break;
            case FacingDirection.Right:
                currentFacingDirection = FacingDirection.Down;
                break;
            case FacingDirection.Down:
                currentFacingDirection = FacingDirection.Left;
                break;
            case FacingDirection.Left:
                currentFacingDirection = FacingDirection.Up;
                break;
        }
        UpdateOccupiedCells();

        //UpdateRaycastLocationOffset();
    }

    public void RotateCounterclockwise()
    {
        // Rotate visually
        transform.Rotate(0f, 0f, 90f);

        // Swap area dimensions (width <-> height)
        area.size = new Vector3Int(area.size.y, area.size.x, area.size.z);

        // Flip between horizontal / vertical
        ItemIsHorizontal = !ItemIsHorizontal;
        adjustPosition = ItemIsHorizontal ? originalPositionHorizontal : originalPositionVertical;

        // Update facing direction (counterclockwise)
        switch (currentFacingDirection)
        {
            case FacingDirection.Up:
                currentFacingDirection = FacingDirection.Left;
                break;
            case FacingDirection.Left:
                currentFacingDirection = FacingDirection.Down;
                break;
            case FacingDirection.Down:
                currentFacingDirection = FacingDirection.Right;
                break;
            case FacingDirection.Right:
                currentFacingDirection = FacingDirection.Up;
                break;
        }
        UpdateOccupiedCells();

        //UpdateRaycastLocationOffset();
    }
    */
    /*
    private void UpdatePositionFromRotation()
    {
        // grab the Z angle, rounded to the nearest int, in 0..359
        var rawZ = Mathf.RoundToInt(transform.eulerAngles.z) % 360;
        if (rawZ < 0) rawZ += 360;

        // if we're at 90° or 270° (i.e. “vertical”), swap W/H
        if (rawZ == 90)
            adjustPosition = originalPosition + new Vector3Int(_originalSize.x, 0, 0);
        else if (rawZ == 270)
            adjustPosition = new Vector3Int(5, -originalPosition.x, 0);
        else if (rawZ == 180)
            adjustPosition = new Vector3Int(0, _originalSize.y - originalPosition.y, 0);
        else
            adjustPosition = originalPosition;  
    }
    */

    public void RotateClockwise()
    {
        var grid = GridPlacementSystem.Instance.gridLayout;

        // anchor on the same cell center the cursor is using
        Vector3Int currentCell = grid.WorldToCell(transform.position);
        Vector3 cellCenter = grid.CellToWorld(currentCell) + grid.cellSize / 2f;

        // apply rotation first
        transform.Rotate(0f, 0f, -90f);

        // re-apply anchored position using the rotated offset
        Vector3 worldOffset = transform.rotation * placementOffset;
        transform.position = cellCenter + worldOffset;

        // update facing dir
        switch (currentFacingDirection)
        {
            case FacingDirection.Up: currentFacingDirection = FacingDirection.Right; break;
            case FacingDirection.Right: currentFacingDirection = FacingDirection.Down; break;
            case FacingDirection.Down: currentFacingDirection = FacingDirection.Left; break;
            case FacingDirection.Left: currentFacingDirection = FacingDirection.Up; break;
        }

        UpdateOccupiedCells();
    }

    public void RotateCounterclockwise()
    {
        var grid = GridPlacementSystem.Instance.gridLayout;

        Vector3Int currentCell = grid.WorldToCell(transform.position);
        Vector3 cellCenter = grid.CellToWorld(currentCell) + grid.cellSize / 2f;

        transform.Rotate(0f, 0f, 90f);

        Vector3 worldOffset = transform.rotation * placementOffset;
        transform.position = cellCenter + worldOffset;

        switch (currentFacingDirection)
        {
            case FacingDirection.Up: currentFacingDirection = FacingDirection.Left; break;
            case FacingDirection.Left: currentFacingDirection = FacingDirection.Down; break;
            case FacingDirection.Down: currentFacingDirection = FacingDirection.Right; break;
            case FacingDirection.Right: currentFacingDirection = FacingDirection.Up; break;
        }

        UpdateOccupiedCells();
    }

    // items cannot be scaled otherwise the forbidden sign is scaled as well
    public void ShowPlacementFeedback(bool canPlace)
    {
        // 1️⃣ Tint the item sprite
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.color = canPlace ? Color.white : new Color(1f, 0.4f, 0.4f, 1f);

        // 2️⃣ Find the ForbiddenSign
        var forbidden = transform.Find("ForbiddenSign");
        if (forbidden == null)
            return;

        var fr = forbidden.GetComponent<SpriteRenderer>();
        if (fr == null)
            return;

        // Toggle visibility but keep script active
        fr.enabled = !canPlace;

        if (!canPlace)
        {
            // --- Get center ---
            Vector3 targetCenter = transform.position;

            var parentSprite = GetComponentInChildren<SpriteRenderer>();
            var parentCollider = GetComponentInChildren<Collider2D>();

            if (parentSprite != null)
                targetCenter = parentSprite.bounds.center;
            else if (parentCollider != null)
                targetCenter = parentCollider.bounds.center;

            // --- Apply item-specific offset in local space ---
            Vector3 adjustedCenter = targetCenter + transform.rotation * forbiddenSignOffset;

            // --- Force world-space position ---
            forbidden.position = adjustedCenter;

            // --- Keep the sign completely independent of parent's transform ---
            forbidden.SetPositionAndRotation(adjustedCenter, Quaternion.identity);

            // --- Apply world scale that ignores parent scale entirely ---
            Vector3 worldScale = Vector3.one * forbiddenSignScale;
            forbidden.localScale = Vector3.one; // neutralize local skew first
            forbidden.localScale = new Vector3(
                worldScale.x / transform.lossyScale.x,
                worldScale.y / transform.lossyScale.y,
                worldScale.z / transform.lossyScale.z
            );
        }


    }



    public enum FacingDirection
    {
        Up,
        Down,
        Left,
        Right
    }



}
