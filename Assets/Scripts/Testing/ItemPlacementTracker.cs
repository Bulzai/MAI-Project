using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacementTracker : MonoBehaviour
{
    private bool isRandomPlacement = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        GridItem.OnGridItemPlaced += RecordItemPlacement;
    }

    private void OnDisable()
    {
        GridItem.OnGridItemPlaced -= RecordItemPlacement;
    }

    private void RecordItemPlacement (GameObject placedGridItem)
    {
        GridItem gridItem = placedGridItem.GetComponent<GridItem>();
        if (gridItem == null) return;

        string playerName = "COM";
        if (gridItem.player != null)
            playerName = gridItem.player.gameObject.name;


        string itemName = placedGridItem.name;
        itemName = System.Text.RegularExpressions.Regex.Replace(itemName, @"\(Clone\)", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        itemName = itemName.Trim();
        itemName = itemName.ToLower();

        Vector3 position = placedGridItem.transform.position;

        RoundController roundController = Object.FindAnyObjectByType<RoundController>();
        if (roundController == null) Debug.LogError("ItemPlacementTracker: RoundController not found in scene!");
        int round = roundController.currentRound;

        string data = "";

        if (!isRandomPlacement) data = $"{round+1},ItemPlacement,{itemName},{playerName}:{position}";
        else data = $"{round+1},ItemPlacement,{itemName},COM:{position}";

        TestingLogger.LogToCSV(data);

        Debug.Log($"item {itemName} placed at {position} in round {round + 1}");
    }
}
