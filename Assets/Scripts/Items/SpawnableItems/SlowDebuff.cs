using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[DefaultExecutionOrder(100)]
public class SlowDebuff : MonoBehaviour
{
    [SerializeField] private bool affectHorizontal = true;
    [SerializeField] private bool affectVertical = false;

    [Header("Visuals")]
    public SpriteRenderer playerSprite;
    public Color slowColor = Color.blue;
    private Color originalColor;
    private Coroutine _visualCo;
    public GameObject freezeLogo;


    private Rigidbody2D rb;
    private float activeUntil = 0f;
    private float multiplier = 1f;

    private void Awake() => rb = GetComponent<Rigidbody2D>();


    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += DeactivateIceLogo;
        GameEvents.OnMainGameStateExited += DeactivateIceLogo;
    }

    


    private void Start()
    {
        if (playerSprite != null)
            originalColor = playerSprite.color;
        else
            originalColor = Color.white;
    }

    public void ApplySpeedModifier(float m, float seconds)
    {
        multiplier = m;
        activeUntil = Time.time + Mathf.Max(0.01f, seconds);

        // Visuals starten/refreshen
        if (_visualCo != null) StopCoroutine(_visualCo);

        // Wir blinken nur, wenn es ein Slow ist (multiplier < 1)
        if (multiplier < 1f)
        {
            _visualCo = StartCoroutine(SlowVisualRoutine());
        }
        else
        {
            // Optional: Bei Speed-up (m > 1) Farbe zurücksetzen oder andere Farbe wählen
            playerSprite.color = originalColor;
        }
    }

    private IEnumerator SlowVisualRoutine()
    {
        float blinkInterval = 0.15f; // Geschwindigkeit des Blinkens

        if(freezeLogo != null)
            freezeLogo.SetActive(true);


        while (Time.time < activeUntil)
        {
            if (playerSprite != null)
            {
                // Wechsel zwischen Blau und Original
                playerSprite.color = (playerSprite.color == originalColor) ? slowColor : originalColor;
            }
            yield return new WaitForSeconds(blinkInterval);
        }
        if (freezeLogo != null)
            freezeLogo.SetActive(false);

        // Am Ende zurück zur Originalfarbe
        if (playerSprite != null) playerSprite.color = originalColor;
        _visualCo = null;
    }

    private void FixedUpdate()
    {
        if (!rb) return;
        if (Time.time >= activeUntil || Mathf.Approximately(multiplier, 1f)) return;

        var v = rb.velocity;
        if (affectHorizontal) v.x *= multiplier;
        if (affectVertical) v.y *= multiplier;
        rb.velocity = v;
    }

    // Sicherheitshalber beim Deaktivieren Farbe resetten
    private void OnDisable()
    {
        if (playerSprite != null) playerSprite.color = originalColor;
    }

    private void DeactivateIceLogo()
    {
        freezeLogo.gameObject.SetActive(false); 
    }
}