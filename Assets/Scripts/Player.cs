using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
        private Chart _chart;

        [Header("Judgement")] 
        public float ignorable;
        public float tooEarly;
        public float fairEarly;
        public float greatEarly;
        public float preciseEarly;
        public float preciseLate;
        public float greatLate;
        public float fairLate;
        public float tooLate;

        [Header("Inspector Setup")]
        [Tooltip("For the next scene")]
        public Result result;

        [Header("Cached Data")]
        [Tooltip("Current target note of its line")]
        [SerializeField] private List<NoteSystem> cachedNotes;
        [Tooltip("Start judge of currently activated long note")]
        [SerializeField] private List<Judgement> cachedJudges;
        [Tooltip("Start judge of currently activated long note")]
        [SerializeField] private List<bool> cachedIsEarly;
        
        // Not Serializable
        private List<Queue<NoteSystem>> _noteQueue;

        [Header("In-game Data")] 
        [SerializeField] private int noteCount;
        [SerializeField] private int score = 0;
        [SerializeField] private int combo = 0;
        [SerializeField] private Timer timer;
        [SerializeField] private int scrollSpeed = 40;
        
        [Header("Observer Setup")] 
        [SerializeField] public UnityEvent<int> scoreChanged;
        [SerializeField] public UnityEvent<int> comboIncreased;
        [SerializeField] public UnityEvent<Judgement, bool, int> judged;
        [SerializeField] public UnityEvent<int> speedChanged;
        
        [Header("Getter")]
        public static Func<double> getBeat;
        public static Func<int> getScrollSpeed;
        
        // MonoBehaviour components.
        private InputHandler _ih;
        private AudioManager _a;
        private NoteManager _n;

        private void Awake()
        {
            _chart = Selected.Instance.chart;
            
            // Check if is for debugging
            if (Selected.Instance.situation == Situation.Debug)
            {
                _chart = Chart.GetTestChart();
                Debug.Log("Debugging only.");
            }
            
            // Set getters
            getBeat = () => timer.Beat;
            getScrollSpeed = () => scrollSpeed;

            // Create space for cache
            cachedNotes = new List<NoteSystem>(_chart.button);
            cachedJudges = new List<Judgement>(_chart.button);
            
            // Use info from chart
            noteCount = _chart.notes.Count + _chart.longNotes.Count;

            // Get component
            _ih = GetComponent<InputHandler>();
            _a = GetComponent<AudioManager>();
            _n = GetComponent<NoteManager>();
            
            // Set Timer
            timer = new Timer();
            timer.SetTimer(_chart.bpm, _chart.GetEndBeat());
            timer.OnFinished += OnFinished;

            // Set NoteManager
            _noteQueue = _n.Initialize(_chart);

            // Cache
            for (var i = 0; i < _chart.button; i++)
            {
                cachedNotes.Add(Get(i));
                cachedJudges.Add(Judgement.Precise);
                cachedIsEarly.Add(false);
            }
            
            // Add listener
            _ih.onButtonPressedEx.AddListener(ButtonPressListener);
            _ih.onButtonReleasedEx.AddListener(ButtonReleaseListener);
        }
        
        private void OnDestroy()
        {
            _ih.onButtonPressedEx.RemoveListener(ButtonPressListener);
            _ih.onButtonPressedEx.RemoveListener(ButtonReleaseListener);
        }
        
        private void Update()
        {
            timer.Update();
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
                    if (system.EndTime - timer.Time >= tooLate) continue;

                    OnJudge(Judgement.Poor, false, JudgeTarget.LongNoteEnd, i);
                    Release(target);
                    cachedNotes[i] = Get(i);

                    continue;
                }

                // Check missed notes.
                if (target.Time - timer.Time >= tooLate) continue;
                
                OnJudge(Judgement.Break, false, JudgeTarget.Note, i);
                Release(target);
                cachedNotes[i] = Get(i);
            }
        }
        
        private void ButtonPressListener(int btn, double rawTime)
        {
            JudgeButtonPressed(btn, timer.GetGameTime(rawTime));
            _a.PlaySoundNAudio();
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            JudgeButtonReleased(btn, timer.GetGameTime(rawTime));
        }

        private void OnJudge(Judgement judgement, bool isEarly, JudgeTarget target, int line)
        {
            if (target is JudgeTarget.Note or JudgeTarget.LongNoteEnd && judgement != Judgement.Break)
                AddScore((int) judgement);

            if (judgement == Judgement.Break)
                ResetCombo();
            else
                AddCombo(1);
            
            judged.Invoke(judgement, isEarly, line);
        }

        // Instead of using _t.Time, using time param is ideal.
        private void JudgeButtonPressed(int btn, double time)
        {
            var target = cachedNotes[btn];
            if (target == null) return;

            // TODO
            // var delta = target.Time - time;
            var delta = target.Time - timer.Time;

            if (delta >= ignorable || delta < tooLate)
                return;
            
            Judgement result;
            if      (delta >      tooEarly) result = Judgement.Break;
            else if (delta >     fairEarly) result = Judgement.Poor;
            else if (delta >    greatEarly) result = Judgement.Fair;
            else if (delta >  preciseEarly) result = Judgement.Great;
            else if (delta >= preciseLate)  result = Judgement.Precise;
            else if (delta >=   greatLate)  result = Judgement.Great;
            else if (delta >=    fairLate)  result = Judgement.Fair;
            else                            result = Judgement.Poor;

            var isEarly = delta >= 0;

            if (target is LongNoteSystem system)
            {
                OnJudge(result, isEarly, JudgeTarget.LongNoteStart, btn);
                
                if (result == Judgement.Break)
                {
                    Release(target);
                    cachedNotes[btn] = Get(btn);
                    return;
                }
                
                system.SetActive(timer.TimeToBeat(timer.Time), () => OnTicked(result, isEarly, btn));
                
                cachedJudges[btn] = result;
                cachedIsEarly[btn] = isEarly;
            }
            else // target is NoteSystem
            {
                OnJudge(result, isEarly, JudgeTarget.Note, btn);
                
                Release(target);
                cachedNotes[btn] = Get(btn);
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
                OnJudge(Judgement.Break, true, JudgeTarget.LongNoteEnd, btn);
            }
            else
            {
                OnJudge(cachedJudges[btn], cachedIsEarly[btn], JudgeTarget.LongNoteEnd, btn);
            }

            Release(cachedNotes[btn]);
            cachedNotes[btn] = Get(btn);
        }

        private void OnTicked(Judgement result, bool isEarly, int line)
        {
            OnJudge(result, isEarly, JudgeTarget.LongNoteTick, line);
        }

        private NoteSystem Get(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Dequeue();
        }

        private void Release(NoteSystem target)
        {
            target.gameObject.SetActive(false);
        }

        public void OnPause()
        {
            Debug.Log("Esc Pressed");
            timer.PauseOrResume();
        } 
        
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

        private void ResetCombo()
        {
            combo = 0;
        }

        private void OnFinished()
        {
            Result.Instance.score = score;
            SceneManager.LoadScene("Result");
        }

        public void OnSpeedUp()
        {
            if (scrollSpeed >= 99)
                return;
            
            scrollSpeed += 1;
            Debug.Log(scrollSpeed);
            speedChanged?.Invoke(scrollSpeed);
        }

        public void OnSpeedDown()
        {
            if (scrollSpeed <= 5)
                return;
            
            scrollSpeed -= 1;
            Debug.Log(scrollSpeed);
            speedChanged?.Invoke(scrollSpeed);
        }
    }

    public enum Judgement
    {
        Precise = 100,
        Great = 50,
        Fair = 20,
        Poor = 1,
        Break = -1
    }

    public enum JudgeTarget
    {
        Note,
        LongNoteStart,
        LongNoteTick,
        LongNoteEnd
    }
}