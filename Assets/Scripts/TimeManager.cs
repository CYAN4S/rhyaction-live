using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private double initialTime = -5d;
        
        [Header("Debug")]
        [SerializeField] private bool isRunning;

        private decimal _bpm;

        private void Awake()
        {
            isRunning = false;
            Time = initialTime;
        }

        public void SetBpm(decimal bpm)
        {
            _bpm = bpm;
            Beat = initialTime / 60d * (double) bpm;
            isRunning = true;
        }

        [field: SerializeField] public double Time { get; private set; }
        [field: SerializeField] public double Beat { get; private set; }

        public void Update()
        {
            if (!isRunning) return;
            
            Time = UnityEngine.Time.timeAsDouble + initialTime;
            Beat = Time / 60d * (double) _bpm;
        }

        public double GetGameTime(double rawTime)
        {
            return rawTime + initialTime;
        }

        public double TimeToBeat(double time)
        {
            return time / 60d * (double) _bpm;
        }

        public double BeatToTime(double beat)
        {
            return beat * 60d / (double) _bpm;
        }
    }
}