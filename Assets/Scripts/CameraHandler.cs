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
        [SerializeField]
        private LayerMask _environmentLayer;
        private PlayerManager _playerManager;
        private Vector3 _cameraFollowVelocity = Vector3.zero;

        public static CameraHandler singleton;

        [SerializeField]
        private float _horizontalSensitivity = 0.01f;
        [SerializeField]
        private float _followSpeed = 0.1f;
        [SerializeField]
        private float _switchTargetSpeed = 0.1f;
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
        [SerializeField]
        private float _availableTargetRadius = 26;

        private Transform _currentLockOnTarget;
        private List<CharacterManager> _availableTargets = new List<CharacterManager>();
        private Transform _nearestLockOnTarget;
        private Transform _leftLockTarget;
        private Transform _rightLockTarget;
        private Transform _rightCandidate;
        private Transform _leftCandidate;
        
        [SerializeField]
        private float _cameraPivotYOffset;
        private const float _CameraPivotYOffsetConstant = 0.15f;

        public Transform CurrentLockOnTarget { get => _currentLockOnTarget; set => _currentLockOnTarget = value; }
        public Transform NearestLockOnTarget { get => _nearestLockOnTarget; set => _nearestLockOnTarget = value; }
        public Transform LeftLockTarget { get => _leftLockTarget; set => _leftLockTarget = value; }
        public Transform RightLockTarget { get => _rightLockTarget; set => _rightLockTarget = value; }


        private void Awake()
        {
            singleton = this;
            _myTransform = transform;
            _cameraTransform.position = new Vector3(0, 0, -3.5f);
            _cameraTransform.rotation = Quaternion.Euler(new Vector3(-10, 0, 0));

            CharacterController controller = _targetTransform.GetComponent<CharacterController>();
            _cameraPivotYOffset = controller.height - _CameraPivotYOffsetConstant;

            _cameraPivotTransform.position = new Vector3(0, _cameraPivotYOffset, -0.025f);
            _defaultPosition = _cameraTransform.localPosition.z;
            _ignoreLayers = ~(1 << 8 | 1 << 11 | 1 << 10);
            _inputHandler = FindObjectOfType<InputHandler>();
            _playerManager = FindObjectOfType<PlayerManager>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start()
        {
            _environmentLayer = LayerMask.NameToLayer("Floor");
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

                //transform.rotation = targetRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, delta / _switchTargetSpeed);
                

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

        public void UpdateAvailableTargets(float delta)
        {
            FindAvailableTargets();
            UpdateLockOnParameters();
        }
        
        private void FindAvailableTargets()
        {
            Collider[] colliders = Physics.OverlapSphere(_targetTransform.position, _availableTargetRadius);

            for (int i = 0; i < colliders.Length; i++)
            {
                EnemyManager character = colliders[i].GetComponent<EnemyManager>();

                if (character != null)
                {
                    Vector3 lockTargetDirection = character.transform.position - _targetTransform.position;
                    float distanceFromTarget = Vector3.Distance(_targetTransform.position, character.transform.position);
                    float viewableAngle = Vector3.Angle(lockTargetDirection, _cameraTransform.forward);
                    RaycastHit hit;

                    if (character.transform.root != _targetTransform.transform.root && viewableAngle > -50 && viewableAngle < 50 && distanceFromTarget <= _maximumLockOnDistance)
                    {

                        if (Physics.Linecast(_playerManager.LockOnTransform.position, character.LockOnTransform.position, out hit))
                        {
                            Debug.DrawLine(_playerManager.LockOnTransform.position, character.LockOnTransform.position);

                            if (hit.transform.gameObject.layer == _environmentLayer)
                            {

                            }
                            else
                            {
                                _availableTargets.Add(character);
                            }
                        }
                    }
                }
            }
        }

        //Function that finds the left and right candidate for target change and finds the nearest target to lock
        private void UpdateLockOnParameters()
        {
            float shortestDistance = Mathf.Infinity;
            float shortestRightAngle = Mathf.Infinity;
            float shortestLeftAngle = Mathf.Infinity;
            for (int k = 0; k < _availableTargets.Count; k++)
            {
                Vector3 lockTargetDirection = _availableTargets[k].transform.position - _cameraTransform.position;
                Vector3 viewDirection = _cameraTransform.forward;
                lockTargetDirection.y = 0;
                viewDirection.y = 0;
                float relativeAngle = Vector3.SignedAngle(viewDirection, lockTargetDirection, Vector3.up);
                float distanceFromTarget = Vector3.Distance(_targetTransform.position, _availableTargets[k].transform.position);

                if (relativeAngle > 0.0f && Mathf.Abs(relativeAngle) < shortestRightAngle && _availableTargets[k].LockOnTransform != _currentLockOnTarget)
                {
                    shortestRightAngle = Mathf.Abs(relativeAngle);
                    _rightCandidate = _availableTargets[k].LockOnTransform;
                }

                if (relativeAngle < 0.0f && Mathf.Abs(relativeAngle) < shortestLeftAngle && _availableTargets[k].LockOnTransform != _currentLockOnTarget)
                {
                    shortestLeftAngle = Mathf.Abs(relativeAngle);
                    _leftCandidate = _availableTargets[k].LockOnTransform;
                }

                if (distanceFromTarget < shortestDistance)
                {
                    shortestDistance = distanceFromTarget;
                    _nearestLockOnTarget = _availableTargets[k].LockOnTransform;
                }
            }
        }

        public void HandleLockOn(float delta)
        {
            if (_inputHandler.LockOnFlag)
            {
                if (_inputHandler.LockOnRightFlag)
                {
                    _rightLockTarget = _rightCandidate;
                }

                if (_inputHandler.LockOnLeftFlag)
                {
                    _leftLockTarget = _leftCandidate;
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