using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableCracker : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float breakDelay = 1f;     // wobble time before breaking
    [SerializeField] private float respawnDelay = 1.5f; // normal respawn
    [SerializeField] private float hoverRespawnDelay = 0.5f; // faster respawn for hover-trigger

    [Header("Optional Visuals")]
    [SerializeField] private Sprite crackedSprite;
    [SerializeField] private ParticleSystem breakVFX;
    [SerializeField] private AudioClip breakSFX;
    [SerializeField] private float sfxVolume = 1f;

    private SpriteRenderer _renderer;
    private Sprite _originalSprite;
    private Collider2D _collider;
    private SelectableItem _selectable;  // ← to gate picking
    private bool _isBreaking;
    private bool _isBroken;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalSprite = _renderer ? _renderer.sprite : null;
        _collider = GetComponent<Collider2D>();
        _selectable = GetComponent<SelectableItem>(); // optional
    }

    public bool IsBrokenOrBreaking => _isBroken || _isBreaking;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsBrokenOrBreaking) return;
        if (!collision.collider.CompareTag("Player")) return;

        // Normal break path (uses normal respawn delay)
        StartCoroutine(BreakAfterDelay(respawnDelay));
    }

    /// <summary>
    /// Called from hover logic. Starts break with faster reset.
    /// </summary>
    public void BreakInstantly(bool fastReset = true)
    {
        if (IsBrokenOrBreaking) return;
        StartCoroutine(BreakAfterDelay(fastReset ? hoverRespawnDelay : respawnDelay));
    }

    private IEnumerator BreakAfterDelay(float chosenRespawnDelay)
    {
        _isBreaking = true;

        // wobble during breakDelay
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
        _isBroken = true;
        _isBreaking = false;

        if (_collider) _collider.enabled = false;

        // not pickable while broken
        if (_selectable) _selectable.isAvailable = false;

        if (_renderer && crackedSprite) _renderer.sprite = crackedSprite;

        if (breakVFX)
        {
            var vfx = Instantiate(breakVFX, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, 2f);
        }

        if (breakSFX) AudioSource.PlayClipAtPoint(breakSFX, transform.position, sfxVolume);
    }

    private void ResetCracker()
    {
        _isBroken = false;
        _isBreaking = false;

        if (_collider) _collider.enabled = true;

        // pickable again after reset
        if (_selectable) _selectable.isAvailable = true;

        if (_renderer && _originalSprite) _renderer.sprite = _originalSprite;
    }
}
