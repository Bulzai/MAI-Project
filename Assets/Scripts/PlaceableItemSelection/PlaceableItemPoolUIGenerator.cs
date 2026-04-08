using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class PlaceableItemPoolUIGenerator : MonoBehaviour
{
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private Transform grid;
    [SerializeField] private PlaceableItemSelection placeableItemSelection;
    [SerializeField] private UnityEngine.UI.Button continueButton;

    private IEnumerator WaitAndGenerate()
    {
        // Wait for the end of the frame so Awake() runs on all objects
        yield return new WaitForEndOfFrame();

        if (PlaceableItemSelection.Instance != null)
        {
            GenerateToggles();
        }
        else
        {
            Debug.LogError("Still can't find the Manager! Is it in the scene?");
        }
    }

    private void GenerateToggles()
    {
        GameObject firstToggle = null;
        GameObject lastToggle = null;

        Debug.Log("GENERATOR: I am now running!");

        // check if the manager exists yet
        if (PlaceableItemSelection.Instance == null)
        {
            Debug.LogWarning("Manager not found! Waiting for next frame...");
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

            // get script
            PlaceableItemToggleUI toggleScript = newToggleGO.GetComponent<PlaceableItemToggleUI>();

            if (toggleScript != null)
            {
                toggleScript.SetupToggle(item);

                // track for navigation
                if (firstToggle == null) firstToggle = newToggleGO;
                lastToggle = newToggleGO;
            }
            else
            {
                Debug.LogError("The ItemToggle prefab is missing the PlaceableItemToggleUI script!");
                continue; // Skip to the next item instead of crashing the whole loop
            }

            // get data from item
            SpriteRenderer sprite = item.GetComponentInChildren<SpriteRenderer>();

            if (sprite != null)
            {
                // take sprite and put to UI image
                newToggleGO.transform.Find("Image").GetComponent<UnityEngine.UI.Image>().sprite = sprite.sprite;
            }

            // set toggle logic
            //newToggleGO.GetComponent<PlaceableItemToggleUI>().SetupToggle(item);
        }

        if (continueButton != null && lastToggle != null)
        {
            var nav = continueButton.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;

            // link play button up to last item
            nav.selectOnUp = lastToggle.GetComponentInChildren<UnityEngine.UI.Toggle>();
            continueButton.navigation = nav;

            // set down to go back to play
            //var tNav = lastToggle.GetComponentInChildren<UnityEngine.UI.Toggle>().navigation;
            //tNav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            //tNav.selectOnDown = continueButton;
            //lastToggle.GetComponentInChildren<UnityEngine.UI.Toggle>().navigation = tNav;
        }

        // get button component
        //UnityEngine.UI.Button playButton = continueButton.GetComponent<UnityEngine.UI.Button>();
        // get toggle component from first item
        //UnityEngine.UI.Navigation playNav = playButton.navigation;
        // TODO ADD LOGIC FOR NAV WHEN BUTTON UP TO LAST ITEM
        //playNav.selectOnUp = first
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndGenerate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
