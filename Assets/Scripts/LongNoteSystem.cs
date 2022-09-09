using System;
using Core;

namespace CYAN4S
{
    public class LongNoteSystem : NoteSystem
    {
        private LongNoteTranslator _nt;
        public bool IsProgress => _nt.State == LongNoteState.Progress;
        public double EndTime { get; private set; }

        protected override void Update()
        {
            _nt.Update(getBeat(), 4);
        }

        public void InstanceInitialize(LongNoteData data, double startTime, double endTime, Func<double> beat)
        {
            noteData = data;
            Time = startTime;
            EndTime = endTime;
            getBeat = beat;

            _nt = new LongNoteTranslator(noteData, rectTransform);
        }

        public void OnProgress()
        {
            _nt.SetLongNoteState(LongNoteState.Progress);
        }
    }
}