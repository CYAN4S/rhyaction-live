using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class IdleLongNoteState : ILongNoteState
    {
        private readonly LongNoteSystem _note;
        public IdleLongNoteState(LongNoteSystem note) => _note = note;

        public void Update()
        {
            _note.UpdateLong();
        }
    }

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

    public class LongNoteSystem : NoteSystem
    {
        public bool IsInProgress => state.CurrentState is ActiveLongNoteState;

        public double EndTime { get; private set; }

        private Queue<double> _ticks;
        private Action _onTick;

        private double _length;

        public LongNoteStateMachine state;

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

        public void InstanceInitialize(LongNoteData data, double startTime, double endTime, Func<double> beat)
        {
            noteData = data;
            Time = startTime;
            EndTime = endTime;
            getBeat = beat;

            _length = (noteData as LongNoteData)?.length;
        }

        public void SetActive(double startBeat, Action onTick)
        {
            state.TransitionTo(state.activeState);
            SetTicks(startBeat, onTick);
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

        private float GetYEndPos()
        {
            return GetYPos(noteData.beat + _length, getBeat(), getScrollSpeed());
        }
    }

    public interface ILongNoteState : IState { }

    [Serializable]
    public class LongNoteStateMachine
    {
        public ILongNoteState CurrentState { get; private set; }

        public IdleLongNoteState idleState;
        public ActiveLongNoteState activeState;

        public LongNoteStateMachine(LongNoteSystem note)
        {
            idleState = new IdleLongNoteState(note);
            activeState = new ActiveLongNoteState(note);
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
}