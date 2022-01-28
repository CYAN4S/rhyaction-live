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

        // TODO DATA
        private int button = 4;
        private List<NoteData> _notes;
        private List<LongNoteData> _longNotes;

        private List<List<NoteSystem>> _noteTemp;

        public float CurrentTime { get; private set; }
        public double CurrentBeat { get; private set; }

        private List<Queue<NoteSystem>> _noteQueue;
        private List<NoteSystem> _currentLongNotes;

        public float rushToBreak;
        public float ignorable;
        public float missed;

        private float Delta(float time) => time - CurrentTime;
        private bool RushToBreak(float delta) => delta > rushToBreak && delta <= ignorable;
        private bool IsOk(float delta) => delta <= rushToBreak && delta >= missed;
        private bool Missed(float delta) => delta < missed;

        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();

            _notes = new List<NoteData>();
            _longNotes = new List<LongNoteData>();
            _noteQueue = new List<Queue<NoteSystem>>();
            _noteTemp = new List<List<NoteSystem>>();
            _currentLongNotes = new List<NoteSystem>();

            // TODO DATA
            for (var i = 0; i < 10; i++)
                _notes.Add(new NoteData(new Fraction(i, 4), i % 3 + 1, null));
            for (var i = 0; i < 10; i += 4)
                _longNotes.Add(new LongNoteData(new Fraction(i, 4), 0, null, new Fraction(1, 2)));

            // TODO DATA
            for (var i = 0; i < button; i++)
            {
                _noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
                _currentLongNotes.Add(null);
            }

            // TODO MATH
            NoteSystem.StaticInitialize(xPos,
                beat => (float) (beat - CurrentBeat) * 100f * scrollSpeed,
                beat => (float) beat * 100f * scrollSpeed,
                endBeat => (float) (endBeat - CurrentBeat) * 100f * scrollSpeed);

            foreach (var note in _notes)
            {
                var noteSystem = Instantiate(notePrefab, notesParent);
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);

                // _noteQueue[note.line].Enqueue(noteSystem);
                _noteTemp[note.line].Add(noteSystem);
            }

            foreach (var note in _longNotes)
            {
                var noteSystem = Instantiate(notePrefab, notesParent);
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);
                _noteTemp[note.line].Add(noteSystem);
            }

            for (var i = 0; i < _noteTemp.Count; i++)
            {
                var temp = _noteTemp[i];
                temp.Sort(((a, b) => a.Time.CompareTo(b.Time)));
                _noteQueue[i] = new Queue<NoteSystem>(temp);
            }

            // Value Initialize
            CurrentTime = -5f;
            _inputHandler.onButtonPressed.AddListener(OnButtonPressed);
            _inputHandler.onButtonPressed.AddListener(OnButtonIsPressed);
            _inputHandler.onButtonPressed.AddListener(OnButtonReleased);
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
                _noteQueue[btn].Dequeue();

                if (target.IsLongNote)
                {
                    Debug.Log("Long");
                    target.State = NoteState.LongCurrent;
                    _currentLongNotes[btn] = target;
                }
                else
                {
                    target.gameObject.SetActive(false);
                }
            }
            else if (RushToBreak(delta))
            {
                Debug.Log("Too Early");
                _noteQueue[btn].Dequeue();

                target.gameObject.SetActive(false);
            }
            else
            {
                // Debug.Log("Ignored");
            }
        }

        private void OnButtonIsPressed(int btn)
        {
            if (!_currentLongNotes[btn])
            {
                return;
            }
        }

        private void OnButtonReleased(int btn)
        {
            if (!_currentLongNotes[btn])
            {
                return;
            }

            _currentLongNotes[btn].gameObject.SetActive(false);
            _currentLongNotes[btn] = null;
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
                if (queue.Count == 0) continue;
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
            _inputHandler.onButtonPressed.RemoveListener(OnButtonIsPressed);
            _inputHandler.onButtonPressed.RemoveListener(OnButtonReleased);
        }
    }
}