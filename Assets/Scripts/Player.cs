using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private NoteSystem notePrefab;
        [SerializeField] private RectTransform notesParent;

        [SerializeField] private JudgeStandardSO judgeStandard;
        [SerializeField] private DoubleSO currentBeatSO;
        [SerializeField] private DoubleSO currentTimeSO;
        [SerializeField] private FloatSO scrollSpeedSO;

        [SerializeField] [Range(1.0f, 9.9f)] private float scrollSpeed;

        [field: SerializeField] public double CurrentTime { get; private set; }
        [field: SerializeField] public double CurrentBeat { get; private set; }

        private InputHandler _ih;
        private NoteFactory _f;
        private List<NoteSystem> _cachedNotes;

        private double Delta(double time)
        {
            return time - CurrentTime;
        }

        private bool RushToBreak(double delta)
        {
            return delta > judgeStandard.rushToBreak && delta <= judgeStandard.ignorable;
        }

        private bool IsOk(double delta)
        {
            return delta <= judgeStandard.rushToBreak && delta >= judgeStandard.missed;
        }

        private bool Missed(double delta)
        {
            return delta < judgeStandard.missed;
        }

        private double TimeFromRaw(double rawTime)
        {
            return rawTime + currentTimeSO.initialValue;
        }

        // private readonly Queue<Tuple<int, double, bool>> _tasks = new();
        private readonly Queue<Action> _tasks = new();

        private Chart _chart;

        private void ButtonPressListener(int btn, double time)
        {
            _tasks.Enqueue(() => OnButtonPressed(btn, time));
        }

        private void ButtonReleaseListener(int btn, double time)
        {
            _tasks.Enqueue(() => OnButtonReleased(btn, time));
        }

        private void Awake()
        {
            //TODO
            _chart = Chart.GetTestChart();

            _ih = GetComponent<InputHandler>();
            _cachedNotes = new List<NoteSystem>(_chart.button);

            _f = new NoteFactory(_chart,
                () => Instantiate(notePrefab, notesParent),
                target => target.gameObject.SetActive(false)
            );

            for (var i = 0; i < _chart.button; i++)
                _cachedNotes.Add(_f.Get(i));

            // Value Initialize
            CurrentTime = currentTimeSO.initialValue;
            scrollSpeed = scrollSpeedSO.initialValue;

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
            var target = _cachedNotes[btn];
            if (target == null) return;

            var delta = target.Time - TimeFromRaw(time);

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
            CurrentTime = currentTimeSO.initialValue + Time.timeAsDouble;
            CurrentBeat = CurrentTime / 60d * 120d; // TODO MATH

            currentTimeSO.Value = CurrentTime;
            currentBeatSO.Value = CurrentBeat;
            scrollSpeedSO.Value = scrollSpeed;

            while (_tasks.Count != 0)
                _tasks.Dequeue().Invoke();
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _chart.button; i++)
            {
                var target = _cachedNotes[i];

                if (target == null) continue;
                if (target.IsLongNote && target.IsProgress) continue;
                if (!Missed(Delta(target.Time))) continue;

                Debug.Log("BREAK");
                _f.Release(target);
                _cachedNotes[i] = _f.Get(i);
            }
        }
    }

    public class NoteFactory
    {
        private readonly List<Queue<NoteSystem>> _noteQueue;
        private readonly Action<NoteSystem> _destroy;

        public NoteFactory(Chart chart, Func<NoteSystem> instantiate, Action<NoteSystem> destroy)
        {
            var button = chart.button;
            var notes = chart.notes;
            var longNotes = chart.longNotes;

            _noteQueue = new List<Queue<NoteSystem>>();
            var noteTemp = new List<List<NoteSystem>>();

            _destroy = destroy;

            for (var i = 0; i < button; i++)
            {
                noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            foreach (var note in notes)
            {
                var noteSystem = instantiate();
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);

                noteTemp[note.line].Add(noteSystem);
            }

            foreach (var note in longNotes)
            {
                var noteSystem = instantiate();
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);
                noteTemp[note.line].Add(noteSystem);
            }

            for (var i = 0; i < noteTemp.Count; i++)
            {
                var temp = noteTemp[i];
                temp.Sort((a, b) => a.Time.CompareTo(b.Time));
                _noteQueue[i] = new Queue<NoteSystem>(temp);
            }
        }

        public NoteSystem Get(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Dequeue();
        }

        public void Release(NoteSystem target)
        {
            _destroy(target);
        }
    }
}