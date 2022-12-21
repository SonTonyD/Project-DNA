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

        void Start()
        {
            _inputHandler = GetComponent<InputHandler>();
            _anim = GetComponentInChildren<Animator>();
            _cameraHandler = CameraHandler.singleton;
            _playerMovement = GetComponent<PlayerMovement>();

            SetCharacterHeightFromModel();
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
            _playerMovement.HandleMovement(delta);
        }

        private void SetCharacterHeightFromModel()
        {
            CharacterController controller = _playerMovement.GetComponent<CharacterController>();
            SkinnedMeshRenderer mesh = transform.GetComponentInChildren<SkinnedMeshRenderer>();
            controller.height = mesh.bounds.center.y + mesh.bounds.extents.y - _HeightComputeConstant;
            float controllerCenterY = (mesh.bounds.center.y + mesh.bounds.extents.y) / 2;
            controller.center = new Vector3(0, controllerCenterY, 0);
        }
    }
}

