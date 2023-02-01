using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class EnemyManager : CharacterManager
    {
        private HitboxHandler _hitbox;

        private void Awake()
        {
            if (GetComponentInChildren<HitboxHandler>())
            {
                _hitbox = GetComponentInChildren<HitboxHandler>();
            }
        }

        private void Update()
        {
            if (_hitbox)
            {
                //List<int> activeFrames = new() { 3 };
                //Attack attack = new Attack(activeFrames, 32, _hitbox);
            }
        }
    }
}