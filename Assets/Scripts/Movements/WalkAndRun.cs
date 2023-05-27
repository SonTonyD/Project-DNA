using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DNA
{
    public class WalkAndRun : MonoBehaviour
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
        /// Moves the character forward, back, right, left
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleWalkAndRun(float delta)
        {
            _movementData._inputHandler.HandleMovementInputs(delta);

            if (_movementData._cameraHandler.CurrentLockTarget == null)
            {
                _movementData._moveDirection = _movementData._cameraObject.forward * _movementData._inputHandler.Vertical;
                _movementData._moveDirection += _movementData._cameraObject.right * _movementData._inputHandler.Horizontal;
            }
            else
            {
                Vector3 targetDirection = (_movementData._cameraHandler.CurrentLockTarget.transform.position - transform.position).normalized;
                Vector3 perpendicularVector = Vector3.Cross(targetDirection, Vector3.up);
                _movementData._moveDirection = targetDirection * _movementData._inputHandler.Vertical;
                _movementData._moveDirection += -perpendicularVector * _movementData._inputHandler.Horizontal;
            }

            _movementData._moveDirection.Normalize();
            _movementData._moveDirection.y = 0;

            float speed = _movementData._movementSpeed;

            // If the player is moving the left stick at more than 85% of the maximum in any direction, set the speed modulation to 1 (maximal value)
            if ((Mathf.Abs(_movementData._inputHandler.Vertical) > _movementData._DiagonalInputThreshold && Mathf.Abs(_movementData._inputHandler.Horizontal) > _movementData._DiagonalInputThreshold) ||
                Mathf.Max(Mathf.Abs(_movementData._inputHandler.Vertical), Mathf.Abs(_movementData._inputHandler.Horizontal)) > _movementData._OrthogonalInputThreshold)
            {
                _movementData._speedModulation = 1;
            }
            else
            {
                _movementData._speedModulation = Mathf.Max(Mathf.Abs(_movementData._inputHandler.Vertical), Mathf.Abs(_movementData._inputHandler.Horizontal));
            }

            // Set minimal speed modulation to 0.15
            _movementData._speedModulation = Mathf.Max(_movementData._speedModulation, _movementData._MinimalSpeedModulation);

            // If the character is sprinting, set the speed modulation to 1 (maximal value)
            if (_movementData._inputHandler.SprintFlag)
            {
                speed = _movementData._sprintSpeed;
                _movementData._speedModulation = 1;
            }

            _movementData._moveDirection *= speed;

            // Move the character vertically and/or horizontally
            if (_movementData._isStepping)
            {
                _movementData._controller.Move(new Vector3(_movementData._horizontalVelocity.x, _movementData._verticalVelocity, _movementData._horizontalVelocity.z) * delta);
            }
            if (_movementData._isDashing)
            {
                _movementData._controller.Move(_movementData._dashVelocity * delta);
            }
            else
            {
                _movementData._controller.Move(_movementData._moveDirection.normalized * (speed * _movementData._speedModulation * delta) + new Vector3(0.0f, _movementData._verticalVelocity, 0.0f) * delta);
            }

            _movementData._animatorHandler.UpdateAnimatorMovementValues(_movementData._inputHandler.MoveAmount, 0);

            // Rotate character in the correct direction if rotation is enabled and is not stepping
            if (_movementData._animatorHandler.IsRotationEnabled)
            {
                HandleRotation(delta);
            }
        }

        /// <summary>
        /// Handles character rotation
        /// </summary>
        /// <param name="delta">Time between frames</param>
        private void HandleRotation(float delta)
        {
            Vector3 targetDirection;

            if (_movementData._cameraHandler.CurrentLockTarget == null && _movementData._isStepping)
            {
                return;
            }

            if (_movementData._cameraHandler.CurrentLockTarget == null)
            {
                targetDirection = _movementData._cameraObject.forward * _movementData._inputHandler.Vertical;
                targetDirection += _movementData._cameraObject.right * _movementData._inputHandler.Horizontal;
            }
            else if (_movementData._isStepping)
            {
                targetDirection = (_movementData._cameraHandler.CurrentLockTarget.transform.position - transform.position);
                Vector3 perpendicularVector = Vector3.Cross(targetDirection, Vector3.up);
                targetDirection += -perpendicularVector * _movementData._inputHandler.Horizontal;
            }
            else if (_movementData._isDashing)
            {
                targetDirection = (_movementData._cameraHandler.CurrentLockTarget.transform.position - transform.position);
            }
            else
            {
                targetDirection = (_movementData._cameraHandler.CurrentLockTarget.transform.position - transform.position);
                Vector3 perpendicularVector = Vector3.Cross(targetDirection, Vector3.up);
                targetDirection *= _movementData._inputHandler.Vertical;
                targetDirection += -perpendicularVector * _movementData._inputHandler.Horizontal;
            }

            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = _movementData._characterTransform.forward;
            }

            Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(_movementData._characterTransform.rotation, lookRotation, _movementData._rotationSpeed * delta);

            _movementData._characterTransform.rotation = targetRotation;
        }
    }
}

