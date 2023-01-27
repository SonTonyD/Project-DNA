using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CharacterStats : MonoBehaviour
    {
        [SerializeField]
        private int _health;

        private void Start()
        {
            _health = 100;
        }

        public int Health { get => _health; set => _health = value; }
    }
}

