using UnityEngine;

public class GIFManager : MonoBehaviour
{
    public static GIFManager Instance;
    private GameObject currentGIF;

    void Awake()
    {
        Instance = this;
    }

    public void ShowGIF(string gifName)
    {
        // Destroy old
        if (currentGIF != null)
            Destroy(currentGIF);

        // Load the new prefab
        GameObject prefab = Resources.Load<GameObject>("ItemDescriptionGIFs/" + gifName);

        if (!prefab)
        {
            Debug.LogWarning("GIF prefab not found: " + gifName);
            return;
        }

        // Create new GIF
        currentGIF = Instantiate(prefab, transform);
        currentGIF.SetActive(true);
    }

    public void HideGIF()
    {
        if (currentGIF != null)
            currentGIF.SetActive(false);
    }
}
