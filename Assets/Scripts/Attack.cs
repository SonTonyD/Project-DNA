using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    [CreateAssetMenu(fileName = "New Attack", menuName = "Attack")]
    public class Attack : ScriptableObject
    {
        [Header("References")]
        [SerializeField]
        private string _name;
        [SerializeField]
        private GameObject _hitbox;

        [Header("Attack effect")]
        [SerializeField]
        private int _damage;

        [Header("Attack Frame Parameters")]
        [SerializeField]
        private int _cancelFrame;
        [SerializeField]
        private int _lastRecoveryFrame;
        [SerializeField]
        private List<int> _activeFrames;

        [Header("Hitbox Parameters")]
        [SerializeField]
        private float _hitboxRange;
        [SerializeField]
        private float _hitboxSpawnHeight;
        

        public string Name { get => _name; set => _name = value; }
        public GameObject Hitbox { get => _hitbox; set => _hitbox = value; }
        public int LastRecoveryFrame { get => _lastRecoveryFrame; set => _lastRecoveryFrame = value; }
        public List<int> ActiveFrames { get => _activeFrames; set => _activeFrames = value; }
        public float HitboxRange { get => _hitboxRange; set => _hitboxRange = value; }
        public float HitboxSpawnHeight { get => _hitboxSpawnHeight; set => _hitboxSpawnHeight = value; }
        public int CancelFrame { get => _cancelFrame; set => _cancelFrame = value; }
        public int Damage { get => _damage; set => _damage = value; }
    }
}
