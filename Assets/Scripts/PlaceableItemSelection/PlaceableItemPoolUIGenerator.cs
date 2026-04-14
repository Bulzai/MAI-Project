using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableItemPoolUIGenerator : MonoBehaviour
{
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private Transform grid;
    [SerializeField] public Transform gifGrid;
    [SerializeField] private PlaceableItemSelection placeableItemSelection;
    [SerializeField] private UnityEngine.UI.Button continueButton;

    private Toggle firstToggle;

    public Selectable GetDefaultSelectable()
    {
        return firstToggle;
    }


    private IEnumerator WaitAndGenerate()
    {
        yield return new WaitForEndOfFrame();

        if (PlaceableItemSelection.Instance != null)
        {
            GenerateToggles();
        }
        else
        {
            Debug.LogError("No manager in scene!");
        }
    }

    private void GenerateToggles()
    {
        firstToggle = null;
        GameObject lastToggle = null;

        // check if the manager exists yet
        if (PlaceableItemSelection.Instance == null)
        {
            Debug.LogWarning("Manager not found!");
            return;
        }

        Debug.Log("GENERATOR: Found " + PlaceableItemSelection.Instance.getGeneralItemPool().Count + " items.");

        // clear existing toggles - stop double spawning
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        foreach (GameObject item in PlaceableItemSelection.Instance.getGeneralItemPool())
        {
            // create toggle ui
            GameObject newToggleGO = Instantiate(togglePrefab, grid);

            // get gif go on item prefab
            Transform itemGif = item.transform.Find("Gif");

            if (itemGif != null && gifGrid != null)
            {
                // instantiante into gif grid
                GameObject spawnedGif = Instantiate(itemGif.gameObject, gifGrid);

                // reset position
                //spawnedGif.transform.localPosition = Vector3.zero;

                spawnedGif.transform.localScale = Vector3.one * 0.6f;
                spawnedGif.transform.localPosition = Vector3.zero;
                spawnedGif.transform.localRotation = Quaternion.identity;

                //spawnedGif.transform.localScale = Vector3.one * 1f;
                
                // 6. Tell the Hover script about this new gif
                HoverOrSelectScale hoverLogic = newToggleGO.GetComponent<HoverOrSelectScale>();
                if (hoverLogic != null)
                {
                    hoverLogic.gifObject = spawnedGif;

                    // hide the GIF by default if you only want it to show on hover
                    spawnedGif.SetActive(false);
                }

            }
            else Debug.Log("Either gif or grid is null");

                // get script
                PlaceableItemToggleUI toggleScript = newToggleGO.GetComponent<PlaceableItemToggleUI>();

            if (toggleScript != null)
            {
                toggleScript.SetupToggle(item);

                // track for navigation
                if (firstToggle == null) firstToggle = newToggleGO.GetComponentInChildren<Toggle>();
                lastToggle = newToggleGO;
            }
            else
            {
                continue; // Skip to the next item instead of crashing the whole loop
            }

            // get data from item
            SpriteRenderer sprite = item.GetComponentInChildren<SpriteRenderer>();

            if (sprite != null)
            {
                Image uiImage = newToggleGO.transform.Find("Image").GetComponent<Image>();

                if (uiImage != null)
                {
                    uiImage.sprite = sprite.sprite;

                    // prevents stretching by forcing ui element 
                    // to respect the original sprite's width/height ratio.
                    uiImage.preserveAspect = true;
                }
            }

        }

        if (continueButton != null && lastToggle != null)
        {
            var nav = continueButton.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;

            // link play button up to last item
            nav.selectOnUp = lastToggle.GetComponentInChildren<UnityEngine.UI.Toggle>();
            continueButton.navigation = nav;
        }

        // force update in case toggles arent found
        Canvas.ForceUpdateCanvases();
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndGenerate());
    }
}
