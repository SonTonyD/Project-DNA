using System.Collections;
using System.Collections.Generic;
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
        private Vector3 _moveDirection;
        [HideInInspector]
        private Transform _myTransform;
        [HideInInspector]
        private AnimatorHandler _animatorHandler;
        [SerializeField]
        private GameObject _normalCamera;
        [SerializeField]
        private CharacterController _controller;
        [SerializeField]
        private Hitbox _hitbox;


        [Header("Jump Stats")]
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
        private Vector3 _horizontalVelocity = Vector3.zero;
        [SerializeField]
        private float _terminalVelocity = 50.0f;

        [SerializeField]
        private bool _isDodging = false;
        [SerializeField]
        private const float _DodgeAnimationDuration = 1.0f;

        [Header("Stats")]
        [SerializeField]
        private float _movementSpeed = 8.0f;
        [SerializeField]
        private float _rotationSpeed = 10.0f;
        [SerializeField]
        private float _jumpHeight = 2f;
        [SerializeField]
        private float _gravity = -15.0f;
        [SerializeField]
        private float _sprintSpeed = 12.0f;
        [SerializeField]
        private float _speedModulation = 0f;

        private Vector3 _normalVector;

        public CharacterController Controller { get => _controller; set => _controller = value; }


        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _inputHandler = GetComponent<InputHandler>();
            _animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _hitbox = GetComponentInChildren<Hitbox>();
            _cameraObject = Camera.main.transform;
            _myTransform = transform;
            _animatorHandler.Initialize();
            _groundLayers = LayerMask.GetMask("Floor");
        }

        #region Movement

        private void HandleRotation(float delta)
        {
            Vector3 targetDir = _cameraObject.forward * _inputHandler.Vertical;
            targetDir += _cameraObject.right * _inputHandler.Horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
            {
                targetDir = _myTransform.forward;
            }

            Quaternion lookRotation = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(_myTransform.rotation, lookRotation, _rotationSpeed * delta);

            _myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta)
        {
            _inputHandler.TickInput(delta);
            _moveDirection = _cameraObject.forward * _inputHandler.Vertical;
            _moveDirection += _cameraObject.right * _inputHandler.Horizontal;
            _moveDirection.Normalize();
            _moveDirection.y = 0;

            float speed = _movementSpeed;

            if ((Mathf.Abs(_inputHandler.Vertical) > 0.5 && Mathf.Abs(_inputHandler.Horizontal) > 0.5) ||
                Mathf.Max(Mathf.Abs(_inputHandler.Vertical), Mathf.Abs(_inputHandler.Horizontal)) > 0.85)
            {
                _speedModulation = 1;
            }
            else
            {
                _speedModulation = Mathf.Max(Mathf.Abs(_inputHandler.Vertical), Mathf.Abs(_inputHandler.Horizontal));
            }
            _speedModulation = Mathf.Max(_speedModulation, 0.15f);

            if (_inputHandler.SprintFlag)
            {
                speed = _sprintSpeed;
                _speedModulation = 1;
            }

            _moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_moveDirection, _normalVector);
            if (_horizontalVelocity != Vector3.zero)
            {
                _controller.Move(new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z) * delta);
            }
            else
            {
                _controller.Move(_moveDirection.normalized * (speed * _speedModulation * delta) + new Vector3(0.0f, _verticalVelocity, 0.0f) * delta);
            }

            _animatorHandler.UpdateAnimatorValues(_inputHandler.MoveAmount, 0);

            if (_animatorHandler.IsRotationEnabled && _horizontalVelocity == Vector3.zero)
            {
                HandleRotation(delta);
            }
        }

        public void HandleJumping(float delta)
        {
            _animatorHandler.PlayAnimation("Grounded", _isGrounded);
            _animatorHandler.PlayAnimation("Jump", false);

            if (_inputHandler.JumpFlag && _isGrounded)
            {
                _didSecondJump = false;
                _animatorHandler.PlayAnimation("Jump", true);
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
            else if (_inputHandler.JumpFlag && !_isGrounded && !_didSecondJump)
            {
                _didSecondJump = true;
                _animatorHandler.PlayAnimation("Jump", true);
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            if (_isGrounded)
            {
                _didSecondJump = false;
                if (_verticalVelocity < 0)
                {
                    _verticalVelocity = -2f;
                }
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += _gravity * delta;
            }
        }

        public void HandleDodge(float delta)
        {
            if (_inputHandler.DodgeFlag)
            {
                // Invulnerability frames
                // Set animation having exit time and using its root motion to move the player

                _moveDirection = _cameraObject.forward * _inputHandler.MovementInput.y;
                _moveDirection += _cameraObject.right * _inputHandler.MovementInput.x;
                _moveDirection.y = 0;


                if (_moveDirection != Vector3.zero && !_isDodging)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(_moveDirection);
                    _myTransform.rotation = lookRotation;

                    _isDodging = true;
                    _animatorHandler.PlayAnimation("Dodge", true);
                    _horizontalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity) * _moveDirection.normalized;

                    // Wait for dodge animation end to reset dodging variables
                    Invoke(nameof(ResetDodge), _DodgeAnimationDuration);
                }
            }
        }

        private void ResetDodge()
        {
            _animatorHandler.PlayAnimation("Dodge", false);
            _horizontalVelocity = Vector3.zero;
            _isDodging = false;
        }

        public void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new(transform.position.x, transform.position.y - _groundedOffset,
                transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        public void HandleAttack(float delta)
        {
            if (_inputHandler.AttackFlag)
            {
                List<int> activeFrames = new() {6};
                Attack attack = Attack.CreateInstance(activeFrames, 32, _hitbox);
                attack.LaunchAttack();
            }
        }

        #region Enable/Disable Movement

        public void DisableMove()
        {
            _inputHandler.IsMoveDisabled = true;
        }

        public void EnableMove()
        {
            _inputHandler.IsMoveDisabled = false;
        }
        #endregion

        #endregion
    }
}