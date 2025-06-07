using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


// also for cursor and movement improvements https://www.youtube.com/watch?v=n5EN2J2FxOQ
// see https://www.youtube.com/watch?v=gFpmJtO0NT4 for an overview
public class GridPlacementSystem : MonoBehaviour
{

    public static GridPlacementSystem gridPlacementSystem;

    [SerializeField] private Camera placementCamera;
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;

    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    private GridItem gridItem;
    private Vector3 previousPosition;
    private BoundsInt previousArea;

    #region Unity Methods

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }


    private void Awake()
    {
        gridPlacementSystem = this;
    }

    private void Start()
    {
        string tilePath = "Tiles/";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "TilesGrid_0"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "TilesGrid_2"));
        tileBases.Add(TileType.Blue, Resources.Load<TileBase>(tilePath + "TilesGrid_4"));
    }

    /*
    private void Update()
    {
        if (!gridItem)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }

            if (!gridItem.Placed)
            {
                Vector2 touchPos = (Vector2)placementCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

                if (previousPosition != (Vector3)cellPos)
                {
                    gridItem.transform.localPosition = gridLayout.CellToLocalInterpolated((Vector3)cellPos)
                        + new Vector3(.5f, .5f, 0f);
                    previousPosition = (Vector3)cellPos;
                    FollowItem();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (gridItem.CanBePlaced())
                {
                    gridItem.Place();
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right Click
            {
                ClearArea();
                Destroy(gridItem.gameObject);
            }
    }
    */

    private void Update()
    {
        if (!gridItem || gridItem.Placed)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 screenPos = Input.mousePosition;
        screenPos.z = -placementCamera.transform.position.z;  // e.g. 10 if camera.z == -10

        Vector3 world3 = placementCamera.ScreenToWorldPoint(screenPos);
        Vector2 world2 = world3;              // drop the Z
        Vector3Int cellPos = gridLayout.LocalToCell(world3);

        if (previousPosition != (Vector3)cellPos)
        {
            gridItem.transform.localPosition = gridLayout.CellToLocalInterpolated((Vector3)cellPos)
                + new Vector3(.5f, .5f, 0f);
            previousPosition = (Vector3)cellPos;
            FollowItem();
        }

        if (Input.GetMouseButtonDown(0)) // LEFT CLICK
        {
            if (gridItem.CanBePlaced())
            {
                gridItem.Place();
            }
        }
        else if (Input.GetMouseButtonDown(1)) // RIGHT CLICK
        {
            ClearArea();
            Destroy(gridItem.gameObject);
            GameEvents.ToggleGrid();
        }
    }


    #endregion


    #region Tilemap Placement

    #endregion


    #region Item Placement

    public void InitializeWithItem(GameObject item)
    {
        gridItem = Instantiate(item, Vector3.zero, Quaternion.identity).GetComponent<GridItem>();
        FollowItem();
        GameEvents.ItemSelectionPanelOpened();
    }
    private void ClearArea()
    {
        TileBase[] toClear = new TileBase[previousArea.size.x * previousArea.size.y * previousArea.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTilemap.SetTilesBlock(previousArea, toClear);
    }

    private void FollowItem()
    {
        ClearArea();

        gridItem.area.position = gridLayout.WorldToCell(gridItem.gameObject.transform.position) - new Vector3Int(1,1,0); // adjust to center the item correctly
        BoundsInt buildingArea = gridItem.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.White])
            {
                tileArray[i] = tileBases[TileType.Blue];
            }
            else
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }

        TempTilemap.SetTilesBlock(buildingArea, tileArray);
        previousArea = buildingArea;
    }

    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        foreach (var b in baseArray)
        {
            if(b != tileBases[TileType.White])
            {
                Debug.Log("Cannot place here");
                return false;
            }
        }

        return true;
    }

    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, TempTilemap);
        SetTilesBlock(area, TileType.Blue, MainTilemap);
    }



    #endregion

}


public enum TileType
{

    Empty,
    White,
    Blue,
    Red
}
