using System;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class TimeManager
    {
        private const double PrepareTime = 5d;
        private decimal _bpm;

        public TimeManager(decimal bpm)
        {
            _bpm = bpm;
            Time = -PrepareTime;
            Beat = -PrepareTime / 60d * (double) bpm;
        }

        [field: SerializeField]
        public double Time { get; private set; }
        
        [field: SerializeField]
        public double Beat { get; private set; }

        public void Update()
        {
            Time = UnityEngine.Time.timeAsDouble - PrepareTime;
            Beat = Time / 60d * (double) _bpm;
        }

        public double GetGameTime(double rawTime)
        {
            return rawTime - PrepareTime;
        }
    }
}