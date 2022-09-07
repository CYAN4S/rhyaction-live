using System;
using System.Collections.Generic;

namespace Core
{
    [Serializable]
    public class ChartData
    {
        public int button;
        public decimal level;

        public decimal bpm;

        public List<NoteData> notes;
        public List<LongNoteData> longNotes;
    }

    [Serializable]
    public class TrackData
    {
        public string title;
    }
}