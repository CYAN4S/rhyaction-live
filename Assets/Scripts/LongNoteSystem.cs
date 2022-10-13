using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class LongNoteSystem : NoteSystem
    {
        private LongNoteTranslator _nt;
        public bool IsInProgress => State == LongNoteState.InProgress;
        public double EndTime { get; private set; }

        private Queue<double> _ticks;

        private Action _onTick;
        
        public LongNoteState State { get; private set; }

        protected override void Update()
        {
            _nt.Update(getBeat(), 4);

            if (IsInProgress)
            {
                CheckTick();
            }
        }

        private void CheckTick()
        {
            if (_ticks is null)
                return;
            
            if (_ticks.Count == 0)
                return;

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

            _nt = new LongNoteTranslator(data, rectTransform);
            State = LongNoteState.Idle;
        }

        public void OnProgress()
        {
            State = LongNoteState.InProgress;
            _nt.SetLongNoteState(State);
        }

        public void SetTicks(double startTime, Action onTick)
        {
            var length = ((LongNoteData) noteData).length;
            var count = (int) (length * new Fraction(4));
            
            _ticks = new Queue<double>(Enumerable.Range(1, count - 1).Select(x => startTime + x * 0.25));
            _onTick = onTick;
        }
    }
}