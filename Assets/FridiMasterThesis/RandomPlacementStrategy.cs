using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class RandomPlacementStrategy : MonoBehaviour
{
    public static RandomPlacementStrategy Instance { get; private set; }
    
    [SerializeField] private BoxCollider2D placeItemStateBounds;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private Animator transitionAnimator;

    private int attemptsPerSpawn = 2000;
    private int itemsToPlaceFirst3Rounds = 3;
    private int itemstoPlaceLast3Rounds = 2;
    [Header("Grid + Tilemap")]
    private HashSet<Vector3Int> platformCells;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap platformMap;
    [SerializeField] private TileBase validPlatformTile;
    private int allowedMaxLethalityScore = 99;
    private int[] maxTotalCellVisits = {1,1,1}; // {current round, previous round, two rounds ago}

    private int nrOfCellsAboveLethalityThreshold = 4;
    private int minNrOfCellVisitsThreshold = 25;
    private float minUtilizationThreshold = 0.33f;
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
    
    private IEnumerator PlacementRoutine(System.Func<IEnumerator> placementMethod, float minDuration)
    {
        Debug.Log("started placement routine with method: " + placementMethod.Method.Name);
        float startTime = Time.time;

        // Run the actual placement (random or adaptive)
        yield return StartCoroutine(placementMethod());

        float elapsed = Time.time - startTime;

        // Wait remaining time if needed
        if (elapsed < minDuration)
        {
            yield return new WaitForSeconds(minDuration - elapsed);
        }
        CircleLoadAnim.playAnim = false;
        Debug.Log("Circleloadanim playanim set to false");
        yield return new WaitForSeconds(0.6f);
        PlacementFinished();
    }

    public void StartRandomPlacementCoroutine()
    {
        StartCoroutine(PlacementRoutine(StartRandomPlacement, 3f));
    }
    
    public IEnumerator StartRandomPlacement()
    {

        int itemsToPlacePerRound = 0;
        if (PlayTestDataManager.Instance.roundIndex.Value < 4)
            itemsToPlacePerRound = itemsToPlaceFirst3Rounds;
        else
            itemsToPlacePerRound = itemstoPlaceLast3Rounds;
        
        
        for (int i = 0; i < itemsToPlacePerRound; i++)
        {
            // do this on other thread
            GameObject gridItem = ItemPools.Instance.GetRandomItem();
            GameObject gridItemInstance = Instantiate(gridItem);
            TryPlaceItem(placeItemStateBounds.bounds, gridItemInstance);
        }

        yield return null;
    }
    public void StartAdaptivePlacementCoroutine()
    {
        StartCoroutine(PlacementRoutine(StartAdaptivePlacement, 3f));
    }

    public void PlacementFinished()
    {
        //HIDE LOADING TEXT
        Debug.Log("Placement finished, starting countdown...");
        StartCoroutine(CountdownManager.Instance.StartCountdown());
        CircleLoadAnim.playAnim = true;

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

    public IEnumerator StartAdaptivePlacement()
    {
        int itemsToPlacePerRound = 0;
        if (PlayTestDataManager.Instance.roundIndex.Value < 4)
            itemsToPlacePerRound = itemsToPlaceFirst3Rounds;
        else
            itemsToPlacePerRound = itemstoPlaceLast3Rounds;


        for (int i = 0; i < itemsToPlacePerRound; i++)
        {
            Array values = Enum.GetValues(typeof(ItemType));
            int validCount = values.Length - 1; // exclude None at index 0
            int randomIndex = UnityEngine.Random.Range(0, validCount);
            ItemType randomItemType = (ItemType)values.GetValue(randomIndex + 1); // +1 to skip None
            
            GameObject instantiatedItem = Instantiate(ItemPools.Instance.GetItemFromKey(randomItemType));
            instantiatedItem.SetActive(true);
            if (!GetBestItemPlacement(placeItemStateBounds.bounds, instantiatedItem, randomItemType, out PlacementCandidate bestCandidate, out PlacementCandidate bestCandidateBothThresholdsMet))
                bestCandidate.itemType = ItemType.none;
            
            Destroy(instantiatedItem);

            if (bestCandidate.itemType == ItemType.none && bestCandidateBothThresholdsMet.itemType == ItemType.none)
            {
                // TODO MAKE RANDOM PLACEMENT
                Debug.Log("no item got a valid placement candidate");
            } else if (bestCandidateBothThresholdsMet.itemType != ItemType.none)
            {
                GameObject bestItem = Instantiate(ItemPools.Instance.GetItemFromKey(randomItemType));
                bestItem.transform.rotation = bestCandidateBothThresholdsMet.rotation;
                bestItem.transform.position = bestCandidateBothThresholdsMet.position;
                Physics2D.SyncTransforms();
                GridItem gridItemScript = bestItem.GetComponent<GridItem>();
                gridItemScript.UpdateHitCells();
                gridItemScript.Place();
                bestItem.transform.SetParent(itemsParent);
                bestItem.layer = LayerMask.NameToLayer("Ground/Wall");
            } else
            {
                Debug.Log("No candidate met both thresholds, placing item with best overall score: " + bestCandidate.itemType);
                GameObject bestItem = Instantiate(ItemPools.Instance.GetItemFromKey(randomItemType));
                bestItem.transform.rotation = bestCandidate.rotation;
                bestItem.transform.position = bestCandidate.position;
                Physics2D.SyncTransforms();
                GridItem gridItemScript = bestItem.GetComponent<GridItem>();
                gridItemScript.UpdateHitCells();
                gridItemScript.Place();
                bestItem.transform.SetParent(itemsParent);
                bestItem.layer = LayerMask.NameToLayer("Ground/Wall");
            }
            maxTotalCellVisits = new int[]{1,1,1};
        }
        yield return null;
    }

    //TODO this is always going to select the first item that is effect shooter because in round 0 there ar eno cellvisits
    private bool GetBestPlacementCandidate(PlacementCandidate[] placementCandidates, out PlacementCandidate bestCandidate, out PlacementCandidate bestCandidateBothThresholdsMet)
    {
        bool returnValue = false;
        bestCandidate = PlacementCandidate.Default();
        bestCandidateBothThresholdsMet = PlacementCandidate.Default();
        int bestVisits = -1;
        float bestLethality = 100f;
        float bestOverallScore = -1f;
        float currentOverallScore = -1f;
        float bestOverallScoreBothThresholdsMet = -1f;
        float currentOverallScoreBothThresholdsMet = -1f;
        float totalCellVisitsWeightCurrentRound = 0.2f;
        float totalCellVisitsWeightPreviousRound = 0.1f;
        float totalCellVisitsWeightTwoRoundsAgo = 0.05f;
        float averageLethalityWeight = 0.2f;
        float attackRangeUtilizationWeight = 0.45f;
        
        //Debug.Log("MaxTotalCellVisits: " + maxTotalCellVisits[0] + " " + maxTotalCellVisits[1] + " " + maxTotalCellVisits[2]);
        foreach(PlacementCandidate candidate in placementCandidates)
        {
            if (candidate.itemType == ItemType.none)
                continue;

            float normalizedTotalCellVisits = candidate.totalCellVisitsCurrentRound / maxTotalCellVisits[0] *
                                              totalCellVisitsWeightCurrentRound +
                                              candidate.totalCellVisitsPreviousRound / maxTotalCellVisits[1] *
                                              totalCellVisitsWeightPreviousRound +
                                              candidate.totalCellVisitsTwoRoundsAgo / maxTotalCellVisits[2] *
                                              totalCellVisitsWeightTwoRoundsAgo;
            currentOverallScore =  normalizedTotalCellVisits -
                                  candidate.averageLethalityScore * averageLethalityWeight +
                                  candidate.attackRangeUtilizationScore * attackRangeUtilizationWeight;

            int totalCellVisits = candidate.totalCellVisitsCurrentRound + candidate.totalCellVisitsPreviousRound + candidate.totalCellVisitsTwoRoundsAgo;
            if (currentOverallScore > bestOverallScore)
            {

                //Debug.Log("Evaluating candidate: " + candidate.itemType + " totalCellVisitsCurrentRound: " + candidate.totalCellVisitsCurrentRound +
                //          " totalCellVisitsPreviousRound: " + candidate.totalCellVisitsPreviousRound + " totalCellVisitsTwoRoundsAgo: " + candidate.totalCellVisitsTwoRoundsAgo +
                //          " averageLethalityScore: " + candidate.averageLethalityScore + " attackRangeUtilizationScore: " 
                //          + candidate.attackRangeUtilizationScore);
                //Debug.Log("currentOverallScore: " + currentOverallScore + " bestOverallScore: " + bestOverallScore);
                bestOverallScore = currentOverallScore;
                bestCandidate = candidate;
                returnValue = true;
            }

            
            if (totalCellVisits > minNrOfCellVisitsThreshold && candidate.attackRangeUtilizationScore > minUtilizationThreshold)
            {
                if (currentOverallScore > bestOverallScoreBothThresholdsMet)
                {
                    Debug.Log("totalCellVisits: " + totalCellVisits +
                              " attackRangeUtilizationScore: " + candidate.attackRangeUtilizationScore);
                    bestOverallScoreBothThresholdsMet = currentOverallScore;
                    bestCandidateBothThresholdsMet = candidate;
                    returnValue = true;
                }
            }
            
            
        }
        
        Debug.Log("Decided on: " + bestCandidate.itemType + " with overall score: " + bestOverallScore);
        return returnValue;
    }
    
    private bool GetBestItemPlacement(Bounds bounds, GameObject instantiatedItem, ItemType itemType, out PlacementCandidate bestCandidate, out PlacementCandidate bestCandidateBothThresholdsMet)
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
            
            instantiatedItem.transform.position = candidateWorld;
            if (CheckSpaceAndCalculateScores(instantiatedItem, itemType, out PlacementCandidate validCandidate))
            {
                candidates[attempt] = validCandidate;
            }
        }

        returnValue = GetBestPlacementCandidate(candidates, out bestCandidate, out bestCandidateBothThresholdsMet);
        return returnValue;

    }
    
    private bool CheckSpaceAndCalculateScores(GameObject gridItemGO, ItemType itemType, out PlacementCandidate placementCandidate)
    {
        placementCandidate = PlacementCandidate.Default();
        GridItem gridItemScript = gridItemGO.GetComponent<GridItem>();
        gridItemScript.RotateRandomly();
        Physics2D.SyncTransforms();
        //Debug.Log("griditemGO position: " + gridItemGO.transform.position);
        //Physics2D.simulationMode = SimulationMode2D.Script;
        //Physics2D.Simulate(0f);
        if (!gridItemScript.CanBePlaced())
        {
            placementCandidate.canBePlaced = false;
            return false;
        }
        gridItemScript.UpdateHitCells();
        gridItemScript.GetCellScores(out float averageLethalityScore, 
            out int[] totalCellVisits, out int maxLethatilityScore, 
            out float attackRangeUtilizationScore, out int ownLethalityScore, out int NrOfCellsAboveLethality);
        placementCandidate.averageLethalityScore = averageLethalityScore;
        placementCandidate.totalCellVisitsCurrentRound = totalCellVisits[0];
        placementCandidate.totalCellVisitsPreviousRound = totalCellVisits[1];
        placementCandidate.totalCellVisitsTwoRoundsAgo = totalCellVisits[2];
        placementCandidate.canBePlaced = true;
        if( totalCellVisits[0] > maxTotalCellVisits[0])
            maxTotalCellVisits[0] = totalCellVisits[0];
        if( totalCellVisits[1] > maxTotalCellVisits[1])
            maxTotalCellVisits[1] = totalCellVisits[1];
        if( totalCellVisits[2] > maxTotalCellVisits[2])
            maxTotalCellVisits[2] = totalCellVisits[2];
        
        placementCandidate.position = gridItemGO.transform.position;
        placementCandidate.maxLethalityScore = maxLethatilityScore;
        placementCandidate.rotation = gridItemGO.transform.rotation;
        placementCandidate.itemType = itemType;
        placementCandidate.attackRangeUtilizationScore = attackRangeUtilizationScore;
        gridItemScript.Reset();
        //GameObject bestItem = Instantiate(ItemPools.Instance.GetItemFromKey(itemType));
        //bestItem.transform.rotation = gridItemGO.transform.rotation;
        //bestItem.transform.position = gridItemGO.transform.position;
        //Physics2D.SyncTransforms();
        //GridItem gridItemScripta = bestItem.GetComponent<GridItem>();
        //Debug.Log("canbeplaced (should always be true: " + placementCandidate.canBePlaced + " position: " + placementCandidate.position);
        //Debug.Log("bestitem position: " + bestItem.transform.position);
        //Debug.Log("TESTING INSTANTIATED ITEM which can be placed, item can be placed: " + gridItemScripta.CanBePlaced());
        //Destroy(bestItem);
        
        //Debug.Log("Scores for item: " + itemType + " averageLethalityScore: " + averageLethalityScore + " totalCellVisitsCurrentRound: " + totalCellVisits[0] +
        //          " totalCellVisitsPreviousRound: " + totalCellVisits[1] + " totalCellVisitsTwoRoundsAgo: " + totalCellVisits[2] +
        //          " attackRangeUtilizationScore: " + attackRangeUtilizationScore);
        if(NrOfCellsAboveLethality > nrOfCellsAboveLethalityThreshold)
            return false;
        return true;
    }

}

