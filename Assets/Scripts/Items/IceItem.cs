using UnityEngine;

public class IceItem : MonoBehaviour
{
    public GameObject icePatchPrefab;
    public float width = 4f;

    public void Use(Vector3 dropPos)
    {
        var go = Instantiate(icePatchPrefab, dropPos, Quaternion.identity);
        // ggf. die Collider-Breite anpassen:
        var col = go.GetComponent<BoxCollider2D>();
        if (col) col.size = new Vector2(width, col.size.y);
    }
}
