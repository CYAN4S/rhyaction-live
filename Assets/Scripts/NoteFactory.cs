using System;
using System.Collections.Generic;

namespace CYAN4S
{
    public class NoteFactory
    {
        private readonly List<Queue<NoteSystem>> _noteQueue;
        private readonly Action<NoteSystem> _destroy;

        public NoteFactory(Chart chart, Func<NoteSystem> instantiate, Action<NoteSystem> destroy)
        {
            var button = chart.button;
            var notes = chart.notes;
            var longNotes = chart.longNotes;

            _noteQueue = new List<Queue<NoteSystem>>();
            var noteTemp = new List<List<NoteSystem>>();

            _destroy = destroy;

            for (var i = 0; i < button; i++)
            {
                noteTemp.Add(new List<NoteSystem>());
                _noteQueue.Add(new Queue<NoteSystem>());
            }

            foreach (var note in notes)
            {
                var noteSystem = instantiate();
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);

                noteTemp[note.line].Add(noteSystem);
            }

            foreach (var note in longNotes)
            {
                var noteSystem = instantiate();
                // TODO MATH
                noteSystem.InstanceInitialize(note, (float) note.beat * 0.5f);
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