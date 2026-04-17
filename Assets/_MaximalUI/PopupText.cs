using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float lifeTime = 3f;

    private TextMeshProUGUI textMesh;
    private Color startColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        startColor = textMesh.color;
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        lifeTime -= Time.deltaTime;

        float alpha = Mathf.Clamp01(lifeTime);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string message)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        textMesh.text = message;
        startColor = textMesh.color;
    }
}