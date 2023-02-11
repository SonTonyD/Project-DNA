using UnityEngine;

namespace DNA
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerControl _inputActions;

        [Header("Movements and Camera")]
        [SerializeField]
        protected CameraHandler _cameraHandler;
        [SerializeField]
        protected float _horizontal;
        [SerializeField]
        protected float _vertical;
        [SerializeField]
        protected float _moveAmount;
        [SerializeField]
        protected float _mouseX;
        [SerializeField]
        protected float _mouseY;
        [SerializeField]
        protected bool _isMoveDisabled;

        protected Vector2 _movementInput;
        protected Vector2 _cameraInput;

        [Header("Jump")]
        [SerializeField]
        protected bool _aInput;
        [SerializeField]
        protected bool _jumpFlag;
        protected bool _attackFlag;

        [Header("Sprint")]
        [SerializeField]
        protected bool _isTimerStarted;
        [SerializeField]
        protected float _walkStartTime;
        [SerializeField]
        protected const float _SprintStartDuration = 5f;
        [SerializeField]
        protected bool _sprintFlag;

        [Header("Guard and Step")]
        [SerializeField]
        protected bool _stepFlag;

        [Header("Lock")]
        [SerializeField]
        protected bool _rsInput;
        [SerializeField]
        protected bool _rsRightInput;
        [SerializeField]
        protected bool _rsLeftInput;
        [SerializeField]
        protected bool _lockFlag;
        [SerializeField]
        protected bool _lockRightFlag;
        [SerializeField]
        protected bool _lockLeftFlag;

        public float Horizontal { get => _horizontal; set => _horizontal = value; }
        public float Vertical { get => _vertical; set => _vertical = value; }
        public float MoveAmount { get => _moveAmount; set => _moveAmount = value; }
        public float MouseX { get => _mouseX; set => _mouseX = value; }
        public float MouseY { get => _mouseY; set => _mouseY = value; }
        public Vector2 MovementInput { get => _movementInput; set => _movementInput = value; }
        public bool JumpFlag { get => _jumpFlag; set => _jumpFlag = value; }
        public bool SprintFlag { get => _sprintFlag; set => _sprintFlag = value; }
        public bool StepFlag { get => _stepFlag; set => _stepFlag = value; }
        public bool LockFlag { get => _lockFlag; set => _lockFlag = value; }
        public bool LockRightFlag { get => _lockRightFlag; set => _lockRightFlag = value; }
        public bool LockLeftFlag { get => _lockLeftFlag; set => _lockLeftFlag = value; }
        public bool AttackFlag { get => _attackFlag; set => _attackFlag = value; }
        public bool IsMoveDisabled { get => _isMoveDisabled; set => _isMoveDisabled = value; }


        private void Start()
        {
            _cameraHandler = FindObjectOfType<CameraHandler>();
        }

        public void OnEnable()
        {
            // Read stick and mouse inputs
            if (_inputActions == null)
            {
                _inputActions = new PlayerControl();
                _inputActions.PlayerMovement.Movement.performed += i => _movementInput = i.ReadValue<Vector2>();
                _inputActions.PlayerMovement.Camera.performed += i => _cameraInput = i.ReadValue<Vector2>();
            }

            _inputActions.Enable();
        }

        public void OnDisable()
        {
            _inputActions.Disable();
        }

        /// <summary>
        /// Handles all movement inputs and sets up the correct movement flags for the Player Movement class
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleMovementInputs(float delta)
        {
            HandleMoveInput();
            HandleJumpInput();
            HandleSprintInput();
            HandleLockInput(delta);
            HandleGuardAndStepInput();
            HandleAttackInput(delta);
        }

        /// <summary>
        /// Handles forward, back, right, left movement inputs
        /// </summary>
        private void HandleMoveInput()
        {
            // Character movements if enabled
            if (!_isMoveDisabled)
            {
                _horizontal = _movementInput.x;
                _vertical = _movementInput.y;
                _moveAmount = Mathf.Clamp01(Mathf.Abs(_horizontal) + Mathf.Abs(_vertical));
            }
            else
            {
                _horizontal = 0;
                _vertical = 0;
                _moveAmount = 0;
            }

            // Camera movements
            _mouseX = _cameraInput.x;
            _mouseY = _cameraInput.y;
        }

        /// <summary>
        /// Handles jump inputs
        /// </summary>
        private void HandleJumpInput()
        {
            _aInput = _inputActions.PlayerActions.Jump.triggered;

            // If the player has pushed the A button down, set the jump flag to true
            if (_aInput)
            {
                _jumpFlag = true;
            }
            else
            {
                _jumpFlag = false;
            }
        }

        /// <summary>
        /// Handles sprint inputs when the player has run continuously for _SprintStartDuration seconds
        /// </summary>
        private void HandleSprintInput()
        {
            // If the move amount is 1 (running), start timer
            if (_moveAmount == 1 && !_isTimerStarted)
            {
                _walkStartTime = Time.time;
                _isTimerStarted = true;
            }
            else if (_moveAmount < 1 && _isTimerStarted)
            {
                _isTimerStarted = false;
            }

            // If the timer exceeds _SprintStartDuration seconds, set the sprint flag to true
            if (_isTimerStarted && Time.time - _walkStartTime >= _SprintStartDuration)
            {
                _sprintFlag = true;
            }
            else
            {
                _sprintFlag = false;
            }
        }

        /// <summary>
        /// Handles lock and switch lock target inputs
        /// </summary>
        /// <param name="delta">Time between frames</param>
        private void HandleLockInput(float delta)
        {
            _rsInput = _inputActions.PlayerActions.Lock.triggered;

            // If the player has pushed the right stick down, set the lock flag to true
            if (_rsInput && !_lockFlag)
            {
                _rsInput = false;

                // Search for lock targets
                _cameraHandler.UpdateLockTargets();

                // Lock on the nearest target
                if (_cameraHandler.NearestLockTarget != null)
                {
                    _cameraHandler.CurrentLockTarget = _cameraHandler.NearestLockTarget;
                    _lockFlag = true;
                }
            }
            else if (_rsInput && _lockFlag)
            {
                _rsInput = false;
                _lockFlag = false;

                // Clear lock targets
                _cameraHandler.ClearLockTargets();
            }

            _rsLeftInput = _inputActions.PlayerMovement.LockLeft.triggered;
            _rsRightInput = _inputActions.PlayerMovement.LockRight.triggered;

            // If the player is locking on a target and has moved the right stick to the left, set the lock left flag to true
            if (_lockFlag && _rsLeftInput)
            {
                _lockLeftFlag = true;
                _rsLeftInput = false;

                // Lock left target
                _cameraHandler.HandleLock();

                if (_cameraHandler.LeftLockTarget != null)
                {
                    _cameraHandler.CurrentLockTarget = _cameraHandler.LeftLockTarget;
                }

                _lockLeftFlag = false;
            }

            // If the player is locking on a target and has moved the right stick to the right, set the lock right flag to true
            if (_lockFlag && _rsRightInput)
            {
                _lockRightFlag = true;
                _rsRightInput = false;

                // Lock right target
                _cameraHandler.HandleLock();

                if (_cameraHandler.RightLockTarget != null)
                {
                    _cameraHandler.CurrentLockTarget = _cameraHandler.RightLockTarget;
                }

                _lockRightFlag = false;
            }
        }

        /// <summary>
        /// Handles guard inputs and front, back, side step inputs
        /// </summary>
        private void HandleGuardAndStepInput()
        {
            // If the player is pressing the right trigger and is moving the left stick, set the step flag to true and disable its movements
            if (_inputActions.PlayerActions.Guard.IsPressed())
            {
                _isMoveDisabled = true;
                if (Mathf.Abs(_movementInput.x) + Mathf.Abs(_movementInput.y) > 0.0f)
                {
                    _stepFlag = true;
                }
                else if (Mathf.Abs(_movementInput.x) + Mathf.Abs(_movementInput.y) == 0.0f)
                {
                    _stepFlag = false;
                }
                _isMoveDisabled = false;
                _isTimerStarted = false; //Disable sprint after a dodge
            }
            else
            {
                _stepFlag = false;
            }
        }

        private void HandleAttackInput(float delta)
        {
            if (_inputActions.PlayerActions.Attack.triggered)
            {
                AttackFlag = true;
            }
            else
            {
                AttackFlag = false;
            }
        }
    }
}