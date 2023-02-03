using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class HitboxHandler : MonoBehaviour
    {
        private BoxCollider _boxCollider;
        private MeshRenderer _meshRenderer;
        private InputHandler _inputHandler;
        private PlayerAttacker _playerAttacker;

        private bool _isAttacking;
        private LayerMask _enemyLayer;
        private bool _hasInputHandler;

        private int _lastRecoveryFrame;
        private List<int> _activeFrames;
        private int _damage;

        private int _counter;

        private void Awake()
        {
            GetReferences();
            InitializeHitboxVariables();
        }


        private void Update()
        {
            if (_isAttacking)
            {
                DisableMovement(true);
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
                    hurtbox.HurtboxManager.TakeDamage(_damage, _counter, _activeFrames);
                }
            }
        }

        public void Apply()
        {
            if (_activeFrames.Contains(_counter) && _boxCollider.enabled == false)
            {
                EnableHitbox();
            }
            else
            {
                DisableHitbox();
            }
            
            if (_counter > _lastRecoveryFrame)
            {
                EndAttack();
                Destroy(gameObject);
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
            DisableMovement(false);
            _isAttacking = false;
            _playerAttacker.IsAntiSpamActivated = false;
            _counter = 0;
        }

        private void DisableMovement(bool value)
        {
            if (_hasInputHandler)
            {
                _inputHandler.IsMoveDisabled = value;
            }
        }

        public void SetAttack(Attack attack)
        {
            this._activeFrames = attack.ActiveFrames;
            this._lastRecoveryFrame = attack.LastRecoveryFrame;
            this._damage = attack.Damage;
            _isAttacking = true;
        }

        private void GetReferences()
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

            _playerAttacker = GetComponentInParent<PlayerAttacker>();
        }

        private void InitializeHitboxVariables()
        {
            _boxCollider.isTrigger = true;
            _enemyLayer.value = LayerMask.GetMask("Enemy", "Controller");

            DisableHitbox();
            _isAttacking = false;
            _counter = 1;
            _lastRecoveryFrame = -1;
        }



    }
}


