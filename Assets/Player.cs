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
        [SerializeField] [Range(1.0f, 9.9f)] private float scrollSpeed;

        private int button = 4;
        private List<NoteData> _notes;

        public static float CurrentTime { get; private set; }
        public static double CurrentBeat { get; private set; }

        private List<Queue<NoteSystem>> _noteQueue;

        public float rushToBreak = 0.1f;
        public float ignorable = 0.15f;
        public float missed = -0.1f;

        private float Delta(float time) => time - CurrentTime;
        private bool RushToBreak(float delta) => delta > rushToBreak && delta <= ignorable;
        private bool IsOk(float delta) => delta <= rushToBreak && delta >= missed;
        private bool Missed(float delta) => delta < missed;
        
        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();

            _notes = new List<NoteData>();
            _noteQueue = new List<Queue<NoteSystem>>();

            // TODO DATA
            for (var i = 0; i < 1000; i++)
            {
                _notes.Add(new NoteData(new Fraction(i, button), i % button, null));
            }

            // TODO DATA
            for (var i = 0; i < button; i++)
            {
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            // TODO MATH
            NoteSystem.StaticInitialize(xPos, beat => (float) (beat - CurrentBeat) * 100f * scrollSpeed);

            foreach (var note in _notes)
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
            if (_noteQueue[btn].Count == 0)
                return;
            
            var target = _noteQueue[btn].Peek();

            var delta = Delta(target.Time);

            if (IsOk(delta))
            {
                Debug.Log("OK");
                target.gameObject.SetActive(false);
                _noteQueue[btn].Dequeue();
            }
            else if (RushToBreak(delta))
            {
                Debug.Log("Too Early");
                target.gameObject.SetActive(false);
                _noteQueue[btn].Dequeue();
            }
            else
            {
                // Debug.Log("Ignored");
            }
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime;
            // TODO MATH
            CurrentBeat = CurrentTime / 60d * 120d;
        }

        private void LateUpdate()
        {
            foreach (var queue in _noteQueue)
            {
                var target = queue.Peek();

                if (!Missed(Delta(target.Time))) continue;
                
                Debug.Log("Break");
                target.gameObject.SetActive(false);
                queue.Dequeue();
            }
        }

        private void OnDestroy()
        {
            _inputHandler.onButtonPressed.RemoveListener(OnButtonPressed);
        }
    }
}