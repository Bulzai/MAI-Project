using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomPlacementStrategy : MonoBehaviour
{
    public static RandomPlacementStrategy Instance { get; private set; }
    
    [SerializeField] private BoxCollider2D placeItemStateBounds;
    [SerializeField] private Transform itemsParent;
    
    private int attemptsPerSpawn = 100;
    private int itemsToPlacePerRound = 3;
    
    [Header("Grid + Tilemap")]
    private HashSet<Vector3Int> platformCells;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap platformMap;
    [SerializeField] private TileBase validPlatformTile;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        platformCells = new HashSet<Vector3Int>();
        foreach (var pos in platformMap.cellBounds.allPositionsWithin)
        {
            if (platformMap.GetTile(pos) == validPlatformTile)
                platformCells.Add(pos);
        }
        
    }
    

    public void StartRandomPlacement()
    {
        for (int i = 0; i < itemsToPlacePerRound; i++)
        {
            // do this on other thread
            GameObject gridItem = ItemPools.Instance.GetRandomItem();
            GameObject gridItemInstance = Instantiate(gridItem);
            TryPlaceItem(placeItemStateBounds.bounds, gridItemInstance);
        }
            PlacementFinished();
    }

    public void PlacementFinished()
    {
        GameEvents.ChangeState(GameState.MainGameState);
    }
    
        
    private bool CheckSpaceAndPlace(GameObject gridItemGO)
    {
        GridItem gridItemScript = gridItemGO.GetComponent<GridItem>();
        if (gridItemScript.CanBePlaced())       
        {
            gridItemScript.Place();
            gridItemGO.transform.SetParent(itemsParent);
            gridItemGO.layer = LayerMask.NameToLayer("Ground/Wall");
            return true;
        }

        return false;
    }
    
    
    private void TryPlaceItem(Bounds bounds, GameObject gridItem)
    {
        for (int attempt = 0; attempt < attemptsPerSpawn; attempt++)
        {
            int x = Random.Range((int)bounds.min.x, (int)bounds.max.x);
            int y = Random.Range( (int) bounds.min.y, (int) bounds.max.y);

            Vector3 candidateWorld = new Vector3(x, y, 0);
            Vector3Int candidateCell = grid.WorldToCell(candidateWorld);

            // skip platforms
            if (platformCells.Contains(candidateCell))
                continue;
            
            gridItem.transform.position = candidateWorld;
            Debug.Log("GridItem candidate position: " + candidateWorld);
            if (CheckSpaceAndPlace(gridItem))
                return;
            
        }
    }
    
}
