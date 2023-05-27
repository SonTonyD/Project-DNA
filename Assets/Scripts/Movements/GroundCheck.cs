using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DNA
{
    public class GroundCheck : MonoBehaviour
    {
        private PlayerMovementData _movementData;
        private PlayerMovement _playerMovement;
        private string _scriptableObjectPath;

        private void Start()
        {
            _playerMovement = GetComponentInParent<PlayerMovement>();
            _scriptableObjectPath = _playerMovement.getPlayerMovementDataPath();
            _movementData = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(_scriptableObjectPath);
        }

        /// <summary>
        /// Checks if the character is grounded
        /// </summary>
        public void HandleGroundedCheck()
        {
            // Set sphere position near character feet, with offset
            Vector3 spherePosition = new(transform.position.x, transform.position.y - _movementData._groundedOffset,
                transform.position.z);

            // Check if the sphere touch the ground
            _movementData._isGrounded = Physics.CheckSphere(spherePosition, _movementData._controller.radius, _movementData._groundLayers,
                QueryTriggerInteraction.Ignore);
        }
    }
}


