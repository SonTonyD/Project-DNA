using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CameraHandler : MonoBehaviour
    {
        private InputHandler _inputHandler;
        [SerializeField]
        private Transform _targetTransform;
        [SerializeField]
        private Transform _cameraTransform;
        [SerializeField]
        private Transform _cameraPivotTransform;
        private Vector3 _cameraTransformPosition;
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

        private float _targetPosition;
        private float _defaultPosition;
        private float _lookAngle;
        private float _pivotAngle;
        [SerializeField]
        private float _minimumPivot = -30.0f;
        [SerializeField]
        private float _maximumPivot = 70.0f;

        [SerializeField]
        private float _cameraSphereRadius = 0.3f;
        [SerializeField]
        private float _cameraCollisionOffset = 0.2f;
        [SerializeField]
        private float _minimumCameraOffset = 0.5f;
        [SerializeField]
        private float _maximumLockOnDistance = 30;

        private Transform _currentLockOnTarget;
        private List<CharacterManager> _availableTargets = new List<CharacterManager>();
        private Transform _nearestLockOnTarget;
        private Transform _leftLockTarget;
        private Transform _rightLockTarget;

        public Transform CurrentLockOnTarget { get => _currentLockOnTarget; set => _currentLockOnTarget = value; }
        public Transform NearestLockOnTarget { get => _nearestLockOnTarget; set => _nearestLockOnTarget = value; }
        public Transform LeftLockTarget { get => _leftLockTarget; set => _leftLockTarget = value; }
        public Transform RightLockTarget { get => _rightLockTarget; set => _rightLockTarget = value; }

        private void Awake()
        {
            singleton = this;
            _myTransform = transform;
            _cameraTransform.position = new Vector3(0, 0, -3.5f);
            _cameraTransform.rotation = Quaternion.Euler(new Vector3(-8, 0, 0));
            _cameraPivotTransform.position = new Vector3(0, 1.54f, -0.025f);
            _defaultPosition = _cameraTransform.localPosition.z;
            _ignoreLayers = ~(1 << 8 | 1 << 11 | 1 << 10);
            _inputHandler = FindObjectOfType<InputHandler>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(_myTransform.position, _targetTransform.position, ref _cameraFollowVelocity, delta / _followSpeed);
            _myTransform.position = targetPosition;
            HandleCameraCollisions(delta);
        }

        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
        {
            if (_inputHandler.LockOnFlag == false && _currentLockOnTarget == null)
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
            else
            {
                Vector3 dir = _currentLockOnTarget.position - transform.position;
                dir.Normalize();
                dir.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(dir);

                transform.rotation = targetRotation;

                dir = _currentLockOnTarget.position - _cameraPivotTransform.position;
                dir.Normalize();

                targetRotation = Quaternion.LookRotation(dir);
                Vector3 eulerAngle = targetRotation.eulerAngles;
                eulerAngle.y = 0;
                _cameraPivotTransform.localEulerAngles = eulerAngle;
            }
        }

        private void HandleCameraCollisions(float delta)
        {
            _targetPosition = _defaultPosition;
            RaycastHit hit;
            Vector3 direction = _cameraTransform.position - _cameraPivotTransform.position;
            direction.Normalize();

            if (Physics.SphereCast(_cameraPivotTransform.position, _cameraSphereRadius, direction, out hit, Mathf.Abs(_targetPosition), _ignoreLayers))
            {
                float dis = Vector3.Distance(_cameraPivotTransform.position, hit.point);
                _targetPosition = -(dis - _cameraCollisionOffset);
            }

            if (Mathf.Abs(_targetPosition) < _minimumCameraOffset)
            {
                _targetPosition = -_minimumCameraOffset;
            }

            _cameraTransformPosition.z = Mathf.Lerp(_cameraTransform.localPosition.z, _targetPosition, delta / 0.2f);
            _cameraTransform.localPosition = _cameraTransformPosition;
        }

        public void HandleLockOn()
        {
            float shortestDistance = Mathf.Infinity;
            float shortestDistanceOfLeftTarget = Mathf.Infinity;
            float shortestDistanceOfRightTarget = Mathf.Infinity;

            Collider[] colliders = Physics.OverlapSphere(_targetTransform.position, 26);

            for (int i = 0; i < colliders.Length; i++)
            {
                CharacterManager character = colliders[i].GetComponent<CharacterManager>();

                if (character != null)
                {
                    Vector3 lockTargetDirection = character.transform.position - _targetTransform.position;
                    float distanceFromTarget = Vector3.Distance(_targetTransform.position, character.transform.position);
                    float viewableAngle = Vector3.Angle(lockTargetDirection, _cameraTransform.forward);

                    if (character.transform.root != _targetTransform.transform.root && viewableAngle > -50 && viewableAngle < 50 && distanceFromTarget <= _maximumLockOnDistance)
                    { 
                        _availableTargets.Add(character);
                    }
                }
            }

            for (int k = 0; k < _availableTargets.Count; k++)
            {
                float distanceFromTarget = Vector3.Distance(_targetTransform.position, _availableTargets[k].transform.position);

                if (distanceFromTarget < shortestDistance)
                {
                    shortestDistance = distanceFromTarget;
                    _nearestLockOnTarget = _availableTargets[k].LockOnTransform;
                }
                
                if (_inputHandler.LockOnFlag)
                {
                    Vector3 relativeEnemyPosition = _currentLockOnTarget.InverseTransformPoint(_availableTargets[k].transform.position);
                    var distanceFromLeftTarget = _currentLockOnTarget.transform.position.x - _availableTargets[k].transform.position.x;
                    var distanceFromRightTarget = _currentLockOnTarget.transform.position.x + _availableTargets[k].transform.position.x;

                    if (relativeEnemyPosition.x > 0.00 && distanceFromLeftTarget < shortestDistanceOfLeftTarget)
                    {
                        shortestDistanceOfLeftTarget = distanceFromLeftTarget;
                        _leftLockTarget = _availableTargets[k].LockOnTransform;
                    }

                    if (relativeEnemyPosition.x < 0.00 && distanceFromRightTarget < shortestDistanceOfRightTarget)
                    {
                        shortestDistanceOfRightTarget = distanceFromRightTarget;
                        _rightLockTarget = _availableTargets[k].LockOnTransform;
                    }

                }
            }
        }

        public void ClearLockOnTargets()
        {
            _availableTargets.Clear();
            _currentLockOnTarget = null;
            _nearestLockOnTarget = null;
        }
    }
}


