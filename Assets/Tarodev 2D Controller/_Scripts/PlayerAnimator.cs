using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Drives the Animator with Speed, Jump, Grounded, and IsRunning.
    /// Handles tilt, sprite flip, particles, and audio.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _anim;              // Animator on "Sprite"
        [SerializeField] private SpriteRenderer _sprite;      // SpriteRenderer on "Sprite"
        [SerializeField] private Rigidbody2D _rb;             // Player Rigidbody (auto-found if null)

        [Header("Animation Settings")]
        [Tooltip("Horizontal speed threshold to be considered running.")]
        [SerializeField] private float runThreshold = 0.1f;
        [Tooltip("Approximate maximum ground speed for blending Speed 0..1.")]
        [SerializeField] private float maxGroundSpeedEstimate = 6f;
        [Tooltip("Smoothing factor for Speed parameter.")]
        [SerializeField, Range(0f, 25f)] private float speedLerp = 12f;

        [Header("Tilt Settings")]
        [SerializeField] private float _maxTilt = 5f;
        [SerializeField] private float _tiltSpeed = 20f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] _footsteps;

        private AudioSource _source;
        private IPlayerController _player;
        private bool _grounded;
        private bool _isRunning;
        private float _speedParam;
        private ParticleSystem.MinMaxGradient _currentGradient;

        // Animator parameter hashes
        private static readonly int SpeedKey = Animator.StringToHash("Speed");      // float
        private static readonly int JumpKey = Animator.StringToHash("Jump");       // trigger
        private static readonly int GroundedKey = Animator.StringToHash("Grounded");   // bool
        private static readonly int IsRunningKey = Animator.StringToHash("IsRunning");  // bool                                                                                   
        private static readonly int OnWallKey = Animator.StringToHash("OnWall");

        private float _lastFacingDir = 1f; // 1 = right, -1 = left

        private void Awake()
        {
            if (_rb == null) _rb = GetComponentInParent<Rigidbody2D>();
            _player = GetComponentInParent<IPlayerController>();
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (_player == null) return;
            _player.Jumped += OnJumped;
            _player.GroundedChanged += OnGroundedChanged;
            _player.WallStateChanged += OnWallChanged;

            if (_moveParticles != null) _moveParticles.Play();
        }

        private void OnDisable()
        {
            if (_player == null) return;
            _player.Jumped -= OnJumped;
            _player.GroundedChanged -= OnGroundedChanged;
            _player.WallStateChanged -= OnWallChanged;

            if (_moveParticles != null) _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null || _rb == null) return;

            HandleSpriteFlip();
            HandleLocomotion();
            //HandleTilt();
            DetectGroundColor();
        }

        private void HandleSpriteFlip()
        {
            // Prefer velocity for facing direction
            if (_rb.velocity.x != 0f)
                _sprite.flipX =  _rb.velocity.x < 0f;
        }

        private void HandleLocomotion()
        {
            // --- Speed parameter ---
            float hSpeed = Mathf.Abs(_rb.velocity.x);
            float targetSpeed01 = Mathf.Clamp01(hSpeed / Mathf.Max(maxGroundSpeedEstimate, 0.01f));
            _speedParam = Mathf.Lerp(_speedParam, targetSpeed01, 1f - Mathf.Exp(-speedLerp * Time.deltaTime));
            _anim.SetFloat(SpeedKey, _speedParam);

            // --- IsRunning bool ---
            _isRunning = _grounded && hSpeed > runThreshold;
            _anim.SetBool(IsRunningKey, _isRunning);

            // --- Move particles scale ---
            if (_moveParticles != null)
            {
                _moveParticles.transform.localScale = Vector3.MoveTowards(
                    _moveParticles.transform.localScale,
                    Vector3.one * _speedParam,
                    2f * Time.deltaTime);
            }
        }

        private void HandleTilt()
        {
            var desiredTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
            _anim.transform.up = Vector3.RotateTowards(
                _anim.transform.up,
                desiredTilt * Vector2.up,
                _tiltSpeed * Time.deltaTime,
                0f);
        }
        private void OnWallChanged(bool onWall, int dir)
        {
            _anim.SetBool(OnWallKey, onWall);
            // Optional: face into the wall
            if (onWall)
            {
                // if your art faces left by default, adjust as needed
                bool movingRight = dir > 0;
                _sprite.flipX = /* faceRightByDefault ? !movingRight : movingRight */ movingRight;
            }
        }
        private void OnJumped()
        {
            _anim.SetTrigger(JumpKey);

            if (_grounded)
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                if (_jumpParticles != null) _jumpParticles.Play();
            }
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;
            _anim.SetBool(GroundedKey, grounded);

            if (grounded)
            {
                DetectGroundColor();
                SetColor(_landParticles);

                // play land/footstep audio
                if (_footsteps != null && _footsteps.Length > 0 && _source != null)
                    _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);

                // restart particles
                if (_moveParticles != null) _moveParticles.Play();

                if (_landParticles != null)
                {
                    _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
                    _landParticles.Play();
                }
            }
            else
            {
                if (_moveParticles != null) _moveParticles.Stop();
            }
        }

        private void DetectGroundColor()
        {
            var hit = Physics2D.Raycast(transform.position, Vector3.down, 2f);
            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;

            var color = r.color;
            _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
            SetColor(_moveParticles);
        }

        private void SetColor(ParticleSystem ps)
        {
            if (ps == null) return;
            var main = ps.main;
            main.startColor = _currentGradient;
        }
    }
}
