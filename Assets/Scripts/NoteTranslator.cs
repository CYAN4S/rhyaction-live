using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteTranslator
    {
        public const float Multiply = 200f;
        
        protected readonly RectTransform rt;

        protected readonly float[] xPos = {-150f, -50f, 50f, 150f};

        protected readonly int line;
        protected readonly double beat;

        public NoteTranslator(NoteData noteData, RectTransform rectTransform)
        {
            line = noteData.line;
            beat = noteData.beat;

            rt = rectTransform;
        }

        public static float GetYPos(double noteBeat, double currentBeat, float scrollSpeed)
        {
            return (float) (noteBeat - currentBeat) * Multiply * scrollSpeed;
        }

        public virtual void Update(double currentBeat, float scrollSpeed)
        {
            rt.localPosition = new Vector3(xPos[line], GetYPos(beat, currentBeat, scrollSpeed));
        }
    }

    public class LongNoteTranslator : NoteTranslator
    {
        private Action<double, float> _onUpdate;

        protected readonly double length;

        public LongNoteTranslator(LongNoteData noteData, RectTransform rt) : base(noteData, rt)
        {
            length = noteData.length;
            
            _onUpdate += UpdateLong;
        }

        public override void Update(double currentBeat, float scrollSpeed)
        {
            _onUpdate?.Invoke(currentBeat, scrollSpeed);
        }

        public void SetLongNoteState(LongNoteState state)
        {
            switch (state)
            {
                case LongNoteState.Idle:
                    throw new Exception("No defined state translation to Idle.");
                case LongNoteState.InProgress:
                    _onUpdate -= UpdateLong;
                    _onUpdate += UpdateActiveLong;
                    break;
                case LongNoteState.Missed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public static float GetYSize(double noteLength, float scrollSpeed)
        {
            return (float) noteLength * 100f * scrollSpeed;
        }

        private void UpdateLong(double currentBeat, float scrollSpeed)
        {
            rt.localPosition = new Vector3(xPos[line], GetYPos(beat, currentBeat, scrollSpeed));
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, (float) (length * Multiply * scrollSpeed));
        }

        private void UpdateActiveLong(double currentBeat, float scrollSpeed)
        {
            rt.localPosition = new Vector3(xPos[line], 0);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,
                Mathf.Max((float) (length + beat - currentBeat) * Multiply * scrollSpeed, 0f));
        }
    }
}