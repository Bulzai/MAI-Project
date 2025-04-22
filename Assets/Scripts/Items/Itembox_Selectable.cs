using UnityEngine;

public class Itembox_Selectable : MonoBehaviour
{

    public float spawnRate;

    private void Start()
    {
    }
    public void OnMouseEnter()
    {
        OutlineManager.Instance.AddGameObject(gameObject);
    }

    public void OnMouseExit()
    {
        OutlineManager.Instance.RemoveGameObject(gameObject);
    }

    private void OnMouseDown()
    {
        Destroy(gameObject);
    }

    public float GetSpawnRate()
    {
        return spawnRate;
    }
}
