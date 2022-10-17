using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        [Header("Judgement")]
        public float rushToBreak;   // Always higher than 0.
        public float ignorable;     // Always higher than 0.
        public float missed;        // Always lower than 0.

        [Header("Debug")]
        [SerializeField] private List<NoteSystem> cachedNotes;
        [SerializeField] private Chart chart;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private int noteCount;
        [SerializeField] private int score = 0;
        [SerializeField] private int combo = 0;

        [Header("Observer Setup")]
        [SerializeField] public UnityEvent<int> scoreChanged;
        [SerializeField] public UnityEvent<int> comboIncreased;
        [SerializeField] public UnityEvent broken;

        // Transaction between FixedUpdate and Update.
        private readonly Queue<Action> _tasks = new();

        // MonoBehaviour components.
        private InputHandler _ih;
        private AudioManager _a;
        private NoteManager _n;
        
        // TODO
        public static Func<double> getBeat;

        private void OnAddScore(int add)
        {
            score += add;
            scoreChanged?.Invoke(score);
        }

        private void OnAddCombo(int add)
        {
            combo += add;
            comboIncreased?.Invoke(combo);
        }

        private void OnBreak()
        {
            ResetCombo();
            broken?.Invoke();
        }

        private void ResetCombo()
        {
            combo = 0;
        }

        private void Awake()
        {
            //TODO
            chart = Chart.GetTestChart();

            // Create using info from chart
            cachedNotes = new List<NoteSystem>(chart.button);
            noteCount = chart.notes.Count + chart.longNotes.Count;
            timeManager = new TimeManager(chart.bpm);
            getBeat = () => timeManager.Beat;

            // Get component
            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();
            _n = GetComponent<NoteManager>();

            // Add listener
            _ih.onButtonPressed.AddListener(ButtonPressListener);
            _ih.onButtonReleased.AddListener(ButtonReleaseListener);

            ////

            // Set NoteManager
            _n.Initialize();

            ////

            // Cache from factory
            for (var i = 0; i < chart.button; i++)
                cachedNotes.Add(_n.Get(i));
        }

        private void Update()
        {
            timeManager.Update();

            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }

        private void LateUpdate()
        {
            // Check for missed notes and unreleased long notes.
            for (var i = 0; i < chart.button; i++)
            {
                var target = cachedNotes[i];

                if (target is null) continue;

                // Check unreleased long notes.
                if (target is LongNoteSystem {IsInProgress: true} system)
                {
                    if (!Missed(system.EndTime - timeManager.Time)) continue;

                    _n.Release(target);
                    cachedNotes[i] = _n.Get(i);
                    OnAddScore(1);

                    continue;
                }

                // Check missed notes.
                if (!Missed(target.Time - timeManager.Time)) continue;

                _n.Release(target);
                cachedNotes[i] = _n.Get(i);
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
            _tasks.Enqueue(() => JudgeButtonPressed(btn, timeManager.GetGameTime(rawTime)));
            _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => JudgeButtonReleased(btn, timeManager.GetGameTime(rawTime)));
        }

        // Instead of using _t.Time, using time param is ideal.
        private void JudgeButtonPressed(int btn, double time)
        {
            var target = cachedNotes[btn];
            if (target == null) return;

            var delta = target.Time - time;

            if (IsOk(delta))
            {
                Debug.Log($"OK: {delta}");

                if (target is LongNoteSystem system)
                {
                    system.OnProgress();
                    system.SetTicks(time, OnTicked);
                }
                else
                {
                    // Note hit.
                    _n.Release(target);
                    cachedNotes[btn] = _n.Get(btn);
                    OnAddScore(100);
                    OnAddCombo(1);
                }

                return;
            }

            if (RushToBreak(delta))
            {
                _n.Release(target);
                cachedNotes[btn] = _n.Get(btn);

                OnBreak();
                return;
            }
        }

        private void JudgeButtonReleased(int btn, double time)
        {
            // Check only if note is long note, and is in progress.
            var target = cachedNotes[btn] as LongNoteSystem;

            if (!target) return;
            if (!target.IsInProgress) return;

            // Released so early.
            if (target.EndTime - time > rushToBreak)
            {
                OnBreak();
            }
            else
            {
                OnAddScore(100);
                OnAddCombo(1);
                Debug.Log("Released!");
            }

            _n.Release(cachedNotes[btn]);
            cachedNotes[btn] = _n.Get(btn);
        }

        private void OnTicked()
        {
            OnAddCombo(1);
            Debug.Log("tick");
        }
    }

    public enum Judgement
    {
        Precise = 0,
        Nice,
        Fair,
        Break = -1
    }
}