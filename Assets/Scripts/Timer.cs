using System;
using Core;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class Timer
    {
        [SerializeField] public double initialTime = -3d;

        [Header("Debug")]
        [SerializeField] private TimerStateMachine state;

        public decimal _bpm;

        public Timer()
        {
            Time = initialTime;
            state = new TimerStateMachine(this);
            state.Initialize(state.beforeStart);
        }

        public void SetBpm(decimal bpm)
        {
            _bpm = bpm;
            Beat = initialTime / 60d * (double) bpm;
            state.TransitionTo(state.running);
        }

        [field: SerializeField] public double Time { get; set; }
        [field: SerializeField] public double Beat { get; set; }

        public void Update()
        {
            state.Update();
        }

        public double GetGameTime(double rawTime)
        {
            return rawTime + initialTime;
        }

        public double TimeToBeat(double time)
        {
            return time / 60d * (double) _bpm;
        }

        public double BeatToTime(double beat)
        {
            return beat * 60d / (double) _bpm;
        }

        public void PauseOrResume()
        {
            switch (state.CurrentState)
            {
                case Paused:  Resume(); break;
                case Running: Pause();  break;
            }
        }

        public void Pause()
        {
            state.TransitionTo(state.paused);
        }

        public void Resume()
        {
            state.TransitionTo(state.running);
        }
    }

    public interface ITimerState : IState { }

    public class BeforeStart : ITimerState { }

    public class Running : ITimerState
    {
        private readonly Timer _timer;

        public Running(Timer timer) => _timer = timer;

        public void Enter()
        {
            // Time.timeScale = 1f;
        }

        public void Update()
        {
            _timer.Time += Time.deltaTime;
            _timer.Beat = _timer.Time / 60d * (double) _timer._bpm;
        }
    }

    public class Paused : ITimerState
    {
        public void Enter()
        {
            // Time.timeScale = 0;
        }
    }

    public class Resuming : ITimerState
    {
        
    }

    [Serializable]
    public class TimerStateMachine
    {
        public ITimerState CurrentState { get; private set; }

        public BeforeStart beforeStart;
        public Running running;
        public Paused paused;
        public Resuming resuming;

        public TimerStateMachine(Timer timer)
        {
            beforeStart = new BeforeStart();
            running = new Running(timer);
            paused = new Paused();
            resuming = new Resuming();
        }

        public void Initialize(ITimerState startingState)
        {
            CurrentState = startingState;
            startingState.Enter();
        }

        public void TransitionTo(ITimerState nextState)
        {
            CurrentState.Exit();
            CurrentState = nextState;
            nextState.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}