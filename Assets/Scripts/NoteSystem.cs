using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteSystem : MonoBehaviour
    {
        // Awake
        private RectTransform _rectTransform;
        
        // From init
        private NoteData _noteData;
        public double Time { get; private set; }
        private NoteTranslator _nt;
        private Func<double> _beat;

        // Public
        public bool IsLongNote => _noteData is LongNoteData;
        public bool IsProgress => (_nt as LongNoteTranslator)?.State == LongNoteState.Progress;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void InstanceInitialize(NoteData noteData, double time, NoteType noteType, Func<double> beat)
        {
            _noteData = noteData;
            Time = time;

            _nt = noteType switch
            {
                NoteType.Normal => new NoteTranslator(_noteData, _rectTransform),
                NoteType.Long => new LongNoteTranslator(_noteData, _rectTransform),
                _ => throw new ArgumentOutOfRangeException(nameof(noteType), noteType, null)
            };
            
            _beat = beat;
        }

        public void OnProgress()
        {
            (_nt as LongNoteTranslator)?.SetLongNoteState(LongNoteState.Progress);
        }

        private void Update()
        {
            _nt.Update(_beat(), 4);
        }
    }

    public enum NoteType
    {
        Normal,
        Long
    }

    public enum LongNoteState
    {
        Idle, // -> Progress, Missed
        Progress, // -> Cut, Missed, DEACTIVATE
        Missed, // -> DEACTIVATE
        Cut // -> Progress, Missed
    }
}