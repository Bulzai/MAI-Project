using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{

    [Header("Attachment Settings")]
    public bool requiresSupport = false;               // true for icy/sticky surfaces
    public bool SupportedItemCanBePlaced;
    public bool Placed { get; private set; }
    public BoundsInt area;
    // Start is called before the first frame update
    private Vector3Int _originalSize;      // to remember your neutral W×H
    public Vector3Int originalPositionHorizontal;
    public Vector3Int originalPositionVertical;

    public Vector3Int adjustPosition;
    private bool ItemIsHorizontal = true;



    void Awake()
    {
        _originalSize = area.size;
        adjustPosition = originalPositionHorizontal;
    }

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
    }

    // rotating the item
    public void RotateClockwise()
    {
        // rotate sprite
        transform.Rotate(0f, 0f, -90f);
        //UpdateAreaFromRotation();
        var oldSize = area.size;
        var newSize = new Vector3Int(oldSize.y, oldSize.x, oldSize.z);
        area.size = newSize;
        //UpdatePositionFromRotation();
        ItemIsHorizontal = !ItemIsHorizontal;
        if (ItemIsHorizontal == true)
            adjustPosition = originalPositionHorizontal;
        else if (ItemIsHorizontal == false)
            adjustPosition = originalPositionVertical;
    }

    public void RotateCounterclockwise()
    {
        transform.Rotate(0f, 0f, 90f);
        //UpdateAreaFromRotation();
        var oldSize = area.size;
        var newSize = new Vector3Int(oldSize.y, oldSize.x, oldSize.z);
        area.size = newSize;
        ItemIsHorizontal = !ItemIsHorizontal;
        if (ItemIsHorizontal == true)
            adjustPosition = originalPositionHorizontal;
        else if (ItemIsHorizontal == false)
            adjustPosition = originalPositionVertical;

        //UpdatePositionFromRotation();
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


}
