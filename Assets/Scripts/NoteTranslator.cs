using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public interface ITransformByTime
    {
        
    }

    public interface IHasState
    {
        
    }
    
    public class NoteTranslator
    {
        protected readonly RectTransform rt;
        protected readonly NoteData d;

        protected readonly float[] xPos = {-150f, -50f, 50f, 150f};

        public NoteTranslator(NoteData noteData, RectTransform rt)
        {
            d = noteData;
            this.rt = rt;
        }

        public virtual void Update(double currentBeat, float scrollSpeed)
        {
            rt.localPosition = new Vector3(xPos[d.line], (float) (d.beat - currentBeat) * 100f * scrollSpeed);
        }
    }

    public class LongNoteTranslator : NoteTranslator
    {
        private Action<double, float> _onUpdate;
        public LongNoteState State { get; private set; }

        public LongNoteTranslator(NoteData noteData, RectTransform rt) : base(noteData, rt)
        {
            State = LongNoteState.Idle;
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
                case LongNoteState.Progress:
                    if (this.State == LongNoteState.Idle)
                    {
                        _onUpdate -= UpdateLong;
                        _onUpdate += UpdateActiveLong;
                    }

                    break;
                case LongNoteState.Missed:
                    break;
                case LongNoteState.Cut:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            this.State = state;
        }
        
        private void UpdateLong(double currentBeat, float scrollSpeed)
        {
            rt.localPosition = new Vector3(xPos[d.line], (float) (d.beat - currentBeat) * 100f * scrollSpeed);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, (float) ((LongNoteData) d).length * 100f * scrollSpeed);
        }

        private void UpdateActiveLong(double currentBeat, float scrollSpeed)
        {
            rt.localPosition = new Vector3(xPos[d.line], 0);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,
                Mathf.Max((float) (((LongNoteData) d).length + d.beat - currentBeat) * 100f * scrollSpeed, 0f));
        }
    }
}