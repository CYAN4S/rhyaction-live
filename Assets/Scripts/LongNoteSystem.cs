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
        public bool IsProgress => _nt.State == LongNoteState.Progress;
        public double EndTime { get; private set; }

        private Queue<double> _ticks;

        private Action _onTick;

        protected override void Update()
        {
            _nt.Update(getBeat(), 4);

            if (IsProgress)
            {
                CheckTick();
            }
        }

        private void CheckTick()
        {
            if (_ticks is null)
            {
                return;
            }
            
            if (_ticks.Count == 0)
            {
                return;
            }

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

            _nt = new LongNoteTranslator(noteData, rectTransform);
        }

        public void OnProgress()
        {
            _nt.SetLongNoteState(LongNoteState.Progress);
        }

        public void SetTicks(double startTime, Action onTick)
        {
            var length = ((LongNoteData) noteData).length;
            var count = (int) (length * new Fraction(8));
            
            Debug.Log($"{length}, {count}");
            _ticks = new Queue<double>(Enumerable.Range(1, count - 1).Select(x => startTime + x * 0.25));
            _onTick = onTick;
        }
    }
}