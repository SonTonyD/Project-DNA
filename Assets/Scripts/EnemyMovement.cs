using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class EnemyMovement : CharacterMovement
    {
        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _enemyInputHandler = GetComponent<EnemyInputHandler>();
            _animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _playerAttacker = GetComponent<EnemyAttacker>();
            _characterTransform = transform;
            _animatorHandler.Initialize();
            _groundLayers = LayerMask.GetMask("Floor");
        }

        public override Vector3 GetMoveDirection()
        {
            Vector3 moveDirection = new Vector3();
            moveDirection = new Vector3(_enemyInputHandler.Horizontal, 0, _enemyInputHandler.Vertical);
            return moveDirection;
        }

        public override Vector3 GetTargetDirection()
        {
            Vector3 targetDirection;
            targetDirection = new Vector3(_enemyInputHandler.Horizontal, 0, _enemyInputHandler.Vertical);
            return targetDirection;
        }
    }
}


