using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DNA
{
    public class PlayerManager : CharacterManager
    {
        private InputHandler _inputHandler;
        private Animator _anim;
        private CameraHandler _cameraHandler;
        private PlayerMovement _playerMovement;
        private const float _HeightComputeConstant = 0.15f;
        private const float _defaultSlopeLimit = 45f;
        private const float _defaultStepOffset = 0.1f;
        private const float _defaultSkinWidth = 0.08f;
        private const float _defaultMinMoveDistance = 0.001f;
        private const float _defaultRadius = 0.2f;


        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            _anim = GetComponentInChildren<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();

            InitializeCharacterController();
        }

        void Start()
        {
            _cameraHandler = CameraHandler.singleton;
        }

        void Update()
        {
            float delta = Time.deltaTime;

            if (_cameraHandler != null)
            {
                _cameraHandler.FollowTarget(delta);
                _cameraHandler.HandleCameraRotation(delta, _inputHandler.MouseX, _inputHandler.MouseY);
                _cameraHandler.UpdateAvailableTargets(delta);
            }

            _playerMovement.GroundedCheck();
            _playerMovement.HandleJumping(delta);
            _playerMovement.HandleDodge(delta);
            _playerMovement.HandleMovement(delta);
        }

        private void InitializeCharacterController()
        {
            CharacterController controller = _playerMovement.GetComponent<CharacterController>();

            #region Height Computation
            SkinnedMeshRenderer mesh = transform.GetComponentInChildren<SkinnedMeshRenderer>();
            controller.height = mesh.bounds.center.y + mesh.bounds.extents.y - _HeightComputeConstant;
            float controllerCenterY = (mesh.bounds.center.y + mesh.bounds.extents.y) / 2;
            controller.center = new Vector3(0, controllerCenterY, 0);
            #endregion

            controller.slopeLimit = _defaultSlopeLimit;
            controller.stepOffset = _defaultStepOffset;
            controller.skinWidth = _defaultSkinWidth;
            controller.minMoveDistance = _defaultMinMoveDistance;
            controller.radius = _defaultRadius;
        }
    }
}