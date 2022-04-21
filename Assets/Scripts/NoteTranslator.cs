using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteTranslator
    {
        private readonly RectTransform _rt;
        private readonly NoteType _type;
        private readonly NoteData _d;

        private Action<double, float> _onUpdate;

        private float[] _xPos = {-150f, -50f, 50f, 150f};

        public LongNoteState _state;

        public NoteTranslator(NoteData noteData, RectTransform rt, NoteType noteType)
        {
            if (noteType == NoteType.Long && noteData is not LongNoteData)
            {
                throw new Exception("noteData can convert into LongNoteType");
            }

            _d = noteData;
            _rt = rt;
            _type = noteType;
            _state = LongNoteState.Idle;

            _onUpdate += noteType switch
            {
                NoteType.Normal => UpdateNormal,
                NoteType.Long => UpdateLong,
                _ => throw new ArgumentOutOfRangeException(nameof(noteType), noteType, null)
            };
        }

        public void SetLongNoteState(LongNoteState state)
        {
            if (_type != NoteType.Long)
                throw new Exception("State translation is for long note.");

            switch (state)
            {
                case LongNoteState.Idle:
                    throw new Exception("No defined state translation to Idle.");
                case LongNoteState.Progress:
                    if (_state == LongNoteState.Idle)
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

            _state = state;
        }

        private void UpdateNormal(double currentBeat, float scrollSpeed)
        {
            _rt.localPosition = new Vector3(_xPos[_d.line], (float) (_d.beat - currentBeat) * 100f * scrollSpeed);
        }

        private void UpdateLong(double currentBeat, float scrollSpeed)
        {
            _rt.localPosition = new Vector3(_xPos[_d.line], (float) (_d.beat - currentBeat) * 100f * scrollSpeed);
            _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, (float) ((LongNoteData) _d).length * 100f * scrollSpeed);
        }

        private void UpdateActiveLong(double currentBeat, float scrollSpeed)
        {
            _rt.localPosition = new Vector3(_xPos[_d.line], 0);
            _rt.sizeDelta = new Vector2(_rt.sizeDelta.x,
                Mathf.Max((float) (((LongNoteData) _d).length + _d.beat - currentBeat) * 100f * scrollSpeed, 0f));
        }

        public void Update(double currentBeat, float scrollSpeed)
        {
            _onUpdate?.Invoke(currentBeat, scrollSpeed);
        }
    }
}