using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnItems : MonoBehaviour
{
    // countdown
    // -- start
    public float startTimeRemaining = 5f;
    private bool startTimeIsRunning = false;
    public TMP_Text startTimer;
    // -- item box
    public float itemTimeRemaining = 10f;
    private bool itemTimerIsRunning = false;
    public TMP_Text itemTimerText;

    // item spawner details
    public int numberToSpawn;
    public List<GameObject> itemPool;
    private List<GameObject> itemsInBox = new List<GameObject>();

    // box to spawn in
    public List<GameObject> spawnBoxes = new List<GameObject>();
    public GameObject itemBox;

    // Start is called before the first frame update
    void Start()
    {
        startTimeIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(startTimeIsRunning)
        {
            if(startTimeRemaining > 1)
            {
                startTimeRemaining -= Time.deltaTime;
                startTimer.text = Mathf.FloorToInt(startTimeRemaining % 60).ToString();
            }
            else
            {
                OpenItemBox();
            }
        }
        if(itemTimerIsRunning)
        {
            if (itemTimeRemaining > 1)
            {
                itemTimeRemaining -= Time.deltaTime;
                itemTimerText.text = "Timer: " + Mathf.FloorToInt(itemTimeRemaining % 60).ToString();
            }
            else
            {
                CloseItemBox();
            }
        }
    }

    public void SpawnObjects()
    {
        int randomItem = 0, randomTile = 0;
        GameObject toSpawn, newItem;
        // copy spawn box for this run
        List<GameObject> copy_spawnBoxes = spawnBoxes;

        // position
        float screenX, screenY;
        Vector2 position;

        // run to spawn items
        for(int i = 0; i < numberToSpawn; i++)
        {
            // random int to choose which item to spawn
            randomItem = Random.Range(0, itemPool.Count);
            toSpawn = itemPool[randomItem];

            // get spawnrate
            Itembox_Selectable toSpawnGo = toSpawn.GetComponent<Itembox_Selectable>();
            float probability = toSpawnGo.GetSpawnRate();

            // check if its probability
            float check = Random.Range(0, 100);
            if (check <= probability)
            {
                // choose random tile in box
                randomTile = Random.Range(0, copy_spawnBoxes.Count);
                MeshCollider collider = copy_spawnBoxes[randomTile].GetComponent<MeshCollider>();
                screenX = Random.Range(collider.bounds.min.x + 1, collider.bounds.max.x - 1);
                screenY = Random.Range(collider.bounds.min.y + 1, collider.bounds.max.y - 1);
                position = new Vector2(screenX, screenY);

                // instatate new item in chosen random tile
                newItem = Instantiate(toSpawn, position, toSpawn.transform.rotation);
                // destroy after x seconds
                Destroy(newItem, itemTimeRemaining-1);

                // add new item to have outline
                itemsInBox.Add(newItem);

                // Remove from list so no other item can spawn in this tile
                copy_spawnBoxes.RemoveAt(randomTile);
            }
            else
            {
                Debug.Log("under probability: check: "+ check +" probability: " + probability);
                i--;
            }

            
        }
    }

    public void OpenItemBox()
    {
        // turn off all start timer things
        startTimeIsRunning = false;
        startTimer.enabled = false;

        // turn on all item box things
        itemBox.SetActive(true);
        itemTimerIsRunning = true;
        itemTimerText.enabled = true;
        SpawnObjects();
    }

    public void CloseItemBox()
    {
        Debug.Log("Time has run out!");
        itemTimeRemaining = 0;
        itemTimerText.text = "Timer: " + Mathf.FloorToInt(itemTimeRemaining % 60).ToString();
        itemTimerIsRunning = false;

        // turn of item ui
        itemBox.SetActive(false);
        itemTimerText.enabled = false;
    }
}
