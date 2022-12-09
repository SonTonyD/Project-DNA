using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CameraHandler : MonoBehaviour
    {
        public Transform targetTransform;
        public Transform cameraTransform;
        public Transform cameraPivotTransform;
        private Transform myTransform;
        private LayerMask ignoreLayers;
        private Vector3 cameraFollowVelocity = Vector3.zero;

        public static CameraHandler singleton;

        public float horizontalSensitivity = 0.05f;
        public float followSpeed = 0.1f;
        public float verticalSensibility = 0.05f;

        private float defaultPosition;
        private float lookAngle;
        private float pivotAngle;
        public float minimumPivot = -30.0f;
        public float maximumPivot = 70.0f;


        private void Awake()
        {
            singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
            cameraTransform.position = new Vector3(0, 0, -3.5f);
            cameraTransform.rotation = Quaternion.Euler(new Vector3(-8, 0, 0));
            cameraPivotTransform.position = new Vector3(0, 1.54f, -0.025f);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            myTransform.position = targetPosition;
        }

        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
        {
            lookAngle += (mouseXInput * horizontalSensitivity) / delta;
            pivotAngle += (mouseYInput * verticalSensibility) / delta;
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation;
        }
    }
}


