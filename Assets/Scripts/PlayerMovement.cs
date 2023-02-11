using UnityEngine;

namespace DNA
{
    public class PlayerMovement : CharacterMovement
    {

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _inputHandler = GetComponent<InputHandler>();
            _animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _playerAttacker = GetComponent<PlayerAttacker>();
            _cameraObject = Camera.main.transform;
            _characterTransform = transform;
            _animatorHandler.Initialize();
            _groundLayers = LayerMask.GetMask("Floor");
        }

        public override Vector3 GetMoveDirection()
        {
            Vector3 moveDirection = new Vector3();
            moveDirection = _cameraObject.forward * _inputHandler.Vertical;
            moveDirection += _cameraObject.right * _inputHandler.Horizontal;
            return moveDirection;
        }

        public override Vector3 GetTargetDirection()
        {
            Vector3 targetDirection = _cameraObject.forward * _inputHandler.Vertical;
            targetDirection += _cameraObject.right * _inputHandler.Horizontal;
            return targetDirection;
        }

    }
}