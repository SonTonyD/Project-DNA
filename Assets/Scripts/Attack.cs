using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Attack : ScriptableObject
    {
        private Hitbox _hitbox;

        private int _lastRecoveryFrame;
        private List<int> _activeFrames;

        public int FinalFrame { get => _lastRecoveryFrame; set => _lastRecoveryFrame = value; }

        public void Init(List<int> activeFrames, int lastRecoveryFrame, Hitbox hitbox)
        {
            this._activeFrames = activeFrames;
            this._lastRecoveryFrame = lastRecoveryFrame;
            this._hitbox = hitbox;
        }
        public static Attack CreateInstance(List<int> activeFrames, int lastRecoveryFrame, Hitbox hitbox)
        {
            var attack = ScriptableObject.CreateInstance<Attack>();
            attack.Init(activeFrames, lastRecoveryFrame, hitbox);
            return attack;
        }

        public void LaunchAttack()
        {
            if (_hitbox.IsAttacking == false)
            {
                _hitbox.IsAttacking = true;
                _hitbox.SetFrameParameters(_activeFrames, _lastRecoveryFrame);
            }
        }
    }
}
