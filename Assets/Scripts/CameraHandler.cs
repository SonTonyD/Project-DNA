using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CameraHandler : MonoBehaviour
    {
        private InputHandler _inputHandler;
        private Transform _characterTransform;
        private PlayerManager _playerManager;
        public static CameraHandler singleton;

        [Header("References")]
        [SerializeField]
        private Transform _targetTransform;
        [SerializeField]
        private Transform _cameraTransform;
        [SerializeField]
        private Transform _cameraPivotTransform;
        [SerializeField]
        private LayerMask _environmentLayer;

        private Vector3 _cameraTransformPosition;
        private LayerMask _ignoreLayers;
        private Vector3 _cameraFollowVelocity = Vector3.zero;

        [Header("Camera Control Variables")]
        [SerializeField]
        private float _horizontalSensitivity = 1.0f;
        [SerializeField]
        private float _verticalSensitivity = 1.0f;
        [SerializeField]
        private float _followSpeed = 0.1f;
        [SerializeField]
        private float _minimumPivot = -30.0f;
        [SerializeField]
        private float _maximumPivot = 70.0f;
        [SerializeField]
        private float _cameraSphereRadius = 0.2f;
        [SerializeField]
        private float _cameraCollisionOffset = 0.2f;
        [SerializeField]
        private float _minimumCameraOffset = 0.2f;
        [SerializeField]
        private float _cameraPivotYOffset;

        private const float _SensitivityMultiplier = 100f;
        private float _targetPosition;
        private float _defaultPosition;
        private float _lookAngle;
        private float _pivotAngle;
        private const float _CameraPivotVerticalOffset = 0.15f;

        [Header("Camera Lock Variables")]
        [SerializeField]
        private float _switchLockTargetSpeed = 0.1f;
        [SerializeField]
        private float _maximumLockDistance = 30;
        [SerializeField]
        private float _availableLockTargetRadius = 26;

        private List<CharacterManager> _availableLockTargets = new();
        private Transform _currentLockTarget;
        private Transform _nearestLockTarget;
        private Transform _leftLockTarget;
        private Transform _rightLockTarget;
        private Transform _rightLockTargetCandidate;
        private Transform _leftLockTargetCandidate;
        private bool _isLockRotationEnded;

        public Transform CurrentLockTarget { get => _currentLockTarget; set => _currentLockTarget = value; }
        public Transform NearestLockTarget { get => _nearestLockTarget; set => _nearestLockTarget = value; }
        public Transform LeftLockTarget { get => _leftLockTarget; set => _leftLockTarget = value; }
        public Transform RightLockTarget { get => _rightLockTarget; set => _rightLockTarget = value; }


        private void Awake()
        {
            singleton = this;
            _characterTransform = transform;
            _cameraTransform.position = new Vector3(0, 0, -3.5f);
            _cameraTransform.rotation = Quaternion.Euler(new Vector3(-10, 0, 0));

            _defaultPosition = _cameraTransform.localPosition.z;
            _ignoreLayers = ~(1 << 8 | 1 << 11 | 1 << 10);
            _inputHandler = FindObjectOfType<InputHandler>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start()
        {
            _environmentLayer = LayerMask.NameToLayer("Floor");

            _playerManager = FindObjectOfType<PlayerManager>();
            CharacterController controller = _targetTransform.GetComponent<CharacterController>();
            _cameraPivotYOffset = controller.height - _CameraPivotVerticalOffset;
            _cameraPivotTransform.position = new Vector3(-0.025f, _cameraPivotYOffset, -0.025f);
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(_characterTransform.position, _targetTransform.position, ref _cameraFollowVelocity, _followSpeed);
            _characterTransform.position = targetPosition;
            HandleCameraCollisions();
        }

        /// <summary>
        /// Rotates camera and lock camera
        /// </summary>
        /// <param name="delta">Time between frames</param>
        /// <param name="mouseXInput">Mouse horizontal value</param>
        /// <param name="mouseYInput">Mouse vertical value</param>
        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
        {
            // If no lock
            if (!_inputHandler.LockFlag && _currentLockTarget == null)
            {
                // Set camera rotation to look direction
                _lookAngle += (mouseXInput * _horizontalSensitivity * _SensitivityMultiplier) * delta;
                _pivotAngle += (mouseYInput * _verticalSensitivity * _SensitivityMultiplier) * delta;
                _pivotAngle = Mathf.Clamp(_pivotAngle, _minimumPivot, _maximumPivot);

                Vector3 rotation = Vector3.zero;
                rotation.y = _lookAngle;
                Quaternion targetRotation = Quaternion.Euler(rotation);
                _characterTransform.rotation = targetRotation;

                rotation = Vector3.zero;
                rotation.x = _pivotAngle;

                targetRotation = Quaternion.Euler(rotation);
                _cameraPivotTransform.localRotation = targetRotation;
            } 
            else
            {
                // Set camera rotation to lock target
                Vector3 direction = _currentLockTarget.position - transform.position;
                direction.Normalize();
                direction.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Wait for smooth camera rotation on lock to end to hard lock on lock target
                if (!_isLockRotationEnded && transform.rotation == targetRotation)
                {
                    _isLockRotationEnded = true;
                }

                // Hard lock on target when smooth camera rotation ended
                if (_isLockRotationEnded)
                {
                    transform.rotation = targetRotation;
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _switchLockTargetSpeed);
                }

                direction = _currentLockTarget.position - _cameraPivotTransform.position;
                direction.Normalize();

                targetRotation = Quaternion.LookRotation(direction);
                Vector3 eulerAngle = targetRotation.eulerAngles;
                eulerAngle.y = 0;
                _cameraPivotTransform.localEulerAngles = eulerAngle;
            }
        }

        /// <summary>
        /// Takes into account walls to prevent the camera from going through walls
        /// </summary>
        private void HandleCameraCollisions()
        {
            _targetPosition = _defaultPosition;
            Vector3 direction = _cameraTransform.position - _cameraPivotTransform.position;
            direction.Normalize();

            // Cast a sphere in the direction of the camera to check if it is colliding with walls
            if (Physics.SphereCast(_cameraPivotTransform.position, _cameraSphereRadius, direction, out RaycastHit hit, Mathf.Abs(_targetPosition), _ignoreLayers))
            {
                float dis = Vector3.Distance(_cameraPivotTransform.position, hit.point);
                _targetPosition = -(dis - _cameraCollisionOffset);
            }

            // Correct camera position
            if (Mathf.Abs(_targetPosition) < _minimumCameraOffset)
            {
                _targetPosition = -_minimumCameraOffset;
            }

            _cameraTransformPosition.z = Mathf.Lerp(_cameraTransform.localPosition.z, _targetPosition, 0.2f);
            _cameraTransform.localPosition = _cameraTransformPosition;
        }

        /// <summary>
        /// Finds available lock targets and updates lock parameters
        /// </summary>
        public void UpdateLockTargets()
        {
            FindAvailableLockTargets();
            UpdateLockParameters();
        }

        /// <summary>
        /// Finds available lock targets in a sphere arround the character
        /// </summary>
        private void FindAvailableLockTargets()
        {
            // Create a sphere arround the character and detects lock targets (colliders)
            Collider[] colliders = Physics.OverlapSphere(_targetTransform.position, _availableLockTargetRadius);

            for (int i = 0; i < colliders.Length; i++)
            {
                // Get the Enemy Manager corresponding to a collider of a detected lock target
                EnemyManager enemy = colliders[i].GetComponent<EnemyManager>();

                // If a lock target is found
                if (enemy != null)
                {
                    // Compute distance and angle from lock target
                    Vector3 lockTargetDirection = enemy.transform.position - _targetTransform.position;
                    float distanceFromTarget = Vector3.Distance(_targetTransform.position, enemy.transform.position);
                    float viewableAngle = Vector3.Angle(lockTargetDirection, _cameraTransform.forward);

                    // If lock target is in a viewable angle and at a lockable distance
                    if (enemy.transform.root != _targetTransform.transform.root && viewableAngle > -50 && viewableAngle < 50 && distanceFromTarget <= _maximumLockDistance)
                    {
                        // Check if lock target is in direct view with a line cast
                        if (Physics.Linecast(_playerManager.LockTransform.position, enemy.LockTransform.position, out RaycastHit hit))
                        {
                            if (hit.transform.gameObject.layer == _environmentLayer)
                            {
                                // Do nothing if the lock target is behind a wall or an environment element
                            }
                            else
                            {
                                // Add lock target to available targets
                                _availableLockTargets.Add(enemy);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the left and right candidate for target switch and finds the nearest target to lock
        /// </summary>
        private void UpdateLockParameters()
        {
            float shortestDistance = Mathf.Infinity;
            float shortestRightAngle = Mathf.Infinity;
            float shortestLeftAngle = Mathf.Infinity;

            // For all available lock targets found
            for (int k = 0; k < _availableLockTargets.Count; k++)
            {
                // Compute distance and angle from available lock target
                Vector3 lockTargetDirection = _availableLockTargets[k].transform.position - _cameraTransform.position;
                Vector3 viewDirection = _cameraTransform.forward;
                lockTargetDirection.y = 0;
                viewDirection.y = 0;
                float relativeAngle = Vector3.SignedAngle(viewDirection, lockTargetDirection, Vector3.up);
                float distanceFromTarget = Vector3.Distance(_targetTransform.position, _availableLockTargets[k].transform.position);

                // Set right lock target candidate if it has the shortest right angle
                if (relativeAngle > 0.0f && Mathf.Abs(relativeAngle) < shortestRightAngle && _availableLockTargets[k].LockTransform != _currentLockTarget)
                {
                    shortestRightAngle = Mathf.Abs(relativeAngle);
                    _rightLockTargetCandidate = _availableLockTargets[k].LockTransform;
                }

                // Set left lock target candidate if it has the shortest left angle
                if (relativeAngle < 0.0f && Mathf.Abs(relativeAngle) < shortestLeftAngle && _availableLockTargets[k].LockTransform != _currentLockTarget)
                {
                    shortestLeftAngle = Mathf.Abs(relativeAngle);
                    _leftLockTargetCandidate = _availableLockTargets[k].LockTransform;
                }

                // Set shortest distance if it has a shorter distance
                if (distanceFromTarget < shortestDistance)
                {
                    shortestDistance = distanceFromTarget;
                    _nearestLockTarget = _availableLockTargets[k].LockTransform;
                }
            }
        }

        /// <summary>
        /// Locks the camera on the nearest left or right target
        /// </summary>
        public void HandleLock()
        {
            if (_inputHandler.LockFlag)
            {
                _isLockRotationEnded = false;
                UpdateLockTargets();
                if (_inputHandler.LockRightFlag)
                {
                    _rightLockTarget = _rightLockTargetCandidate;
                }

                if (_inputHandler.LockLeftFlag)
                {
                    _leftLockTarget = _leftLockTargetCandidate;
                }
            }
        }

        /// <summary>
        /// Clears all lock targets and reset lock variables
        /// </summary>
        public void ClearLockTargets()
        {
            _currentLockTarget = null;
            _nearestLockTarget = null;
            _isLockRotationEnded = false;
            _availableLockTargets.Clear();
        }
    }
}