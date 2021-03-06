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
        public UnityEvent<int, double> onButtonPressed;
        public UnityEvent<int> onButtonIsPressed;
        public UnityEvent<int, double> onButtonReleased;

        private void Awake()
        {
            Physics.autoSimulation = false;
        }

        private void FixedUpdate()
        {
            for (var i = 0; i < keys4B.Length; i++)
            {
                var key = keys4B[i];
                
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    onButtonPressed?.Invoke(i, Time.fixedTimeAsDouble);
                }

                if (Keyboard.current[key].isPressed)
                {
                    onButtonIsPressed?.Invoke(i);
                }

                if (Keyboard.current[key].wasReleasedThisFrame)
                {
                    onButtonReleased?.Invoke(i, Time.fixedTimeAsDouble);
                }
            }
        }
    }
}
