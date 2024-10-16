using System;
using System.Collections.Generic;
using FMOD;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace CYAN4S
{
    public class Player : MonoBehaviour
    {
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

        [Header("Visual")]
        public GearScriptableObject gearset;
        public RectTransform gearTransform;
        private Gear gear;

        [Header("Cached Data")]
        [Tooltip("Current target note of its line")]
        [SerializeField] private List<NoteSystem> cachedNotes;
        [Tooltip("Cached long note delta")]
        [SerializeField] private List<double> cachedDelta;
        [Tooltip("Cached long note cut time")]
        [SerializeField] private List<double> cachedCutTime;
        
        // Not Serializable
        private List<Queue<NoteSystem>> noteQueue;

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
        [SerializeField] private FMODUnity.EventReference clapEvent;

        [Header("Score Weight")] 
        [SerializeField] private int[] scoreWeight;
        
        // Getter
        public static Func<double> getBeat;
        public static Func<int> getScrollSpeed;

        // MonoBehaviour components.
        private InputHandler inputHandler;
        private NoteManager noteManager;

        private Chart chart;
        private Channel channel;
        private double pausedTime;
        private double pausedBeat;
        
        // TODO
        private Dictionary<string, Sound> sounds = new();

        private void Awake()
        {
            // Chart is from the previous scene via `Selected` singleton object.
            chart = Selected.Instance.chart;
            
            // Set getters
            getBeat = () => timer.CurrentBeat;
            getScrollSpeed = () => scrollSpeed;

            // Create space for cache
            cachedNotes = new List<NoteSystem>(chart.button);
            cachedDelta = new List<double>(chart.button);
            cachedCutTime = new List<double>(chart.button);
            
            // Use info from chart
            noteCount = chart.notes.Count + chart.longNotes.Count;

            // Get component
            inputHandler = GetComponent<InputHandler>();
            noteManager = GetComponent<NoteManager>();
            
            // Set Timer
            timer = new Timer();
            timer.SetTimer(chart.bpm, chart.GetEndBeat());

            timer.state.finished.OnEnter += OnFinished;
            timer.state.paused.OnEnter += OnPaused;
            timer.state.resuming.OnEnter += () =>
            {
                resuming?.Invoke();
                FMODUnity.RuntimeManager.PlayOneShot(resumingEvent);
            };
            timer.state.resuming.OnExit += () =>
            {
                resumed?.Invoke();
                channel.setPaused(false);
            };
            
            // Prepare sound
            if (chart.audio != "")
            {
                var sound = AudioManager.PrepareSound(chart.audio, chart.rootPath);
                if (sound is Sound s)
                    timer.onZero += () => channel = AudioManager.PlaySound(s);
            }

            foreach (var note in chart.notes)
            {
                if (sounds.ContainsKey(note.audioPath))
                {
                    continue;
                }
                
                var sound = AudioManager.PrepareSound(note.audioPath, chart.rootPath);
                if (sound is Sound s)
                    sounds.Add(note.audioPath, s);
            }

            // Set Gear
            var targetGear = (chart.button) switch 
            {
                4 => gearset.prefab4B,
                5 => gearset.prefab5B,
                6 => gearset.prefab6B,
                8 => gearset.prefab8B,
                _ => throw new Exception("Error here!")
            };
            gear = Instantiate<Gear>(targetGear, gearTransform);
            
            // Set NoteManager
            noteQueue = noteManager.Initialize(chart, gear);

            // Cache
            for (var i = 0; i < chart.button; i++)
            {
                cachedNotes.Add(Get(i));
                cachedDelta.Add(0);
                cachedCutTime.Add(0);
            }
            
            // Add listener
            inputHandler.onButtonPressedEx.AddListener(ButtonPressListener);
            inputHandler.onButtonReleasedEx.AddListener(ButtonReleaseListener);
        }
        
        private void OnDestroy()
        {
            inputHandler.onButtonPressedEx.RemoveListener(ButtonPressListener);
            inputHandler.onButtonPressedEx.RemoveListener(ButtonReleaseListener);
        }
        
        private void Update()
        {
            timer.Update();
        }

        private void LateUpdate()
        {
            if (timer.Current is not (Running or Resuming))
                return;
            
            // Check for missed notes and unreleased long notes.
            for (var i = 0; i < chart.button; i++)
            {
                var target = cachedNotes[i];

                if (target is null) continue;

                // Check unreleased long notes.
                if (target is LongNoteSystem { Current: ActiveLongNoteState } system)
                {
                    if (system.EndTime - timer.CurrentTime >= tooLate) continue;
                        
                    Judge(Judgement.Poor, false, JudgeTarget.LongNoteEnd, i);
                    Release(target);
                    cachedNotes[i] = Get(i);
                        
                    continue;
                }

                // Check missed notes.
                if (target is LongNoteSystem { Current: CutLongNoteState } note)
                {
                    if (note.cutTime - timer.CurrentTime >= tooLate)
                        continue;
                }
                else if (target.Time - timer.CurrentTime >= tooLate) 
                    continue;

                Judge(Judgement.Break, false, JudgeTarget.Note, i);
                Release(target);
                cachedNotes[i] = Get(i);
            }
        }
        
        private void ButtonPressListener(int btn, double rawTime)
        {
            JudgeButtonPressed(btn, timer.GetGameTime(rawTime));
            // FMODUnity.RuntimeManager.PlayOneShot(clapEvent);
        }

        private void ButtonReleaseListener(int btn, double rawTime)
        {
            JudgeButtonReleased(btn, timer.GetGameTime(rawTime));
        }

        private void Judge(Judgement judgement, bool isEarly, JudgeTarget target, int line)
        {
            if (target is JudgeTarget.Note or JudgeTarget.LongNoteEnd && judgement != Judgement.Break)
            {
                AddScore(scoreWeight[(int) judgement]);
                judgeCount[(int)judgement, isEarly ? 0 : 1]++;
            }

            if (judgement == Judgement.Break)
            {
                ResetCombo();
                judgeCount[(int)judgement, isEarly ? 0 : 1]++;
            }
            else if (target != JudgeTarget.LongNoteCut)
            {
                AddCombo(1);
            }

            judged.Invoke(judgement, isEarly, line);
        }

        private Judgement GetJudgement(double delta)
        {
            if (delta >      tooEarly) return Judgement.Break;
            if (delta >     fairEarly) return Judgement.Poor;
            if (delta >    greatEarly) return Judgement.Fair;
            if (delta >  preciseEarly) return Judgement.Great;
            if (delta >= preciseLate)  return Judgement.Precise;
            if (delta >=   greatLate)  return Judgement.Great;
            if (delta >=    fairLate)  return Judgement.Fair;
            if (delta >=     tooLate)  return Judgement.Poor;
                                       return Judgement.Break;
        }

        private void JudgeButtonPressed(int btn, double time)
        {
            if (timer.Current is not (Running or Resuming))
                return;
            
            var target = cachedNotes[btn];
            if (target == null) return;

            if (sounds.ContainsKey(target.Path))
            {
                AudioManager.PlaySound(sounds[target.Path]);
            }
            
            if (target is LongNoteSystem { Current: CutLongNoteState } note)
            {
                var cDelta = cachedCutTime[btn] - timer.CurrentTime;
                if (cDelta >= ignorable || cDelta < tooLate)
                    return;

                var r = GetJudgement(cachedDelta[btn]);
                var x = cachedDelta[btn] >= 0;
                
                if (r == Judgement.Break)
                {
                    Judge(r, x, JudgeTarget.LongNoteCut, btn);
                    Release(target);
                    cachedNotes[btn] = Get(btn);
                    return;
                }
                
                Judge(r, x, JudgeTarget.LongNoteCut, btn);
                note.SetActive(0, () => OnTick(r, x, btn));
                return;
            }

            var delta = target.Time - timer.CurrentTime;

            if (timer.Current is Resuming)
            {
                var pDelta = target.Time - pausedTime;
                delta = math.abs(delta) >= math.abs(pDelta) ? delta : pDelta;
            }
            
            if (delta >= ignorable || delta < tooLate)
                return;

            var result = GetJudgement(delta);
            var isEarly = delta >= 0;

            if (target is LongNoteSystem system)
            {
                Judge(result, isEarly, JudgeTarget.LongNoteStart, btn);
                
                if (result == Judgement.Break)
                {
                    Release(target);
                    cachedNotes[btn] = Get(btn);
                    return;
                }
                
                cachedDelta[btn] = delta;
                system.SetActive(timer.TimeToBeat(timer.CurrentTime), () => OnTick(result, isEarly, btn));
            }
            else // target is NoteSystem
            {
                Judge(result, isEarly, JudgeTarget.Note, btn);
                Release(target);
                cachedNotes[btn] = Get(btn);
            }
        }

        private void JudgeButtonReleased(int btn, double time)
        {
            if (timer.Current is not (Running or Resuming))
                return;
            
            var target = cachedNotes[btn] as LongNoteSystem;

            if (!target) return;
            if (target.Current is not ActiveLongNoteState) return;

            var delta = target.EndTime - timer.CurrentTime;
            
            Release(cachedNotes[btn]);
            cachedNotes[btn] = Get(btn);

            // Released so early.
            if (delta > tooEarly)
            {
                Judge(Judgement.Break, true, JudgeTarget.LongNoteEnd, btn);
            }
            else
            {
                var r = GetJudgement(cachedDelta[btn]);
                var x = cachedDelta[btn] >= 0;
                Judge(r, x, JudgeTarget.LongNoteEnd, btn);
            }
        }

        private void OnTick(Judgement result, bool isEarly, int line)
        {
            Judge(result, isEarly, JudgeTarget.LongNoteTick, line);
        }

        private NoteSystem Get(int value)
        {
            return noteQueue[value].Count == 0 ? null : noteQueue[value].Dequeue();
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

        private void OnPaused()
        {
            pausedTime = timer.CurrentTime;
            pausedBeat = timer.CurrentBeat;
            cachedNotes.ForEach(note => (note as LongNoteSystem)?.Pause(pausedTime));

            for (int i = 0; i < cachedCutTime.Count; i++)
            {
                if (cachedNotes[i] is LongNoteSystem {Current: CutLongNoteState})
                {
                    cachedCutTime[i] = timer.CurrentTime;
                }
            }
            
            paused?.Invoke();
            channel.setPaused(true);
            FMODUnity.RuntimeManager.PlayOneShot(pausedEvent);
        }

        private void OnFinished()
        {
            Result.Instance.score = score;
            Result.Instance.judgeCount = judgeCount;
            Result.Instance.accuracy = (double)score / (noteCount * scoreWeight[0]) * 100d;
            channel.stop();
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

        public void Back()
        {
            channel.stop();
            SceneManager.LoadScene("Select");
        }

        public void Retry()
        {
            channel.stop();
            SceneManager.LoadScene("Ingame");
        }
    }
    
    public enum Judgement { Precise, Great, Fair, Poor, Break }

    public enum JudgeTarget { Note, LongNoteStart, LongNoteTick, LongNoteEnd,
        LongNoteCut
    }
}