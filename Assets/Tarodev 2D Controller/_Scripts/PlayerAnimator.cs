using System.Collections.Generic;
using UnityEngine;

namespace TarodevController
{

    public class PlayerAnimator : MonoBehaviour
    {

        [Header("Base placeholders (from Base.controller)")]
        [SerializeField] private AnimationClip idleBase;
        [SerializeField] private AnimationClip runBase;
        [SerializeField] private AnimationClip jumpBase;
        [SerializeField] private AnimationClip landBase;
        [SerializeField] private AnimationClip wallBase;
        [SerializeField] private AnimationClip deathBase;

        [Header("Animation Setup")]
        [Tooltip("The single shared base controller that defines all states (Idle, Run, Jump, Land, Wall, Death).")]
        [SerializeField] private RuntimeAnimatorController baseController;

        [Tooltip("Character-specific animation library with clips for all health/fire states.")]
        [SerializeField] public CharacterAnimationLibrary library;

        [SerializeField] private PlayerHealthSystem _health;

        [Header("References")]
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Rigidbody2D _rb;
        private IPlayerController _player;
        private AudioSource _source;

        [Header("Locomotion Settings")]
        [Tooltip("Speed threshold above which character is considered running.")]
        [SerializeField] private float runThreshold = 0.1f;
        [Tooltip("Approximate max ground speed for normalization of Speed parameter.")]
        [SerializeField] private float maxGroundSpeedEstimate = 6f;
        [Tooltip("Interpolation smoothing for speed parameter.")]
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

        private bool _grounded;
        private bool _isRunning;
        private float _speedParam;
        private ParticleSystem.MinMaxGradient _currentGradient;

        // Animator parameter hashes
        private static readonly int SpeedKey = Animator.StringToHash("Speed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IsRunningKey = Animator.StringToHash("IsRunning");
        private static readonly int OnWallKey = Animator.StringToHash("OnWall");
        private static readonly int IsDeadKey = Animator.StringToHash("IsDead");

        // Cache of generated AnimatorOverrideControllers
        private readonly Dictionary<(bool onFire, HealthTier tier), AnimatorOverrideController> _aocCache
            = new Dictionary<(bool, HealthTier), AnimatorOverrideController>();

        private bool _lastOnFire;
        private HealthTier _lastTier;

        private void Awake()
        {
            if (_anim == null) _anim = GetComponentInChildren<Animator>(true);
            if (_rb == null) _rb = GetComponentInParent<Rigidbody2D>();
            _player = GetComponentInParent<IPlayerController>();
            _source = GetComponent<AudioSource>();

            _anim.updateMode = AnimatorUpdateMode.Normal;
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

        private void Start()
        {
            ApplyAnimatorForCurrentState(force: true);
        }

        private void Update()
        {
            if (_player == null || _rb == null) return;

            ApplyAnimatorForCurrentState();
            HandleSpriteFlip();
            HandleLocomotion();
            DetectGroundColor();
        }

        // --- Animator Variant Selection (Health + Fire) ---
        public void ApplyAnimatorForCurrentState(bool force = false)
        {
            if (_health == null || library == null || baseController == null || _anim == null) return;

            float hp01 = (_health.maxHealth > 0)
                ? Mathf.Clamp01((float)_health.currentHealth / (float)_health.maxHealth)
                : 1f;

            HealthTier tier = hp01 > 0.66f ? HealthTier.Full
                             : hp01 > 0.33f ? HealthTier.Half
                                            : HealthTier.Low;

            bool onFire = _health.isBurning;

            if (!force && onFire == _lastOnFire && tier == _lastTier) return;
            _lastOnFire = onFire; _lastTier = tier;

            MotionClips clips = SelectClips(library, onFire, tier);

            var key = (onFire, tier);
            if (!_aocCache.TryGetValue(key, out var aoc))
            {
                aoc = BuildOverride(baseController, clips);
                _aocCache[key] = aoc;
            }

            if (_anim.runtimeAnimatorController != aoc)
                _anim.runtimeAnimatorController = aoc;
        }

        private static MotionClips SelectClips(CharacterAnimationLibrary lib, bool onFire, HealthTier tier)
        {
            if (!onFire)
            {
                switch (tier)
                {
                    case HealthTier.Full: return lib.NormalFull;
                    case HealthTier.Half: return lib.NormalHalf;
                    default: return lib.NormalLow;
                }
            }
            else
            {
                switch (tier)
                {
                    case HealthTier.Full: return lib.FireFull;
                    case HealthTier.Half: return lib.FireHalf;
                    default: return lib.FireLow;
                }
            }
        }

        private AnimatorOverrideController BuildOverride(RuntimeAnimatorController baseCtrl, MotionClips clips)
        {
            var aoc = new AnimatorOverrideController(baseCtrl);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            void Add(AnimationClip baseClip, AnimationClip repl, string tag)
            {
                if (baseClip == null) { Debug.LogError($"[PA] Base placeholder missing: {tag}"); return; }
                if (repl == null) { Debug.LogWarning($"[PA] Replacement NULL for: {tag}"); return; }
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(baseClip, repl));
            }

            Add(idleBase, clips.Idle, "Idle");
            Add(runBase, clips.Run, "Run");
            Add(jumpBase, clips.Jump, "Jump");
            Add(landBase, clips.Land, "Land");
            Add(wallBase, clips.Wall, "Wall");
            Add(deathBase, clips.Death, "Death");

            aoc.ApplyOverrides(overrides);
            Debug.Log($"[PA] ApplyOverrides: {overrides.Count} replacements applied");
            return aoc;
        }


        // --- Locomotion / Flip / Tilt ---
        private void HandleLocomotion()
        {
            float hSpeed = Mathf.Abs(_rb.velocity.x);
            float targetSpeed01 = Mathf.Clamp01(hSpeed / Mathf.Max(maxGroundSpeedEstimate, 0.01f));
            _speedParam = Mathf.Lerp(_speedParam, targetSpeed01, 1f - Mathf.Exp(-speedLerp * Time.deltaTime));
            _anim.SetFloat(SpeedKey, _speedParam);

            _isRunning = _grounded && hSpeed > runThreshold;
            _anim.SetBool(IsRunningKey, _isRunning);

            if (_moveParticles != null)
            {
                _moveParticles.transform.localScale = Vector3.MoveTowards(
                    _moveParticles.transform.localScale,
                    Vector3.one * _speedParam,
                    2f * Time.deltaTime);
            }
        }

        private void HandleSpriteFlip()
        {
            if (_rb.velocity.x != 0f)
                _sprite.flipX = _rb.velocity.x < 0f;
        }

        private void OnWallChanged(bool onWall, int dir)
        {
            _anim.SetBool(OnWallKey, onWall);
            if (onWall)
            {
                bool movingRight = dir > 0;
                _sprite.flipX = movingRight;
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

                if (_footsteps != null && _footsteps.Length > 0 && _source != null)
                    _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);

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

        // --- Ground color particles ---
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

        public void SetLibrary(CharacterAnimationLibrary lib, bool forceApply = true)
        {
            library = lib;
            if (forceApply) ApplyAnimatorForCurrentState(force: true);
        }
    }
}