public struct PlacementCandidate
{
    public static PlacementCandidate Default()
    {
        return new PlacementCandidate
        {
            itemType = ItemType.none,
            position = Vector3.zero,
            rotation = Quaternion.identity,
            averageLethalityScore = 0f,
            maxLethalityScore = 0,
            totalCellVisitsCurrentRound = 0,
            totalCellVisitsPreviousRound = 0,
            totalCellVisitsTwoRoundsAgo = 0,
            attackRangeUtilizationScore = 0f,
            canBePlaced = false
        };
    }
    
    public ItemType itemType;
    public Vector3 position;
    public Quaternion rotation;
    public float averageLethalityScore;
    public int maxLethalityScore;
    public int totalCellVisitsCurrentRound;
    public int totalCellVisitsPreviousRound;
    public int totalCellVisitsTwoRoundsAgo;
    public float attackRangeUtilizationScore;
    public bool canBePlaced;
    public bool minUtilizationThresholdMet;
    public bool minCellVisitsThresholdMet;
}



/*
    public void StartAdaptivePlacement()
    {

        for (int i = 0; i < itemsToPlacePerRound; i++)
        {

            PlacementCandidate[] placementCandidates = new PlacementCandidate[System.Enum.GetValues(typeof(ItemType)).Length];
            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                if(itemType == ItemType.none) continue;
                GameObject instantiatedItem = Instantiate(ItemPools.Instance.GetItemFromKey(itemType));
                instantiatedItem.SetActive(true);
                if (GetBestItemPlacement(placeItemStateBounds.bounds, instantiatedItem, itemType, out PlacementCandidate bestCandidate))
                    placementCandidates[(int)itemType] = bestCandidate;
                Destroy(instantiatedItem);
            }
            
            bool  foundItem = GetBestPlacementCandidate(placementCandidates, out PlacementCandidate bestPlacementCandidate);
            if (!foundItem)
            {
                // TODO MAKE RANDOM PLACEMENT
                Debug.Log("no item got a valid placement candidate");
            }
            ItemType bestItemType = bestPlacementCandidate.itemType;
            Debug.Log("best item type: " + bestItemType);
            if (bestItemType != ItemType.none)
            {
                GameObject bestItem = Instantiate(ItemPools.Instance.GetItemFromKey(bestItemType));
                bestItem.transform.rotation = bestPlacementCandidate.rotation;
                bestItem.transform.position = bestPlacementCandidate.position;
                Physics2D.SyncTransforms();
                GridItem gridItemScript = bestItem.GetComponent<GridItem>();
                //Debug.Log("INSTANTIATING ITEM, item can be placed: " + gridItemScript.CanBePlaced());
                //Debug.Log("placementcandidate.canbeplaced: " + bestPlacementCandidate.canBePlaced);
                //GameObject instantiatedItem = ItemPools.Instance.GetInstantiatedItem(bestItemType);
                //instantiatedItem.SetActive(true);
                //instantiatedItem.transform.rotation = bestPlacementCandidate.rotation;
                //instantiatedItem.transform.position = bestPlacementCandidate.position;
                //Physics2D.SyncTransforms();
                //Debug.Log("pooled item can be placed: " + instantiatedItem.GetComponent<GridItem>().CanBePlaced());
                gridItemScript.UpdateHitCells();
                gridItemScript.Place();
                //instantiatedItem.GetComponent<GridItem>().Place();
                bestItem.transform.SetParent(itemsParent);
                bestItem.layer = LayerMask.NameToLayer("Ground/Wall");
            }
            maxTotalCellVisits = new int[]{1,1,1};
        }
        PlacementFinished();

    }
*/