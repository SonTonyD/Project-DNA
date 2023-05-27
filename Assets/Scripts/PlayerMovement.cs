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

        public readonly float _StepPowerMultiplier = 100f;
        public readonly float _OrthogonalStepInputThreshold = 0.9f;
        public readonly float _AntiSpiralConstant = 0.0425f;
        public readonly float _MinimalStepMovementInput = 0.5f;

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