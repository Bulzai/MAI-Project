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

            Dictionary<ItemType, PlacementCandidate> placementCandidates = new Dictionary<ItemType, PlacementCandidate>();
            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                Debug.Log("itemType: " + itemType);
                if(itemType == ItemType.none) continue;
                GameObject instantiatedItem = ItemPools.Instance.GetInstantiatedItem(itemType);
                instantiatedItem.SetActive(true);
                if (GetBestItemPlacement(placeItemStateBounds.bounds, instantiatedItem, out PlacementCandidate bestCandidate))
                    placementCandidates.Add(itemType, bestCandidate);
                instantiatedItem.SetActive(false);
            }
            
            // choose random PlacementCandidate from the Dictionary and place the corresponding item there
            ItemType bestItemType = ItemType.none;
            int bestVisits = -1;
            float bestLethality = 100f;
            
            Debug.Log("placement candidates: ");
            
            //TODO this is always going to select the first item that is effect shooter because in round 0 there ar eno cellvisits
            foreach(var kvp in placementCandidates)
            {
                Debug.Log("kvp: " + kvp.Key + " visits: " + kvp.Value.totalCellVisits + " lethality: " + kvp.Value.averageLethalityScore);
                ItemType candidateItemType = kvp.Key;
                PlacementCandidate candidate = kvp.Value;
                    
                if (candidate.totalCellVisits > bestVisits || 
                    (candidate.totalCellVisits == bestVisits && candidate.averageLethalityScore < bestLethality))
                {
                    bestItemType = candidateItemType;
                    bestVisits = candidate.totalCellVisits;
                    bestLethality = candidate.averageLethalityScore;
                }
            }

            if (bestItemType != ItemType.none)
            {
                GameObject bestItem = Instantiate(ItemPools.Instance.GetItemFromKey(bestItemType));
                bestItem.transform.rotation = placementCandidates[bestItemType].rotation;
                Debug.Log("Instantiated item: " + bestItem.name + " at position: " + placementCandidates[bestItemType].position);
                bestItem.transform.position = placementCandidates[bestItemType].position;

                GridItem gridItemScript = bestItem.GetComponent<GridItem>();
                gridItemScript.UpdateHitCells();
                gridItemScript.Place();
                bestItem.transform.SetParent(itemsParent);
                bestItem.layer = LayerMask.NameToLayer("Ground/Wall");
            }
        }
        PlacementFinished();

    }

    private bool GetBestItemPlacement(Bounds bounds, GameObject gridItem, out PlacementCandidate bestCandidate)
    {
        bool returnValue = false;
        
        PlacementCandidate[] candidates = new PlacementCandidate[attemptsPerSpawn];
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

            if (CheckSpaceAndCalculateScores(gridItem, out PlacementCandidate validCandidate))
            {
                Debug.Log("checkspaceandcalculatescores was true");
                candidates[attempt] = validCandidate;
                returnValue = true;
            }
        }

        // get item with highest total cell visits, if tie then lowest average lethality score, if tie then random of the candidates
        bestCandidate = new PlacementCandidate();
        int bestVisits = -1;
        float bestLethality = 100f;
        
        foreach(PlacementCandidate candidate in candidates)
        {
            if (candidate.totalCellVisits > bestVisits || 
                (candidate.totalCellVisits == bestVisits && candidate.averageLethalityScore < bestLethality))
            {
                bestCandidate = candidate;
                bestVisits = candidate.totalCellVisits;
                bestLethality = candidate.averageLethalityScore;
            }
        }

        return returnValue;

    }
    
    private bool CheckSpaceAndCalculateScores(GameObject gridItemGO, out PlacementCandidate placementCandidate)
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
        gridItemScript.GetCellScores(out float averageLethalityScore, out int totalCellVisits, out int maxLethatilityScore);
        placementCandidate.averageLethalityScore = averageLethalityScore;
        placementCandidate.totalCellVisits = totalCellVisits;
        placementCandidate.position = gridItemGO.transform.position;
        placementCandidate.maxLethalityScore = maxLethatilityScore;
        placementCandidate.rotation = gridItemGO.transform.rotation;
        gridItemScript.Reset();
        if(placementCandidate.maxLethalityScore > allowedMaxLethalityScore)
            return false;
        return true;
    }

}

public struct PlacementCandidate
{
    public Vector3 position;
    public Quaternion rotation;
    public float averageLethalityScore;
    public int maxLethalityScore;
    public int totalCellVisits;
}