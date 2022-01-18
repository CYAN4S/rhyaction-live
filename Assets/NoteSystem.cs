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
        private static Func<float> _yPos; // TODO: Use this.

        private RectTransform _rectTransform;
        private NoteData _noteData;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public static void StaticInitialize(float[] xPos, Func<float> yPos)
        {
            _xPos = xPos;
            _yPos = yPos;
        }

        public void InstanceInitialize(NoteData noteData)
        {
            _noteData = noteData;

            _rectTransform.localPosition = new Vector3(_xPos[_noteData.line], (float) _noteData.beat * 100);
        }

        private void Update()
        {
            _rectTransform.localPosition =
                new Vector3(_xPos[_noteData.line], (float) (_noteData.beat - Player.CurrentBeat) * 1000f);
        }
    }
}