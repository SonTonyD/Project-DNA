using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DNA
{
    public class Dash : FrameAction
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
        /// Makes the character do a homing dash
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleDash()
        {
            actionCondition = (_movementData._inputHandler.DashFlag &&
                                    _movementData._cameraHandler.CurrentLockTarget != null) ||
                                _movementData._isDashing ||
                                _movementData._isDashFrameCountStarted;

            SetFrameConditions(
                _movementData._isDashFrameCountStarted,
                _movementData._dashFrameCount,
                _movementData._dashStartupFrameNumber,
                9999,
                2
            );

            ExecuteFrameAction();
        }

        #region FrameAction functions

        protected override void ExecuteStartup()
        {
            _movementData._isStepping = false;
            _movementData._isDashFrameCountStarted = true;
        }

        protected override void ExecuteActive()
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

        protected override void ExecuteRecovery()
        {
            Debug.Log("Recovery not implemented yet for " + this.GetType().Name);
        }

        protected override void ExecuteReset()
        {
            // After recovery frames, reset dash
            // TODO: ResetDash after no hurtbox collision
            _movementData._dashFrameCount = 1;
            _movementData._isDashFrameCountStarted = false;
            _movementData._dashVelocity = Vector3.zero;
            _movementData._isDashing = false;
        }

        protected override void IncrementFrameCount()
        {
            // Increment frame count
            if (_movementData._isDashFrameCountStarted)
            {
                _movementData._dashFrameCount += 1;
            }
        }

        #endregion
    }
}

