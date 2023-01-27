using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _lockOnTransform;
        public Transform LockOnTransform { get => _lockOnTransform; set => _lockOnTransform = value; }
    }
}

