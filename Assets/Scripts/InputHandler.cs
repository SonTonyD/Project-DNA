using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField]
        private float _horizontal;
        [SerializeField]
        private float _vertical;
        [SerializeField]
        private float _moveAmount;
        [SerializeField]
        private float _mouseX;
        [SerializeField]
        private float _mouseY;

        [SerializeField]
        private bool _a_Input;
        [SerializeField]
        private bool _jumpFlag;

        [SerializeField]
        private bool _leftTrigger_Input;
        [SerializeField]
        private bool _sprintFlag;

        private PlayerControl _inputActions;

        private Vector2 _movementInput;
        private Vector2 _cameraInput;

        public float Horizontal { get => _horizontal; set => _horizontal = value; }
        public float Vertical { get => _vertical; set => _vertical = value; }
        public bool SprintFlag { get => _sprintFlag; set => _sprintFlag = value; }
        public bool JumpFlag { get => _jumpFlag; set => _jumpFlag = value; }
        public float MoveAmount { get => _moveAmount; set => _moveAmount = value; }
        public float MouseX { get => _mouseX; set => _mouseX = value; }
        public float MouseY { get => _mouseY; set => _mouseY = value; }

        private void Update()
        {

        }

        public void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerControl();
                _inputActions.PlayerMovement.Movement.performed += inputActions => _movementInput = inputActions.ReadValue<Vector2>();
                _inputActions.PlayerMovement.Camera.performed += i => _cameraInput = i.ReadValue<Vector2>();
            }

            _inputActions.Enable();
        }

        public void OnDisable()
        {
            _inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
            HandleJumpInput(delta);
            HandleSprintInput(delta);
        }

        private void MoveInput(float delta)
        {
            _horizontal = _movementInput.x;
            _vertical = _movementInput.y;
            _moveAmount = Mathf.Clamp01(Mathf.Abs(_horizontal) + Mathf.Abs(_vertical));
            _mouseX = _cameraInput.x;
            _mouseY = _cameraInput.y;
        }

        private void HandleJumpInput(float delta)
        {
            _a_Input = _inputActions.PlayerActions.Jump.triggered;
            if (_a_Input)
            {
                _jumpFlag = true;
            }
            else
            {
                _jumpFlag = false;
            }
        }

        private void HandleSprintInput(float delta)
        {
            _leftTrigger_Input = _inputActions.PlayerActions.Sprint.IsPressed();
            if (_leftTrigger_Input)
            {
                _sprintFlag = true;
            }
            else
            {
                _sprintFlag = false;
            }
        }
    }
}

