using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItems : MonoBehaviour
{
    public int numberToSpawn;
    public List<GameObject> itemPool;

    public GameObject spawnBox;

    private List<GameObject> itemsInBox = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        SpawnObjects();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnObjects()
    {
        int randomItem = 0;
        GameObject toSpawn, newItem;
        MeshCollider collider = spawnBox.GetComponent<MeshCollider>();

        float screenX, screenY;
        Vector2 position;
        for(int i = 0; i < numberToSpawn; i++)
        {
            randomItem = Random.Range(0, itemPool.Count);
            toSpawn = itemPool[randomItem];

            // check for spawn rate probability before spawning
            Itembox_Selectable toSpawnGo = toSpawn.GetComponent<Itembox_Selectable>();
            float probability = toSpawnGo.GetSpawnRate();
            float check = Random.Range(0, 100);
            if(check <= probability)
            {
                screenX = Random.Range(collider.bounds.min.x + 1, collider.bounds.max.x - 1);
                screenY = Random.Range(collider.bounds.min.y + 1, collider.bounds.max.y - 1);
                position = new Vector2(screenX, screenY);

                newItem = Instantiate(toSpawn, position, toSpawn.transform.rotation);

                itemsInBox.Add(newItem);

                // Optional: Remove from list
                //itemPool.Remove(toSpawn);
            }
            else
            {
                Debug.Log("under probability: check: "+ check +" probability: " + probability);
                i--;
            }

            
        }
    }
}
