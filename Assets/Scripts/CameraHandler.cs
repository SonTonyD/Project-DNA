using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CameraHandler : MonoBehaviour
    {
        [SerializeField]
        private Transform _targetTransform;
        [SerializeField]
        private Transform _cameraTransform;
        [SerializeField]
        private Transform _cameraPivotTransform;
        private Transform _myTransform;
        private LayerMask _ignoreLayers;
        private Vector3 _cameraFollowVelocity = Vector3.zero;

        public static CameraHandler singleton;

        [SerializeField]
        private float _horizontalSensitivity = 0.01f;
        [SerializeField]
        private float _followSpeed = 0.1f;
        [SerializeField]
        private float _verticalSensitivity = 0.01f;

        private float _defaultPosition;
        private float _lookAngle;
        private float _pivotAngle;
        [SerializeField]
        private float _minimumPivot = -30.0f;
        [SerializeField]
        private float _maximumPivot = 70.0f;


        private void Awake()
        {
            singleton = this;
            _myTransform = transform;
            _defaultPosition = _cameraTransform.localPosition.z;
            _ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
            _cameraTransform.position = new Vector3(0, 0, -3.5f);
            _cameraTransform.rotation = Quaternion.Euler(new Vector3(-8, 0, 0));
            _cameraPivotTransform.position = new Vector3(0, 1.54f, -0.025f);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(_myTransform.position, _targetTransform.position, ref _cameraFollowVelocity, delta / _followSpeed);
            _myTransform.position = targetPosition;
        }

        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
        {
            _lookAngle += (mouseXInput * _horizontalSensitivity) / delta;
            _pivotAngle += (mouseYInput * _verticalSensitivity) / delta;
            _pivotAngle = Mathf.Clamp(_pivotAngle, _minimumPivot, _maximumPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = _lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            _myTransform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = _pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            _cameraPivotTransform.localRotation = targetRotation;
        }
    }
}


