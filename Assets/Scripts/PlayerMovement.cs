using UnityEngine;

namespace DNA
{
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Data/Player Movement Data")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("References")]
        [SerializeField]
        public Transform _cameraObject;
        [SerializeField]
        public InputHandler _inputHandler;
        [HideInInspector]
        public Transform _characterTransform;
        [HideInInspector]
        public AnimatorHandler _animatorHandler;
        [SerializeField]
        public GameObject _normalCamera;
        [SerializeField]
        public CharacterController _controller;
        [SerializeField]
        public CameraHandler _cameraHandler;

        public Vector3 _moveDirection;

        [Header("Movement Variables")]
        [SerializeField]
        public float _movementSpeed = 8.0f;
        [SerializeField]
        public float _rotationSpeed = 15.0f;
        [SerializeField]
        public float _jumpHeight = 2f;
        [SerializeField]
        public float _gravity = -15.0f;
        [SerializeField]
        public float _sprintSpeed = 12.0f;
        [SerializeField]
        public float _speedModulation = 0f;

        public readonly float _DiagonalInputThreshold = 0.5f;
        public readonly float _OrthogonalInputThreshold = 0.85f;
        public readonly float _MinimalSpeedModulation = 0.15f;

        [Header("Jump Variables")]
        [SerializeField]
        public bool _isGrounded;
        [SerializeField]
        public LayerMask _groundLayers;
        [SerializeField]
        public float _groundedOffset = -0.08f;
        [SerializeField]
        public bool _didSecondJump = false;
        [SerializeField]
        public float _verticalVelocity;
        [SerializeField]
        public float _terminalVelocity = 50.0f;

        [Header("Step Variables")]
        [SerializeField]
        public Vector3 _horizontalVelocity = Vector3.zero;
        [SerializeField]
        public bool _isStepping = false;
        [SerializeField]
        public float _stepPower = 5f;
        [SerializeField]
        public int _stepStartupFrameNumber = 4;
        [SerializeField]
        public int _stepActiveFrameNumber = 15;
        [SerializeField]
        public int _stepRecoveryFrameNumber = 10;
        [SerializeField]
        public bool _isRecoveringFromStep = false;
        [SerializeField]
        public int _stepFrameCount = 1;
        [SerializeField]
        public bool _isStepFrameCountStarted = false;
        [SerializeField]
        public Vector2 _stepMovementInput;
        [SerializeField]
        public Vector2 _currentStepMovementInput;

        public const float _StepPowerMultiplier = 100f;
        public const float _OrthogonalStepInputThreshold = 0.9f;
        public const float _AntiSpiralConstant = 0.0425f;
        public const float _MinimalStepMovementInput = 0.5f;

        [Header("Dash Variables")]
        [SerializeField]
        public bool _isDashing = false;
        [SerializeField]
        public Vector3 _dashVelocity = Vector3.zero;
        [SerializeField]
        public float _dashPower = 6f;
        [SerializeField]
        public int _dashStartupFrameNumber = 8;
        //[SerializeField]
        //private int _dashRecoveryFrameNumber = 20;
        [SerializeField]
        public int _dashFrameCount = 1;
        [SerializeField]
        public bool _isDashFrameCountStarted = false;

        public const float _DashPowerMultiplier = 100f;

