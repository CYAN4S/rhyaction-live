using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        private Chart _chart;
        
        [Header("Judgement")]
        public float ignorable;     // Always higher than 0.
        public float rushToBreak;   // Always higher than 0.
        public float fairEarly;
        public float greatEarly;
        public float greatLate;
        public float fairLate;
        public float missed;        // Always lower than 0.

        [SerializeField] private List<NoteSystem> cachedNotes;
        
        [Header("In-game Data")]
        [SerializeField] private int noteCount;
        [SerializeField] private int score = 0;
        [SerializeField] private int combo = 0;

        [Header("Observer Setup")]
        [SerializeField] public UnityEvent<int> scoreChanged;
        [SerializeField] public UnityEvent<int> comboIncreased;
        [SerializeField] public UnityEvent broken;
        [SerializeField] public UnityEvent<Judgement, bool> judged;

        // Transaction between FixedUpdate and Update.
        private readonly Queue<Action> _tasks = new();

        // MonoBehaviour components.
        private InputHandler _ih;
        private AudioManager _a;
        private NoteManager _n;
        private TimeManager _t;
        
        // TODO
        public static Func<double> getBeat;
        private List<Queue<NoteSystem>> _noteQueue;

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
            _chart = IngameDebugger.chart;

            // Create using info from chart
            cachedNotes = new List<NoteSystem>(_chart.button);
            noteCount = _chart.notes.Count + _chart.longNotes.Count;
            getBeat = () => _t.Beat;

            // Get component
            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();
            _n = GetComponent<NoteManager>();
            _t = GetComponent<TimeManager>();

            // Add listener
            _ih.onButtonPressed.AddListener(ButtonPressListener);
            _ih.onButtonReleased.AddListener(ButtonReleaseListener);

            ////

            // Set NoteManager
            _noteQueue = _n.Initialize();
            _t.SetBpm(_chart.bpm);

            ////

            // Cache from factory
            for (var i = 0; i < _chart.button; i++)
                cachedNotes.Add(Get(i));
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
                var target = cachedNotes[i];

                if (target is null) continue;

                // Check unreleased long notes.
                if (target is LongNoteSystem {IsInProgress: true} system)
                {
                    if (!Missed(system.EndTime - _t.Time)) continue;

                    Release(target);
                    cachedNotes[i] = Get(i);
                    OnAddScore(1);

                    continue;
                }

                // Check missed notes.
                if (!Missed(target.Time - _t.Time)) continue;

                Release(target);
                cachedNotes[i] = Get(i);
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
            _tasks.Enqueue(() => JudgeButtonPressed(btn, _t.GetGameTime(rawTime)));
            // _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => JudgeButtonReleased(btn, _t.GetGameTime(rawTime)));
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
                    system.SetTicks(_t.TimeToBeat(time), OnTicked);
                }
                else
                {
                    // Note hit.
                    Release(target);
                    cachedNotes[btn] = Get(btn);
                    OnAddScore(100);
                }
                
                OnAddCombo(1);

                return;
            }

            if (RushToBreak(delta))
            {
                Release(target);
                cachedNotes[btn] = Get(btn);

                OnBreak();
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

            Release(cachedNotes[btn]);
            cachedNotes[btn] = Get(btn);
        }

        private void OnTicked()
        {
            OnAddCombo(1);
        }


        private NoteSystem Get(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Dequeue();
        }

        private void Release(NoteSystem target)
        {
            target.gameObject.SetActive(false);
        }
    }

    public enum Judgement
    {
        Precise = 0,
        Great,
        Fair,
        Poor,
        Break = -1,
    }
}