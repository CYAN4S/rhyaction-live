using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    [Serializable]
    public class Timer
    {
        [SerializeField] public double initialTime = -3d;
        [SerializeField] public double revertTime = 3d;

        [Header("Debug")]
        [SerializeField] public TimerStateMachine state;
        [field: SerializeField] public double Time { get; set; }
        [field: SerializeField] public double Beat { get; set; }
        
        public float _bpm;
        public double endBeat;

        public Action OnFinished;

        public bool onZeroInvoked = false;
        public Action onZero;

        public Timer()
        {
            Time = initialTime;
            state = new TimerStateMachine(this);
            state.Initialize(state.beforeStart);
        }

        public void SetTimer(float bpm, double getEndBeat)
        {
            _bpm = bpm;
            Beat = initialTime / 60d * bpm;
            endBeat = getEndBeat;
            
            state.TransitionTo(state.running);
        }

        public void Update() => state.Update();

        public double GetGameTime(double rawTime) => rawTime + initialTime;

        public double TimeToBeat(double time) => time / 60d *  _bpm;
        public double BeatToTime(double beat) => beat * 60d / _bpm;

        public void PauseOrResume()
        {
            switch (state.CurrentState)
            {
                case Paused:  Resume(); break;
                case Running: Pause();  break;
            }
        }

        public void Pause() => state.TransitionTo(state.paused);
        public void Resume() => state.TransitionTo(state.resuming);
    }

    // State Pattern
    public interface ITimerState : IState { }

    [Serializable]
    public class BeforeStart : ITimerState { }

    [Serializable]
    public class Running : ITimerState
    {
        private readonly Timer _timer;
        public Running(Timer timer) => _timer = timer;

        public void Update()
        {
            _timer.Time += Time.deltaTime;
            _timer.Beat = _timer.Time / 60d * _timer._bpm;

            if (_timer.Time >= 0 && !_timer.onZeroInvoked)
            {
                _timer.onZero?.Invoke();
                _timer.onZeroInvoked = true;
            }

            if (_timer.Beat >= _timer.endBeat + 4)
            {
                _timer.state.TransitionTo(_timer.state.finished);
            }
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
            _timer.Beat = _timer.Time / 60d * _timer._bpm;

            if (_target <= _timer.Time)
                _timer.state.TransitionTo(_timer.state.running);
        }
    }

    [Serializable]
    public class Finished : ITimerState
    {
        private readonly Timer _timer;
        public Finished(Timer timer) => _timer = timer;
        
        public void Enter()
        {
            _timer.OnFinished?.Invoke();
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
        public Finished finished;

        public TimerStateMachine(Timer timer)
        {
            beforeStart = new BeforeStart();
            running = new Running(timer);
            paused = new Paused();
            resuming = new Resuming(timer);
            finished = new Finished(timer);
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