        public CharacterController Controller { get => _controller; set => _controller = value; }
    }

    public class PlayerMovement : MonoBehaviour
    {
        private PlayerMovementData _movementData;

        [Header("Movement References")]
        private WalkAndRun _walkAndRun;
        private Jump _jump;
        private Step _step;
        private Dash _dash;
        private GroundCheck _groundCheck;

        public WalkAndRun WalkAndRun { get => _walkAndRun; set => _walkAndRun = value; }
        public Jump Jump { get => _jump; set => _jump = value; }
        public Step Step { get => _step; set => _step = value; }
        public Dash Dash { get => _dash; set => _dash = value; }
        public GroundCheck GroundCheck { get => _groundCheck; set => _groundCheck = value; }

        private void Start()
        {
            SetMovementDataReferences();
            GetMovementReferences();
        }

        private void SetMovementDataReferences()
        {
            _movementData._controller = GetComponent<CharacterController>();
            _movementData._inputHandler = GetComponent<InputHandler>();
            _movementData._animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _movementData._cameraObject = Camera.main.transform;
            _movementData._cameraHandler = CameraHandler.singleton;
            _movementData._characterTransform = transform;
            _movementData._animatorHandler.Initialize();
            _movementData._groundLayers = LayerMask.GetMask("Floor");
        }

        private void GetMovementReferences()
        {
            _walkAndRun = GetComponentInChildren<WalkAndRun>();
            _jump = GetComponentInChildren<Jump>();
            _step = GetComponentInChildren<Step>();
            _dash = GetComponentInChildren<Dash>();
            _groundCheck = GetComponentInChildren<GroundCheck>();
        }

        #region Movement

        /// <summary>
        /// Makes the character do a jump
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleJump(float delta)
        {
            _animatorHandler.PlayAnimation(_animatorHandler.GroundedString, _isGrounded);
            _animatorHandler.PlayAnimation(_animatorHandler.JumpString, false);

            // If the jump flag is true and the character is grounded, make the character jump
            if (_inputHandler.JumpFlag && _isGrounded && !_isStepping && !_isDashing)
            {
                _didSecondJump = false;
                _animatorHandler.PlayAnimation(_animatorHandler.JumpString, true);
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
            // If the jump flag is true and the character is in air and did not do a second jump, make the character jump
            else if (_inputHandler.JumpFlag && !_isGrounded && !_didSecondJump && !_isStepping && !_isDashing)
            {
                _didSecondJump = true;
                _animatorHandler.PlayAnimation(_animatorHandler.JumpString, true);
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            // If the character is grounded, fix the character to the ground
            if (_isGrounded)
            {
                _didSecondJump = false;
                if (_verticalVelocity < 0)
                {
                    _verticalVelocity = -2f;
                }
            }

            // Apply gravity by reducing the character vertical velocity if it has vertical velocity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += _gravity * delta;
            }
        }

        /// <summary>
        /// Makes the character do a front, back, side step
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleStep(float delta)
        {
            if ((_inputHandler.StepFlag && !_isStepping) ||
                (_isStepping && _cameraHandler.CurrentLockTarget != null) ||
                (_isStepping && _isRecoveringFromStep) ||
                (_isStepFrameCountStarted))
            {
                // Start count for startup
                if (!_isStepFrameCountStarted)
                {
                    _stepMovementInput = _inputHandler.MovementInput;

                    // If movement input too low, do not step
                    if (Mathf.Abs(_stepMovementInput.x) + Mathf.Abs(_stepMovementInput.y) > _MinimalStepMovementInput)
                    {
                        _isStepFrameCountStarted = true;
                    }
                }
                // After startup
                else if (_isStepFrameCountStarted && _stepFrameCount > _stepStartupFrameNumber &&
                    _stepFrameCount <= _stepStartupFrameNumber + _stepActiveFrameNumber + _stepRecoveryFrameNumber)
                {
                    // Set recovery bool after active frames
                    if (!_isRecoveringFromStep && _stepFrameCount > _stepStartupFrameNumber + _stepActiveFrameNumber)
                    {
                        _isRecoveringFromStep = true;
                    }

                    SetStepMoveDirection();
                    ExecuteStep();
                }
                // After recovery frames, reset step
                else if (_isStepFrameCountStarted && _stepFrameCount > _stepStartupFrameNumber + _stepActiveFrameNumber + _stepRecoveryFrameNumber)
                {
                    ResetStep();
                }
            }

            // Increment frame count
            if (_isStepFrameCountStarted)
            {
                _stepFrameCount += 1;
            }
        }

        /// <summary>
        /// Computes and sets/updates the step move direction
        /// </summary>
        private void SetStepMoveDirection()
        {
            // Takes movement inputs for the start of the step then keep them until the end of the step
            if (!_isStepping)
            {
                _currentStepMovementInput = _stepMovementInput;
            }
            else
            {
                _stepMovementInput = _currentStepMovementInput;
            }

            // Set step direction
            if (_cameraHandler.CurrentLockTarget == null)
            {
                _moveDirection = _cameraObject.forward * _stepMovementInput.y;
                _moveDirection += _cameraObject.right * _stepMovementInput.x;
            }
            // Step when locking
            else
            {
                Vector2 normalizedMovementInput = _stepMovementInput.normalized;
                Vector3 stepTransformForward = (_cameraHandler.CurrentLockTarget.transform.position - _characterTransform.position).normalized;
                Vector3 stepTransformRight = -Vector3.Cross(stepTransformForward, Vector3.up);

                // Free direction step
                if (Mathf.Abs(normalizedMovementInput.y) <= _OrthogonalStepInputThreshold &&
                    Mathf.Abs(normalizedMovementInput.x) <= _OrthogonalStepInputThreshold)
                {
                    _moveDirection = _cameraObject.forward * _stepMovementInput.y;
                    _moveDirection += _cameraObject.right * _stepMovementInput.x;
                }
                // Front and back step
                else if (Mathf.Abs(normalizedMovementInput.y) > _OrthogonalStepInputThreshold)
                {
                    _moveDirection = stepTransformForward * _stepMovementInput.y;
                }
                // Side step
                else if (Mathf.Abs(normalizedMovementInput.x) > _OrthogonalStepInputThreshold)
                {
                    _moveDirection = stepTransformForward * _AntiSpiralConstant;
                    _moveDirection += stepTransformRight * _stepMovementInput.x;
                }
            }

            _moveDirection.y = 0;
        }

        /// <summary>
        /// Executes step action with the correct parameters and play its animation
        /// </summary>
        private void ExecuteStep()
        {
            // If the player inputs a direction and the character is not already stepping, rotate character in the direction and make step
            if ((_moveDirection != Vector3.zero && !_isStepping) ||
                (_isStepping && _cameraHandler.CurrentLockTarget != null) ||
                (_isRecoveringFromStep && _isStepping))
            {
                if (_cameraHandler.CurrentLockTarget == null)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(_moveDirection);
                    _characterTransform.rotation = lookRotation;
                }

                // If recovering from step, slow down step power by a factor of 2
                if (!_isRecoveringFromStep)
                {
                    _horizontalVelocity = Mathf.Sqrt(_stepPower * _StepPowerMultiplier) * _moveDirection.normalized;
                }
                else
                {
                    _horizontalVelocity = Mathf.Sqrt(_stepPower / 2 * _StepPowerMultiplier) * _moveDirection.normalized;
                }

                // Start step
                if (!_isStepping)
                {
                    _isDashing = false;
                    _isStepping = true;
                    _animatorHandler.PlayAnimation(_animatorHandler.StepString, true);
                }
            }
        }

        /// <summary>
        /// Resets front, back, side step variables to authorize another step
        /// </summary>
        private void ResetStep()
        {
            _stepFrameCount = 1;
            _isStepFrameCountStarted = false;
            _animatorHandler.PlayAnimation(_animatorHandler.StepString, false);
            _horizontalVelocity = Vector3.zero;
            _stepMovementInput = Vector2.zero;
            _currentStepMovementInput = Vector2.zero;
            _isStepFrameCountStarted = false;
            _isRecoveringFromStep = false;
            _isStepping = false;
        }

        /// <summary>
        /// Makes the character do a homing dash
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleDash(float delta)
        {
            if ((_inputHandler.DashFlag && _cameraHandler.CurrentLockTarget != null) ||
                _isDashing ||
                _isDashFrameCountStarted)
            {
                // Start count for startup
                if (!_isDashFrameCountStarted)
                {
                    _isStepping = false;
                    _isDashFrameCountStarted = true;
                }
                // After startup
                else if (_isDashFrameCountStarted && _dashFrameCount > _dashStartupFrameNumber &&
                    _dashFrameCount > _dashStartupFrameNumber /*<= _dashStartupFrameNumber + _dashActiveFrameNumber + _dashRecoveryFrameNumber*/)
                {
                    Vector3 _moveDirection = (_cameraHandler.CurrentLockTarget.transform.position - _characterTransform.position).normalized;

                    if (_isGrounded)
                    {
                        _moveDirection.y = -0.15f;
                    }

                    _dashVelocity = Mathf.Sqrt(_dashPower * _DashPowerMultiplier) * _moveDirection.normalized;

                    _isDashing = true;

                    // Stop homing dash when near lock target => TODO: replace by collider collision with floor, wall and other entities
                    float distance = Vector3.Distance(_cameraHandler.CurrentLockTarget.transform.position, _characterTransform.position);

                    if (distance < 1.8f || _isStepping)
                    {
                        // TODO: move this after recovery when handled
                        _isDashing = false;
                        _dashFrameCount = 1;
                        _isDashFrameCountStarted = false;
                    }
                }
                // After recovery frames, reset dash
                //else if (_isDashFrameCountStarted && _dashFrameCount > _dashStartupFrameNumber + _dashActiveFrameNumber + _dashRecoveryFrameNumber)
                //{
                    // TODO: ResetDash after no hurtbox collision
                //}
            }

            // Increment frame count
            if (_isDashFrameCountStarted)
            {
                _dashFrameCount += 1;
            }
        }

        /// <summary>
        /// Checks if the character is grounded
        /// </summary>
        public void HandleGroundedCheck()
        {
            // Set sphere position near character feet, with offset
            Vector3 spherePosition = new(transform.position.x, transform.position.y - _groundedOffset,
                transform.position.z);

            // Check if the sphere touch the ground
            _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        #endregion
    }
}