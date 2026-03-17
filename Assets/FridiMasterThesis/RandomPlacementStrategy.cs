using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    private int allowedMaxLethalityScore = 99;
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
        // SHOW LOADING TEXT

        StartAdaptivePlacement();
        return;
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
        //HIDE LOADING TEXT
        Debug.Log("Placement finished, starting countdown...");
        StartCoroutine(CountdownManager.Instance.StartCountdown());
    }
    
        
    private bool CheckSpaceAndPlace(GameObject gridItemGO)
    {
        GridItem gridItemScript = gridItemGO.GetComponent<GridItem>();
        gridItemScript.RotateRandomly();
        Physics2D.SyncTransforms();
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

            if (CheckSpaceAndPlace(gridItem))
                return;
            
        }
    }
    
    public void ClearItemParent()
    {
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void StartAdaptivePlacement()
    {

        for (int i = 0; i < itemsToPlacePerRound; i++)
        {

            PlacementCandidate[] placementCandidates = new PlacementCandidate[System.Enum.GetValues(typeof(ItemType)).Length];
            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                Debug.Log("itemType: " + itemType);
                if(itemType == ItemType.none) continue;
                GameObject instantiatedItem = ItemPools.Instance.GetInstantiatedItem(itemType);
                instantiatedItem.SetActive(true);
                if (GetBestItemPlacement(placeItemStateBounds.bounds, instantiatedItem, itemType, out PlacementCandidate bestCandidate))
                    placementCandidates[(int)itemType] = bestCandidate;
                instantiatedItem.SetActive(false);
            }
            
            bool  smthWentWrong = GetBestPlacementCandidate(placementCandidates, out PlacementCandidate bestPlacementCandidate);
            if (smthWentWrong)
            {
                Debug.Log("no item got a valid placement candidate");
            }
            ItemType bestItemType = bestPlacementCandidate.itemType;
            if (bestItemType != ItemType.none)
            {
                GameObject bestItem = Instantiate(ItemPools.Instance.GetItemFromKey(bestItemType));
                bestItem.transform.rotation = bestPlacementCandidate.rotation;
                Debug.Log("Instantiated item: " + bestItem.name + " at position: " + bestPlacementCandidate.position);
                bestItem.transform.position = bestPlacementCandidate.position;
                Physics2D.SyncTransforms();
                GridItem gridItemScript = bestItem.GetComponent<GridItem>();
                gridItemScript.UpdateHitCells();
                gridItemScript.Place();
                bestItem.transform.SetParent(itemsParent);
                bestItem.layer = LayerMask.NameToLayer("Ground/Wall");
            }
        }
        PlacementFinished();

    }

    //TODO this is always going to select the first item that is effect shooter because in round 0 there ar eno cellvisits
    private bool GetBestPlacementCandidate(PlacementCandidate[] placementCandidates, out PlacementCandidate bestCandidate)
    {
        bool returnValue = false;
        bestCandidate = new PlacementCandidate();

        int bestVisits = -1;
        float bestLethality = 100f;
        float bestOverallScore = -1f;
        float currentOverallScore = -1f;
        float totalCellVisitsWeight = 0.45f;
        float averageLethalityWeight = 0.1f;
        float attackRangeUtilizationWeight = 0.45f;
        
        foreach(PlacementCandidate candidate in placementCandidates)
        {
            currentOverallScore = candidate.totalCellVisits * totalCellVisitsWeight -
                               candidate.averageLethalityScore * averageLethalityWeight +
                               candidate.attackRangeUtilizationScore * attackRangeUtilizationWeight;
            
            if (currentOverallScore > bestOverallScore)
            {
                Debug.Log("Candidate: " + candidate.itemType + " totalCellVisits: " + candidate.totalCellVisits + 
                          " averageLethalityScore: " + candidate.averageLethalityScore + " attackRangeUtilizationScore: " 
                          + candidate.attackRangeUtilizationScore + " overallScore: " + currentOverallScore);
                bestOverallScore = currentOverallScore;
                bestCandidate = candidate;
                returnValue = true;
            }
        }
        return returnValue;
    }
    
    private bool GetBestItemPlacement(Bounds bounds, GameObject gridItem, ItemType itemType, out PlacementCandidate bestCandidate)
    {
        bool returnValue = false;
        
        PlacementCandidate[] candidates = new PlacementCandidate[attemptsPerSpawn];
        for (int attempt = 0; attempt < attemptsPerSpawn; attempt++)
        {
            int x = Random.Range((int)bounds.min.x, (int)bounds.max.x);
            int y = Random.Range( (int) bounds.min.y, (int) bounds.max.y);

            Vector3 candidateWorld = new Vector3(x, y, 0);
            Vector3Int candidateCell = grid.WorldToCell(candidateWorld);

            // skip red tiles
            if (platformCells.Contains(candidateCell))
                continue;
            
            gridItem.transform.position = candidateWorld;

            if (CheckSpaceAndCalculateScores(gridItem, itemType, out PlacementCandidate validCandidate))
            {
                Debug.Log("checkspaceandcalculatescores was true");
                candidates[attempt] = validCandidate;
            }
        }
        
        returnValue = GetBestPlacementCandidate(candidates, out bestCandidate);
        return returnValue;

    }
    
    private bool CheckSpaceAndCalculateScores(GameObject gridItemGO, ItemType itemType, out PlacementCandidate placementCandidate)
    {
        placementCandidate = new PlacementCandidate();
        GridItem gridItemScript = gridItemGO.GetComponent<GridItem>();
        int rotation = gridItemScript.RotateRandomly();
        Physics2D.SyncTransforms();
        if (!gridItemScript.CanBePlaced())
        {
            return false;
        }
        gridItemScript.UpdateHitCells();
        gridItemScript.GetCellScores(out float averageLethalityScore, out int totalCellVisits, out int maxLethatilityScore, out float attackRangeUtilizationScore);
        placementCandidate.averageLethalityScore = averageLethalityScore;
        placementCandidate.totalCellVisits = totalCellVisits;
        placementCandidate.position = gridItemGO.transform.position;
        placementCandidate.maxLethalityScore = maxLethatilityScore;
        placementCandidate.rotation = gridItemGO.transform.rotation;
        placementCandidate.itemType = itemType;
        placementCandidate.attackRangeUtilizationScore = attackRangeUtilizationScore;
        gridItemScript.Reset();
        if(placementCandidate.maxLethalityScore > allowedMaxLethalityScore)
            return false;
        return true;
    }

}

public struct PlacementCandidate
{
    public ItemType itemType;
    public Vector3 position;
    public Quaternion rotation;
    public float averageLethalityScore;
    public int maxLethalityScore;
    public int totalCellVisits;
    public float attackRangeUtilizationScore;
}