using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class CharacterStats : MonoBehaviour
    {
        [SerializeField]
        private int _maxHealth = 100;
        [SerializeField]
        private int _currentHealth;

        private void Start()
        {
            _currentHealth = _maxHealth;
        }

        public int CurrentHealth { get => _currentHealth; set => _currentHealth = value; }
    }
}

