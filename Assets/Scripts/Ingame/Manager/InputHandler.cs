using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace CYAN4S
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerInput _input;
        
        // TODO MODE
        [SerializeField] private Key[] keys4B;
        [SerializeField] private Key key4BSpeedUp;
        [SerializeField] private Key key4BSpeedDown;
        
        [SerializeField] private Key[] keys6B;
        [SerializeField] private Key key6BSpeedUp;
        [SerializeField] private Key key6BSpeedDown;
        
        private Key[] keys;
        private Key keySpeedUp;
        private Key keySpeedDown;

        [Header("Inspector Setup")]
        public UnityEvent<int, double> onButtonPressed;
        public UnityEvent<int, double> onButtonIsPressed;
        public UnityEvent<int, double> onButtonReleased;

        public UnityEvent<int, double> onButtonPressedEx;
        public UnityEvent<int, double> onButtonIsPressedEx;
        public UnityEvent<int, double> onButtonReleasedEx;

        public UnityEvent onSpeedUpPressed;
        public UnityEvent onSpeedDownPressed;

        public UnityEvent onPausePressed;
        
        private readonly Queue<Action> _tasks = new();

        private void Awake()
        {
            // Chart is from the previous scene via `Selected` singleton object.
            var chart = Selected.Instance.chart;
            
            // Check if is for debugging
            if (Selected.Instance.situation == Situation.Debug)
            {
                chart = Chart.GetTestChart();
                Debug.Log("Debugging only.");
            }

            (keys, keySpeedUp, keySpeedDown) = chart.button switch
            {
                4 => (keys4B, key4BSpeedUp, key4BSpeedDown),
                6 => (keys6B, key6BSpeedUp, key6BSpeedDown),
                _ => throw new Exception("Unsupported Button")
            };
        }

        // TODO FixedUpdate
        private void Update()
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
            
            if (Keyboard.current[key4BSpeedUp].wasPressedThisFrame)
                _tasks.Enqueue(() => onSpeedUpPressed?.Invoke());
            
            if (Keyboard.current[key4BSpeedDown].wasPressedThisFrame)
                _tasks.Enqueue(() => onSpeedDownPressed?.Invoke());

            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
                _tasks.Enqueue(() => onPausePressed?.Invoke());
            
            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }
    }
}