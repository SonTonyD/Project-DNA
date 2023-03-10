using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{


    public abstract class CharacterMovement : MonoBehaviour
    {

        [Header("References")]
        [SerializeField]
        protected Transform _cameraObject;
        [SerializeField]
        protected InputHandler _inputHandler;
        protected EnemyInputHandler _enemyInputHandler;
        [HideInInspector]
        protected Transform _characterTransform;
        [HideInInspector]
        protected AnimatorHandler _animatorHandler;
        [SerializeField]
        private GameObject _normalCamera;
        [SerializeField]
        protected CharacterController _controller;
        protected PlayerAttacker _playerAttacker;

        private Vector3 _moveDirection;

        [Header("Movement Variables")]
        [SerializeField]
        protected float _movementSpeed = 8.0f;
        [SerializeField]
        protected float _rotationSpeed = 10.0f;
        [SerializeField]
        protected float _jumpHeight = 2f;
        [SerializeField]
        protected float _gravity = -15.0f;
        [SerializeField]
        protected float _sprintSpeed = 12.0f;
        [SerializeField]
        protected float _speedModulation = 0f;

        protected const float _DiagonalInputThreshold = 0.5f;
        protected const float _OrthogonalInputThreshold = 0.85f;
        protected const float _MinimalSpeedModulation = 0.15f;

        [Header("Jump Variables")]
        [SerializeField]
        protected bool _isGrounded;
        [SerializeField]
        protected LayerMask _groundLayers;
        [SerializeField]
        protected float _groundedOffset = -0.08f;
        [SerializeField]
        protected bool _didSecondJump = false;
        [SerializeField]
        protected float _verticalVelocity;
        [SerializeField]
        protected float _terminalVelocity = 50.0f;

        [Header("Step Variables")]
        [SerializeField]
        protected Vector3 _horizontalVelocity = Vector3.zero;
        [SerializeField]
        protected bool _isStepping = false;
        [SerializeField]
        protected float _stepPower = 5f;
        [SerializeField]
        protected float _stepDuration = 0.25f;

        protected const float _stepPowerMultiplier = 100f;

        public CharacterController Controller { get => _controller; set => _controller = value; }



        public abstract Vector3 GetTargetDirection();
        public abstract Vector3 GetMoveDirection();


        /// <summary>
        /// Handles character rotation
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleRotation(float delta)
        {
            Vector3 targetDirection = GetTargetDirection();
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
            _moveDirection = GetMoveDirection();
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
            else
            {
                _controller.Move(_moveDirection.normalized * (speed * _speedModulation * delta) + new Vector3(0.0f, _verticalVelocity, 0.0f) * delta);
            }

            _animatorHandler.UpdateAnimatorMovementValues(_inputHandler.MoveAmount, 0);

            // Rotate character in the correct direction if rotation is enabled and is not stepping
            if (_animatorHandler.IsRotationEnabled && !_isStepping)
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
            if (_inputHandler.JumpFlag && _isGrounded && !_isStepping)
            {
                _didSecondJump = false;
                _animatorHandler.PlayAnimation(_animatorHandler.JumpString, true);
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
            // If the jump flag is true and the character is in air and did not do a second jump, make the character jump
            else if (_inputHandler.JumpFlag && !_isGrounded && !_didSecondJump && !_isStepping)
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
            if (_inputHandler.StepFlag)
            {
                // Set step direction
                _moveDirection = GetMoveDirection();
                _moveDirection.y = 0;

                // If the player inputs a direction and the character is not already stepping, rotate character in the direction and make step
                if (_moveDirection != Vector3.zero && !_isStepping)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(_moveDirection);
                    _characterTransform.rotation = lookRotation;

                    _isStepping = true;
                    _animatorHandler.PlayAnimation(_animatorHandler.StepString, true);
                    _horizontalVelocity = Mathf.Sqrt(_stepPower * _stepPowerMultiplier) * _moveDirection.normalized;

                    // Wait for step duration to end to reset step variables
                    Invoke(nameof(ResetStep), _stepDuration);
                }
            }
        }

        /// <summary>
        /// Resets front, back, side step variables to authorize another step
        /// </summary>
        public void ResetStep()
        {
            _animatorHandler.PlayAnimation(_animatorHandler.StepString, false);
            _horizontalVelocity = Vector3.zero;
            _isStepping = false;
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

        public void HandleAttack(float delta)
        {
            if (_inputHandler.AttackFlag)
            {
                _playerAttacker.HandleLightAttack();
                _animatorHandler.TriggerState("Punching");
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

    }
}


