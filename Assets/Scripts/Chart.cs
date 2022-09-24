using System;
using System.Collections.Generic;
using Core;

namespace CYAN4S
{
    [Serializable]
    public class Chart
    {
        public string title;
        public int button;
        public decimal level;
        public decimal bpm;

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;

        private Chart()
        {
        }

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

            // for (var i = 0; i < 20; i++)
            //     target.notes.Add(new NoteData(new Fraction(i, 4), i % 4, @"C:/Temp/clap.wav"));
            // for (var i = 20; i < 36; i += 4)
            //     target.longNotes.Add(new LongNoteData(new Fraction(i, 4), i / 4 % 4, @"C:/Temp/clap.wav",
            //         new Fraction(1, 2)));
                target.longNotes.Add(new LongNoteData(new Fraction(0), 1, @"C:/Temp/clap.wav",
                    new Fraction(2)));

            return target;
        }
    }
}