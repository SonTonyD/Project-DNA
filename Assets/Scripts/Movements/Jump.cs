using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Jump : MonoBehaviour
    {
        private PlayerMovementData _movementData;

        /// <summary>
        /// Makes the character do a jump
        /// </summary>
        /// <param name="delta">Time between frames</param>
        public void HandleJump(float delta)
        {
            _movementData._animatorHandler.PlayAnimation(_movementData._animatorHandler.GroundedString, _movementData._isGrounded);
            _movementData._animatorHandler.PlayAnimation(_movementData._animatorHandler.JumpString, false);

            // If the jump flag is true and the character is grounded, make the character jump
            if (_movementData._inputHandler.JumpFlag && _movementData._isGrounded && !_movementData._isStepping && !_movementData._isDashing)
            {
                _movementData._didSecondJump = false;
                _movementData._animatorHandler.PlayAnimation(_movementData._animatorHandler.JumpString, true);
                _movementData._verticalVelocity = Mathf.Sqrt(_movementData._jumpHeight * -2f * _movementData._gravity);
            }
            // If the jump flag is true and the character is in air and did not do a second jump, make the character jump
            else if (_movementData._inputHandler.JumpFlag && !_movementData._isGrounded && !_movementData._didSecondJump && !_movementData._isStepping && !_movementData._isDashing)
            {
                _movementData._didSecondJump = true;
                _movementData._animatorHandler.PlayAnimation(_movementData._animatorHandler.JumpString, true);
                _movementData._verticalVelocity = Mathf.Sqrt(_movementData._jumpHeight * -2f * _movementData._gravity);
            }

            // If the character is grounded, fix the character to the ground
            if (_movementData._isGrounded)
            {
                _movementData._didSecondJump = false;
                if (_movementData._verticalVelocity < 0)
                {
                    _movementData._verticalVelocity = -2f;
                }
            }

            // Apply gravity by reducing the character vertical velocity if it has vertical velocity
            if (_movementData._verticalVelocity < _movementData._terminalVelocity)
            {
                _movementData._verticalVelocity += _movementData._gravity * delta;
            }
        }
    }
}

