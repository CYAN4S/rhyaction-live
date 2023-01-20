using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CYAN4S
{
    [Serializable]
    public class Timer
    {
        [SerializeField] public double initialTime = -3d;
        [SerializeField] public double revertTime = 3d;

        [Header("Debug")]
        [SerializeField] public TimerStateMachine state;

        [SerializeField] private double time;
        [SerializeField] private double beat;
        [SerializeField] private float bpm;
        [SerializeField] private double endBeat;
        
        public double CurrentTime => time;
        public double CurrentBeat => beat;

        public Action onFinished;

        public bool onZeroInvoked = false;
        public Action onZero;

        public Action paused;
        public Action onResume;


        public Timer()
        {
            time = initialTime;
            state = new TimerStateMachine(this);
            state.Initialize(state.beforeStart);
        }

        public void SetTimer(float bpm, double getEndBeat)
        {
            this.bpm = bpm;
            beat = initialTime / 60d * bpm;
            endBeat = getEndBeat;
            
            state.TransitionTo(state.running);
        }

        public void Update() => state.Update();

        public double GetGameTime(double rawTime) => rawTime + initialTime;

        public double TimeToBeat(double time) => time / 60d *  bpm;
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
            time -= revertTime;
            beat = TimeToBeat(CurrentTime);
        }
    }

    // State Pattern
    public enum TimerState
    {
        BeforeStart, Running, Paused, Resuming, Finished
    }
    public interface ITimerState : IState
    {
    }

    public class BeforeStart : ITimerState
    {
        
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
    }

    public class Paused : ITimerState
    {
        private readonly Timer _timer;
        public Paused(Timer timer) => _timer = timer;
        
        public void Enter()
        {
            Debug.Log("Paused");
            _timer.paused?.Invoke();
        }
    }

    public class Resuming : ITimerState
    {
        private readonly Timer _timer;
        public Resuming(Timer timer) => _timer = timer;

        private double _target;
        
        public void Enter()
        {
            Debug.Log("Resuming");
            
            _target = _timer.CurrentTime;
            _timer.RevertTime();

            _timer.onResume?.Invoke();
        }

        public void Update()
        {
            _timer.AddDeltaTime();

            if (_target <= _timer.CurrentTime)
                _timer.state.TransitionTo(_timer.state.running);
        }
    }

    public class Finished : ITimerState
    {
        private readonly Timer _timer;
        public Finished(Timer timer) => _timer = timer;
        
        public void Enter()
        {
            _timer.onFinished?.Invoke();
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