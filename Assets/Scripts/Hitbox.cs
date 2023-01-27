using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Hitbox : MonoBehaviour
    {
        private BoxCollider _boxCollider;
        private MeshRenderer _meshRenderer;

        private bool _isRecovering;
        private bool _isAttacking;
        private LayerMask _enemyLayer;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true;
            _meshRenderer = GetComponent<MeshRenderer>();
            _enemyLayer.value = LayerMask.GetMask("Enemy", "Controller");
        }

        private void Start()
        {
            SetReset();
            _isRecovering = false;
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

        public void Apply(float startupFrame, float activeFrame, float recoveryFrame)
        {
            if (_isRecovering == false && _isAttacking == false)
            {
                _isAttacking = true;
                Invoke(nameof(SetActive), startupFrame);
                Invoke(nameof(SetReset), activeFrame + startupFrame);
                Invoke(nameof(StartRecovering), activeFrame + startupFrame);
                Invoke(nameof(EndRecovering), activeFrame + startupFrame + recoveryFrame);
            }
        }

        private void SetActive()
        {
            _boxCollider.enabled = true;
            _meshRenderer.enabled = true;
        }

        private void SetReset()
        {
            _boxCollider.enabled = false;
            _meshRenderer.enabled = false;
        }

        private void StartRecovering()
        {
            _isRecovering = true;
        }
        private void EndRecovering()
        {
            _isRecovering = false;
            _isAttacking = false;
        }



    }
}


