using System;
using System.Collections.Generic;
using System.Linq;
using Core;

namespace CYAN4S
{
    [Serializable]
    public class Chart
    {
        public string title;
        public int button;
        public int level;
        public float bpm;

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;

        // TODO
        [NonSerialized] public string audio;
        [NonSerialized] public string rootPath;


        public static double GetEndBeat(Chart chart)
        {
            // var a = chart.notes?.Max(note => (double)note.beat) ?? 0;
            double a = 0;
            if (chart.notes == null) a = 0;
            else if (chart.notes is { Count: 0 }) a = 0;
            else a = chart.notes?.Max(note => (double)note.beat) ?? 0;

            var b = chart.longNotes.Count == 0 ? 0 : chart.longNotes.Max(note => note.beat + (double) note.length);

            return a > b ? a : b;
        }

        public double GetEndBeat()
        {
            return GetEndBeat(this);
        }
    }
}