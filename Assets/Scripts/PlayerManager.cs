using UnityEngine;


namespace DNA
{
    public class PlayerManager : CharacterManager
    {
        private InputHandler _inputHandler;
        private Animator _animator;
        private CameraHandler _cameraHandler;
        private PlayerMovement _playerMovement;
        private const float _HeightComputeConstant = 0.15f;
        private const float _DefaultSlopeLimit = 45f;
        private const float _DefaultStepOffset = 0.1f;
        private const float _DefaultSkinWidth = 0.08f;
        private const float _DefaultMinMoveDistance = 0.001f;
        private const float _DefaultRadius = 0.3f;

        
        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            _animator = GetComponentInChildren<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();

            InitializeCharacterController();
        }

        private void Start()
        {
            _cameraHandler = CameraHandler.singleton;
        }

        private void Update()
        {
            // Time between frames for all the program
            float delta = Time.deltaTime;

            // Camera control and movements
            if (_cameraHandler != null)
            {
                _cameraHandler.FollowTarget(delta);
                _cameraHandler.HandleCameraRotation(delta, _inputHandler.MouseX, _inputHandler.MouseY);
            }

            // Character control and movements

            _playerMovement.groundCheck.HandleGroundedCheck();
            _playerMovement.step.HandleStep();
            _playerMovement.dash.HandleDash();
            _playerMovement.jump.HandleJump(delta);
            _playerMovement.walkAndRun.HandleMovements(delta);
        }

        /// <summary>
        /// Sets up the Character Controller with default constants
        /// </summary>
        /// <remarks>
        /// The height computation is temporary because we will use models with known constant sizes
        /// </remarks>
        private void InitializeCharacterController()
        {
            CharacterController controller = _playerMovement.GetComponent<CharacterController>();

            #region Height Computation
            SkinnedMeshRenderer mesh = transform.GetComponentInChildren<SkinnedMeshRenderer>();
            controller.height = mesh.bounds.center.y + mesh.bounds.extents.y - _HeightComputeConstant;
            float controllerCenterY = (mesh.bounds.center.y + mesh.bounds.extents.y) / 2;
            controller.center = new Vector3(0, controllerCenterY, 0);
            #endregion

            controller.slopeLimit = _DefaultSlopeLimit;
            controller.stepOffset = _DefaultStepOffset;
            controller.skinWidth = _DefaultSkinWidth;
            controller.minMoveDistance = _DefaultMinMoveDistance;
            controller.radius = _DefaultRadius;
        }
    }
}