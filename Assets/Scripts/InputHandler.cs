using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        public bool a_Input;
        public bool jumpFlag;

        public bool leftTrigger_Input;
        public bool sprintFlag;


        PlayerControl inputActions;
        CameraHandler cameraHandler;

        Vector2 movementInput;
        Vector2 cameraInput;


        private void Start()
        {
            cameraHandler = CameraHandler.singleton;
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.handleCameraRotation(delta, mouseX, mouseY);
            }
        }

        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControl();
                inputActions.PlayerMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
                
            }

            inputActions.Enable();
        }

        public void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
            HandleJumpInput(delta);
            HandleSprintInput(delta);
        }

        private void MoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }

        private void HandleJumpInput(float delta)
        {
            a_Input = inputActions.PlayerActions.Jump.triggered;
            if (a_Input)
            {
                jumpFlag = true;
            }
            else
            {
                jumpFlag = false;
            }

        }

        private void HandleSprintInput(float delta)
        {
            leftTrigger_Input = inputActions.PlayerActions.Sprint.IsPressed();
            if (leftTrigger_Input)
            {
                sprintFlag = true;
            }
            else
            {
                sprintFlag = false;
            }
        }

    }
}

