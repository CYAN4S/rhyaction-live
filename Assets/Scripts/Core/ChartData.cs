using System;
using System.Collections.Generic;

namespace Core
{
    [Serializable]
    public class ChartData
    {
        public int button;
        public decimal level;

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;
        public List<BpmData> bpms;
    }

    [Serializable]
    public class BpmData
    {
        public Fraction beat;
        public double bpm;
    }

    [Serializable]
    public class TrackData
    {
        public string title;
    }
}