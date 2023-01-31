using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _lockTransform;

        public Transform LockTransform { get => _lockTransform; set => _lockTransform = value; }
    }
}

