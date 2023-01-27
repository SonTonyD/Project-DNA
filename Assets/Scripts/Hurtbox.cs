using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class Hurtbox : MonoBehaviour
    {
        private HurtboxManager _hurtboxManager;
        public HurtboxManager HurtboxManager { get => _hurtboxManager; set => _hurtboxManager = value; }
    }
}

