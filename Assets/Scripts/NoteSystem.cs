using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public enum NoteState
    {
        Plain,
        Long,
        LongCurrent
    }

    public class NoteSystem : MonoBehaviour
    {
        private static float[] _xPos;
        private static Func<Fraction, float> _yPos;
        private static Func<Fraction, float> _height;
        private static Func<Fraction, float> _heightIn;


        private RectTransform _rectTransform;
        private NoteData _noteData;
        private LongNoteData _longNoteData;
        private NoteState _state;

        public bool IsLongNote => _noteData is LongNoteData;

        public NoteState State
        {
            get => _state;
            set => _state = value;
        }

        public float Time { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public static void StaticInitialize(float[] xPos, Func<Fraction, float> yPos, Func<Fraction, float> height, Func<Fraction, float> heightIn)
        {
            _xPos = xPos;
            _yPos = yPos;
            _height = height;
            _heightIn = heightIn;
        }

        public void InstanceInitialize(NoteData noteData, float time)
        {
            _noteData = noteData;
            Time = time;
            _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], 0f);
            State = NoteState.Plain;

            if (_noteData is LongNoteData longNoteData)
            {
                State = NoteState.Long;
                _longNoteData = longNoteData;
            }
        }

        private void Update()
        {
            switch (State)
            {
                case NoteState.Plain:
                    _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], _yPos(_noteData.beat));
                    break;
                case NoteState.Long:
                    _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], _yPos(_noteData.beat));
                    _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _height(((LongNoteData)_noteData).length));
                    break;
                case NoteState.LongCurrent:
                    _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], 0);
                    _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _height(_noteData.beat + ((LongNoteData)_noteData).length));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}