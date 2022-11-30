using System;
using System.Collections;
using Core;
using UnityEngine;

namespace CYAN4S
{
    [Serializable]
    public class Timer
    {
        [SerializeField] public double initialTime = -3d;
        [SerializeField] public double revertTime = 3d;

        [Header("Debug")]
        [SerializeField] public TimerStateMachine state;

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
            state.TransitionTo(state.resuming);
        }
    }

    public interface ITimerState : IState { }

    [Serializable]
    public class BeforeStart : ITimerState { }

    [Serializable]
    public class Running : ITimerState
    {
        private readonly Timer _timer;
        public Running(Timer timer) => _timer = timer;

        public void Enter()
        {
            Debug.Log("Running");
        }

        public void Update()
        {
            _timer.Time += Time.deltaTime;
            _timer.Beat = _timer.Time / 60d * (double) _timer._bpm;
        }
    }

    [Serializable]
    public class Paused : ITimerState
    {
        public void Enter()
        {
            Debug.Log("Paused");
        }
    }

    [Serializable]
    public class Resuming : ITimerState
    {
        private readonly Timer _timer;
        public Resuming(Timer timer) => _timer = timer;

        private double _target;
        
        public void Enter()
        {
            Debug.Log("Resuming");
            
            _target = _timer.Time;
            _timer.Time -= _timer.revertTime;
            _timer.Beat = _timer.TimeToBeat(_timer.Time);
        }

        public void Update()
        {
            _timer.Time += Time.deltaTime;
            _timer.Beat = _timer.Time / 60d * (double) _timer._bpm;

            if (_target <= _timer.Time)
                _timer.state.TransitionTo(_timer.state.running);
        }
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
            resuming = new Resuming(timer);
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