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

        private RectTransform _rectTransform;
        private NoteData _noteData;
        
        public float Time { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public static void StaticInitialize(float[] xPos, Func<Fraction, float> yPos)
        {
            _xPos = xPos;
            _yPos = yPos;
        }

        public void InstanceInitialize(NoteData noteData, float time)
        {
            _noteData = noteData;
            Time = time;
            _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], 0f);
        }

        private void Update()
        {
            _rectTransform.localPosition =
                // new Vector3(_xPos[_noteData.line], (float) (_noteData.beat - Player.CurrentBeat) * 1000f);
                new Vector3(_xPos[_noteData.line], _yPos(_noteData.beat));
        }
    }
}