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
        
        [SerializeField] private JudgeStandardSO judgeStandard;
        [SerializeField] private DoubleSO currentBeatSO;
        [SerializeField] private FloatSO currentTimeSO;
        [SerializeField] private FloatSO scrollSpeedSO;
        
        [SerializeField] [Range(1.0f, 9.9f)] private float scrollSpeed;

        [field: SerializeField] public float CurrentTime { get; private set; }
        [field: SerializeField] public double CurrentBeat { get; private set; }
        
        private InputHandler _inputHandler;
        private List<NoteSystem> _cachedNotes;
        private NoteFactory _factory;

        private float Delta(float time) => time - CurrentTime;
        private bool RushToBreak(float delta) => delta > judgeStandard.rushToBreak && delta <= judgeStandard.ignorable;
        private bool IsOk(float delta) => delta <= judgeStandard.rushToBreak && delta >= judgeStandard.missed;
        private bool Missed(float delta) => delta < judgeStandard.missed;
        
        // TODO DATA
        private int button = 4;
        private List<NoteData> _notes;
        private List<LongNoteData> _longNotes;

        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();

            _notes = new List<NoteData>();
            _longNotes = new List<LongNoteData>();
            _cachedNotes = new List<NoteSystem>(button);

            // TODO DATA
            for (var i = 20; i < 30; i++)
                _notes.Add(new NoteData(new Fraction(i, 4), i % 4, null));
            for (var i = 0; i < 20; i += 4)
                _longNotes.Add(new LongNoteData(new Fraction(i, 4), i / 4 % 4, null, new Fraction(1, 2)));

            _factory = new NoteFactory(button, _notes, _longNotes, () => Instantiate(notePrefab, notesParent));

            // TODO DATA
            for (var i = 0; i < button; i++)
            {
                _cachedNotes.Add(_factory.Get(i));
            }

            // Value Initialize
            CurrentTime = currentTimeSO.initialValue;
            scrollSpeed = scrollSpeedSO.initialValue;

            _inputHandler.onButtonPressed.AddListener(OnButtonPressed);
            _inputHandler.onButtonReleased.AddListener(OnButtonReleased);
        }
        
        private void OnDestroy()
        {
            _inputHandler.onButtonPressed.RemoveListener(OnButtonPressed);
            _inputHandler.onButtonPressed.RemoveListener(OnButtonReleased);
        }

        private void OnButtonPressed(int btn)
        {
            var target = _cachedNotes[btn];
            if (target == null) return;

            var delta = Delta(target.Time);

            if (IsOk(delta))
            {
                Debug.Log("OK");
                
                if (target.IsLongNote)
                {
                    Debug.Log("Long!");
                    target.OnProgress();
                }
                else
                {
                    _factory.Release(target);
                    _cachedNotes[btn] = _factory.Get(btn);
                }
            }
            else if (RushToBreak(delta))
            {
                Debug.Log("Too Early");

                _factory.Release(target);
                _cachedNotes[btn] = _factory.Get(btn);
            }
            else
            {
                Debug.Log("Ignored");
            }
        }

        private void OnButtonReleased(int btn)
        {
            if (!_cachedNotes[btn]) return;
            if (!_cachedNotes[btn].IsLongNote) return;
            if (!_cachedNotes[btn].IsProgress) return;

            Debug.Log("Released!");

            _factory.Release(_cachedNotes[btn]);
            _cachedNotes[btn] = _factory.Get(btn);
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime;
            CurrentBeat = CurrentTime / 60d * 120d; // TODO MATH
            
            currentTimeSO.Value = CurrentTime;
            currentBeatSO.Value = CurrentBeat;
            scrollSpeedSO.Value = scrollSpeed;
        }

        private void LateUpdate()
        {
            for (var i = 0; i < button; i++)
            {
                var target = _cachedNotes[i];

                if (target == null) continue;
                if (target.IsLongNote && target.IsProgress) continue;
                if (!Missed(Delta(target.Time))) continue;

                Debug.Log("Break");
                _factory.Release(target);
                _cachedNotes[i] = _factory.Get(i);
            }
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

        public NoteSystem Get(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Dequeue();
        }

        public void Release(NoteSystem target)
        {
            target.gameObject.SetActive(false);
        }
    }

    public class ChartReader
    {
        // TODO
    }
}