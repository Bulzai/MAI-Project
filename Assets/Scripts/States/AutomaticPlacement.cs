using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class AutomaticPlacement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(RunAutoPlacement());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Debug.Log("ENTER AUTOMATIC PLACEMNT");

        StopAllCoroutines();
        StartCoroutine(RunAutoPlacement());
    }

    private void OnDisable()
    {
        Debug.Log("EXIT AUTOMATIC PLACEMNT");
    }

    private IEnumerator RunAutoPlacement()
    {
        GridPlacementSystem.Instance.ResetMainTileMap();

        // wait for managers to initialize
        yield return new WaitForEndOfFrame();

        if (ItemDisplay.Instance == null)
        {
            Debug.LogError("Manager not found!");
            yield break;
        }

        int playerCount = PlayerManager.Instance != null ? PlayerManager.Instance.playerCount : 1;

        // max = playercount, min = playercount - 1
        int itemsToPlace = Random.Range(playerCount, playerCount + 1);

        // safety check: do not place 0 items if only 1 player is active
        if (itemsToPlace < 1) itemsToPlace = 1;

        Debug.Log($"auto-placing {itemsToPlace} items for {playerCount} players.");

        // get item pool and valid cells
        List<GameObject> pool = ItemDisplay.Instance.getGeneralItemPool();
        List<Vector3Int> validCells = GetAllValidCells();

        bool placed = false;
        int attempts = 0;

        // 3. Placement Loop
        for (int i = 0; i < itemsToPlace; i++)
        {
            if (validCells.Count == 0 || pool.Count == 0) break;

            // get random prefab from pool
            GameObject prefab = pool[Random.Range(0, pool.Count)];
            GameObject spawnedItem = Instantiate(prefab, PlayerManager.Instance.itemContainer);
            GridItem gridItem = spawnedItem.GetComponent<GridItem>();

            attempts = 0;
            placed = false;

            while(!placed && attempts < 15)
            {
                attempts++;

                // convert cell position to world position and item offset
                Vector3Int randomCell = validCells[Random.Range(0, validCells.Count)];
                Vector3 worldPosition = GridPlacementSystem.Instance.gridLayout.CellToWorld(randomCell)
                                        + (GridPlacementSystem.Instance.gridLayout.cellSize / 2f);

                spawnedItem.transform.position = worldPosition + gridItem.placementOffset;

                if (gridItem.CanBePlaced())
                {
                    gridItem.Place();

                    // remove cell from local available list
                    foreach (var cell in GetOccupiedCellsForItem(gridItem))
                    {
                        // now taken
                        GridPlacementSystem.Instance.TakeCell(cell);
                        // remove from our local list of available spots for the next item in the loop
                        validCells.Remove(cell);
                    }

                    placed = true;
                }

            }

            // placement did not work so delete item
            if(!placed) Destroy(spawnedItem);
        }
        GameEvents.ChangeState(GameState.PlaceItemState);
    }

    private List<Vector3Int> GetAllValidCells()
    {
        List<Vector3Int> whiteTiles = new List<Vector3Int>();
        Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;

        // get bounds
        BoundsInt bounds = mainMap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (GridPlacementSystem.Instance.CanTakeCell(pos))
            {
                whiteTiles.Add(pos);
            }
        }
        return whiteTiles;
    }

    // get which tiles are occupied
    private List<Vector3Int> GetOccupiedCellsForItem(GridItem item)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        // get collider to see how much space it would take up
        Collider2D collider = item.GetComponent<Collider2D>();

        if (collider == null)
        {
            // default to 1x1 if no collider is found
            cells.Add(GridPlacementSystem.Instance.gridLayout.WorldToCell(item.transform.position - item.placementOffset));
            return cells;
        }

        // get bounds 
        Bounds area = collider.bounds;

        // convert min max corners of world bounds to tilemap cell coordinates
        Vector3Int minCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.min);
        Vector3Int maxCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                cells.Add(new Vector3Int(x, y, 0));
            }
        }
        return cells;
    }
}
