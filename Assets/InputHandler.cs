using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace CYAN4S
{
    public class InputHandler : MonoBehaviour
    {
        // TODO MODE
        [SerializeField] private Key[] keys4B;
        public UnityEvent<int> onButtonPressed;

        private void Update()
        {
            for (var i = 0; i < keys4B.Length; i++)
            {
                var key = keys4B[i];
                
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    onButtonPressed?.Invoke(i);
                }
            }
        }
    }
}
