using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableCracker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time in seconds before the cracker breaks after the player lands on it.")]
    [SerializeField] private float breakDelay = 1f;
    [Tooltip("Should the cracker respawn after breaking? (0 = never)")]
    [SerializeField] private float respawnDelay = 0f;

    [Header("Optional Visuals")]
    [SerializeField] private Sprite crackedSprite;
    [SerializeField] private ParticleSystem breakVFX;
    [SerializeField] private AudioClip breakSFX;
    [SerializeField] private float sfxVolume = 1f;

    private SpriteRenderer _renderer;
    private Sprite _originalSprite;
    private Collider2D _collider;
    private bool _isBreaking;
    private bool _isBroken;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalSprite = _renderer ? _renderer.sprite : null;
        _collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //  Only trigger if the player hits from above
        if (_isBroken || _isBreaking) return;
        if (!collision.collider.CompareTag("Player")) return;

        StartCoroutine(BreakAfterDelay());

 
    }

    private IEnumerator BreakAfterDelay()
    {
        _isBreaking = true;

        // Optional: visual feedback (slight wobble)
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        while (elapsed < breakDelay)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 30f) * 0.02f;
            transform.localScale = startScale + new Vector3(shake, -shake, 0);
            yield return null;
        }

        DoBreak();
        transform.localScale = startScale;

        if (respawnDelay > 0)
        {
            yield return new WaitForSeconds(respawnDelay);
            ResetCracker();
        }
    }

    private void DoBreak()
    {
        _isBroken = true;
        _collider.enabled = false;


        if (_renderer && crackedSprite)
            _renderer.sprite = crackedSprite;

        if (breakVFX)
        {
            var vfx = Instantiate(breakVFX, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, 2f);
        }

        if (breakSFX)
            AudioSource.PlayClipAtPoint(breakSFX, transform.position, sfxVolume);
    }

    private void ResetCracker()
    {
        _isBroken = false;
        _isBreaking = false;
        _collider.enabled = true;
        if (_renderer && _originalSprite)
            _renderer.sprite = _originalSprite;
    }
}
