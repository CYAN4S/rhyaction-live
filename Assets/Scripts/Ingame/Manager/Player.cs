using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

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
        [SerializeField] private int score;
        [SerializeField] private int combo;
        [SerializeField] private int scrollSpeed = 40;
        [SerializeField] private Timer timer;
        [SerializeField] private int[,] judgeCount = new int[Enum.GetNames(typeof(Judgement)).Length, 2];
        
        [Header("Observer Setup")] 
        [SerializeField] public UnityEvent<int> scoreChanged;
        [SerializeField] public UnityEvent<int> comboIncreased;
        [SerializeField] public UnityEvent<Judgement, bool, int> judged;
        [SerializeField] public UnityEvent<int> speedChanged;
        [SerializeField] public UnityEvent paused;
        [SerializeField] public UnityEvent resuming;
        [SerializeField] public UnityEvent resumed;

        [Header("Audio")]
        [SerializeField] private FMODUnity.EventReference speedUpEvent;
        [SerializeField] private FMODUnity.EventReference speedDownEvent;
        [SerializeField] private FMODUnity.EventReference pausedEvent;
        [SerializeField] private FMODUnity.EventReference resumingEvent;

        [Header("Score Weight")] 
        [SerializeField] private int[] scoreWeight;
        
        // Getter
        public static Func<double> getBeat;
        public static Func<int> getScrollSpeed;

        // MonoBehaviour components.
        private InputHandler _ih;
        private NoteManager _n;

        private Channel _channel;

        private void Awake()
        {
            // Chart is from the previous scene via `Selected` singleton object.
            _chart = Selected.Instance.chart;
            
            // Check if is for debugging
            if (Selected.Instance.situation == Situation.Debug)
            {
                _chart = Chart.GetTestChart();
                Debug.Log("Debugging only.");
            }
            
            // Set getters
            getBeat = () => timer.CurrentBeat;
            getScrollSpeed = () => scrollSpeed;

            // Create space for cache
            cachedNotes = new List<NoteSystem>(_chart.button);
            cachedJudges = new List<Judgement>(_chart.button);
            
            // Use info from chart
            noteCount = _chart.notes.Count + _chart.longNotes.Count;

            // Get component
            _ih = GetComponent<InputHandler>();
            _n = GetComponent<NoteManager>();
            
            // Set Timer
            timer = new Timer();
            timer.SetTimer(_chart.bpm, _chart.GetEndBeat());

            timer.state.finished.OnEnter += OnFinished;
            timer.state.paused.OnEnter += () =>
            {
                paused?.Invoke();
                _channel.setPaused(true);
                FMODUnity.RuntimeManager.PlayOneShot(pausedEvent);
            };
            timer.state.resuming.OnEnter += () =>
            {
                resuming?.Invoke();
                FMODUnity.RuntimeManager.PlayOneShot(resumingEvent);
            };
            timer.state.resuming.OnExit += () =>
            {
                resumed?.Invoke();
                _channel.setPaused(false);
            };
            
            
            // Prepare sound
            if (_chart.audio != "")
            {
                var sound = AudioManager.PrepareSound(_chart.audio);
                if (sound is Sound s)
                    timer.onZero += () =>
                    {
                        _channel = AudioManager.PlaySound(s);
                    };
            }
            
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
                    if (system.EndTime - timer.CurrentTime >= tooLate) continue;

                    OnJudge(Judgement.Poor, false, JudgeTarget.LongNoteEnd, i);
                    Release(target);
                    cachedNotes[i] = Get(i);

                    continue;
                }

                // Check missed notes.
                if (target.Time - timer.CurrentTime >= tooLate) continue;

                OnJudge(Judgement.Break, false, JudgeTarget.Note, i);
                Release(target);
                cachedNotes[i] = Get(i);
            }
        }
        
        private void ButtonPressListener(int btn, double rawTime)
        {
            JudgeButtonPressed(btn, timer.GetGameTime(rawTime));
            // TODO Key sound
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            JudgeButtonReleased(btn, timer.GetGameTime(rawTime));
        }

        private void OnJudge(Judgement judgement, bool isEarly, JudgeTarget target, int line)
        {
            if (target is JudgeTarget.Note or JudgeTarget.LongNoteEnd && judgement != Judgement.Break)
                AddScore(scoreWeight[(int) judgement]);

            if (judgement == Judgement.Break)
                ResetCombo();
            else
                AddCombo(1);
            
            judged.Invoke(judgement, isEarly, line);
        }

        private void JudgeButtonPressed(int btn, double time)
        {
            var target = cachedNotes[btn];
            if (target == null) return;

            // TODO
            // var delta = target.Time - time;
            var delta = target.Time - timer.CurrentTime;

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
                
                system.SetActive(timer.TimeToBeat(timer.CurrentTime), () => OnTicked(result, isEarly, btn));
                
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

            var delta = target.EndTime - timer.CurrentTime;
            
            Release(cachedNotes[btn]);
            cachedNotes[btn] = Get(btn);

            // Released so early.
            if (delta > tooEarly)
            {
                OnJudge(Judgement.Break, true, JudgeTarget.LongNoteEnd, btn);
            }
            else
            {
                OnJudge(cachedJudges[btn], cachedIsEarly[btn], JudgeTarget.LongNoteEnd, btn);
            }
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

        public void OnPauseKeyPressed()
        {
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
            Result.Instance.judgeCount = judgeCount;
            SceneManager.LoadScene("Result");
        }

        public void OnSpeedUp()
        {
            if (scrollSpeed >= 99)
                return;
            
            scrollSpeed += 1;
            speedChanged?.Invoke(scrollSpeed);
            FMODUnity.RuntimeManager.PlayOneShot(speedUpEvent);
        }

        public void OnSpeedDown()
        {
            if (scrollSpeed <= 5)
                return;
            
            scrollSpeed -= 1;
            speedChanged?.Invoke(scrollSpeed);
            FMODUnity.RuntimeManager.PlayOneShot(speedDownEvent);
        }
    }
    
    public enum Judgement { Precise, Great, Fair, Poor, Break }

    public enum JudgeTarget { Note, LongNoteStart, LongNoteTick, LongNoteEnd }
}