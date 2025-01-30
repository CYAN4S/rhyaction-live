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
        private PlayerInput _input;

        [SerializeField] private Schema schema4B;
        [SerializeField] private Schema schema5B;
        [SerializeField] private Schema schema6B;
        [SerializeField] private Schema schema8B;

        public int currentMiddle = -1;

        private Schema schema;

        [Header("Inspector Setup")]
        // public UnityEvent<int, double> onButtonPressed;
        // public UnityEvent<int, double> onButtonReleased;

        public UnityEvent<int, double> onButtonPressedEx;
        public UnityEvent<int, double> onButtonReleasedEx;

        public UnityEvent onSpeedUpPressed;
        public UnityEvent onSpeedDownPressed;

        public UnityEvent onPausePressed;

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

        private void Loop5B()
        {
            var time = Time.fixedTimeAsDouble;

            for (var i = 0; i < schema.play.Length; i++)
            {
                var key = schema.play[i];
                var i1 = i;

                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    if (i1 == 0 | i1 == 1) 
                    {
                        onButtonPressedEx?.Invoke(i, Time.fixedTimeAsDouble);
                        // _tasks.Enqueue(() => onButtonPressed?.Invoke(i1, time));
                    }

                    if (i1 == 4 | i1 == 5)
                    {
                        onButtonPressedEx?.Invoke(i - 1, Time.fixedTimeAsDouble);
                        // _tasks.Enqueue(() => onButtonPressed?.Invoke(i1 - 1, time));
                    }

                    if (i1 == 2 | i1 == 3)
                    {
                        if (Keyboard.current[i1 == 2 ? schema.play[3] : schema.play[2]].isPressed)
                        {
                            onButtonReleasedEx?.Invoke(2, Time.fixedTimeAsDouble);
                            // _tasks.Enqueue(() => onButtonReleased?.Invoke(2, time));
                        }
                        onButtonPressedEx?.Invoke(2, Time.fixedTimeAsDouble);
                        // _tasks.Enqueue(() => onButtonPressed?.Invoke(2, time));

                        currentMiddle = i1;
                    }
                }

                if (Keyboard.current[key].wasReleasedThisFrame)
                {
                    if (i1 == 0 | i1 == 1)
                    {
                        onButtonReleasedEx?.Invoke(i, Time.fixedTimeAsDouble);
                        // _tasks.Enqueue(() => onButtonReleased?.Invoke(i1, time));
                    }

                    if (i1 == 4 | i1 == 5)
                    {
                        onButtonReleasedEx?.Invoke(i - 1, Time.fixedTimeAsDouble);
                        // _tasks.Enqueue(() => onButtonReleased?.Invoke(i1 - 1, time));
                    }

                    if (i1 == currentMiddle)
                    {
                        onButtonReleasedEx?.Invoke(2, Time.fixedTimeAsDouble);
                        // _tasks.Enqueue(() => onButtonReleased?.Invoke(2, time));
                    }
                }
            }
        }

        private void LoopNormal()
        {
            var time = Time.fixedTimeAsDouble;

            for (var i = 0; i < schema.play.Length; i++)
            {
                var key = schema.play[i];
                var i1 = i;

                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    onButtonPressedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    _tasks.Enqueue(() => onButtonPressed?.Invoke(i1, time));
                }

                if (Keyboard.current[key].wasReleasedThisFrame)
                {
                    onButtonReleasedEx?.Invoke(i, Time.fixedTimeAsDouble);
                    _tasks.Enqueue(() => onButtonReleased?.Invoke(i1, time));
                }
            }
        }


        // TODO FixedUpdate
        private void Update()
        {
            Loop();

            if (Keyboard.current[schema.speedUp].wasPressedThisFrame)
                _tasks.Enqueue(() => onSpeedUpPressed?.Invoke());

            if (Keyboard.current[schema.speedDown].wasPressedThisFrame)
                _tasks.Enqueue(() => onSpeedDownPressed?.Invoke());

            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
                _tasks.Enqueue(() => onPausePressed?.Invoke());

            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }
    }
}