using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    public class HurtboxManager : MonoBehaviour
    {

        private void Awake()
        {
            Hurtbox[] hurtboxes = GetComponentsInChildren<Hurtbox>();
        }

    }
}

