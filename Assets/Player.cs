using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private NoteSystem notePrefab;
        [SerializeField] private RectTransform notesParent;
        
        [SerializeField] private float[] xPos;

        public List<NoteData> notes = new List<NoteData>();
        public static float CurrentTime { get; private set; }
        public static double CurrentBeat { get; private set; }

        private void Awake()
        {
            // TODO
            for (int i = 0; i < 100; i++)
            {
                notes.Add(new NoteData(new Fraction(i, 4), i % 4, null));
            }

            NoteSystem.StaticInitialize(xPos, () => 255f);

            foreach (var note in notes)
            {
                var noteSystem = Instantiate(notePrefab, notesParent);
                noteSystem.InstanceInitialize(note);
            }

            // Value Initialize
            CurrentTime = 0f;
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime;
            CurrentBeat = CurrentTime / 60d * 120d;
        }
    }
}