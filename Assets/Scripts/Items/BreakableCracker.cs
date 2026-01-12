using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableCracker : MonoBehaviour
{
    public static event Action OnCrackerBroken;
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer; // child sprite
    [SerializeField] private Sprite originalSprite;         // healthy cookie
    [SerializeField] private Sprite[] breakFrames;          // sliced animation frames (ordered)

    [Header("Timing")]
    [SerializeField] private float breakDelay = 1f;         // wobble time before breaking
    [SerializeField] private float frameTime = 0.12f;       // each frame of breaking anim
    [SerializeField] private float respawnDelay = 1.5f;     // respawn after break
    [SerializeField] private float hoverRespawnDelay = 0.5f;// faster respawn for hover trigger

    private Collider2D _collider;
    private SelectableItem _selectable;
    private bool _isBreaking;
    private bool _isBroken;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _selectable = GetComponent<SelectableItem>();

        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (!originalSprite && spriteRenderer)
            originalSprite = spriteRenderer.sprite;
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateExited += ResetCracker;
        GameEvents.OnPlaceItemStateEntered += ResetCracker;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateExited -= ResetCracker;
        GameEvents.OnPlaceItemStateEntered -= ResetCracker;
    }

    public bool IsBrokenOrBreaking => _isBroken || _isBreaking;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsBrokenOrBreaking) return;
        if (!collision.collider.CompareTag("Player")) return;

        StartCoroutine(BreakAfterDelay(respawnDelay));
    }

    /// <summary>
    /// Called externally (e.g. by a hover aura or trigger).
    /// </summary>
    public void BreakInstantly(bool fastReset = true)
    {
        if (IsBrokenOrBreaking) return;
        StartCoroutine(BreakAfterDelay(fastReset ? hoverRespawnDelay : respawnDelay));
    }

    private IEnumerator BreakAfterDelay(float chosenRespawnDelay)
    {
        _isBreaking = true;

        // Small wobble before breaking
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        while (elapsed < breakDelay)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 30f) * 0.02f;
            transform.localScale = startScale + new Vector3(shake, -shake, 0);
            yield return null;
        }
        transform.localScale = startScale;

        DoBreak();

        if (chosenRespawnDelay > 0f)
        {
            yield return new WaitForSeconds(chosenRespawnDelay);
            ResetCracker();
        }
    }

    private void DoBreak()
    {

        OnCrackerBroken?.Invoke();
        // Play breaking animation frames
        if (spriteRenderer && breakFrames.Length > 0)
            StartCoroutine(PlayBreakAnimation());
    }

    private IEnumerator PlayBreakAnimation()
    {
        if (_selectable) _selectable.isAvailable = false;

        foreach (var frame in breakFrames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameTime);
        }

        _isBroken = true;
        _isBreaking = false;
        if (_collider) _collider.enabled = false;
    }

    private void ResetCracker()
    {
        _isBroken = false;
        _isBreaking = false;

        if (_collider) _collider.enabled = true;
        if (_selectable) _selectable.isAvailable = true;

        if (spriteRenderer && originalSprite)
            spriteRenderer.sprite = originalSprite;
    }
}
