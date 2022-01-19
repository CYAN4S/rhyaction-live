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
        [SerializeField] private float[] judgeStandard;
        [SerializeField] [Range(1.0f, 9.9f)] private float scrollSpeed;

        public List<NoteData> notes = new List<NoteData>();

        public static float CurrentTime { get; private set; }
        public static double CurrentBeat { get; private set; }

        private List<Queue<NoteSystem>> _noteQueue = new List<Queue<NoteSystem>>();


        private void Awake()
        {
            // TODO DATA
            for (int i = 0; i < 1000; i++)
            {
                notes.Add(new NoteData(new Fraction(i, 4), i % 4, null));
            }

            // TODO DATA
            for (int i = 0; i < 4; i++)
            {
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            // TODO MATH
            NoteSystem.StaticInitialize(xPos, beat => (float) (beat - CurrentBeat) * 100f * scrollSpeed);

            foreach (var note in notes)
            {
                var noteSystem = Instantiate(notePrefab, notesParent);
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float)note.beat * 0.5f);
                
                _noteQueue[note.line].Enqueue(noteSystem);
            }

            // Value Initialize
            CurrentTime = 0f;
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime;
            CurrentBeat = CurrentTime / 60d * 120d;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                _noteQueue[0].Dequeue().gameObject.SetActive(false);
            }
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                _noteQueue[1].Dequeue().gameObject.SetActive(false);
            }
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                _noteQueue[2].Dequeue().gameObject.SetActive(false);
            }
            if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
            {
                _noteQueue[3].Dequeue().gameObject.SetActive(false);
            }
        }
    }
}