using UnityEngine;

namespace DNA
{
	public class BasicRigidBodyPush : MonoBehaviour
	{
		[SerializeField]
		private LayerMask _pushLayers;
		
		[SerializeField]
		private bool _isPushEnabled;
		[Range(0.5f, 5f)] public float strength = 1.1f;

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (_isPushEnabled) PushRigidBodies(hit);
		}

		private void PushRigidBodies(ControllerColliderHit hit)
		{
			// make sure we hit a non kinematic rigidbody
			Rigidbody rigidBody = hit.collider.attachedRigidbody;
			if (rigidBody == null || rigidBody.isKinematic) return;

			// make sure we only push desired layer(s)
			var bodyLayerMask = 1 << rigidBody.gameObject.layer;
			if ((bodyLayerMask & _pushLayers.value) == 0) return;

			// We dont want to push objects below us
			if (hit.moveDirection.y < -0.3f) return;

			// Calculate push direction from move direction, horizontal motion only
			Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

			// Apply the push and take strength into account
			rigidBody.AddForce(pushDirection * strength, ForceMode.Impulse);
		}
	}
}