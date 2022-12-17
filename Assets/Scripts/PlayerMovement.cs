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

        [Header("Jump Stats")]
        [SerializeField]
        private bool _isGrounded;
        [SerializeField]
        private LayerMask _GroundLayers;
        [SerializeField]
        private float _GroundedOffset = -0.08f;
        [SerializeField]
        private float _verticalVelocity;
        [SerializeField]
        private float _terminalVelocity = 53.0f;
        [SerializeField]
        private bool _didSecondJump = false;

        [Header("Stats")]
        [SerializeField]
        private float movementSpeed = 8.0f;
        [SerializeField]
        private float rotationSpeed = 10.0f;
        [SerializeField]
        private float jumpHeight = 1.8f;
        [SerializeField]
        private float gravity = -15.0f;
        [SerializeField]
        private float sprintSpeed = 12.0f;


        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _inputHandler = GetComponent<InputHandler>();
            _animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _cameraObject = Camera.main.transform;
            _myTransform = transform;
            _animatorHandler.Initialize();
            _GroundLayers = LayerMask.GetMask("Floor");
        }

        #region Movement
        Vector3 normalVector;
        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = _inputHandler.MoveAmount;

            targetDir = _cameraObject.forward * _inputHandler.Vertical;
            targetDir += _cameraObject.right * _inputHandler.Horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
            {
                targetDir = _myTransform.forward;
            }

            float rs = rotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(_myTransform.rotation, tr, rs * delta);

            _myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta)
        {
            _inputHandler.TickInput(delta);
            _moveDirection = _cameraObject.forward * _inputHandler.Vertical;
            _moveDirection += _cameraObject.right * _inputHandler.Horizontal;
            _moveDirection.Normalize();
            _moveDirection.y = 0;

            float speed = movementSpeed;

            if (_inputHandler.SprintFlag)
            {
                speed = sprintSpeed;
            }

            _moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_moveDirection, normalVector);
            _controller.Move(_moveDirection.normalized * (speed * delta) + new Vector3(0.0f, _verticalVelocity, 0.0f) * delta);

            _animatorHandler.UpdateAnimatorValues(_inputHandler.MoveAmount, 0);

            if (_animatorHandler.IsRotationEnabled)
            {
                HandleRotation(delta);
            }
        }

        public void HandleJumping(float delta)
        {
            _animatorHandler.SetGroundedAnimation(_isGrounded);
            _animatorHandler.SetJumpAnimation(_inputHandler.JumpFlag);

            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f;
            }

            if ((_inputHandler.JumpFlag && _isGrounded) || (_inputHandler.JumpFlag && !_didSecondJump && !_isGrounded))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                
                if (_didSecondJump )
                {
                    _didSecondJump = false;
                    _inputHandler.JumpFlag = false;
                }

                if (_inputHandler.JumpFlag && !_didSecondJump)
                {
                    _didSecondJump = true;
                }
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += gravity * delta;
            }

            if (_animatorHandler.Anim.GetBool("isInteracting"))
            {
                return;
            }
        }

        public void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _GroundedOffset,
                transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        #endregion
    }
}

