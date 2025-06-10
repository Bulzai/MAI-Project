using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{


    public bool Placed { get; private set; }
    public BoundsInt area;
    // Start is called before the first frame update

    public bool CanBePlaced()
    {
        Vector3Int positionInt = GridPlacementSystem.Instance.gridLayout.LocalToCell(transform.position) - new Vector3Int(1, 1, 0); // adjust to center the item correctly;
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
        Vector3Int positionInt = GridPlacementSystem.Instance.gridLayout.LocalToCell(transform.position) - new Vector3Int(1, 1, 0); // adjust to center the item correctly;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        GridPlacementSystem.Instance.TakeArea(areaTemp);

    }


}
