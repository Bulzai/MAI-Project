using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridItem : MonoBehaviour
{


    public bool Placed { get; private set; }
    public BoundsInt area;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Grid Methods

    public bool CanBePlaced()
    {
        Vector3Int positionInt = GridPlacementSystem.gridPlacementSystem.gridLayout.LocalToCell(transform.position) - new Vector3Int(1, 1, 0); // adjust to center the item correctly;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if (GridPlacementSystem.gridPlacementSystem.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }

    public void Place()
    {
        Vector3Int positionInt = GridPlacementSystem.gridPlacementSystem.gridLayout.LocalToCell(transform.position) - new Vector3Int(1, 1, 0); // adjust to center the item correctly;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        GridPlacementSystem.gridPlacementSystem.TakeArea(areaTemp);
        GameEvents.ToggleGrid();
    }


    #endregion Grid Methods



}
