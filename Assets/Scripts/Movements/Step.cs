using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DNA
{
    public class Step : MonoBehaviour
    {
        private PlayerMovementData _movementData;
        private PlayerMovement _playerMovement;
        private string _scriptableObjectPath;

        private void Start()
        {
            _playerMovement = GetComponentInParent<PlayerMovement>();
            _scriptableObjectPath = _playerMovement.getPlayerMovementDataPath();
            _movementData = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(_scriptableObjectPath);
        }

        /// <summary>
        /// Makes the character do a front, back, side step
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleStep(float delta)
        {
            if ((_movementData._inputHandler.StepFlag && !_movementData._isStepping) ||
                (_movementData._isStepping && _movementData._cameraHandler.CurrentLockTarget != null) ||
                (_movementData._isStepping && _movementData._isRecoveringFromStep) ||
                (_movementData._isStepFrameCountStarted))
            {
                // Start count for startup
                if (!_movementData._isStepFrameCountStarted)
                {
                    _movementData._stepMovementInput = _movementData._inputHandler.MovementInput;

                    // If movement input too low, do not step
                    if (Mathf.Abs(_movementData._stepMovementInput.x) + Mathf.Abs(_movementData._stepMovementInput.y) > _movementData._MinimalStepMovementInput)
                    {
                        _movementData._isStepFrameCountStarted = true;
                    }
                }
                // After startup
                else if (_movementData._isStepFrameCountStarted && _movementData._stepFrameCount > _movementData._stepStartupFrameNumber &&
                    _movementData._stepFrameCount <= _movementData._stepStartupFrameNumber + _movementData._stepActiveFrameNumber + _movementData._stepRecoveryFrameNumber)
                {
                    // Set recovery bool after active frames
                    if (!_movementData._isRecoveringFromStep && _movementData._stepFrameCount > _movementData._stepStartupFrameNumber + _movementData._stepActiveFrameNumber)
                    {
                        _movementData._isRecoveringFromStep = true;
                    }

                    SetStepMoveDirection();
                    ExecuteStep();
                }
                // After recovery frames, reset step
                else if (_movementData._isStepFrameCountStarted && _movementData._stepFrameCount > _movementData._stepStartupFrameNumber + _movementData._stepActiveFrameNumber + _movementData._stepRecoveryFrameNumber)
                {
                    ResetStep();
                }
            }

            // Increment frame count
            if (_movementData._isStepFrameCountStarted)
            {
                _movementData._stepFrameCount += 1;
            }
        }

        /// <summary>
        /// Computes and sets/updates the step move direction
        /// </summary>
        private void SetStepMoveDirection()
        {
            // Takes movement inputs for the start of the step then keep them until the end of the step
            if (!_movementData._isStepping)
            {
                _movementData._currentStepMovementInput = _movementData._stepMovementInput;
            }
            else
            {
                _movementData._stepMovementInput = _movementData._currentStepMovementInput;
            }

            // Set step direction
            if (_movementData._cameraHandler.CurrentLockTarget == null)
            {
                _movementData._moveDirection = _movementData._cameraObject.forward * _movementData._stepMovementInput.y;
                _movementData._moveDirection += _movementData._cameraObject.right * _movementData._stepMovementInput.x;
            }
            // Step when locking
            else
            {
                Vector2 normalizedMovementInput = _movementData._stepMovementInput.normalized;
                Vector3 stepTransformForward = (_movementData._cameraHandler.CurrentLockTarget.transform.position - _movementData._characterTransform.position).normalized;
                Vector3 stepTransformRight = -Vector3.Cross(stepTransformForward, Vector3.up);

                // Free direction step
                if (Mathf.Abs(normalizedMovementInput.y) <= _movementData._OrthogonalStepInputThreshold &&
                    Mathf.Abs(normalizedMovementInput.x) <= _movementData._OrthogonalStepInputThreshold)
                {
                    _movementData._moveDirection = _movementData._cameraObject.forward * _movementData._stepMovementInput.y;
                    _movementData._moveDirection += _movementData._cameraObject.right * _movementData._stepMovementInput.x;
                }
                // Front and back step
                else if (Mathf.Abs(normalizedMovementInput.y) > _movementData._OrthogonalStepInputThreshold)
                {
                    _movementData._moveDirection = stepTransformForward * _movementData._stepMovementInput.y;
                }
                // Side step
                else if (Mathf.Abs(normalizedMovementInput.x) > _movementData._OrthogonalStepInputThreshold)
                {
                    _movementData._moveDirection = stepTransformForward * _movementData._AntiSpiralConstant;
                    _movementData._moveDirection += stepTransformRight * _movementData._stepMovementInput.x;
                }
            }

            _movementData._moveDirection.y = 0;
        }

        /// <summary>
        /// Executes step action with the correct parameters and play its animation
        /// </summary>
        private void ExecuteStep()
        {
            // If the player inputs a direction and the character is not already stepping, rotate character in the direction and make step
            if ((_movementData._moveDirection != Vector3.zero && !_movementData._isStepping) ||
                (_movementData._isStepping && _movementData._cameraHandler.CurrentLockTarget != null) ||
                (_movementData._isRecoveringFromStep && _movementData._isStepping))
            {
                if (_movementData._cameraHandler.CurrentLockTarget == null)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(_movementData._moveDirection);
                    _movementData._characterTransform.rotation = lookRotation;
                }

                // If recovering from step, slow down step power by a factor of 2
                if (!_movementData._isRecoveringFromStep)
                {
                    _movementData._horizontalVelocity = Mathf.Sqrt(_movementData._stepPower * _movementData._StepPowerMultiplier) * _movementData._moveDirection.normalized;
                }
                else
                {
                    _movementData._horizontalVelocity = Mathf.Sqrt(_movementData._stepPower / 2 * _movementData._StepPowerMultiplier) * _movementData._moveDirection.normalized;
                }

                // Start step
                if (!_movementData._isStepping)
                {
                    _movementData._isDashing = false;
                    _movementData._isStepping = true;
                    _movementData._stepInitialHorizontalValue = _movementData._inputHandler.Horizontal;
                    _movementData._animatorHandler.PlayAnimation(_movementData._animatorHandler.StepString, true);
                }
            }
        }

        /// <summary>
        /// Resets front, back, side step variables to authorize another step
        /// </summary>
        private void ResetStep()
        {
            _movementData._stepFrameCount = 1;
            _movementData._isStepFrameCountStarted = false;
            _movementData._animatorHandler.PlayAnimation(_movementData._animatorHandler.StepString, false);
            _movementData._horizontalVelocity = Vector3.zero;
            _movementData._stepMovementInput = Vector2.zero;
            _movementData._currentStepMovementInput = Vector2.zero;
            _movementData._isStepFrameCountStarted = false;
            _movementData._isRecoveringFromStep = false;
            _movementData._isStepping = false;
        }
    }
}

