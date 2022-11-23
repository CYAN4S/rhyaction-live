using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteSystem : MonoBehaviour
    {
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
            rt.localPosition = new Vector3(rt.localPosition.x, GetYPos(noteData.beat, getBeat(), 4));
        }

        protected static float GetYPos(double noteBeat, double currentBeat, float scrollSpeed)
        {
            return (float) (noteBeat - currentBeat) * 200f * scrollSpeed;
        }
    }
}