using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    [CreateAssetMenu]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("References")]
        [SerializeField]
        public Transform _cameraObject;
        [SerializeField]
        public InputHandler _inputHandler;
        [HideInInspector]
        public Transform _characterTransform;
        [HideInInspector]
        public AnimatorHandler _animatorHandler;
        [SerializeField]
        public GameObject _normalCamera;
        [SerializeField]
        public CharacterController _controller;
        [SerializeField]
        public CameraHandler _cameraHandler;

        public Vector3 _moveDirection;

        [Header("Movement Variables")]
        [SerializeField]
        public float _movementSpeed = 8.0f;
        [SerializeField]
        public float _rotationSpeed = 15.0f;
        [SerializeField]
        public float _jumpHeight = 2f;
        [SerializeField]
        public float _gravity = -15.0f;
        [SerializeField]
        public float _sprintSpeed = 12.0f;
        [SerializeField]
        public float _speedModulation = 0f;

        public readonly float _DiagonalInputThreshold = 0.5f;
        public readonly float _OrthogonalInputThreshold = 0.85f;
        public readonly float _MinimalSpeedModulation = 0.15f;

        [Header("Jump Variables")]
        [SerializeField]
        public bool _isGrounded;
        [SerializeField]
        public LayerMask _groundLayers;
        [SerializeField]
        public float _groundedOffset = -0.08f;
        [SerializeField]
        public bool _didSecondJump = false;
        [SerializeField]
        public float _verticalVelocity;
        [SerializeField]
        public float _terminalVelocity = 50.0f;

        [Header("Step Variables")]
        [SerializeField]
        public Vector3 _horizontalVelocity = Vector3.zero;
        [SerializeField]
        public bool _isStepping = false;
        [SerializeField]
        public float _stepPower = 5f;
        [SerializeField]
        public int _stepStartupFrameNumber = 4;
        [SerializeField]
        public int _stepActiveFrameNumber = 15;
        [SerializeField]
        public int _stepRecoveryFrameNumber = 10;
        [SerializeField]
        public bool _isRecoveringFromStep = false;
        [SerializeField]
        public int _stepFrameCount = 1;
        [SerializeField]
        public bool _isStepFrameCountStarted = false;
        [SerializeField]
        public Vector2 _stepMovementInput;
        [SerializeField]
        public Vector2 _currentStepMovementInput;
        public float _stepInitialHorizontalValue;

        public readonly float _StepPowerMultiplier = 100f;
        public readonly float _OrthogonalStepInputThreshold = 0.9f;
        public readonly float _AntiSpiralConstant = 0.0425f;
        public readonly float _MinimalStepMovementInput = 0.5f;

        [Header("Dash Variables")]
        [SerializeField]
        public bool _isDashing = false;
        [SerializeField]
        public Vector3 _dashVelocity = Vector3.zero;
        [SerializeField]
        public float _dashPower = 6f;
        [SerializeField]
        public int _dashStartupFrameNumber = 8;
        //[SerializeField]
        //private int _dashRecoveryFrameNumber = 20;
        [SerializeField]
        public int _dashFrameCount = 1;
        [SerializeField]
        public bool _isDashFrameCountStarted = false;

        public readonly float _DashPowerMultiplier = 100f;
    }

}
