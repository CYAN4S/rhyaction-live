using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        [SerializeField] private DoubleSO currentBeatSO;
        [SerializeField] private FloatSO scrollSpeedSO;

        // TODO DATA
        private int button = 4;
        private List<NoteData> _notes;
        private List<LongNoteData> _longNotes;

        public float CurrentTime { get; private set; }
        public double CurrentBeat { get; private set; }


        private List<NoteSystem> _currentLongNotes;

        public float rushToBreak;
        public float ignorable;
        public float missed;

        private float Delta(float time) => time - CurrentTime;
        private bool RushToBreak(float delta) => delta > rushToBreak && delta <= ignorable;
        private bool IsOk(float delta) => delta <= rushToBreak && delta >= missed;
        private bool Missed(float delta) => delta < missed;

        private NoteFactory _factory;

        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();

            _notes = new List<NoteData>();
            _longNotes = new List<LongNoteData>();
            _currentLongNotes = new List<NoteSystem>();

            // TODO DATA
            for (var i = 20; i < 30; i++)
                _notes.Add(new NoteData(new Fraction(i, 4), i % 4, null));
            for (var i = 0; i < 20; i += 4)
                _longNotes.Add(new LongNoteData(new Fraction(i, 4), i / 4 % 4, null, new Fraction(1, 2)));

            _factory = new NoteFactory(button, _notes, _longNotes, () => Instantiate(notePrefab, notesParent));

            // TODO DATA
            for (var i = 0; i < button; i++)
            {
                _currentLongNotes.Add(null);
            }

            // Value Initialize
            CurrentTime = -5f;
            _inputHandler.onButtonPressed.AddListener(OnButtonPressed);
            _inputHandler.onButtonIsPressed.AddListener(OnButtonIsPressed);
            _inputHandler.onButtonReleased.AddListener(OnButtonReleased);
        }

        private void OnButtonPressed(int btn)
        {
            var target = _factory.GetTarget(btn);
            if (target == null) return;
            
            var delta = Delta(target.Time);

            if (IsOk(delta))
            {
                Debug.Log("OK");
                _factory.DequeueTarget(btn);

                if (target.IsLongNote)
                {
                    Debug.Log("Long!");
                    _currentLongNotes[btn] = target;
                    target.AlertInProgress();
                }
                else
                {
                    target.gameObject.SetActive(false);
                }
            }
            else if (RushToBreak(delta))
            {
                Debug.Log("Too Early");
                _factory.DequeueTarget(btn);

                target.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Ignored");
            }
        }

        private void OnButtonIsPressed(int btn)
        {
            if (!_currentLongNotes[btn])
            {
                // TODO
                return;
            }
        }

        private void OnButtonReleased(int btn)
        {
            if (!_currentLongNotes[btn])
            {
                return;
            }

            Debug.Log("Released!");

            _currentLongNotes[btn].gameObject.SetActive(false);
            _currentLongNotes[btn] = null;
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime;
            // TODO MATH
            CurrentBeat = CurrentTime / 60d * 120d;
            currentBeatSO.Value = CurrentBeat;

            scrollSpeedSO.Value = scrollSpeed;
        }


        private void LateUpdate()
        {
            for (int i = 0; i < button; i++)
            {
                var target = _factory.GetTarget(i);
                
                if (target == null) continue;
                if (!Missed(Delta(target.Time))) continue;
                
                Debug.Log("Break");
                target.gameObject.SetActive(false);
                _factory.DequeueTarget(i);
            }
        }

        private void OnDestroy()
        {
            _inputHandler.onButtonPressed.RemoveListener(OnButtonPressed);
            _inputHandler.onButtonPressed.RemoveListener(OnButtonIsPressed);
            _inputHandler.onButtonPressed.RemoveListener(OnButtonReleased);
        }
    }

    public class NoteFactory
    {
        private List<List<NoteSystem>> _noteTemp;
        private List<Queue<NoteSystem>> _noteQueue;

        public NoteFactory(int button, List<NoteData> notes, List<LongNoteData> longNotes, Func<NoteSystem> instantiate)
        {
            _noteQueue = new List<Queue<NoteSystem>>();
            _noteTemp = new List<List<NoteSystem>>();

            for (var i = 0; i < button; i++)
            {
                _noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            foreach (var note in notes)
            {
                var noteSystem = instantiate();
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);

                _noteTemp[note.line].Add(noteSystem);
            }

            foreach (var note in longNotes)
            {
                var noteSystem = instantiate();
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
        }

        public NoteSystem GetTarget(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Peek();
        }

        public NoteSystem DequeueTarget(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Dequeue();
        }
    }
}