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

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;
        public List<BpmData> bpms;

        public Chart(ChartData chartData, TrackData trackData)
        {
            title = trackData.title;
            button = chartData.button;
            notes = chartData.notes;
            longNotes = chartData.longNotes;
            level = chartData.level;
            bpms = chartData.bpms;
        }

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
                bpms = new List<BpmData> {new() {beat = new Fraction(0), bpm = 120}}
            };

            for (var i = 20; i < 30; i++)
                target.notes.Add(new NoteData(new Fraction(i, 4), i % 4, @"C:/Temp/clap.wav"));
            for (var i = 0; i < 20; i += 4)
                target.longNotes.Add(new LongNoteData(new Fraction(i, 4), i / 4 % 4, @"C:/Temp/clap.wav",
                    new Fraction(1, 2)));

            return target;
        }
    }
}