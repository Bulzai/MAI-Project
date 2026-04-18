using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float lifeTime = 1f;

    private TextMeshProUGUI textMesh;
    private Color startColor;

    public bool isImportant = false;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        startColor = textMesh.color;

        if (isImportant)
        {
            lifeTime = 1.5f;
            moveSpeed = 0.6f;
            transform.localScale = Vector3.one * 1.5f;
        }
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

        if (isImportant)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 6f);
        }
    }

    public void SetText(string message)
    {
        textMesh.text = message;
        startColor = textMesh.color;
    }

    public void SetColor(Color color)
    {
        textMesh.color = color;
        startColor = color;
    }
}