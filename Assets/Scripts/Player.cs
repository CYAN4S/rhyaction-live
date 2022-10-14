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
        
        [SerializeField] private List<NoteSystem> cachedNotes;
        
        // MonoBehaviour components.
        private InputHandler _ih;
        private AudioManager _a;
        
        // Pure C# classes.
        [SerializeField]
        private Chart chart;
        private NoteFactory f;
        private TimeManager t;

        [Header("In-game data")]
        [SerializeField]
        private int noteCount;
        [SerializeField]
        private int score = 0;
        [SerializeField]
        private int combo = 0;

        [SerializeField]
        public UnityEvent<int> scoreChanged;
        [SerializeField]
        public UnityEvent<int> comboIncreased;
        [SerializeField] 
        public UnityEvent broken;
        

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
            t = new TimeManager(chart.bpm);

            // Get component
            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();

            // Add listener
            _ih.onButtonPressed.AddListener(ButtonPressListener);
            _ih.onButtonReleased.AddListener(ButtonReleaseListener);

            ////

            // Create factory
            f = new NoteFactory(chart,
                () => Instantiate(notePrefab, notesParent),
                () => Instantiate(longNotePrefab, notesParent),
                target => target.gameObject.SetActive(false),
                () => t.Beat
            );

            ////

            // Cache from factory
            for (var i = 0; i < chart.button; i++)
                cachedNotes.Add(f.Get(i));
        }

        private void Update()
        {
            t.Update();

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
                    if (!Missed(system.EndTime - t.Time)) continue;

                    f.Release(target);
                    cachedNotes[i] = f.Get(i);
                    OnAddScore(1);

                    continue;
                }

                // Check missed notes.
                if (!Missed(target.Time - t.Time)) continue;

                f.Release(target);
                cachedNotes[i] = f.Get(i);
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
            _tasks.Enqueue(() => JudgeButtonPressed(btn, t.GetGameTime(rawTime)));
            _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => JudgeButtonReleased(btn, t.GetGameTime(rawTime)));
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
                    f.Release(target);
                    cachedNotes[btn] = f.Get(btn);
                    OnAddScore(100);
                    OnAddCombo(1);
                }

                return;
            }

            if (RushToBreak(delta))
            {
                f.Release(target);
                cachedNotes[btn] = f.Get(btn);
                
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

            f.Release(cachedNotes[btn]);
            cachedNotes[btn] = f.Get(btn);
        }

        private void OnTicked()
        {
            OnAddCombo(1);
            Debug.Log("tick");
        }
    }

    public enum Judgement
    {
        Precise = 0, Nice, Fair, Break = -1
    }
}