using System;
using System.Collections;
using System.Transactions;
using Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CYAN4S
{
    [Serializable]
    public class Timer
    {
        public TimerStateMachine state;
        public ITimerState Current => state.CurrentState;

        private double _initialTime = -3;
        private double _revertTime = 4;

        [Header("Debug")]
        [SerializeField] private double time;
        [SerializeField] private double beat;
        [SerializeField] private float bpm;
        [SerializeField] private double endBeat;
        
        public double CurrentTime => time;
        public double CurrentBeat => beat;

        public bool onZeroInvoked = false;
        public Action onZero;

        public Timer()
        {
            time = _initialTime;
            state = new TimerStateMachine(this);
            state.Initialize(state.beforeStart);
        }

        public void SetTimer(float bpm, double getEndBeat)
        {
            this.bpm = bpm;
            beat = _initialTime / 60d * bpm;
            endBeat = getEndBeat;
            
            state.TransitionTo(state.running);
        }

        public void Update() => state.Update();

        public double GetGameTime(double rawTime) => rawTime + _initialTime;

        public double TimeToBeat(double time) => time / 60d * bpm;
        public double BeatToTime(double beat) => beat * 60d / bpm;

        public bool Ended => beat >= endBeat + 4;

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

        public void AddDeltaTime()
        {
            time += Time.deltaTime;
            beat = CurrentTime / 60d * bpm;
        }

        public void RevertTime()
        {
            time -= _revertTime;
            beat = TimeToBeat(CurrentTime);
        }
    }

    // State Pattern
    public interface ITimerState : IState
    {
        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }

        void IState.Enter() => OnEnter?.Invoke();
        void IState.Exit() => OnExit?.Invoke();
    }

    public class BeforeStart : ITimerState
    {
        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
    }

    public class Running : ITimerState
    {
        private readonly Timer _timer;
        public Running(Timer timer) => _timer = timer;

        public void Update()
        {
            _timer.AddDeltaTime();

            if (_timer.CurrentTime >= 0 && !_timer.onZeroInvoked)
            {
                _timer.onZero?.Invoke();
                _timer.onZeroInvoked = true;
            }

            if (_timer.Ended)
            {
                _timer.state.TransitionTo(_timer.state.finished);
            }
        }

        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
    }

    public class Paused : ITimerState
    {
        private readonly Timer _timer;
        public Paused(Timer timer) => _timer = timer;
        
        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
    }

    public class Resuming : ITimerState
    {
        private readonly Timer _timer;
        public Resuming(Timer timer) => _timer = timer;

        private double _target;
        
        public void Enter()
        {
            OnEnter?.Invoke();
            _target = _timer.CurrentTime;
            _timer.RevertTime();
        }

        public void Update()
        {
            _timer.AddDeltaTime();
            if (_target <= _timer.CurrentTime)
                _timer.state.TransitionTo(_timer.state.running);
        }

        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
    }

    public class Finished : ITimerState
    {
        private readonly Timer _timer;
        public Finished(Timer timer) => _timer = timer;

        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
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
            paused = new Paused(timer);
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