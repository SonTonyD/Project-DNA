using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DNA
{
    public class AnimatorHandler : MonoBehaviour
    {
        private Animator _anim;
        private PlayerMovement _playerMovement;
        [SerializeField]
        private int _vertical;
        [SerializeField]
        private int _horizontal;
        [SerializeField]
        private bool _isRotationEnabled;
        [SerializeField]
        private bool _isUsingRootMotion;

        private const string _JumpString = "Jump";
        private const string _GroundedString = "Grounded";
        private const string _DodgeString = "Dodge";

        private int _jumpID;
        private int _groundedID;
        private int _dodgeID;

        public bool IsRotationEnabled { get => _isRotationEnabled; set => _isRotationEnabled = value; }
        public Animator Anim { get => _anim; set => _anim = value; }


        public void Initialize()
        {
            _isRotationEnabled = true;
            _anim = GetComponent<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();

            _vertical = Animator.StringToHash("Vertical");
            _horizontal = Animator.StringToHash("Horizontal");
            _jumpID = Animator.StringToHash("Jump");
            _groundedID = Animator.StringToHash("Grounded");
            _dodgeID = Animator.StringToHash("Dodge");
        }

        public void PlayAnimation(string animationName, bool activation, bool isUsingRootMotion = false)
        {
            switch (animationName)
            {
                case _JumpString:
                    _anim.SetBool(_jumpID, activation);
                    break;
                case _GroundedString:
                    _anim.SetBool(_groundedID, activation);
                    break;
                case _DodgeString:
                    _anim.SetBool(_dodgeID, activation);
                    break;
                default:
                    Debug.Log("No matching animation");
                    break;
            }
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