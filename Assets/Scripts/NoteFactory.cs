using System;
using System.Collections.Generic;

namespace CYAN4S
{
    [Serializable]
    public class NoteFactory
    {
        private readonly Action<NoteSystem> _destroy;
        private readonly List<Queue<NoteSystem>> _noteQueue = new();

        public NoteFactory(Chart chart, Func<NoteSystem> makeNote, Func<LongNoteSystem> makeLongNote,
            Action<NoteSystem> destroy, Func<double> beat)
        {
            // Initialize
            _destroy = destroy;

            // Get essential info from chart.
            var bpm = chart.bpm;
            var buttonCount = chart.button;
            var noteDataList = chart.notes;
            var longNoteDataList = chart.longNotes;

            var noteTemp = new List<List<NoteSystem>>();

            for (var i = 0; i < buttonCount; i++)
            {
                noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            foreach (var note in noteDataList)
            {
                var system = makeNote();
                var time = note.beat * 60d / (double) bpm;

                system.InstanceInitialize(note, time, beat);
                noteTemp[note.line].Add(system);
            }

            foreach (var note in longNoteDataList)
            {
                var system = makeLongNote();
                var start = note.beat * 60d / (double) bpm;
                var end = (note.beat + note.length) * 60d / (double) bpm;

                system.InstanceInitialize(note, start, end, beat);
                noteTemp[note.line].Add(system);
            }

            for (var i = 0; i < noteTemp.Count; i++)
            {
                var temp = noteTemp[i];
                temp.Sort((a, b) => a.Time.CompareTo(b.Time));

                _noteQueue[i] = new Queue<NoteSystem>(temp);
            }
        }

        public NoteSystem Get(int value)
        {
            return _noteQueue[value].Count == 0 ? null : _noteQueue[value].Dequeue();
        }

        public void Release(NoteSystem target)
        {
            _destroy(target);
        }
    }
}