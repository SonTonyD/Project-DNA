using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class EnemyManager : CharacterManager
    {
        private Animator _animator;
        private EnemyMovement _enemyMovement;

        private const float _HeightComputeConstant = 0.15f;
        private const float _DefaultSlopeLimit = 45f;
        private const float _DefaultStepOffset = 0.1f;
        private const float _DefaultSkinWidth = 0.08f;
        private const float _DefaultMinMoveDistance = 0.001f;
        private const float _DefaultRadius = 0.3f;



        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _enemyMovement = GetComponent<EnemyMovement>();

            InitializeCharacterController();
        }

        private void Update()
        {
            // Time between frames for all the program
            float delta = Time.deltaTime;

            // Character control and movements
            _enemyMovement.HandleGroundedCheck();
            _enemyMovement.HandleJump(delta);
            _enemyMovement.HandleStep(delta);
            _enemyMovement.HandleMovements(delta);
            _enemyMovement.HandleAttack(delta);
        }


        private void InitializeCharacterController()
        {
            CharacterController controller = _enemyMovement.GetComponent<CharacterController>();

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