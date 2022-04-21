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
        [SerializeField] private DoubleChannelSO currentBeatChannelSO;
        [SerializeField] private DoubleChannelSO currentTimeChannelSO;
        [SerializeField] private FloatChannelSO scrollSpeedSO;

        [SerializeField] [Range(1.0f, 9.9f)] private float scrollSpeed;

        private InputHandler _ih;
        private NoteFactory _f;
        private AudioManager _a;
        private TimeManager _t;
        private List<NoteSystem> _cachedNotes;

        private double Delta(double time) => time - currentTimeChannelSO.value;
        private bool RushToBreak(double delta) => delta > judgeStandard.rushToBreak && delta <= judgeStandard.ignorable;
        private bool IsOk(double delta) => delta <= judgeStandard.rushToBreak && delta >= judgeStandard.missed;
        private bool Missed(double delta) => delta < judgeStandard.missed;
        private double TimeFromRaw(double rawTime) => rawTime + currentTimeChannelSO.initialValue;

        private readonly Queue<Action> _tasks = new();

        private Chart _chart;

        private void ButtonPressListener(int btn, double time)
        {
            _tasks.Enqueue(() => OnButtonPressed(btn, time));
            _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double time)
        {
            _tasks.Enqueue(() => OnButtonReleased(btn, time));
        }

        private void Awake()
        {
            //TODO
            _chart = Chart.GetTestChart();
            Debug.Log(JsonUtility.ToJson(_chart, true));

            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();
            _cachedNotes = new List<NoteSystem>(_chart.button);

            _f = new NoteFactory(_chart,
                () => Instantiate(notePrefab, notesParent),
                target => target.gameObject.SetActive(false)
            );

            _t = new TimeManager(_chart.bpms, currentTimeChannelSO, currentBeatChannelSO);

            for (var i = 0; i < _chart.button; i++)
                _cachedNotes.Add(_f.Get(i));

            // Value Initialize
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
            _t.Update();
            
            scrollSpeedSO.value = scrollSpeed;

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
}