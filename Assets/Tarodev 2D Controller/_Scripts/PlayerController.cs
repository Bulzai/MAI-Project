using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
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

        // >>> Two-speed controls
        [Header("Two-Speed Movement")]
        [Tooltip("Below this stick magnitude, use slow mode. At/above, use normal.")]
        [SerializeField, Range(0f, 1f)] private float slowThreshold = 0.5f;   // 50%
        [Tooltip("MaxSpeed multiplier when in slow mode.")]
        [SerializeField, Range(0.05f, 1f)] private float slowSpeedMultiplier = 0.4f; // 40% speed
        [Tooltip("Treat tiny stick input as zero to prevent creeping.")]
        [SerializeField, Range(0f, 0.3f)] private float analogDeadZone = 0.1f;
        private float _moveSpeedMultiplier = 1f; // computed per-frame

        #region Interface
        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        #endregion

        private float _time;

        public void AddImpulse(Vector2 deltaVelocity) => _externalImpulse += deltaVelocity;

        // ======== WALL STATE ========
        private bool _onWall;
        private int _wallDir;
        private float _lastWallTime;
        private float _wallStickCounter;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _healthSystem = GetComponent<PlayerHealthSystem>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            _wallDir = 1;
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

        private void GatherInput()
        {
            // Start from raw analog input
            Vector2 adjustedInput = movementInput;

            // Invert if confused
            if (_healthSystem != null && _healthSystem.IsConfused())
                adjustedInput = -adjustedInput;

            // >>> Two-speed: compute multiplier from *raw* horizontal magnitude (before snapping)
            float magX = Mathf.Abs(adjustedInput.x);

            // Deadzone to avoid creeping
            if (magX < analogDeadZone) adjustedInput.x = 0f;

            _moveSpeedMultiplier = (magX >= slowThreshold) ? 1f : slowSpeedMultiplier;

            // Now optionally snap to -1/0/1 for classic feel, keeping the multiplier we already computed
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

            // external forces like knockback
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

            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
                                                    Vector2.down, _stats.GrounderDistance, _stats.SolidLayers);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
                                                    Vector2.up, _stats.GrounderDistance, _stats.SolidLayers);

            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            if (!_grounded && _rb.velocity.y > 0f && ceilingHit)
            {
                const float nudge = 0.08f;
                Vector3 pos = transform.position;

                if (!Physics2D.CapsuleCast(pos + Vector3.left * nudge, _col.size, _col.direction, 0,
                                           Vector2.up, _stats.GrounderDistance, _stats.SolidLayers))
                {
                    transform.position += Vector3.left * nudge;
                }
                else if (!Physics2D.CapsuleCast(pos + Vector3.right * nudge, _col.size, _col.direction, 0,
                                                Vector2.up, _stats.GrounderDistance, _stats.SolidLayers))
                {
                    transform.position += Vector3.right * nudge;
                }
            }

            if (!_grounded && groundHit)
            {
                _grounded = true;
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

            if (!_grounded)
            {
                bool leftHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
                                                     Vector2.left, _stats.WallCheckDistance, _stats.SolidLayers);
                bool rightHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
                                                      Vector2.right, _stats.WallCheckDistance, _stats.SolidLayers);

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

            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                float accel = _stats.Acceleration;
                if (!_grounded && Mathf.Abs(_frameVelocity.y) < _stats.ApexThreshold)
                    accel *= _stats.ApexBonusMultiplier;

                // >>> Scale by slow/normal mode (keeps the feel consistent)
                if (_moveSpeedMultiplier < 1f)
                    accel *= _moveSpeedMultiplier;

                float targetMax = _stats.MaxSpeed * _moveSpeedMultiplier;

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
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}
