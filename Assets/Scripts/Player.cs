using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private NoteSystem notePrefab;
        [SerializeField] private LongNoteSystem longNotePrefab;
        [SerializeField] private RectTransform notesParent;

        // Always higher than 0.
        public float rushToBreak;
        public float ignorable;

        // Always lower than 0.
        public float missed;

        // Transaction between FixedUpdate and Update.
        private readonly Queue<Action> _tasks = new();
        
        private List<NoteSystem> _cachedNotes;
        
        // MonoBehaviour components.
        private InputHandler _ih;
        private AudioManager _a;
        
        // Pure C# classes.
        private Chart _chart;
        private NoteFactory _f;
        private TimeManager _t;

        // In-game data.
        private int _noteCount;

        [SerializeField]
        public UnityEvent<int> onScoreChanged;
        private int _score = 0;

        [SerializeField]
        public UnityEvent<int> onComboIncreased;
        [SerializeField] 
        public UnityEvent onBreak;
        private int _combo = 0;

        private void AddScore(int add)
        {
            _score += add;
            onScoreChanged?.Invoke(_score);
        }

        private void AddCombo(int add)
        {
            _combo += add;
            onComboIncreased?.Invoke(_combo);
        }

        private void ResetCombo()
        {
            _combo = 0;
            onBreak?.Invoke();
        }

        private void PerformBreak()
        {
            ResetCombo();
        }

        private void Awake()
        {
            //TODO
            _chart = Chart.GetTestChart();

            // Create using info from chart
            _cachedNotes = new List<NoteSystem>(_chart.button);
            _noteCount = _chart.notes.Count + _chart.longNotes.Count;
            _t = new TimeManager(_chart.bpm);

            // Get component
            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();

            // Add listener
            _ih.onButtonPressed.AddListener(ButtonPressListener);
            _ih.onButtonReleased.AddListener(ButtonReleaseListener);

            ////

            // Create factory
            _f = new NoteFactory(_chart,
                () => Instantiate(notePrefab, notesParent),
                () => Instantiate(longNotePrefab, notesParent),
                target => target.gameObject.SetActive(false),
                () => _t.Beat
            );

            ////

            // Cache from factory
            for (var i = 0; i < _chart.button; i++)
                _cachedNotes.Add(_f.Get(i));
        }

        private void Update()
        {
            _t.Update();

            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }

        private void LateUpdate()
        {
            // Check for missed notes and unreleased long notes.
            for (var i = 0; i < _chart.button; i++)
            {
                var target = _cachedNotes[i];

                if (target is null) continue;

                // Check unreleased long notes.
                if (target is LongNoteSystem {IsInProgress: true} system)
                {
                    if (!Missed(system.EndTime - _t.Time)) continue;

                    _f.Release(target);
                    _cachedNotes[i] = _f.Get(i);
                    AddScore(1);

                    continue;
                }

                // Check missed notes.
                if (!Missed(target.Time - _t.Time)) continue;

                _f.Release(target);
                _cachedNotes[i] = _f.Get(i);
            }
        }

        private void OnDestroy()
        {
            // Remove listener
            _ih.onButtonPressed.RemoveListener(ButtonPressListener);
            _ih.onButtonPressed.RemoveListener(ButtonReleaseListener);
        }

        // Delta == 'time of NOTE' - 'time of GAME'
        private bool RushToBreak(double delta) => delta > rushToBreak && delta <= ignorable;
        private bool IsOk(double delta) => delta <= rushToBreak && delta >= missed;
        private bool Missed(double delta) => delta < missed;

        private void ButtonPressListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => OnButtonPressed(btn, _t.GetGameTime(rawTime)));
            _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => OnButtonReleased(btn, _t.GetGameTime(rawTime)));
        }

        // Instead of using _t.Time, using time param is ideal.
        private void OnButtonPressed(int btn, double time)
        {
            var target = _cachedNotes[btn];
            if (target == null) return;

            var delta = target.Time - time;

            if (IsOk(delta))
            {
                Debug.Log($"OK: {delta}");

                if (target is LongNoteSystem system)
                {
                    system.OnProgress();
                    system.SetTicks(time, OnTick);
                }
                else
                {
                    // Note hit.
                    _f.Release(target);
                    _cachedNotes[btn] = _f.Get(btn);
                    AddScore(100);
                    AddCombo(1);
                }

                return;
            }

            if (RushToBreak(delta))
            {
                _f.Release(target);
                _cachedNotes[btn] = _f.Get(btn);
                
                PerformBreak();
                return;
            }
        }

        private void OnButtonReleased(int btn, double time)
        {
            // Check only if note is long note, and is in progress.
            var target = _cachedNotes[btn] as LongNoteSystem;
            
            if (!target) return;
            if (!target.IsInProgress) return;
            
            // Released so early.
            if (target.EndTime - time > rushToBreak)
            {
                PerformBreak();
            }
            else
            {
                AddScore(100);
                AddCombo(1);
                Debug.Log("Released!");
            }

            _f.Release(_cachedNotes[btn]);
            _cachedNotes[btn] = _f.Get(btn);
        }

        private void OnTick()
        {
            AddCombo(1);
            Debug.Log("tick");
        }
    }

    public enum Judgement
    {
        Precise = 0, Nice, Fair, Break = -1
    }
}