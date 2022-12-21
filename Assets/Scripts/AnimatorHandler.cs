using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DNA
{
    public class AnimatorHandler : MonoBehaviour
    {
        private Animator _anim;
        [SerializeField]
        private int _vertical;
        [SerializeField]
        private int _horizontal;
        [SerializeField]
        private bool _isRotationEnabled;

        private int _jumpID;
        private int _groundedID;
        private int _freeFallID;

        public bool IsRotationEnabled { get => _isRotationEnabled; set => _isRotationEnabled = value; }
        public Animator Anim { get => _anim; set => _anim = value; }


        public void Initialize()
        {
            _isRotationEnabled = true;
            _anim = GetComponent<Animator>();
            _vertical = Animator.StringToHash("Vertical");
            _horizontal = Animator.StringToHash("Horizontal");
            _jumpID = Animator.StringToHash("Jump");
            _groundedID = Animator.StringToHash("Grounded");
            _freeFallID = Animator.StringToHash("FreeFall");
        }

        public void SetJumpAnimation(bool isJumping)
        {
            _anim.SetBool(_jumpID, isJumping);
        }

        public void SetGroundedAnimation(bool isGrounded)
        {
            _anim.SetBool(_groundedID, isGrounded);
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement)
        {
            #region Vertical
            float v = 0;

            if (verticalMovement > 0 && verticalMovement < 0.55f)
            {
                v = 0.5f;
            }
            else if (verticalMovement > 0.55f)
            {
                v = 1;
            }
            else if (verticalMovement < 0 && verticalMovement > -0.55f)
            {
                v = -0.5f;
            }
            else if (verticalMovement < -0.55f)
            {
                v = -1;
            }
            else
            {
                v = 0;
            }
            #endregion

            #region Horizontal
            float h = 0;

            if (horizontalMovement > 0 && horizontalMovement < 0.55f)
            {
                h = 0.5f;
            }
            else if (horizontalMovement > 0.55f)
            {
                h = 1;
            }
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
            {
                h = -0.5f;
            }
            else if (horizontalMovement < -0.55f)
            {
                h = -1;
            }
            else
            {
                h = 0;
            }
            #endregion

            _anim.SetFloat(_vertical, v, 0.1f, Time.deltaTime);
            _anim.SetFloat(_horizontal, h, 0.1f, Time.deltaTime);
        }

        public void PlayerTargetAnimation(string targetAnim, bool isInteracting)
        {
            _anim.applyRootMotion = isInteracting;
            _anim.SetBool("isInteracting", isInteracting);
            _anim.CrossFade(targetAnim, 0.2f);
        }

        public void CanRotate()
        {
            _isRotationEnabled = true;
        }

        public void StopRotate()
        {
            _isRotationEnabled = false;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            
        }

        private void OnLand(AnimationEvent animationEvent)
        {

        }
    }
}