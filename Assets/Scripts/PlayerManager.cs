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


        void Start()
        {
            _inputHandler = GetComponent<InputHandler>();
            _anim = GetComponentInChildren<Animator>();
            _cameraHandler = CameraHandler.singleton;
            _playerMovement = GetComponent<PlayerMovement>();
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
    }
}

