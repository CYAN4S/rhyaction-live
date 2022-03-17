using System.Collections.Generic;
using Core;

namespace CYAN4S
{
    public class Chart
    {
        public string title;
        public int button;
        public decimal level;

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;

        public BpmMeta bpms;

        public Chart(ChartData chartData, TrackData trackData)
        {
        }

        private Chart()
        {
        }

        public static Chart GetTestChart()
        {
            var target = new Chart
            {
                notes = new List<NoteData>(),
                longNotes = new List<LongNoteData>(),
                button = 4
            };

            for (var i = 20; i < 30; i++)
                target.notes.Add(new NoteData(new Fraction(i, 4), i % 4, null));
            for (var i = 0; i < 20; i += 4)
                target.longNotes.Add(new LongNoteData(new Fraction(i, 4), i / 4 % 4, null, new Fraction(1, 2)));

            return target;
        }
    }

    public class BpmMeta
    {
    }
}