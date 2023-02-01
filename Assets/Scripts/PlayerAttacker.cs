using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class PlayerAttacker : MonoBehaviour
    {
        [SerializeField]
        private Attack _lightAttack;
        private bool _isAntiSpamActivated;

        public bool IsAntiSpamActivated { get => _isAntiSpamActivated; set => _isAntiSpamActivated = value; }

        private void Start()
        {
            _isAntiSpamActivated = false;
        }
        public void HandleLightAttack()
        {
            LaunchAttack(_lightAttack);
        }

        private void LaunchAttack(Attack attack)
        { 
            if (_isAntiSpamActivated == false)
            {
                //calculate hitbox position
                Vector3 hitboxSpawnPosition = CalculateHitboxPosition(attack);

                //create hitbox in the scene
                GameObject currentHitbox = Instantiate(attack.Hitbox, hitboxSpawnPosition, gameObject.transform.rotation, gameObject.transform);
                HitboxHandler hitboxHandler = currentHitbox.GetComponent<HitboxHandler>();

                //Set attack to the hitboxHandler and launch the attack
                hitboxHandler.SetAttack(attack);

                //Prevent attack spam
                _isAntiSpamActivated = true;
            }
            
        }

        private Vector3 CalculateHitboxPosition(Attack attack)
        {
            Vector3 spawnPosition = transform.position + transform.forward * attack.HitboxRange;
            spawnPosition.y = transform.position.y + attack.HitboxSpawnHeight;
            return spawnPosition;
        }
    }
}

