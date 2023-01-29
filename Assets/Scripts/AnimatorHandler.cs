using UnityEngine;


namespace DNA
{
    public class AnimatorHandler : MonoBehaviour
    {
        private Animator _animator;
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
        private const string _StepString = "Step";

        private int _animatorJumpID;
        private int _animatorGroundedID;
        private int _animatorStepID;

        public Animator Animator { get => _animator; set => _animator = value; }
        public bool IsRotationEnabled { get => _isRotationEnabled; set => _isRotationEnabled = value; }
        public string JumpString => _JumpString;
        public string StepString => _StepString;
        public string GroundedString => _GroundedString;


        public void Initialize()
        {
            _isRotationEnabled = true;
            _animator = GetComponent<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();

            _vertical = Animator.StringToHash("Vertical");
            _horizontal = Animator.StringToHash("Horizontal");
            _animatorJumpID = Animator.StringToHash("Jump");
            _animatorGroundedID = Animator.StringToHash("Grounded");
            _animatorStepID = Animator.StringToHash("Step");
        }

        /// <summary>
        /// Changes the animator boolean value corresponding to the given animation name string
        /// </summary>
        /// <param name="animationName">Name of the animation to execute or stop</param>
        /// <param name="activation">Execute or stop the given animation</param>
        public void PlayAnimation(string animationName, bool activation, bool isUsingRootMotion = false)
        {
            switch (animationName)
            {
                case _JumpString:
                    _animator.SetBool(_animatorJumpID, activation);
                    break;
                case _GroundedString:
                    _animator.SetBool(_animatorGroundedID, activation);
                    break;
                case _StepString:
                    _animator.SetBool(_animatorStepID, activation);
                    break;
                default:
                    Debug.Log("No matching animation");
                    break;
            }
        }

        /// <summary>
        /// Updates the animator movement values to make the character idle, walk or run
        /// </summary>
        /// <param name="verticalMovement">Value for up and down movements</param>
        /// <param name="horizontalMovement">Value for forward, back, right, left movements</param>
        public void UpdateAnimatorMovementValues(float verticalMovement, float horizontalMovement)
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

            _animator.SetFloat(_vertical, v, 0.1f, Time.deltaTime);
            _animator.SetFloat(_horizontal, h, 0.1f, Time.deltaTime);
        }

        /// <summary>
        /// Enables character rotation
        /// </summary>
        public void EnableRotation()
        {
            _isRotationEnabled = true;
        }

        /// <summary>
        /// Disables character rotation
        /// </summary>
        public void DisableRotation()
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