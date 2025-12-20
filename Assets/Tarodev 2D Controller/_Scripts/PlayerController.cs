using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        // PlayerSelectionState
        public static event Action<PlayerInput> OnPlayerReady;
        public static event Action OnTryStartGame;
        private PlayerInput _playerInput;

        
        
        
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        private Vector2 movementInput = Vector2.zero;

        private bool jumpPressed;
        private bool jumpHeld;

        private PlayerHealthSystem _healthSystem;

        private Vector2 _externalImpulse;

        // ======== TWO-SPEED WALK ========
        [Header("Two-Speed Movement")]
        [Tooltip("Unterhalb dieser Stick-Magnitude (|x|) Slow-Mode, dar�ber Normal.")]
        [SerializeField, Range(0f, 1f)] private float slowThreshold = 0.5f;  // 50%
        [Tooltip("MaxSpeed-Multiplikator im Slow-Mode.")]
        [SerializeField, Range(0.05f, 1f)] private float slowSpeedMultiplier = 0.4f;
        [Tooltip("Kleines Deadzone gegen Kriechen.")]
        [SerializeField, Range(0f, 0.3f)] private float analogDeadZone = 0.1f;
        private float _moveSpeedMultiplier = 1f; // per Frame berechnet

        // ======== ICE SURFACE ========
        [Header("Ice Surface (per Ground-Detection)")]
        [SerializeField, Range(0.1f, 2f)] private float iceAccelMultiplier = 0.6f;
        [SerializeField, Range(0.01f, 2f)] private float iceDecelMultiplier = 0.1f;
        [SerializeField, Range(0.5f, 1.5f)] private float iceMaxSpeedMultiplier = 1.05f;
        [Tooltip("Welche Colliders sind 'Boden'?")]
        [SerializeField] private LayerMask groundLayers;
        [Tooltip("Erlaubt Erkennung des Boden-Colliders via CapsuleCast.")]
        [SerializeField] private float grounderDistance = 0.05f;
        [Tooltip("Optional: Statt Tag 'Ice' kannst du alternativ eine Layer hier angeben (0 = ignorieren).")]
        [SerializeField] private int iceLayer = 0;
        [Tooltip("Wenn true, gilt Eis-Effekt auch in der Luft (z.B. bei sehr glatter Luftkontrolle). Meist false.")]
        [SerializeField] private bool iceAffectsAir = false;
        // --- Extra "ice feel" controls ---
        [Header("Ice Feel")]
        [Tooltip("How strongly input can steer your velocity on ice (lower = slipperier).")]
        [SerializeField, Range(0.05f, 1f)] private float iceTraction = 0.18f;

        [Tooltip("Further reduction to steering when reversing direction on ice.")]
        [SerializeField, Range(0.1f, 1f)] private float iceReverseControl = 0.35f;

        [Tooltip("How quickly speed bleeds off per second with no input on ice (0 = keeps sliding forever).")]
        [SerializeField, Range(0f, 1f)] private float iceSlideLossPerSecond = 0.25f;


        private bool _onIce;                  // aktuell auf Eis?
        private Collider2D _lastGroundCol;    // gemerkter Boden-Collider

        // ======== WALL STATE ========
        private bool _onWall;
        private int _wallDir;
        private float _lastWallTime;
        private float _wallStickCounter;
        public event Action<bool, int> WallStateChanged; // (onWall, wallDir: -1 left, +1 right)
        public bool IsOnWall => _onWall;
        public int WallDir => _wallDir;

        #region Interface
        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        #endregion

        private float _time;

        public void AddImpulse(Vector2 deltaVelocity) => _externalImpulse += deltaVelocity;

        private void Awake()
        {
            // PlayerInput lives on PlayerRoot (parent of PlayerNoPI)
            _playerInput = GetComponentInParent<PlayerInput>();
            if (_playerInput == null)
                Debug.LogError("PlayerController: No PlayerInput found in parents!", this);
            
            
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _healthSystem = GetComponent<PlayerHealthSystem>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            _wallDir = 1;

            if (groundLayers.value == 0)
            {
                // Fallback: verwende die SolidLayers aus _stats, falls LayerMask leer ist
                groundLayers = _stats != null ? _stats.SolidLayers : Physics2D.AllLayers;
            }
            if (grounderDistance <= 0f) grounderDistance = 0.05f;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (GameEvents.CurrentState == GameState.PlayerSelectionState)
                OnTryStartGame?.Invoke();
            
            if (context.started)
            {
                jumpPressed = true;
                jumpHeld = true;
            }
            else if (context.canceled)
            {
                jumpHeld = false;
            }
        }

        public void OnReady(InputAction.CallbackContext context)
        {
            if (GameEvents.CurrentState == GameState.PlayerSelectionState)
                OnPlayerReady?.Invoke(_playerInput);
        }
        
        private void GatherInput()
        {
            Vector2 adjustedInput = movementInput;

            if (_healthSystem != null && _healthSystem.IsConfused())
                adjustedInput = -adjustedInput;

            // Two-speed: anhand *roher* X-Magnitude (vor Snap) berechnen
            float magX = Mathf.Abs(adjustedInput.x);
            if (magX < analogDeadZone) adjustedInput.x = 0f;

            _moveSpeedMultiplier = (magX >= slowThreshold) ? 1f : slowSpeedMultiplier;

            _frameInput = new FrameInput
            {
                JumpDown = jumpPressed,
                JumpHeld = jumpHeld,
                Move = adjustedInput
            };
            jumpPressed = false;

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();

            // external forces
            _frameVelocity += _externalImpulse;
            _externalImpulse = Vector2.zero;

            ApplyMovement();
        }

        #region Collisions
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground & Ceiling als RaycastHit2D, damit wir den Boden-Collider kennen
            RaycastHit2D groundInfo = Physics2D.CapsuleCast(
                _col.bounds.center, _col.size, _col.direction, 0,
                Vector2.down, grounderDistance, groundLayers
            );
            bool groundHit = groundInfo.collider != null;

            RaycastHit2D ceilingInfo = Physics2D.CapsuleCast(
                _col.bounds.center, _col.size, _col.direction, 0,
                Vector2.up, grounderDistance, _stats.SolidLayers
            );
            bool ceilingHit = ceilingInfo.collider != null;

            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // leichte Corner-Korrektur optional
            if (!_grounded && _rb.velocity.y > 0f && ceilingHit)
            {
                const float nudge = 0.08f;
                Vector3 pos = transform.position;
                if (!Physics2D.CapsuleCast(pos + Vector3.left * nudge, _col.size, _col.direction, 0, Vector2.up, grounderDistance, _stats.SolidLayers))
                    transform.position += Vector3.left * nudge;
                else if (!Physics2D.CapsuleCast(pos + Vector3.right * nudge, _col.size, _col.direction, 0, Vector2.up, grounderDistance, _stats.SolidLayers))
                    transform.position += Vector3.right * nudge;
            }

            // Ground state change
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _frameLeftGrounded = float.MinValue;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            // ICE detection (nur wenn Boden ber�hrt wird)
            _onIce = false;
            _lastGroundCol = null;
            if (groundHit)
            {
                _lastGroundCol = groundInfo.collider;

                // Erkennung �ber Tag "Ice" ODER optional �ber eine Ice-Layer-ID
                if (_lastGroundCol.CompareTag("Ice")) _onIce = true;
                else if (iceLayer > 0 && _lastGroundCol.gameObject.layer == iceLayer) _onIce = true;
            }

            // --- before you modify _onWall/_wallDir, cache previous ---
            bool prevOnWall = _onWall;
            int prevDir = _wallDir;

            // WALLS
            if (!_grounded)
            {
                bool leftHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.left, _stats.WallCheckDistance, _stats.SolidLayers);
                bool rightHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.right, _stats.WallCheckDistance, _stats.SolidLayers);

                bool touchingWall = leftHit || rightHit;
                if (touchingWall)
                {
                    _onWall = true;
                    _wallDir = leftHit ? -1 : 1;
                    _lastWallTime = _time;

                    if (Mathf.Sign(_frameInput.Move.x) == _wallDir)
                        _wallStickCounter = _stats.WallStickTime;
                }
                else
                {
                    _onWall = false;
                    _wallStickCounter = Mathf.Max(0, _wallStickCounter - Time.fixedDeltaTime);
                }
            }
            else
            {
                _onWall = false;
                _wallStickCounter = 0;
            }

            // --- after computing, notify changes ---
            if (prevOnWall != _onWall || prevDir != _wallDir)
                WallStateChanged?.Invoke(_onWall, _wallDir);

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }
        #endregion

        #region Jumping
        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote)
            {
                ExecuteJump();
                _jumpToConsume = false;
                return;
            }

            bool canWallCoyote = _time < _lastWallTime + _stats.WallJumpCoyoteTime;
            if (_onWall || canWallCoyote)
            {
                ExecuteWallJump();
                _jumpToConsume = false;
                return;
            }
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        private void ExecuteWallJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;

            int dir = _wallDir == 0 ? 1 : _wallDir;
            _frameVelocity.x = -dir * _stats.WallJumpHorizontalSpeed;
            _frameVelocity.y = Mathf.Max(_frameVelocity.y, _stats.WallJumpPower);

            _wallStickCounter = 0f;

            Jumped?.Invoke();
        }
        #endregion

        #region Horizontal
        private void HandleDirection()
        {
            bool pressingIntoWall = _onWall && Mathf.Sign(_frameInput.Move.x) == _wallDir && _wallStickCounter > 0f;
            if (pressingIntoWall)
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.AirDeceleration * Time.fixedDeltaTime);
                return;
            }

            bool iceActive = ((_grounded || iceAffectsAir) && _onIce);

            // --- No input: deceleration / slide ---
            if (_frameInput.Move.x == 0)
            {
                if (iceActive)
                {
                    // Exponential-like decay: preserves momentum much longer than MoveTowards
                    // speed *= (1 - k * dt)
                    float k = Mathf.Clamp01(iceSlideLossPerSecond * Time.fixedDeltaTime);
                    _frameVelocity.x *= (1f - k);
                }
                else
                {
                    float decel = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, decel * Time.fixedDeltaTime);
                }
                return;
            }

            // --- Input present: steer toward target speed ---
            float accel = _stats.Acceleration;
            if (!_grounded && Mathf.Abs(_frameVelocity.y) < _stats.ApexThreshold)
                accel *= _stats.ApexBonusMultiplier;

            // Two-speed walk support
            if (_moveSpeedMultiplier < 1f) accel *= _moveSpeedMultiplier;
            float targetMax = _stats.MaxSpeed * _moveSpeedMultiplier;

            if (iceActive)
            {
                // Classic ice: weaker accel + slightly higher cap (optional) from your existing fields
                accel *= iceAccelMultiplier;
                targetMax *= iceMaxSpeedMultiplier;

                // Compute desired velocity and steer with limited traction
                float desired = _frameInput.Move.x * targetMax;
                float steer = accel * Time.fixedDeltaTime;

                // If reversing direction, reduce steering even more (feels like sliding past)
                bool reversing = Mathf.Sign(_frameVelocity.x) != Mathf.Sign(desired) && Mathf.Abs(_frameVelocity.x) > 0.01f;
                if (reversing) steer *= iceReverseControl;

                // Traction limits how fast we can change toward desired
                steer *= Mathf.Clamp01(iceTraction);

                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, desired, steer);
            }
            else
            {
                // Normal ground/air handling
                _frameVelocity.x = Mathf.MoveTowards(
                    _frameVelocity.x,
                    _frameInput.Move.x * targetMax,
                    accel * Time.fixedDeltaTime
                );
            }
        }

        #endregion

        #region Gravity
        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);

                if (_onWall && _frameVelocity.y < -_stats.WallSlideSpeed)
                    _frameVelocity.y = -_stats.WallSlideSpeed;
            }
        }
        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        event Action<bool, float> GroundedChanged;
        event Action Jumped;
        event Action<bool, int> WallStateChanged;  // <� add this
        Vector2 FrameInput { get; }
    }

}
