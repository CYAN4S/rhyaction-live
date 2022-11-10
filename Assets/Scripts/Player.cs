using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        private Chart _chart;

        [Header("Judgement")] public float ignorable;
        public float tooEarly;
        public float fairEarly;
        public float greatEarly;
        public float greatLate;
        public float fairLate;
        public float missed;

        [SerializeField] private List<NoteSystem> cachedNotes;
        [SerializeField] private List<Judgement> cachedLongNoteJudges;
        [SerializeField] private List<bool> cachedLongNoteIsEarly;

        [Header("In-game Data")] [SerializeField]
        private int noteCount;

        [SerializeField] private int score = 0;
        [SerializeField] private int combo = 0;

        [Header("Observer Setup")] [SerializeField]
        public UnityEvent<int> scoreChanged;

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

        private bool TooEarly(double delta) => delta > tooEarly && delta <= ignorable;
        private bool IsOk(double delta) => delta <= tooEarly && delta >= missed;
        private bool Missed(double delta) => delta < missed;


        private void AddScore(int add)
        {
            score += add;
            scoreChanged?.Invoke(score);
        }

        private void AddCombo(int add)
        {
            combo += add;
            comboIncreased?.Invoke(combo);
        }

        private void Break()
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
            _chart = Chart.GetTestChart();
            getBeat = () => _t.Beat;

            // Create using info from chart
            cachedNotes = new List<NoteSystem>(_chart.button);
            cachedLongNoteJudges = new List<Judgement>(_chart.button);
            noteCount = _chart.notes.Count + _chart.longNotes.Count;

            // Get component
            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();
            _n = GetComponent<NoteManager>();
            _t = GetComponent<TimeManager>();

            // Add listener
            _ih.onButtonPressedEx.AddListener(ButtonPressListener);
            _ih.onButtonReleasedEx.AddListener(ButtonReleaseListener);

            ////

            // Set NoteManager
            _noteQueue = _n.Initialize(_chart);
            _t.SetBpm(_chart.bpm);

            ////

            // Cache from factory
            for (var i = 0; i < _chart.button; i++)
            {
                cachedNotes.Add(Get(i));
                cachedLongNoteJudges.Add(Judgement.Precise);
                cachedLongNoteIsEarly.Add(false);
            }
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
                    AddScore(1);

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
            _ih.onButtonPressedEx.RemoveListener(ButtonPressListener);
            _ih.onButtonPressedEx.RemoveListener(ButtonReleaseListener);
        }

        // Delta == 'time of NOTE' - 'time of GAME'

        private void ButtonPressListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => JudgeButtonPressed(btn, _t.GetGameTime(rawTime)));
            // _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            _tasks.Enqueue(() => JudgeButtonReleased(btn, _t.GetGameTime(rawTime)));
        }

        private void OnJudge(Judgement judgement, bool isEarly, JudgeTarget target)
        {
            switch (judgement)
            {
                case Judgement.Precise:
                    break;
                case Judgement.Great:
                    break;
                case Judgement.Fair:
                    break;
                case Judgement.Poor:
                    break;
                case Judgement.Break:
                    Break();
                    break;
                default:
                    break;
            }
            
            AddCombo(1);
            judged.Invoke(judgement, isEarly);
        }

        // Instead of using _t.Time, using time param is ideal.
        private void JudgeButtonPressed(int btn, double time)
        {
            var target = cachedNotes[btn];
            if (target == null) return;

            var delta = target.Time - time;

            if (delta >= ignorable || delta < missed)
                return;

            if (delta > tooEarly)
            {
                Release(target);
                cachedNotes[btn] = Get(btn);
                
                OnJudge(Judgement.Break, true, JudgeTarget.Note);
                
                return;
            }

            Judgement result;
            if (delta > fairEarly) result = Judgement.Fair;
            else if (delta > greatEarly) result = Judgement.Great;
            else if (delta > greatLate) result = Judgement.Precise;
            else if (delta > fairEarly) result = Judgement.Great;
            else result = Judgement.Fair;

            var isEarly = delta >= 0;

            if (target is LongNoteSystem system)
            {
                system.OnProgress();
                system.SetTicks(_t.TimeToBeat(time), () => OnTicked(result, isEarly));
                cachedLongNoteJudges[btn] = result;
                cachedLongNoteIsEarly[btn] = isEarly;
                OnJudge(result, isEarly, JudgeTarget.LongNoteStart);
            }
            else // target is NoteSystem
            {
                Release(target);
                cachedNotes[btn] = Get(btn);
                OnJudge(result, isEarly, JudgeTarget.Note);
            }
        }

        private void JudgeButtonReleased(int btn, double time)
        {
            // Check only if note is long note, and is in progress.
            var target = cachedNotes[btn] as LongNoteSystem;

            if (!target) return;
            if (!target.IsInProgress) return;

            var delta = target.EndTime - time;

            // Released so early.
            if (delta > tooEarly)
            {
                OnJudge(Judgement.Break, true, JudgeTarget.LongNoteEnd);
            }
            else
            {
                OnJudge(cachedLongNoteJudges[btn], cachedLongNoteIsEarly[btn], JudgeTarget.LongNoteEnd);
            }

            Release(cachedNotes[btn]);
            cachedNotes[btn] = Get(btn);
        }

        private void OnTicked(Judgement result, bool isEarly)
        {
            AddCombo(1);
            OnJudge(result, isEarly, JudgeTarget.LongNoteTick);
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

    public enum JudgeTarget
    {
        Note,
        LongNoteStart,
        LongNoteTick,
        LongNoteEnd
    }
}