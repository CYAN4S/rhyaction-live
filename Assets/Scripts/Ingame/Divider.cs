using System;
using UnityEngine;

namespace CYAN4S
{
    public class Divider : MonoBehaviour
    {
        // Awake
        private RectTransform _rt;
        private double _beat;
        public static Func<double> getCurrentBeat;
        public static Func<int> getScrollSpeed;

        protected virtual void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }
        
        public void InstanceInitialize(double beat)
        {
            _beat = beat;
        }
        
        private void Update()
        {
            _rt.localPosition = new Vector3(0, NoteSystem.GetYPos(_beat, getCurrentBeat(), getScrollSpeed()));
        }

    }
}
