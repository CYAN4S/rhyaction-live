using System;
using System.Collections.Generic;
using UnityEngine;

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

        private readonly Queue<Action> _tasks = new();
        private AudioManager _a;
        private List<NoteSystem> _cachedNotes;

        private Chart _chart;
        private NoteFactory _f;

        private InputHandler _ih;
        private TimeManager _t;

        private void Awake()
        {
            //TODO
            _chart = Chart.GetTestChart();

            // Create using info from chart
            _cachedNotes = new List<NoteSystem>(_chart.button);
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
                if (target is LongNoteSystem system && system.IsProgress)
                {
                    if (!Missed(system.EndTime - _t.Time)) continue;

                    Debug.Log($"1% {system.EndTime} {_t.Time}");
                    _f.Release(target);
                    _cachedNotes[i] = _f.Get(i);

                    continue;
                }

                // Check missed notes.
                if (!Missed(target.Time - _t.Time)) continue;

                Debug.Log($"BREAK {target.Time} {_t.Time}");
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

        private double Delta(double time)
        {
            return time - _t.Time;
        }

        // Delta == 'time of NOTE' - 'time of GAME'
        private bool RushToBreak(double delta)
        {
            return delta > rushToBreak && delta <= ignorable;
        }

        private bool IsOk(double delta)
        {
            return delta <= rushToBreak && delta >= missed;
        }

        private bool Missed(double delta)
        {
            return delta < missed;
        }

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

            // JUST FOR BENCHMARKING
            // var delay = time - _t.Time;
            // Debug.Log(delay);

            if (IsOk(delta))
            {
                Debug.Log($"OK: {delta}");

                if (target is LongNoteSystem system)
                {
                    system.OnProgress();
                    Debug.Log("On progress");
                }
                else
                {
                    // Note hit.
                    _f.Release(target);
                    _cachedNotes[btn] = _f.Get(btn);
                }

                return;
            }

            if (RushToBreak(delta))
            {
                _f.Release(target);
                _cachedNotes[btn] = _f.Get(btn);
                Debug.Log($"TOO EARLY: {delta}");
                return;
            }

            Debug.Log($"Ignored. {delta}");
        }

        private void OnButtonReleased(int btn, double time)
        {
            // Check only if note is long note, and is in progress.
            if (!_cachedNotes[btn]) return;
            if (_cachedNotes[btn] is not LongNoteSystem) return;
            if (!((LongNoteSystem) _cachedNotes[btn]).IsProgress) return;

            Debug.Log("Released!");

            _f.Release(_cachedNotes[btn]);
            _cachedNotes[btn] = _f.Get(btn);
        }
    }
}