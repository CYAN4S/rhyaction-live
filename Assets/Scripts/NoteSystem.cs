using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class NoteSystem : MonoBehaviour
    {
        private static float[] _xPos;
        private static Func<Fraction, float> _yPos;
        private static Func<Fraction, float> _height;
        private static Func<Fraction, float> _heightIn;


        private RectTransform _rectTransform;
        private NoteData _noteData;
        private LongNoteData _longNoteData;
        public bool IsLongNote => _noteData is LongNoteData;
        
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
        }

        private void Update()
        {
            _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], _yPos(_noteData.beat));

            if (IsLongNote)
            {
                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _height(((LongNoteData)_noteData).length));
            }
        }
    }
}