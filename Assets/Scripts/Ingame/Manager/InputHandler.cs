using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace CYAN4S
{
    [Serializable]
    public struct Schema
    {
        public Key[] play;
        public Key speedUp;
        public Key speedDown;
    }

    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Schema schema4B;
        [SerializeField] private Schema schema5B;
        [SerializeField] private Schema schema6B;
        [SerializeField] private Schema schema8B;

        [Header("Inspector Setup")]
        public UnityEvent<int, double> onButtonPressedEx;
        public UnityEvent<int, double> onButtonReleasedEx;

        public UnityEvent onSpeedUpPressed;
        public UnityEvent onSpeedDownPressed;
        public int currentMiddle = -1;

        public UnityEvent onPausePressed;

        private PlayerInput _input;
        private Schema schema;
        private Action Loop;

        private readonly Queue<Action> _tasks = new();


        private void Awake()
        {
            // Chart is from the previous scene via `Selected` singleton object.
            var chart = Selected.Instance.chart;

            schema = chart.button switch
            {
                4 => schema4B,
                5 => schema5B,
                6 => schema6B,
                8 => schema8B,
                _ => throw new Exception("Unsupported Button")
            };

            Loop = chart.button == 5 ? Loop5B : LoopNormal;
        }
        
        private void Update()
        {
            Loop();
            
            if (Keyboard.current[schema.speedUp].wasPressedThisFrame)
                onSpeedUpPressed?.Invoke();
            
            if (Keyboard.current[schema.speedDown].wasPressedThisFrame)
                onSpeedDownPressed?.Invoke();
            
            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
                onPausePressed?.Invoke();
        }

        private void Loop5B()
        {
            for (var i = 0; i < schema.play.Length; i++)
            {
                var key = schema.play[i];

                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    if (i == 0 | i == 1) 
                    {
                        onButtonPressedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    }

                    if (i == 4 | i == 5)
                    {
                        onButtonPressedEx?.Invoke(i - 1, Time.fixedTimeAsDouble);
                    }

                    if (i == 2 | i == 3)
                    {
                        if (Keyboard.current[i == 2 ? schema.play[3] : schema.play[2]].isPressed)
                        {
                            onButtonReleasedEx?.Invoke(2, Time.fixedTimeAsDouble);
                        }
                        onButtonPressedEx?.Invoke(2, Time.fixedTimeAsDouble);

                        currentMiddle = i;
                    }
                }

                if (Keyboard.current[key].wasReleasedThisFrame)
                {
                    if (i == 0 | i == 1)
                    {
                        onButtonReleasedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    }

                    if (i == 4 | i == 5)
                    {
                        onButtonReleasedEx?.Invoke(i - 1, Time.fixedTimeAsDouble);
                    }

                    if (i == currentMiddle)
                    {
                        onButtonReleasedEx?.Invoke(2, Time.fixedTimeAsDouble);
                    }
                }
            }
        }

        private void LoopNormal()
        {
            for (var i = 0; i < schema.play.Length; i++)
            {
                var key = schema.play[i];

                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    onButtonPressedEx?.Invoke(i, Time.fixedTimeAsDouble);
                }

                if (Keyboard.current[key].wasReleasedThisFrame)
                {
                    onButtonReleasedEx?.Invoke(i, Time.fixedTimeAsDouble);
                }
            }
        }
    }
}