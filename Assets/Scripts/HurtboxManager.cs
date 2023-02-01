using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class HurtboxManager : MonoBehaviour
    {
        private CharacterStats _characterStats;

        private void Awake()
        {
            _characterStats = GetComponent<CharacterStats>();
            Hurtbox[] hurtboxes = GetComponentsInChildren<Hurtbox>();

            foreach (Hurtbox hurtbox in hurtboxes)
            {
                hurtbox.HurtboxManager = this;
            }
        }


        public void TakeDamage(int damage)
        {
            _characterStats.CurrentHealth -= damage;
        }


    }
}

