using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Hitbox : MonoBehaviour
    {
        private BoxCollider _boxCollider;
        private MeshRenderer _meshRenderer;
        private InputHandler _inputHandler;

        private bool _isAttacking;
        private LayerMask _enemyLayer;
        private bool _hasInputHandler;

        private int _lastRecoveryFrame;
        private List<int> _activeFrames;
        private Hitbox[] _hitbox;

        private int _counter;

        public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();

            if (GetComponentInParent<InputHandler>())
            {
                _inputHandler = GetComponentInParent<InputHandler>();
                _hasInputHandler = true;
            }
            else
            {
                _hasInputHandler = false;
            }
            _boxCollider.isTrigger = true;
            _enemyLayer.value = LayerMask.GetMask("Enemy", "Controller");
        }

        private void Start()
        {
            DisableHitbox();
            _isAttacking = false;
            _counter = 1;
            _lastRecoveryFrame = -1;
    }

        private void Update()
        {
            if (_isAttacking)
            {
                if (_hasInputHandler)
                {
                    _inputHandler.IsMoveDisabled = true;
                }
                Apply();
            }
            
        }

        void OnTriggerEnter(Collider other)
        {
            if ((_enemyLayer.value & 1 << other.gameObject.layer) != 0)
            {
                Hurtbox hurtbox = other.gameObject.GetComponent<Hurtbox>();
                if (hurtbox != null && hurtbox.HurtboxManager != null)
                {
                    hurtbox.HurtboxManager.TakeDamage();
                }
            }
        }

        public void Apply()
        {
            if (_activeFrames.Contains(_counter) && !_boxCollider.enabled && !_meshRenderer.enabled)
            {
                //Debug.Log(_counter);
                EnableHitbox();
            }
            else
            {
                DisableHitbox();
            }
            
            if (_counter > _lastRecoveryFrame)
            {
                EndAttack();
            }
            _counter += 1;
        }

        private void EnableHitbox()
        {
            _boxCollider.enabled = true;
            _meshRenderer.enabled = true;
        }

        private void DisableHitbox()
        {
            _boxCollider.enabled = false;
            _meshRenderer.enabled = false;
        }

        private void EndAttack()
        {
            if (_hasInputHandler)
            {
                _inputHandler.IsMoveDisabled = false;
            }
            _isAttacking = false;
            _counter = 0;
        }

        public void SetFrameParameters(List<int> activeFrames, int lastRecoveryFrame)
        {
            this._activeFrames = activeFrames;
            this._lastRecoveryFrame = lastRecoveryFrame;
        }



    }
}


