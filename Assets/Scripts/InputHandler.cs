using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace CYAN4S
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerInput _input;
        
        // TODO MODE
        [SerializeField] private Key[] keys4B;

        public UnityEvent<int, double> onButtonPressed;
        public UnityEvent<int, double> onButtonIsPressed;
        public UnityEvent<int, double> onButtonReleased;

        public UnityEvent<int, double> onButtonPressedEx;
        public UnityEvent<int, double> onButtonIsPressedEx;
        public UnityEvent<int, double> onButtonReleasedEx;

        public UnityEvent onPausePressed;
        
        private readonly Queue<Action> _tasks = new();

        private void FixedUpdate()
        {
            var time = Time.fixedTimeAsDouble;
            
            for (var i = 0; i < keys4B.Length; i++)
            {
                var key = keys4B[i];
                var i1 = i;

                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    onButtonPressedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    _tasks.Enqueue(() => onButtonPressed?.Invoke(i1, time));
                }

                if (Keyboard.current[key].isPressed)
                {
                    onButtonIsPressedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    _tasks.Enqueue(() => onButtonIsPressed?.Invoke(i1, time));
                }

                if (Keyboard.current[key].wasReleasedThisFrame)
                {
                    onButtonReleasedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    _tasks.Enqueue(() => onButtonReleased?.Invoke(i1, time));
                }
            }

            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                _tasks.Enqueue(() => onPausePressed?.Invoke());
            }
        }

        private void Update()
        {
            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }
    }
}