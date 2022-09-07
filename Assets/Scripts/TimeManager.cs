using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class TimeManager
    {
        private decimal _bpm;

        public double Time { get; private set; }
        public double Beat { get; private set; }

        public TimeManager(decimal bpm)
        {
            _bpm = bpm;
            Time = -5;
            Beat = -5d / 60d * (double)bpm;
        }

        public void Update()
        {
            Time = UnityEngine.Time.timeAsDouble - 5d;
            Beat = Time / 60d * (double)_bpm;
        }
    }
}