using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class LongNoteSystem : NoteSystem
    {
        public double EndTime { get; private set; }

        private Queue<double> _ticks;
        private Action _onTick;

        private double _length;

        public double cutTime;
        public double cutBeat;

        public LongNoteStateMachine state;
        public ILongNoteState Current => state.CurrentState;

        protected override void Awake()
        {
            base.Awake();

            state = new LongNoteStateMachine(this);
            state.Initialize(state.idleState);
        }

        protected override void Update()
        {
            state.Update();
        }

        public void CheckTick()
        {
            if (_ticks is null) return;

            if (_ticks.Count == 0) return;

            if (_ticks.Peek() <= getBeat())
            {
                _ticks.Dequeue();
                _onTick.Invoke();
            }
        }

        public void InstanceInitialize(LongNoteData data, double startTime, double endTime)
        {
            noteData = data;
            Time = startTime;
            EndTime = endTime;
            _length = (noteData as LongNoteData)?.length;
        }

        public void SetActive(double startBeat, Action onTick)
        {
            if (Current is IdleLongNoteState)
            {
                state.TransitionTo(state.activeState);
                SetTicks(startBeat, onTick);
            }
            
            if (Current is CutLongNoteState)
            {
                state.TransitionTo(state.activeState);
            }
        }

        private void SetTicks(double startBeat, Action onTick)
        {
            const int ticksPerBeat = 4;

            var length = ((LongNoteData) noteData).length;
            var count = (int) (length * new Fraction(ticksPerBeat));

            _ticks = new Queue<double>(
                Enumerable
                    .Range(1, count - 1)
                    .Select(x => startBeat + (double) x / ticksPerBeat));

            _onTick = onTick;
        }

        public float GetYSize()
        {
            return GetYEndPos() - GetYPos();
        }

        public void UpdateLong()
        {
            rt.localPosition = new Vector3(rt.localPosition.x, GetYPos());
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, GetYSize());
        }

        public void UpdateActiveLong()
        {
            rt.localPosition = new Vector3(rt.localPosition.x, 0);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,Mathf.Max(GetYEndPos(), 0f));
        }

        public void UpdateCutLong()
        {
            rt.localPosition = new Vector3(rt.localPosition.x, GetYPos(cutBeat));
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, GetYEndPos() - GetYPos(cutBeat));
        }

        private float GetYEndPos()
        {
            return GetYPos(noteData.beat + _length, getBeat(), getScrollSpeed());
        }

        public void Pause(double time)
        {
            if (Current is not ActiveLongNoteState) return;
            
            cutBeat = getBeat();
            cutTime = time;
            state.TransitionTo(state.cutLongNoteState);
        }
    }

    /// <summary>
    /// Behaviour of long note changes depend on its own state.
    /// </summary>
    public interface ILongNoteState : IState { }

    [Serializable]
    public class LongNoteStateMachine
    {
        public ILongNoteState CurrentState { get; private set; }

        public IdleLongNoteState idleState;
        public ActiveLongNoteState activeState;
        public CutLongNoteState cutLongNoteState;

        public LongNoteStateMachine(LongNoteSystem note)
        {
            idleState = new IdleLongNoteState(note);
            activeState = new ActiveLongNoteState(note);
            cutLongNoteState = new CutLongNoteState(note);
        }

        public void Initialize(ILongNoteState startingState)
        {
            CurrentState = startingState;
            startingState.Enter();
        }

        public void TransitionTo(ILongNoteState nextState)
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
    
    /// <summary>
    /// Long note which is not in process yet.
    /// </summary>
    public class IdleLongNoteState : ILongNoteState
    {
        private readonly LongNoteSystem _note;
        public IdleLongNoteState(LongNoteSystem note) => _note = note;

        public void Update()
        {
            _note.UpdateLong();
        }
    }

    /// <summary>
    /// Long note which is currently in process.
    /// Head of the note is placed in judgement line.
    /// </summary>
    public class ActiveLongNoteState : ILongNoteState
    {
        private readonly LongNoteSystem _note;
        public ActiveLongNoteState(LongNoteSystem note) => _note = note;

        public void Update()
        {
            _note.UpdateActiveLong();
            _note.CheckTick();
        }
    }

    /// <summary>
    /// Long note which was cut because of pausing by user.
    /// If it pressed, judgement of head should be considered.
    /// Anything is same as IdleLongNoteState.
    /// </summary>
    public class CutLongNoteState : ILongNoteState
    {
        private readonly LongNoteSystem _note;
        public CutLongNoteState(LongNoteSystem note) => _note = note;

        public void Update()
        {
            _note.UpdateCutLong();
        }
    }    
    
    /// <summary>
    /// Long note which is missed, or released too early.
    /// Just for visual, and it only needs to fall.
    /// </summary>
    public class MissedLongNoteState : ILongNoteState
    {
        
    }
}