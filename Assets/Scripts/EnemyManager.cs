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
                List<int> activeFrames = new() { 3 };
                Attack attack = Attack.CreateInstance(activeFrames, 32, _hitbox);
                attack.LaunchAttack();
            }
        }
    }
}