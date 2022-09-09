using System;
using System.Collections.Generic;

namespace CYAN4S
{
    [Serializable]
    public class NoteFactory
    {
        private readonly List<Queue<NoteSystem>> _noteQueue = new();
        private readonly Action<NoteSystem> _destroy;

        public NoteFactory(Chart chart, Func<NoteSystem> instantiate, Action<NoteSystem> destroy, Func<double> beat)
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
                var noteSystem = instantiate();
                noteSystem.InstanceInitialize(note,  note.beat * 60d / (double)bpm, NoteType.Normal, beat);
                noteTemp[note.line].Add(noteSystem);
            }

            foreach (var note in longNoteDataList)
            {
                var noteSystem = instantiate();
                noteSystem.InstanceInitialize(note, note.beat * 60d / (double)bpm, NoteType.Long, beat);
                noteTemp[note.line].Add(noteSystem);
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