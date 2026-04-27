using UnityEngine;

public class GIFManager : MonoBehaviour
{
    public static GIFManager Instance;
    private GameObject currentGIF;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowGIF(string gifName)
    {
        if (currentGIF != null)
            Destroy(currentGIF);

        GameObject prefab = Resources.Load<GameObject>("ItemDescriptionGIFs/" + gifName);

        if (!prefab)
        {
            Debug.LogWarning("GIF prefab not found: " + gifName);
            return;
        }

        currentGIF = Instantiate(prefab, transform);
        currentGIF.SetActive(true);
    }

    public void HideGIF()
    {
        if (currentGIF != null)
        {
            Destroy(currentGIF);
            currentGIF = null;
        }
    }
}