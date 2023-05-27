using UnityEditor;
using UnityEngine;

namespace DNA
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Player Movement Data")]
        private string _scriptableObjectPath = "Assets/Data/PlayerMovementData.asset";
        public PlayerMovementData _movementData;
        
        [Header("Movement References")]
        public WalkAndRun walkAndRun;
        public Jump jump;
        public Step step;
        public Dash dash;
        public GroundCheck groundCheck;

        private void Start()
        {
            GetPlayerMovementData();
            GetMovementDataReferences();
            GetMovementReferences();
        }

        public string getPlayerMovementDataPath()
        {
            return _scriptableObjectPath;
        }

        private void GetPlayerMovementData()
        {
            _movementData = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(_scriptableObjectPath);
        }

        private void GetMovementDataReferences()
        {
            _movementData._controller = GetComponent<CharacterController>();
            _movementData._inputHandler = GetComponent<InputHandler>();
            _movementData._animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _movementData._cameraObject = Camera.main.transform;
            _movementData._cameraHandler = CameraHandler.singleton;
            _movementData._characterTransform = transform;
            _movementData._animatorHandler.Initialize();
            _movementData._groundLayers = LayerMask.GetMask("Floor");
        }

        private void GetMovementReferences()
        {
            walkAndRun = GetComponentInChildren<WalkAndRun>();
            jump = GetComponentInChildren<Jump>();
            step = GetComponentInChildren<Step>();
            dash = GetComponentInChildren<Dash>();
            groundCheck = GetComponentInChildren<GroundCheck>();
        }
    }
}