using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Attack : ScriptableObject
    {
        private Hitbox _hitbox;

        private float _startupFrame;
        private float _activeFrame;
        private float _recoveryFrame;
        private float _totalFrame;

        public float TotalFrame { get => _totalFrame; set => _totalFrame = value; }

        public void Init(float startupFrame, float activeFrame, float recoveryFrame, Hitbox hitbox)
        {
            this._startupFrame = startupFrame / 60f;
            this._activeFrame = activeFrame / 60f;
            this._recoveryFrame = recoveryFrame / 60f;

            this._totalFrame = this._startupFrame + this._activeFrame + this._recoveryFrame;
            this._hitbox = hitbox;
        }
        public static Attack CreateInstance(float startupFrame, float activeFrame, float recoveryFrame, Hitbox hitbox)
        {
            var attack = ScriptableObject.CreateInstance<Attack>();
            attack.Init(startupFrame, activeFrame, recoveryFrame, hitbox);
            return attack;
        }

        public void LaunchAttack()
        {
            _hitbox.Apply(_startupFrame, _activeFrame, _recoveryFrame);
        }
    }
}
