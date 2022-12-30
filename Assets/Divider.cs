using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class Divider : MonoBehaviour
    {
        // Awake
        private RectTransform _rt;
        private double _beat;
        private Func<double> _currentBeat;
        public static Func<int> getScrollSpeed;

        protected virtual void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }
        
        public void InstanceInitialize(double beat, Func<double> currentBeat)
        {
            _beat = beat;
            _currentBeat = currentBeat;
        }
        
        private void Update()
        {
            _rt.localPosition = new Vector3(0, NoteSystem.GetYPos(_beat, _currentBeat(), getScrollSpeed()));
        }

    }
}
