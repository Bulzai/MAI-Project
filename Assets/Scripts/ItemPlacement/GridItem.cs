using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{

    [Header("Attachment Settings")]
    public bool requiresSupport = false;               // true for icy/sticky surfaces
    public Transform ifAttachableAttachHere = null;
    public bool SupportedItemCanBePlaced { get; set; }

public bool Placed { get; private set; }
    public BoundsInt area;
    // Start is called before the first frame update
    //private Vector3Int _originalSize;      // to remember your neutral W×H
    public Vector3Int originalPositionHorizontal;
    public Vector3Int originalPositionVertical;

    public Vector3Int adjustPosition;
    private bool ItemIsHorizontal = true;

    public FacingDirection currentFacingDirection;
    //public int length = 1; // 1 or 2
    public Transform RayCastLocation;
    private List<Vector3Int> occupiedCells = new List<Vector3Int>();


    void Awake()
    {
        //_originalSize = area.size;
        adjustPosition = originalPositionHorizontal;

        // Try to find an existing child called "RayCastLocation"
        RayCastLocation = transform.Find("RayCastLocation");

        // If none found, create it automatically
        if (RayCastLocation == null)
        {
            GameObject rayObj = new GameObject("RayCastLocation");
            RayCastLocation = rayObj.transform;
            RayCastLocation.SetParent(transform);

            Debug.Log($"[GridItem] Created missing RayCastLocation for {name} at {RayCastLocation.localPosition}");
        }
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

    public Vector3Int getAdjustPosition()
    {
        return adjustPosition;
    }

    public void ShowPlacementFeedback(bool canPlace)
    {
        // 1) Tint the item sprite
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = canPlace ? Color.white : new Color(1f, 0.4f, 0.4f, 1f); // tint red
        }

        // 2) Show or hide the forbidden sign overlay
        var forbidden = transform.Find("ForbiddenSign");
        if (forbidden != null)
        {
            forbidden.gameObject.SetActive(!canPlace);
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
