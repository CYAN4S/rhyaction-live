using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteSystem : MonoBehaviour
    {
        public static Func<int> getScrollSpeed;
        protected Func<double> getBeat;

        // From init
        protected NoteData noteData;
        public double Time { get; protected set; }

        // Awake
        protected RectTransform rt;
        
        public void InstanceInitialize(NoteData data, double time, Func<double> beat)
        {
            noteData = data;
            Time = time;
            getBeat = beat;
        }
        
        protected virtual void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        protected virtual void Update()
        {
            rt.localPosition = new Vector3(rt.localPosition.x, GetYPos());
        }

        public static float GetYPos(double noteBeat, double currentBeat, int scrollSpeed)
        {
            return (float) (noteBeat - currentBeat) * 20f * scrollSpeed;
        }
        
        protected float GetYPos()
        {
            return GetYPos(noteData.beat, getBeat(), getScrollSpeed());
        }
    }
}