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

        // This uses only for development.
        public static Chart GetTestChart()
        {
            var target = new Chart
            {
                title = "Testing",
                notes = new List<NoteData>(),
                longNotes = new List<LongNoteData>(),
                button = 4,
                bpm = 120
            };

            for (var i = 0; i < 20; i++)
                target.notes.Add(new NoteData(new Fraction(i, 4), i % 4, @"C:/Temp/clap.wav"));
            for (var i = 20; i < 36; i += 4)
                target.longNotes.Add(new LongNoteData(new Fraction(i, 4), i % 4, @"C:/Temp/clap.wav",
                    new Fraction(1, 2)));
            
            target.longNotes.Add(new LongNoteData(new Fraction(36, 4), 0, @"C:/Temp/clap.wav",
                new Fraction(4)));

            return target;
        }

        public static double GetEndBeat(Chart chart)
        {
            var a = chart.notes.Max(note => (double)note.beat);
            var b = chart.longNotes.Max(note => note.beat + (double)note.length);

            return a > b ? a : b;
        }

        public double GetEndBeat()
        {
            return GetEndBeat(this);
        }
    }
}