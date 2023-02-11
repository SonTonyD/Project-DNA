using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class PlayerInputHandler : InputHandler
    {
        private PlayerControl _inputActions;


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

        public override float GetHorizontalInput()
        {
            return _movementInput.x;
        }

        public override float GetVerticalInput()
        {
            return _movementInput.y;
        }

        public override bool GetAInput()
        {
            return _inputActions.PlayerActions.Jump.triggered;
        }

        public override bool GetRsInput()
        {
            return _inputActions.PlayerActions.Lock.triggered;
        }

        public override bool GetRsLeftInput()
        {
            return _inputActions.PlayerMovement.LockLeft.triggered;
        }

        public override bool GetRsRightInput()
        {
            return _inputActions.PlayerMovement.LockRight.triggered;
        }

        public override bool GetGuardInput()
        {
            return _inputActions.PlayerActions.Guard.IsPressed();
        }

        public override bool GetAttackInput()
        {
            return _inputActions.PlayerActions.Attack.triggered;
        }
    }
}


