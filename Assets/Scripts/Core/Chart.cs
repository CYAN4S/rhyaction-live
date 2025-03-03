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


        public int NoteCount => notes.Count + longNotes.Count;

        public static double GetEndBeat(Chart chart)
        {
            double result = 0;

            if (chart.notes.Count != 0)
            {
                result = chart.notes.Max(note => (double)note.beat);
            }

            if (chart.longNotes.Count != 0)
            {
                double max = chart.longNotes.Max(note => note.beat + (double) note.length);
                if (result < max)
                {
                    result = max;
                }
            }

            return result;
        }

        public double GetEndBeat()
        {
            return GetEndBeat(this);
        }
    }
}