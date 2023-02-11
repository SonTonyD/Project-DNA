using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class EnemyInputHandler : InputHandler
    {
        [Header("Enemy Inputs")]
        [SerializeField]
        private float _moveX;
        [SerializeField]
        private float _moveZ;
        [SerializeField]
        private bool _jumpInput;
        [SerializeField]
        private bool _guardInput;
        [SerializeField]
        private bool _attackInput;

        

        public override float GetHorizontalInput()
        {
            return _moveX;
        }

        public override float GetVerticalInput()
        {
            return _moveZ;
        }

        public override bool GetAInput()
        {
            return _jumpInput;
        }

        public override bool GetRsInput()
        {
            return _rsInput;
        }

        public override bool GetRsLeftInput()
        {
            return _rsLeftInput;
        }

        public override bool GetRsRightInput()
        {
            return _rsRightInput;
        }

        public override bool GetGuardInput()
        {
            return _guardInput;
        }

        public override bool GetAttackInput()
        {
            return _attackInput;
        }
    }
}

