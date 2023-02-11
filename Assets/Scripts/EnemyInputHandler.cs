using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class EnemyInputHandler : MonoBehaviour
    {
        private PlayerControl _inputActions;

        [Header("Enemy Inputs")]
        [SerializeField]
        private float _horizontal;
        [SerializeField]
        private float _vertical;
        [SerializeField]
        private bool _jumpInput;
        [SerializeField]
        private bool _guardInput;

        [Header("Movements and Camera")]
        [SerializeField]
        private CameraHandler _cameraHandler;
        [SerializeField]
        private float _moveAmount;
        [SerializeField]
        private float _mouseX;
        [SerializeField]
        private float _mouseY;
        [SerializeField]
        private bool _isMoveDisabled;

        private Vector2 _movementInput;
        private Vector2 _cameraInput;

        [Header("Jump")]
        private bool _jumpFlag;
        private bool _attackFlag;

        [Header("Sprint")]
        [SerializeField]
        private bool _isTimerStarted;
        [SerializeField]
        private float _walkStartTime;
        [SerializeField]
        private const float _SprintStartDuration = 5f;
        [SerializeField]
        private bool _sprintFlag;

        [Header("Guard and Step")]
        [SerializeField]
        private bool _stepFlag;

        [Header("Lock")]
        [SerializeField]
        private bool _rsInput;
        [SerializeField]
        private bool _rsRightInput;
        [SerializeField]
        private bool _rsLeftInput;
        [SerializeField]
        private bool _lockFlag;
        [SerializeField]
        private bool _lockRightFlag;
        [SerializeField]
        private bool _lockLeftFlag;

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
        public bool GuardInput { get => _guardInput; set => _guardInput = value; }

        public void HandleMovementInputs(float delta)
        {
            HandleMoveInput();
            HandleJumpInput();
            HandleSprintInput();
            //HandleLockInput(delta);
            HandleGuardAndStepInput();
            //HandleAttackInput(delta);
        }

        private void HandleAttackInput(float delta)
        {
            throw new NotImplementedException();
        }

        private void HandleGuardAndStepInput()
        {
            // If the player is pressing the right trigger and is moving the left stick, set the step flag to true and disable its movements
            if (_guardInput)
            {
                _isMoveDisabled = true;
                if (Mathf.Abs(_horizontal) + Mathf.Abs(_vertical) > 0.0f)
                {
                    _stepFlag = true;
                }
                else if (Mathf.Abs(_horizontal) + Mathf.Abs(_vertical) == 0.0f)
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

        private void HandleLockInput(float delta)
        {
            throw new NotImplementedException();
        }

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

        private void HandleJumpInput()
        {
            // If the player has pushed the A button down, set the jump flag to true
            if (_jumpInput)
            {
                _jumpFlag = true;
            }
            else
            {
                _jumpFlag = false;
            }
        }

        private void HandleMoveInput()
        {

            // Character movements if enabled
            if (!_isMoveDisabled)
            {
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
    }
}

