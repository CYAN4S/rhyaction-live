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
        private InputHandler _inputHandler;

        [SerializeField] private float[] xPos;
        
        [SerializeField] private float[] judgeStandard;
        [SerializeField] [Range(1.0f, 9.9f)] private float scrollSpeed;

        private int button = 4;
        private List<NoteData> notes = new List<NoteData>();

        public static float CurrentTime { get; private set; }
        public static double CurrentBeat { get; private set; }

        private List<Queue<NoteSystem>> _noteQueue = new List<Queue<NoteSystem>>();


        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            
            // TODO DATA
            for (int i = 0; i < 1000; i++)
            {
                notes.Add(new NoteData(new Fraction(i, button), i % button, null));
            }

            // TODO DATA
            for (int i = 0; i < button; i++)
            {
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            // TODO MATH
            NoteSystem.StaticInitialize(xPos, beat => (float) (beat - CurrentBeat) * 100f * scrollSpeed);

            foreach (var note in notes)
            {
                var noteSystem = Instantiate(notePrefab, notesParent);
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);

                _noteQueue[note.line].Enqueue(noteSystem);
            }

            // Value Initialize
            CurrentTime = -5f;
            
            _inputHandler.onButtonPressed.AddListener(OnButtonPressed);
        }

        private void OnButtonPressed(int btn)
        {
            var target = _noteQueue[btn].Dequeue();
            
            Debug.Log(target.Time - CurrentTime);
            target.gameObject.SetActive(false);
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime;
            CurrentBeat = CurrentTime / 60d * 120d;
        }

        private void OnDestroy()
        {
            _inputHandler.onButtonPressed.RemoveListener(OnButtonPressed);
        }
    }
}