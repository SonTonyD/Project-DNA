using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class HurtboxManager : MonoBehaviour
    {
        private CharacterStats _characterStats;
        private int _counter;
        private bool _isInvicible;
        private Hurtbox[] _hurtboxes;
        private int _numberOfFrameToWait;

        private void Awake()
        {
            _characterStats = GetComponent<CharacterStats>();
            _hurtboxes = GetComponentsInChildren<Hurtbox>();

            foreach (Hurtbox hurtbox in _hurtboxes)
            {
                hurtbox.HurtboxManager = this;
            }
            _isInvicible = false;
            _counter = 0;
        }

        private void Update()
        {
            if (_isInvicible == true)
            {
                _counter++;

                if (_counter > _numberOfFrameToWait)
                {
                    _isInvicible = false;
                    _counter = 0;
                }
            }
        }

        public void TakeDamage(int damage, int counter, List<int> activeFrames)
        {
            if (_isInvicible == false)
            {
                int currentFrame = counter - 1;
                _characterStats.CurrentHealth -= damage;

                _isInvicible = true;

                int lastConsecutiveInteger = GetLastConsecutiveInteger(activeFrames, activeFrames.IndexOf(currentFrame));
                _numberOfFrameToWait = lastConsecutiveInteger - currentFrame;

                //Debug.Log($"Take Damage at frame: {currentFrame}");
                //Debug.Log($"numberOfFrameToWait: {_numberOfFrameToWait} , {lastConsecutiveInteger} , {currentFrame} , {activeFrames.IndexOf(currentFrame)}");
            }


        }

        static int GetLastConsecutiveInteger(List<int> list, int startIndex)
        {
            int lastConsecutiveInt = -1;
            for (int i = startIndex; i < list.Count - 1; i++)
            {
                if (list[i] + 1 == list[i + 1])
                {
                    lastConsecutiveInt = list[i + 1];
                }
                else
                {
                    break;
                }
            }
            return lastConsecutiveInt;
        }
    }
}

