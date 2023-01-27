using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class EnemyManager : CharacterManager
    {
        private Hitbox _hitbox;

        private void Awake()
        {
            if (GetComponentInChildren<Hitbox>())
            {
                _hitbox = GetComponentInChildren<Hitbox>();
            }
        }

        private void Update()
        {
            if (_hitbox)
            {
                _hitbox.Apply(0.2f, 0.8f, 0.8f); //Make the enemy to attack (test)
            }
        }
    }
}