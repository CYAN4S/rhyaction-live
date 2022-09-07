using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private NoteSystem notePrefab;
        [SerializeField] private RectTransform notesParent;
        
        public float rushToBreak = 0.15f;
        public float ignorable = 0.2f;
        public float missed = -0.15f;

        private InputHandler _ih;
        private NoteFactory _f;
        private AudioManager _a;
        private TimeManager _t;
        private List<NoteSystem> _cachedNotes;

        private double Delta(double time) => time - _t.Time;
        private bool RushToBreak(double delta) => delta > rushToBreak && delta <= ignorable;
        private bool IsOk(double delta) => delta <= rushToBreak && delta >= missed;
        private bool Missed(double delta) => delta < missed;

        private readonly Queue<Action> _tasks = new();

        private Chart _chart;

        private void ButtonPressListener(int btn, double time)
        {
            _tasks.Enqueue(() => OnButtonPressed(btn, time - 5));
            _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double time)
        {
            _tasks.Enqueue(() => OnButtonReleased(btn, time - 5));
        }

        private void Awake()
        {
            //TODO
            _chart = Chart.GetTestChart();

            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();
            _cachedNotes = new List<NoteSystem>(_chart.button);

            _f = new NoteFactory(_chart,
                () => Instantiate(notePrefab, notesParent),
                target => target.gameObject.SetActive(false),
                () => _t.Beat
            );

            _t = new TimeManager(_chart.bpm);

            for (var i = 0; i < _chart.button; i++)
                _cachedNotes.Add(_f.Get(i));

            _ih.onButtonPressed.AddListener(ButtonPressListener);
            _ih.onButtonReleased.AddListener(ButtonReleaseListener);
        }

        private void OnDestroy()
        {
            _ih.onButtonPressed.RemoveListener(ButtonPressListener);
            _ih.onButtonPressed.RemoveListener(ButtonReleaseListener);
        }

        private void OnButtonPressed(int btn, double time)
        {
            Debug.Log(time);
            
            var target = _cachedNotes[btn];
            if (target == null) return;

            var delta = target.Time - time;

            if (IsOk(delta))
            {
                Debug.Log($"OK: {delta}");
                if (target.IsLongNote)
                {
                    target.OnProgress();
                }
                else
                {
                    _f.Release(target);
                    _cachedNotes[btn] = _f.Get(btn);
                }
            }
            else if (RushToBreak(delta))
            {
                _f.Release(target);
                _cachedNotes[btn] = _f.Get(btn);
                Debug.Log($"TOO EARLY: {delta}");
            }
        }

        private void OnButtonReleased(int btn, double time)
        {
            if (!_cachedNotes[btn]) return;
            if (!_cachedNotes[btn].IsLongNote) return;
            if (!_cachedNotes[btn].IsProgress) return;

            Debug.Log("Released!");

            _f.Release(_cachedNotes[btn]);
            _cachedNotes[btn] = _f.Get(btn);
        }

        private void Update()
        {
            _t.Update();
            
            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _chart.button; i++)
            {
                var target = _cachedNotes[i];

                if (target is null) continue;
                if (target.IsLongNote && target.IsProgress) continue;
                if (!Missed(Delta(target.Time))) continue;

                Debug.Log("BREAK");
                _f.Release(target);
                _cachedNotes[i] = _f.Get(i);
            }
        }
    }
}