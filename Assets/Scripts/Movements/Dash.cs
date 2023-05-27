using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Dash : MonoBehaviour
    {
        private PlayerMovementData _movementData;

        /// <summary>
        /// Makes the character do a homing dash
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleDash(float delta)
        {
            if ((_movementData._inputHandler.DashFlag && _movementData._cameraHandler.CurrentLockTarget != null) ||
                _movementData._isDashing ||
                _movementData._isDashFrameCountStarted)
            {
                // Start count for startup
                if (!_movementData._isDashFrameCountStarted)
                {
                    _movementData._isStepping = false;
                    _movementData._isDashFrameCountStarted = true;
                }
                // After startup
                else if (_movementData._isDashFrameCountStarted && _movementData._dashFrameCount > _movementData._dashStartupFrameNumber &&
                    _movementData._dashFrameCount > _movementData._dashStartupFrameNumber /*<= _dashStartupFrameNumber + _dashActiveFrameNumber + _dashRecoveryFrameNumber*/)
                {
                    Vector3 _moveDirection = (_movementData._cameraHandler.CurrentLockTarget.transform.position - _movementData._characterTransform.position).normalized;

                    if (_movementData._isGrounded)
                    {
                        _moveDirection.y = -0.15f;
                    }

                    _movementData._dashVelocity = Mathf.Sqrt(_movementData._dashPower * _movementData._DashPowerMultiplier) * _moveDirection.normalized;

                    _movementData._isDashing = true;

                    // Stop homing dash when near lock target => TODO: replace by collider collision with floor, wall and other entities
                    float distance = Vector3.Distance(_movementData._cameraHandler.CurrentLockTarget.transform.position, _movementData._characterTransform.position);

                    if (distance < 1.8f || _movementData._isStepping)
                    {
                        // TODO: move this after recovery when handled
                        _movementData._isDashing = false;
                        _movementData._dashFrameCount = 1;
                        _movementData._isDashFrameCountStarted = false;
                    }
                }
                // After recovery frames, reset dash
                //else if (_isDashFrameCountStarted && _dashFrameCount > _dashStartupFrameNumber + _dashActiveFrameNumber + _dashRecoveryFrameNumber)
                //{
                // TODO: ResetDash after no hurtbox collision
                //}
            }

            // Increment frame count
            if (_movementData._isDashFrameCountStarted)
            {
                _movementData._dashFrameCount += 1;
            }
        }
    }
}

