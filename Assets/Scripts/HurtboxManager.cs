using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class HurtboxManager : MonoBehaviour
    {
        private CharacterStats _characterStats;

        private float _hitStunTime = 60 / 60f; //Should be ajust properly
        private bool _isIgnoringDamage; //When an entity is hit, several hurtboxes are hit. To avoid this, we used hitStun to ignore damage from other attacker during the hitStun time.


        private void Awake()
        {
            _characterStats = GetComponent<CharacterStats>();
            Hurtbox[] hurtboxes = GetComponentsInChildren<Hurtbox>();

            foreach (Hurtbox hurtbox in hurtboxes)
            {
                hurtbox.HurtboxManager = this;
            }
        }

        private void Start()
        {
            _isIgnoringDamage = false;
        }

        public void TakeDamage()
        {
            if (_isIgnoringDamage == false)
            {
                _characterStats.Health -= 10;
                _isIgnoringDamage = true;
                Invoke(nameof(ResetHitStun), _hitStunTime);
            }
            
        }

        public void ResetHitStun()
        {
            _isIgnoringDamage = false;
        }

    }
}

