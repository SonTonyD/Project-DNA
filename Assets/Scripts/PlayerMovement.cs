using UnityEngine;

namespace DNA
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _cameraObject;
        [SerializeField]
        private InputHandler _inputHandler;
        [HideInInspector]
        private Transform _characterTransform;
        [HideInInspector]
        private AnimatorHandler _animatorHandler;
        [SerializeField]
        private GameObject _normalCamera;
        [SerializeField]
        private CharacterController _controller;
        [SerializeField]
        private CameraHandler _cameraHandler;

        private Vector3 _moveDirection;

        [Header("Movement Variables")]
        [SerializeField]
        private float _movementSpeed = 8.0f;
        [SerializeField]
        private float _rotationSpeed = 15.0f;
        [SerializeField]
        private float _jumpHeight = 2f;
        [SerializeField]
        private float _gravity = -15.0f;
        [SerializeField]
        private float _sprintSpeed = 12.0f;
        [SerializeField]
        private float _speedModulation = 0f;

        private const float _DiagonalInputThreshold = 0.5f;
        private const float _OrthogonalInputThreshold = 0.85f;
        private const float _MinimalSpeedModulation = 0.15f;

        [Header("Jump Variables")]
        [SerializeField]
        private bool _isGrounded;
        [SerializeField]
        private LayerMask _groundLayers;
        [SerializeField]
        private float _groundedOffset = -0.08f;
        [SerializeField]
        private bool _didSecondJump = false;
        [SerializeField]
        private float _verticalVelocity;
        [SerializeField]
        private float _terminalVelocity = 50.0f;

        [Header("Step Variables")]
        [SerializeField]
        private Vector3 _horizontalVelocity = Vector3.zero;
        [SerializeField]
        private bool _isStepping = false;
        [SerializeField]
        private float _stepPower = 5f;
        [SerializeField]
        private int _stepStartupFrameNumber = 4;
        [SerializeField]
        private int _stepActiveFrameNumber = 15;
        [SerializeField]
        private int _stepRecoveryFrameNumber = 10;
        [SerializeField]
        private bool _isRecoveringFromStep = false;
        [SerializeField]
        private int _stepFrameCount = 1;
        [SerializeField]
        private bool _isStepFrameCountStarted = false;
        [SerializeField]
        private Vector2 _stepMovementInput;
        [SerializeField]
        private Vector2 _currentStepMovementInput;

        private const float _StepPowerMultiplier = 100f;
        private const float _OrthogonalStepInputThreshold = 0.9f;
        private const float _AntiSpiralConstant = 0.0425f;
        private const float _MinimalStepMovementInput = 0.5f;

        [Header("Dash Variables")]
        [SerializeField]
        private bool _isDashing = false;
        [SerializeField]
        private Vector3 _dashVelocity = Vector3.zero;
        [SerializeField]
        private float _dashPower = 6f;
        [SerializeField]
        private int _dashStartupFrameNumber = 8;
        //[SerializeField]
        //private int _dashRecoveryFrameNumber = 20;
        [SerializeField]
        private int _dashFrameCount = 1;
        [SerializeField]
        private bool _isDashFrameCountStarted = false;

        private const float _DashPowerMultiplier = 100f;

        public CharacterController Controller { get => _controller; set => _controller = value; }


        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _inputHandler = GetComponent<InputHandler>();
            _animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _cameraObject = Camera.main.transform;
            _cameraHandler = CameraHandler.singleton;
            _characterTransform = transform;
            _animatorHandler.Initialize();
            _groundLayers = LayerMask.GetMask("Floor");
        }

        #region Movement

        /// <summary>
        /// Handles character rotation
        /// </summary>
        /// <param name="delta">Time between frames</param>
        private void HandleRotation(float delta)
        {
            Vector3 targetDirection;

            if (_cameraHandler.CurrentLockTarget == null && _isStepping)
            {
                return;
            }

            if (_cameraHandler.CurrentLockTarget == null)
            {
                targetDirection = _cameraObject.forward * _inputHandler.Vertical;
                targetDirection += _cameraObject.right * _inputHandler.Horizontal;
            }
            else if (_isStepping)
            {
                targetDirection = (_cameraHandler.CurrentLockTarget.transform.position - transform.position);
                Vector3 perpendicularVector = Vector3.Cross(targetDirection, Vector3.up);
                targetDirection += -perpendicularVector * _inputHandler.Horizontal;
            }
            else if (_isDashing)
            {
                targetDirection = (_cameraHandler.CurrentLockTarget.transform.position - transform.position);
            }
            else
            {
                targetDirection = (_cameraHandler.CurrentLockTarget.transform.position - transform.position);
                Vector3 perpendicularVector = Vector3.Cross(targetDirection, Vector3.up);
                targetDirection *= _inputHandler.Vertical;
                targetDirection += -perpendicularVector * _inputHandler.Horizontal;
            }
            
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = _characterTransform.forward;
            }

            Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(_characterTransform.rotation, lookRotation, _rotationSpeed * delta);

            _characterTransform.rotation = targetRotation;
        }

        /// <summary>
        /// Moves the character forward, back, right, left
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleMovements(float delta)
        {
            _inputHandler.HandleMovementInputs(delta);
            
            if (_cameraHandler.CurrentLockTarget == null)
            {
                _moveDirection = _cameraObject.forward * _inputHandler.Vertical;
                _moveDirection += _cameraObject.right * _inputHandler.Horizontal;
            }
            else
            {
                Vector3 targetDirection = (_cameraHandler.CurrentLockTarget.transform.position - transform.position).normalized;
                Vector3 perpendicularVector = Vector3.Cross(targetDirection, Vector3.up);
                _moveDirection = targetDirection * _inputHandler.Vertical;
                _moveDirection += -perpendicularVector * _inputHandler.Horizontal;
            }

            _moveDirection.Normalize();
            _moveDirection.y = 0;

            float speed = _movementSpeed;

            // If the player is moving the left stick at more than 85% of the maximum in any direction, set the speed modulation to 1 (maximal value)
            if ((Mathf.Abs(_inputHandler.Vertical) > _DiagonalInputThreshold && Mathf.Abs(_inputHandler.Horizontal) > _DiagonalInputThreshold) ||
                Mathf.Max(Mathf.Abs(_inputHandler.Vertical), Mathf.Abs(_inputHandler.Horizontal)) > _OrthogonalInputThreshold)
            {
                _speedModulation = 1;
            }
            else
            {
                _speedModulation = Mathf.Max(Mathf.Abs(_inputHandler.Vertical), Mathf.Abs(_inputHandler.Horizontal));
            }

            // Set minimal speed modulation to 0.15
            _speedModulation = Mathf.Max(_speedModulation, _MinimalSpeedModulation);

            // If the character is sprinting, set the speed modulation to 1 (maximal value)
            if (_inputHandler.SprintFlag)
            {
                speed = _sprintSpeed;
                _speedModulation = 1;
            }

            _moveDirection *= speed;

            // Move the character vertically and/or horizontally
            if (_isStepping)
            {
                _controller.Move(new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z) * delta);
            }
            if (_isDashing)
            {
                _controller.Move(_dashVelocity * delta);
            }
            else
            {
                _controller.Move(_moveDirection.normalized * (speed * _speedModulation * delta) + new Vector3(0.0f, _verticalVelocity, 0.0f) * delta);
            }

            _animatorHandler.UpdateAnimatorMovementValues(_inputHandler.MoveAmount, 0);

            // Rotate character in the correct direction if rotation is enabled and is not stepping
            if (_animatorHandler.IsRotationEnabled)
            {
                HandleRotation(delta);
            }
        }

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