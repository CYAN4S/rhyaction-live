using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteSystem : MonoBehaviour
    {
        private NoteTranslator _nt;
        protected Func<double> getBeat;

        // From init
        protected NoteData noteData;

        // Awake
        protected RectTransform rectTransform;
        public double Time { get; protected set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Update()
        {
            _nt.Update(getBeat(), 4);
        }

        public void InstanceInitialize(NoteData data, double time, Func<double> beat)
        {
            noteData = data;
            Time = time;
            getBeat = beat;

            _nt = new NoteTranslator(noteData, rectTransform);
        }
    }

    public enum LongNoteState
    {
        Idle, // -> Progress, Missed
        InProgress, // -> Cut, Missed, DEACTIVATE
        Missed, // -> DEACTIVATE
        Cut // -> Progress, Missed
    }
}