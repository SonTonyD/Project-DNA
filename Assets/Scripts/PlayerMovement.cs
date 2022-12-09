using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class PlayerMovement : MonoBehaviour
    {
        Transform cameraObject;
        InputHandler inputHandler;
        Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        //public new Rigidbody rigidbody;
        public GameObject normalCamera;

        private CharacterController _controller;

        public bool Grounded;
        public LayerMask GroundLayers;
        public float GroundedOffset = -0.07f;
        public float GroundedRadius = 0.26f;


        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private bool _didSecondJump = false;

        [Header("Stats")]
        [SerializeField]
        float movementSpeed = 8;
        [SerializeField]
        float rotationSpeed = 10;
        [SerializeField]
        float jumpHeight = 4;
        [SerializeField]
        float gravity = -10;
        [SerializeField]
        float sprintSpeed = 12;



        void Start()
        {
            //rigidbody = GetComponent<Rigidbody>();

            _controller = GetComponent<CharacterController>();


            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();
            
        }

        public void Update()
        {
            float delta = Time.deltaTime;
            GroundedCheck();
            HandleJumping(delta);
            HandleMovement(delta);
            


        }

        #region Movement
        Vector3 normalVector;
        Vector3 targetPosition;
        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;

            targetDir = cameraObject.forward * inputHandler.vertical;
            targetDir += cameraObject.right * inputHandler.horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
            {
                targetDir = myTransform.forward;
            }
            float rs = rotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }

        

        public void HandleMovement(float delta)
        {
            inputHandler.TickInput(delta);
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;

            if (inputHandler.sprintFlag)
            {
                Debug.Log("Je cours");
                speed = sprintSpeed;
            }
            Debug.Log(speed);

            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            _controller.Move(moveDirection.normalized * (speed * delta) + new Vector3(0.0f, _verticalVelocity, 0.0f) * delta);

            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0);

            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        public void HandleJumping(float delta)
        {

            animatorHandler.SetGroundedAnimation(Grounded);
            animatorHandler.SetJumpAnimation(inputHandler.jumpFlag);


            if (Grounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f;
                
            }

            if ((inputHandler.jumpFlag && Grounded) || (inputHandler.jumpFlag && !_didSecondJump && !Grounded))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                
                if (_didSecondJump )
                {
                    _didSecondJump = false;
                    inputHandler.jumpFlag = false;
                }

                if (inputHandler.jumpFlag && !_didSecondJump)
                {
                    _didSecondJump = true;
                }

                
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += gravity * delta;
            }



            if (animatorHandler.anim.GetBool("isInteracting"))
            {
                return;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, _controller.radius, GroundLayers,
                QueryTriggerInteraction.Ignore);
        }




        #endregion


    }
}

